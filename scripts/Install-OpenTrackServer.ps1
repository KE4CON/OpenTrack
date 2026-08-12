<#
.SYNOPSIS
    One-command server setup for OpenTrack (the self-hosted issue tracker) on Windows.

.DESCRIPTION
    Turns the whole by-hand server install into a single command. It will:
      1. Make sure the tools it needs are present (the .NET 10 SDK and Git) - installing
         them with winget if they're missing.
      2. Get the OpenTrack source code (use the copy this script lives in, or clone it).
      3. Build ("publish") the two server programs: the Web app (what people open in a
         browser) and the API (what the Windows/Mac desktop app talks to).
      4. Point both programs at ONE shared database in a fixed data folder, so they never
         drift into two separate databases.
      5. Open the Windows Firewall for the two ports so other computers on your network
         can reach the server.
      6. Register both programs to start automatically when the machine boots, and start
         them now.
      7. Optionally install a free, private local AI (Ollama) on the same machine.
      8. Print the finish-up steps (the address to open, and how to create the first admin).

    Run it again anytime to UPDATE: it pulls the latest code, rebuilds, and restarts.

    Run it with NO options and it interactively asks a few plain questions (where to keep the data,
    whether to install the local AI, whether to set the first administrator, and the ports), each with
    a default you accept by pressing Enter. Pass any flag or -ConfigFile (or -NonInteractive) and it
    skips the questions and runs unattended.

    Everything is a plain, reversible change - no registry surgery, no third-party services.
    The two programs run as scheduled tasks under the SYSTEM account so they're always on.

.PARAMETER ConfigFile
    Optional path to a settings file (Key = Value per line) that supplies any of the
    parameters below, so you don't have to type them on the command line. A sample file,
    opentrack-server.sample.conf, sits next to this script. Command-line values win over
    the file.

.PARAMETER RepoUrl
    Where to clone OpenTrack from if the source isn't already present.

.PARAMETER Branch
    Which branch to build. Default: main.

.PARAMETER SourceDir
    Where the source code lives / will be cloned. If this script is already inside a clone,
    that clone is used and this is ignored.

.PARAMETER InstallDir
    Where the built, ready-to-run programs are placed (a 'web' and an 'api' subfolder).

.PARAMETER DataDir
    Where the database (and, by default, scheduled backups) live. Keep this on a drive you
    back up. Default: C:\OpenTrack\data.

.PARAMETER BindAddress
    Which network address the server listens on. 0.0.0.0 means "all addresses" so other
    machines on the LAN can reach it (the normal choice). Use 127.0.0.1 to keep it to this
    machine only.

.PARAMETER WebPort
    The port people open in a browser. Default: 5035.

.PARAMETER ApiPort
    The port the desktop app talks to. Default: 5003.

.PARAMETER AutoStart
    'Task' (default) registers boot-time auto-start and starts the server now. 'None' skips
    that (you'd start the programs yourself).

.PARAMETER InstallAi
    Also install a free local AI engine (Ollama) on this machine and turn on OpenTrack's AI
    features pointed at it. Nothing leaves your network. Needs a reasonably capable machine
    (about 16 GB of memory is the sweet spot).

.PARAMETER AiModel
    Which local model to download for -InstallAi. Default: llama3.1.

.PARAMETER AdminEmail
    Optional. If given with -AdminPassword, this account is created/promoted to Administrator
    at first start, so you don't rely on "first person to register becomes admin."

.PARAMETER AdminPassword
    The password for -AdminEmail. Prefer setting this in the config file (readable only by
    administrators) over typing it on the command line.

.PARAMETER RequireHttps
    Force encrypted HTTPS (redirect + HSTS). Leave off for a trusted home/office LAN on plain
    HTTP; turn on if the server is reachable from outside. You must also provide a certificate
    (standard Windows/Kestrel hosting, outside this script).

.PARAMETER SkipPrereqs
    Don't try to install the .NET SDK / Git (assume they're already present).

.PARAMETER SkipFirewall
    Don't add firewall rules.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\Install-OpenTrackServer.ps1

.EXAMPLE
    .\Install-OpenTrackServer.ps1 -ConfigFile .\opentrack-server.conf -InstallAi

.NOTES
    Run in an ELEVATED (Administrator) PowerShell. Windows 10/11. OpenTrack - AGPL v3 - KE4CON.
#>
[CmdletBinding()]
param(
    [string]$ConfigFile,
    [string]$RepoUrl = "https://github.com/KE4CON/OpenTrack.git",
    [string]$Branch = "main",
    [string]$SourceDir = "C:\OpenTrack\src",
    [string]$InstallDir = "C:\OpenTrack\app",
    [string]$DataDir = "C:\OpenTrack\data",
    [string]$BindAddress = "0.0.0.0",
    [int]$WebPort = 5035,
    [int]$ApiPort = 5003,
    [ValidateSet('Task', 'None')][string]$AutoStart = 'Task',
    [switch]$InstallAi,
    [string]$AiModel = "llama3.1",
    [string]$AdminEmail,
    [string]$AdminPassword,
    [switch]$RequireHttps,
    [switch]$SkipPrereqs,
    [switch]$SkipFirewall,
    [switch]$NonInteractive
)

$ErrorActionPreference = 'Stop'
$script:StepNo = 0

# ----------------------------------------------------------------------------- helpers
function Write-Step([string]$msg) {
    $script:StepNo++
    Write-Host ""
    Write-Host ("[{0}] {1}" -f $script:StepNo, $msg) -ForegroundColor Cyan
}
function Write-Ok([string]$msg) { Write-Host "    OK  $msg" -ForegroundColor Green }
function Write-Info([string]$msg) { Write-Host "    -   $msg" -ForegroundColor Gray }
function Write-Warn2([string]$msg) { Write-Host "    !   $msg" -ForegroundColor Yellow }
function Fail([string]$msg) { Write-Host ""; Write-Host "ERROR: $msg" -ForegroundColor Red; exit 1 }

# Interactive prompt helpers (used only when the script is run bare, with no flags/config).
function Ask([string]$q, [string]$def) {
    $a = Read-Host ("{0} [{1}]" -f $q, $def)
    if ([string]::IsNullOrWhiteSpace($a)) { return $def } else { return $a.Trim() }
}
function AskYesNo([string]$q, [bool]$defaultYes) {
    $suffix = if ($defaultYes) { "[Y/n]" } else { "[y/N]" }
    $a = Read-Host ("{0} {1}" -f $q, $suffix)
    if ([string]::IsNullOrWhiteSpace($a)) { return $defaultYes }
    return ($a -match '^\s*(y|yes)\s*$')
}

function Test-Admin {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    $p = New-Object Security.Principal.WindowsPrincipal($id)
    return $p.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Apply a "Key = Value" config file to any parameter the caller did NOT pass explicitly.
function Import-ConfigFile([string]$path) {
    if (-not (Test-Path $path)) { Fail "Config file not found: $path" }
    Write-Info "Reading settings from $path"
    foreach ($line in Get-Content -LiteralPath $path) {
        $t = $line.Trim()
        if ($t -eq "" -or $t.StartsWith("#") -or -not $t.Contains("=")) { continue }
        $k = $t.Substring(0, $t.IndexOf("=")).Trim()
        $v = $t.Substring($t.IndexOf("=") + 1).Trim().Trim('"')
        if (-not $PSBoundParameters.ContainsKey($k)) {
            # Coerce to the parameter's type where it matters.
            if ($k -in @('WebPort', 'ApiPort')) { Set-Variable -Name $k -Value ([int]$v) -Scope 1 }
            elseif ($k -in @('InstallAi', 'RequireHttps', 'SkipPrereqs', 'SkipFirewall')) {
                Set-Variable -Name $k -Value ([System.Convert]::ToBoolean($v)) -Scope 1
            }
            else { Set-Variable -Name $k -Value $v -Scope 1 }
        }
    }
}

function Test-Command([string]$name) {
    $old = $ErrorActionPreference; $ErrorActionPreference = 'SilentlyContinue'
    $c = Get-Command $name
    $ErrorActionPreference = $old
    return $null -ne $c
}

function Get-DotnetSdkMajor {
    if (-not (Test-Command 'dotnet')) { return 0 }
    $max = 0
    foreach ($line in (& dotnet --list-sdks 2>$null)) {
        if ($line -match '^\s*(\d+)\.') { $m = [int]$Matches[1]; if ($m -gt $max) { $max = $m } }
    }
    return $max
}

function Install-WithWinget([string]$id, [string]$friendly) {
    if (-not (Test-Command 'winget')) {
        Fail "$friendly is missing and winget isn't available to install it. Install $friendly manually, then re-run (or use -SkipPrereqs)."
    }
    Write-Info "Installing $friendly via winget ($id) ..."
    & winget install --id $id -e --source winget --accept-package-agreements --accept-source-agreements --disable-interactivity
    if ($LASTEXITCODE -ne 0) { Fail "winget failed to install $friendly (exit $LASTEXITCODE)." }
    # Refresh this session's PATH so the just-installed tool is usable now.
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" +
                [System.Environment]::GetEnvironmentVariable("Path", "User")
}

# Write a JSON file as UTF-8 without a byte-order mark (cleanest for the config reader).
function Write-JsonFile([hashtable]$obj, [string]$path) {
    $json = $obj | ConvertTo-Json -Depth 8
    [System.IO.File]::WriteAllText($path, $json, (New-Object System.Text.UTF8Encoding($false)))
}

function Get-LanIPv4 {
    try {
        $ip = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction Stop |
            Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' -and $_.PrefixOrigin -ne 'WellKnown' } |
            Sort-Object -Property SkipAsSource |
            Select-Object -First 1 -ExpandProperty IPAddress
        if ($ip) { return $ip }
    } catch { }
    return "this-machine's-IP"
}

# ----------------------------------------------------------------------------- start
Write-Host ""
Write-Host "==================================================================" -ForegroundColor White
Write-Host "  OpenTrack - Server Setup" -ForegroundColor White
Write-Host "  Self-hosted issue tracker  -  AGPL v3  -  KE4CON" -ForegroundColor DarkGray
Write-Host "==================================================================" -ForegroundColor White

if ($ConfigFile) { Import-ConfigFile $ConfigFile }

if (-not (Test-Admin)) {
    Fail "Please run this in an ELEVATED PowerShell (right-click PowerShell -> 'Run as administrator'). It needs admin rights to open the firewall and register auto-start."
}

# If run bare (no flags, no -ConfigFile), walk the user through a few plain questions.
$explicitKeys = @('ConfigFile','RepoUrl','Branch','SourceDir','InstallDir','DataDir','BindAddress',
                  'WebPort','ApiPort','InstallAi','AiModel','AdminEmail','AdminPassword','RequireHttps',
                  'AutoStart','SkipPrereqs','SkipFirewall','NonInteractive')
$anyExplicit = $false
foreach ($k in $explicitKeys) { if ($PSBoundParameters.ContainsKey($k)) { $anyExplicit = $true; break } }
$Interactive = (-not $NonInteractive) -and (-not $anyExplicit) -and [Environment]::UserInteractive

if ($Interactive) {
    Write-Host ""
    Write-Host "Let's set up your OpenTrack server. I'll ask a few quick questions." -ForegroundColor White
    Write-Host "Press Enter to accept the default shown in [brackets]." -ForegroundColor White
    Write-Host ""
    Write-Host "First, where should your data (the OpenTrack database) be kept?" -ForegroundColor White
    Write-Host "  Tip: if this PC has a SECOND drive, keeping the data there means your projects and" -ForegroundColor Gray
    Write-Host "  issues survive even if Windows is ever reinstalled on C:. One drive is fine too." -ForegroundColor Gray
    if (AskYesNo "Does this PC have a SECOND drive (besides C:)?" $false) {
        $dl = Ask "  Which drive letter is that second drive?" "D"
        $dl = ($dl -replace '[^A-Za-z]', '')
        if ([string]::IsNullOrEmpty($dl)) { $dl = "D" }
        $dl = $dl.Substring(0, 1).ToUpper()
        if (-not (Test-Path ("{0}:\" -f $dl))) {
            Write-Warn2 ("Drive {0}: doesn't seem to exist yet - make sure it's connected and formatted before you continue." -f $dl)
        }
        $suggestedData = "{0}:\OpenTrack\Data" -f $dl
    } else {
        $suggestedData = "C:\OpenTrack\data"
    }
    $DataDir = Ask "Where should the database and data be kept?" $suggestedData
    if (AskYesNo "Install the free local AI on this machine now? (needs about 16 GB of memory)" $false) {
        $InstallAi = $true
        $AiModel = Ask "  Which AI model?" $AiModel
    }
    if (AskYesNo "Set up the first administrator account now?" $true) {
        $AdminEmail = Ask "  Administrator email" $AdminEmail
        $sec = Read-Host "  Administrator password (typing is hidden)" -AsSecureString
        if ($sec.Length -gt 0) {
            $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
            try { $AdminPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($bstr) }
            finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
        }
    }
    if (-not (AskYesNo "Use the standard ports (browser 5035, desktop app 5003)?" $true)) {
        $WebPort = [int](Ask "  Browser (web) port" "$WebPort")
        $ApiPort = [int](Ask "  Desktop-app (API) port" "$ApiPort")
    }
}

$dbPath = Join-Path $DataDir "opentrack.db"
$connString = "Data Source=$dbPath;Cache=Shared"
$webPublish = Join-Path $InstallDir "web"
$apiPublish = Join-Path $InstallDir "api"

Write-Host ""
Write-Host "Plan:" -ForegroundColor White
Write-Host ("  Source        : {0}" -f $SourceDir)
Write-Host ("  Programs      : {0}" -f $InstallDir)
Write-Host ("  Database/data : {0}" -f $DataDir)
Write-Host ("  Web (browser) : http://{0}:{1}" -f $BindAddress, $WebPort)
Write-Host ("  API (desktop) : http://{0}:{1}" -f $BindAddress, $ApiPort)
Write-Host ("  Local AI      : {0}" -f $(if ($InstallAi) { "yes ($AiModel via Ollama)" } else { "no" }))
Write-Host ("  Auto-start    : {0}" -f $AutoStart)

if ($Interactive -and -not (AskYesNo "`nProceed with the install above?" $true)) {
    Write-Host "Cancelled - nothing was changed." -ForegroundColor Yellow
    exit 0
}

# 1 --------------------------------------------------------------------------- prereqs
Write-Step "Checking prerequisites (.NET 10 SDK and Git)"
if ($SkipPrereqs) {
    Write-Info "Skipping prerequisite install (-SkipPrereqs)."
} else {
    if ((Get-DotnetSdkMajor) -lt 10) {
        Write-Warn2 ".NET 10 SDK not found."
        Install-WithWinget "Microsoft.DotNet.SDK.10" ".NET 10 SDK"
    }
    if (-not (Test-Command 'git')) {
        Write-Warn2 "Git not found."
        Install-WithWinget "Git.Git" "Git"
    }
}
if ((Get-DotnetSdkMajor) -lt 10) { Fail ".NET 10 SDK still not detected. Install it from https://dotnet.microsoft.com/download and re-run." }
if (-not (Test-Command 'git')) { Fail "Git still not detected. Install it from https://git-scm.com and re-run." }
Write-Ok ".NET SDK major version $(Get-DotnetSdkMajor) and Git are present."

# 2 --------------------------------------------------------------------------- source
Write-Step "Getting the OpenTrack source code"
# If this script sits inside a clone that has the source, use that clone.
$scriptRepo = Split-Path -Parent $PSScriptRoot   # scripts\ -> repo root
$repoRoot = $null
if ($scriptRepo -and (Test-Path (Join-Path $scriptRepo "src\OpenTrack.Web\OpenTrack.Web.csproj"))) {
    $repoRoot = $scriptRepo
    Write-Info "Using the repository this script is in: $repoRoot"
    Push-Location $repoRoot
    try { & git pull --ff-only 2>$null | Out-Null; if ($LASTEXITCODE -eq 0) { Write-Info "Pulled latest changes." } } catch { }
    Pop-Location
}
elseif (Test-Path (Join-Path $SourceDir "src\OpenTrack.Web\OpenTrack.Web.csproj")) {
    $repoRoot = $SourceDir
    Write-Info "Updating existing clone at $SourceDir"
    Push-Location $repoRoot
    & git fetch --prune; & git checkout $Branch; & git pull --ff-only
    Pop-Location
}
else {
    Write-Info "Cloning $RepoUrl (branch $Branch) into $SourceDir"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $SourceDir) | Out-Null
    & git clone --branch $Branch --depth 1 $RepoUrl $SourceDir
    if ($LASTEXITCODE -ne 0) { Fail "git clone failed. Check the repo URL / branch / network." }
    $repoRoot = $SourceDir
}
$webProj = Join-Path $repoRoot "src\OpenTrack.Web\OpenTrack.Web.csproj"
$apiProj = Join-Path $repoRoot "src\OpenTrack.API\OpenTrack.API.csproj"
if (-not (Test-Path $webProj)) { Fail "Can't find the Web project at $webProj" }
Write-Ok "Source ready at $repoRoot"

# 3 --------------------------------------------------------------------------- build
Write-Step "Building the server programs (this can take a few minutes the first time)"
New-Item -ItemType Directory -Force -Path $webPublish, $apiPublish | Out-Null
Write-Info "Publishing the Web app ..."
& dotnet publish $webProj -c Release -o $webPublish --nologo
if ($LASTEXITCODE -ne 0) { Fail "Building the Web app failed." }
Write-Info "Publishing the API ..."
& dotnet publish $apiProj -c Release -o $apiPublish --nologo
if ($LASTEXITCODE -ne 0) { Fail "Building the API failed." }
Write-Ok "Both programs built."

# 4 --------------------------------------------------------------------------- data + config
Write-Step "Setting up the data folder and configuration"
New-Item -ItemType Directory -Force -Path $DataDir | Out-Null

# Web host config: shared DB + options (bootstrap admin, AI, HTTPS). Layered on top of appsettings.json.
$webOpen = @{ RequireHttps = [bool]$RequireHttps }
if ($AdminEmail -and $AdminPassword) {
    $webOpen["BootstrapAdmin"] = @{ Email = $AdminEmail; Password = $AdminPassword }
    Write-Info "First administrator will be set to $AdminEmail at startup."
}
if ($InstallAi) {
    $webOpen["Ai"] = @{
        Enabled  = $true
        Provider = "openai"                      # OpenAI-compatible = works with local Ollama
        BaseUrl  = "http://localhost:11434/v1"
        Model    = $AiModel
    }
}
$webConfig = @{ ConnectionStrings = @{ Default = $connString }; OpenTrack = $webOpen }
Write-JsonFile $webConfig (Join-Path $webPublish "appsettings.Production.json")

# API host config: just the shared DB + HTTPS flag.
$apiConfig = @{ ConnectionStrings = @{ Default = $connString }; OpenTrack = @{ RequireHttps = [bool]$RequireHttps } }
Write-JsonFile $apiConfig (Join-Path $apiPublish "appsettings.Production.json")
Write-Ok "Both programs point at one database: $dbPath"

# Launcher .cmd files: set the environment and start each program on its port + address.
$webUrl = "http://{0}:{1}" -f $BindAddress, $WebPort
$apiUrl = "http://{0}:{1}" -f $BindAddress, $ApiPort
$webCmd = Join-Path $webPublish "run-web.cmd"
$apiCmd = Join-Path $apiPublish "run-api.cmd"
@(
    "@echo off",
    "set ASPNETCORE_ENVIRONMENT=Production",
    "cd /d `"$webPublish`"",
    "dotnet `"$webPublish\OpenTrack.Web.dll`" --urls `"$webUrl`""
) | Set-Content -Path $webCmd -Encoding ASCII
@(
    "@echo off",
    "set ASPNETCORE_ENVIRONMENT=Production",
    "cd /d `"$apiPublish`"",
    "dotnet `"$apiPublish\OpenTrack.API.dll`" --urls `"$apiUrl`""
) | Set-Content -Path $apiCmd -Encoding ASCII
Write-Ok "Launchers written."

# 5 --------------------------------------------------------------------------- firewall
Write-Step "Opening the Windows Firewall"
if ($SkipFirewall) {
    Write-Info "Skipping firewall rules (-SkipFirewall)."
} else {
    foreach ($rule in @(@{ n = "OpenTrack Web ($WebPort)"; p = $WebPort }, @{ n = "OpenTrack API ($ApiPort)"; p = $ApiPort })) {
        Get-NetFirewallRule -DisplayName $rule.n -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
        New-NetFirewallRule -DisplayName $rule.n -Direction Inbound -Action Allow -Protocol TCP -LocalPort $rule.p -Profile Any | Out-Null
        Write-Info "Allowed inbound TCP $($rule.p)."
    }
    Write-Ok "Firewall rules in place."
}

# 6 --------------------------------------------------------------------------- optional AI
if ($InstallAi) {
    Write-Step "Installing the local AI (Ollama) and downloading the model"
    if (-not (Test-Command 'ollama')) { Install-WithWinget "Ollama.Ollama" "Ollama" }
    if (Test-Command 'ollama') {
        Write-Info "Downloading model '$AiModel' (one-time, can be large) ..."
        & ollama pull $AiModel
        if ($LASTEXITCODE -ne 0) { Write-Warn2 "Model download didn't complete; you can run 'ollama pull $AiModel' later." }
        else { Write-Ok "Local AI ready ($AiModel). OpenTrack's AI features are turned on and point at it." }
    } else {
        Write-Warn2 "Ollama not detected after install; AI settings were written but you'll need to install Ollama and pull '$AiModel'."
    }
}

# 7 --------------------------------------------------------------------------- auto-start
Write-Step "Setting up auto-start"
function Register-BootTask([string]$name, [string]$cmdPath) {
    Get-ScheduledTask -TaskName $name -ErrorAction SilentlyContinue | Unregister-ScheduledTask -Confirm:$false -ErrorAction SilentlyContinue
    $action = New-ScheduledTaskAction -Execute "cmd.exe" -Argument ('/c "{0}"' -f $cmdPath)
    $trigger = New-ScheduledTaskTrigger -AtStartup
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -LogonType ServiceAccount -RunLevel Highest
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)
    Register-ScheduledTask -TaskName $name -Action $action -Trigger $trigger -Principal $principal -Settings $settings | Out-Null
}
if ($AutoStart -eq 'Task') {
    Register-BootTask "OpenTrack Web" $webCmd
    Register-BootTask "OpenTrack API" $apiCmd
    Write-Ok "Registered 'OpenTrack Web' and 'OpenTrack API' to start at boot (as SYSTEM)."
    Write-Info "Starting them now ..."
    Start-ScheduledTask -TaskName "OpenTrack Web"
    Start-ScheduledTask -TaskName "OpenTrack API"
    Start-Sleep -Seconds 6
} else {
    Write-Info "Auto-start skipped (-AutoStart None). Start the server yourself with:"
    Write-Info "  $webCmd"
    Write-Info "  $apiCmd"
}

# 8 --------------------------------------------------------------------------- done
$ip = Get-LanIPv4
Write-Host ""
Write-Host "==================================================================" -ForegroundColor Green
Write-Host "  OpenTrack is installed." -ForegroundColor Green
Write-Host "==================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "Open it in a browser on this or any computer on your network:" -ForegroundColor White
Write-Host ("   http://{0}:{1}" -f $ip, $WebPort) -ForegroundColor Cyan
Write-Host ""
Write-Host "Point the Windows/Mac desktop app at this server address:" -ForegroundColor White
Write-Host ("   http://{0}:{1}" -f $ip, $ApiPort) -ForegroundColor Cyan
Write-Host ""
if ($AdminEmail -and $AdminPassword) {
    Write-Host "First administrator: $AdminEmail (set at startup - just log in)." -ForegroundColor White
} else {
    Write-Host "First administrator: open the web address above and REGISTER - the first" -ForegroundColor White
    Write-Host "account created becomes the Administrator. Do this soon, before anyone else." -ForegroundColor White
}
Write-Host ""
Write-Host "Data & database live in: $DataDir  (back this folder up)." -ForegroundColor White
Write-Host "Re-run this script anytime to update OpenTrack to the latest code." -ForegroundColor DarkGray
Write-Host ""
