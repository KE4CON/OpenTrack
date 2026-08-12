#!/bin/bash
# =============================================================================
# OpenTrack - one-command server setup for macOS (Apple Silicon or Intel).
#
# The macOS twin of Install-OpenTrackServer.ps1. It will:
#   1. Make sure the tools it needs are present (the .NET 10 SDK and Git),
#      installing them with Homebrew (or Microsoft's installer) if missing.
#   2. Get the OpenTrack source (use the copy this script lives in, or clone it).
#   3. Build ("publish") the two server programs: the Web app (browsers) and the
#      API (the Windows/Mac desktop app).
#   4. Point both at ONE shared database in a fixed data folder.
#   5. Register both to start automatically at boot with launchd, and start them now.
#   6. Optionally install a free, private local AI (Ollama) - which is especially
#      snappy on Apple Silicon thanks to its unified memory.
#   7. Print the finish-up steps (the address to open, how to make the first admin).
#
# Re-run anytime to UPDATE: it pulls the latest code, rebuilds, and restarts.
#
# Usage:
#   ./install-opentrack-server.sh                 # sensible defaults
#   ./install-opentrack-server.sh --config ./opentrack-server.conf
#   ./install-opentrack-server.sh --install-ai
#
# It uses `sudo` for the always-on pieces (writing the launchd daemons and the
# system data folder); you'll be asked for your password once.
#
# OpenTrack - AGPL v3 - KE4CON
# =============================================================================
set -euo pipefail

# --------------------------------------------------------------------- defaults
# Names match the PowerShell script and the shared .conf file, so one settings
# file works on both Windows and Mac.
RepoUrl="https://github.com/KE4CON/OpenTrack.git"
Branch="main"
SourceDir="/usr/local/opentrack/src"
InstallDir="/usr/local/opentrack/app"
DataDir="/usr/local/opentrack/data"
BindAddress="0.0.0.0"
WebPort="5035"
ApiPort="5003"
InstallAi="false"
AiModel="llama3.1"
AdminEmail=""
AdminPassword=""
RequireHttps="false"
AutoStart="launchd"     # or "none"
SkipPrereqs="false"
ConfigFile=""

STEP=0
step()  { STEP=$((STEP+1)); printf '\n\033[36m[%d] %s\033[0m\n' "$STEP" "$1"; }
ok()    { printf '    \033[32mOK  %s\033[0m\n' "$1"; }
info()  { printf '    -   %s\n' "$1"; }
warn()  { printf '    \033[33m!   %s\033[0m\n' "$1"; }
fail()  { printf '\n\033[31mERROR: %s\033[0m\n' "$1" >&2; exit 1; }

# --------------------------------------------------------------------- arg parse
apply_config_file() {
    local path="$1"
    [ -f "$path" ] || fail "Config file not found: $path"
    info "Reading settings from $path"
    while IFS= read -r line || [ -n "$line" ]; do
        line="${line#"${line%%[![:space:]]*}"}"   # ltrim
        case "$line" in ''|\#*) continue ;; esac
        case "$line" in *=*) : ;; *) continue ;; esac
        local k v
        k="$(printf '%s' "${line%%=*}" | xargs)"
        v="${line#*=}"
        v="$(printf '%s' "$v" | sed -e 's/^[[:space:]]*//' -e 's/[[:space:]]*$//' -e 's/^"//' -e 's/"$//')"
        # Only set values we recognize (keeps the same keys as the PowerShell script).
        case "$k" in
            RepoUrl|Branch|SourceDir|InstallDir|DataDir|BindAddress|WebPort|ApiPort|\
            InstallAi|AiModel|AdminEmail|AdminPassword|RequireHttps|SkipPrereqs)
                printf -v "$k" '%s' "$v" ;;
            *) : ;;   # ignore unknown keys (e.g. Windows-only ones)
        esac
    done < "$path"
}

# First pass: pick up --config so its values load before explicit flags override.
args=("$@")
for ((i=0; i<${#args[@]}; i++)); do
    if [ "${args[$i]}" = "--config" ]; then ConfigFile="${args[$((i+1))]:-}"; fi
done
[ -n "$ConfigFile" ] && apply_config_file "$ConfigFile"

while [ $# -gt 0 ]; do
    case "$1" in
        --config)         shift 2;;   # already handled
        --repo-url)       RepoUrl="$2"; shift 2;;
        --branch)         Branch="$2"; shift 2;;
        --source-dir)     SourceDir="$2"; shift 2;;
        --install-dir)    InstallDir="$2"; shift 2;;
        --data-dir)       DataDir="$2"; shift 2;;
        --bind)           BindAddress="$2"; shift 2;;
        --web-port)       WebPort="$2"; shift 2;;
        --api-port)       ApiPort="$2"; shift 2;;
        --install-ai)     InstallAi="true"; shift;;
        --ai-model)       AiModel="$2"; shift 2;;
        --admin-email)    AdminEmail="$2"; shift 2;;
        --admin-password) AdminPassword="$2"; shift 2;;
        --require-https)  RequireHttps="true"; shift;;
        --no-autostart)   AutoStart="none"; shift;;
        --skip-prereqs)   SkipPrereqs="true"; shift;;
        -h|--help)
            grep '^#' "$0" | sed 's/^# \{0,1\}//'; exit 0;;
        *) fail "Unknown option: $1 (use --help)";;
    esac
done

[ "$(uname -s)" = "Darwin" ] || fail "This script is for macOS. On Windows use Install-OpenTrackServer.ps1."

# sudo passthrough (root already? then no sudo needed)
if [ "$(id -u)" -eq 0 ]; then SUDO=""; else SUDO="sudo"; fi

DbPath="$DataDir/opentrack.db"
ConnString="Data Source=$DbPath;Cache=Shared"
WebOut="$InstallDir/web"
ApiOut="$InstallDir/api"

echo
echo "=================================================================="
echo "  OpenTrack - Server Setup (macOS)"
echo "  Self-hosted issue tracker  -  AGPL v3  -  KE4CON"
echo "=================================================================="
echo "Plan:"
echo "  Source        : $SourceDir"
echo "  Programs      : $InstallDir"
echo "  Database/data : $DataDir"
echo "  Web (browser) : http://$BindAddress:$WebPort"
echo "  API (desktop) : http://$BindAddress:$ApiPort"
echo "  Local AI      : $([ "$InstallAi" = "true" ] && echo "yes ($AiModel via Ollama)" || echo "no")"
echo "  Auto-start    : $AutoStart"

# 1 ------------------------------------------------------------------- prereqs
step "Checking prerequisites (.NET 10 SDK and Git)"
# Make common dotnet locations visible in this shell first.
export PATH="$HOME/.dotnet:/usr/local/share/dotnet:/opt/homebrew/bin:/usr/local/bin:$PATH"

dotnet_major() {
    command -v dotnet >/dev/null 2>&1 || { echo 0; return; }
    local m
    m="$(dotnet --list-sdks 2>/dev/null | sed -n 's/^\([0-9][0-9]*\)\..*/\1/p' | sort -rn | head -1)"
    echo "${m:-0}"
}

if [ "$SkipPrereqs" = "true" ]; then
    info "Skipping prerequisite install (--skip-prereqs)."
else
    if ! command -v git >/dev/null 2>&1; then
        warn "Git not found."
        if command -v brew >/dev/null 2>&1; then brew install git; else
            info "Triggering the Xcode Command Line Tools installer (includes Git)..."
            xcode-select --install || true
            fail "Install the Command Line Tools when prompted, then re-run this script."
        fi
    fi
    if [ "$(dotnet_major)" -lt 10 ]; then
        warn ".NET 10 SDK not found."
        if command -v brew >/dev/null 2>&1; then
            info "Installing the .NET SDK with Homebrew..."
            brew install --cask dotnet-sdk || brew install dotnet
        else
            info "Homebrew not found; using Microsoft's install script into ~/.dotnet ..."
            curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
            bash /tmp/dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
            export PATH="$HOME/.dotnet:$PATH"
        fi
    fi
fi
command -v git >/dev/null 2>&1 || fail "Git still not found. Install it and re-run."
[ "$(dotnet_major)" -ge 10 ] || fail ".NET 10 SDK still not detected. Install it (https://dotnet.microsoft.com/download) and re-run."
DOTNET="$(command -v dotnet)"
ok ".NET SDK major $(dotnet_major) and Git are present ($DOTNET)."

# 2 ------------------------------------------------------------------- source
step "Getting the OpenTrack source code"
script_dir="$(cd "$(dirname "$0")" && pwd)"
repo_root=""
if [ -f "$script_dir/../src/OpenTrack.Web/OpenTrack.Web.csproj" ]; then
    repo_root="$(cd "$script_dir/.." && pwd)"
    info "Using the repository this script is in: $repo_root"
    ( cd "$repo_root" && git pull --ff-only >/dev/null 2>&1 && info "Pulled latest changes." ) || true
elif [ -f "$SourceDir/src/OpenTrack.Web/OpenTrack.Web.csproj" ]; then
    repo_root="$SourceDir"
    info "Updating existing clone at $SourceDir"
    ( cd "$repo_root" && git fetch --prune && git checkout "$Branch" && git pull --ff-only )
else
    info "Cloning $RepoUrl (branch $Branch) into $SourceDir"
    $SUDO mkdir -p "$(dirname "$SourceDir")"
    $SUDO chown "$(id -un)" "$(dirname "$SourceDir")" 2>/dev/null || true
    git clone --branch "$Branch" --depth 1 "$RepoUrl" "$SourceDir"
    repo_root="$SourceDir"
fi
WebProj="$repo_root/src/OpenTrack.Web/OpenTrack.Web.csproj"
ApiProj="$repo_root/src/OpenTrack.API/OpenTrack.API.csproj"
[ -f "$WebProj" ] || fail "Can't find the Web project at $WebProj"
ok "Source ready at $repo_root"

# 3 ------------------------------------------------------------------- build
step "Building the server programs (a few minutes the first time)"
$SUDO mkdir -p "$WebOut" "$ApiOut"
$SUDO chown -R "$(id -un)" "$InstallDir"     # so we can publish into it without sudo
info "Publishing the Web app..."
"$DOTNET" publish "$WebProj" -c Release -o "$WebOut" --nologo
info "Publishing the API..."
"$DOTNET" publish "$ApiProj" -c Release -o "$ApiOut" --nologo
ok "Both programs built."

# 4 ------------------------------------------------------------------- data + config
step "Setting up the data folder and configuration"
$SUDO mkdir -p "$DataDir"

require_https_json="false"; [ "$RequireHttps" = "true" ] && require_https_json="true"

optrack_members() {
    local m=()
    m+=("\"RequireHttps\": $require_https_json")
    if [ -n "$AdminEmail" ] && [ -n "$AdminPassword" ]; then
        m+=("\"BootstrapAdmin\": { \"Email\": \"$AdminEmail\", \"Password\": \"$AdminPassword\" }")
        info "First administrator will be set to $AdminEmail at startup."
    fi
    if [ "$InstallAi" = "true" ]; then
        m+=("\"Ai\": { \"Enabled\": true, \"Provider\": \"openai\", \"BaseUrl\": \"http://localhost:11434/v1\", \"Model\": \"$AiModel\" }")
    fi
    local IFS=,; echo "${m[*]}"
}

cat > "$WebOut/appsettings.Production.json" <<JSON
{
  "ConnectionStrings": { "Default": "$ConnString" },
  "OpenTrack": { $(optrack_members) }
}
JSON
cat > "$ApiOut/appsettings.Production.json" <<JSON
{
  "ConnectionStrings": { "Default": "$ConnString" },
  "OpenTrack": { "RequireHttps": $require_https_json }
}
JSON
ok "Both programs point at one database: $DbPath"

# 5 ------------------------------------------------------------------- optional AI
if [ "$InstallAi" = "true" ]; then
    step "Installing the local AI (Ollama) and downloading the model"
    if ! command -v ollama >/dev/null 2>&1; then
        if command -v brew >/dev/null 2>&1; then brew install ollama; else
            warn "Homebrew not found; install Ollama from https://ollama.com, then run 'ollama pull $AiModel'."
        fi
    fi
    if command -v ollama >/dev/null 2>&1; then
        command -v brew >/dev/null 2>&1 && brew services start ollama >/dev/null 2>&1 || true
        info "Downloading model '$AiModel' (one-time, can be large)..."
        ollama pull "$AiModel" || warn "Model download didn't finish; run 'ollama pull $AiModel' later."
        ok "Local AI ready. OpenTrack's AI features are on and point at it."
    fi
fi

# 6 ------------------------------------------------------------------- auto-start (launchd)
step "Setting up auto-start"
write_daemon() {   # $1 label  $2 dll  $3 url  $4 workdir
    local label="$1" dll="$2" url="$3" wd="$4"
    local plist="/Library/LaunchDaemons/$label.plist"
    $SUDO bash -c "cat > '$plist'" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>Label</key><string>$label</string>
  <key>ProgramArguments</key>
  <array>
    <string>$DOTNET</string>
    <string>$dll</string>
    <string>--urls</string>
    <string>$url</string>
  </array>
  <key>WorkingDirectory</key><string>$wd</string>
  <key>EnvironmentVariables</key>
  <dict>
    <key>ASPNETCORE_ENVIRONMENT</key><string>Production</string>
    <key>DOTNET_CLI_TELEMETRY_OPTOUT</key><string>1</string>
  </dict>
  <key>RunAtLoad</key><true/>
  <key>KeepAlive</key><true/>
  <key>StandardOutPath</key><string>$DataDir/${label##*.}.log</string>
  <key>StandardErrorPath</key><string>$DataDir/${label##*.}.err.log</string>
</dict>
</plist>
PLIST
    $SUDO chown root:wheel "$plist"
    $SUDO launchctl bootout system "$plist" >/dev/null 2>&1 || true
    $SUDO launchctl bootstrap system "$plist"
}

if [ "$AutoStart" = "launchd" ]; then
    write_daemon "com.ke4con.opentrack.web" "$WebOut/OpenTrack.Web.dll" "http://$BindAddress:$WebPort" "$WebOut"
    write_daemon "com.ke4con.opentrack.api" "$ApiOut/OpenTrack.API.dll" "http://$BindAddress:$ApiPort" "$ApiOut"
    ok "Registered launchd daemons (start at boot, running now)."
    sleep 5
else
    info "Auto-start skipped (--no-autostart). Start by hand with, e.g.:"
    info "  ASPNETCORE_ENVIRONMENT=Production '$DOTNET' '$WebOut/OpenTrack.Web.dll' --urls http://$BindAddress:$WebPort"
    info "  ASPNETCORE_ENVIRONMENT=Production '$DOTNET' '$ApiOut/OpenTrack.API.dll' --urls http://$BindAddress:$ApiPort"
fi

# 7 ------------------------------------------------------------------- firewall note
step "Firewall"
info "macOS's firewall is per-application and off by default. On a trusted network"
info "there's usually nothing to open. If you have the firewall on, allow incoming"
info "connections for the 'dotnet' process (System Settings > Network > Firewall)."

# 8 ------------------------------------------------------------------- done
ip="$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null || echo "this-mac's-IP")"
echo
echo "=================================================================="
echo "  OpenTrack is installed."
echo "=================================================================="
echo
echo "Open it in a browser on this or any computer on your network:"
printf '   \033[36mhttp://%s:%s\033[0m\n' "$ip" "$WebPort"
echo
echo "Point the Windows/Mac desktop app at this server address:"
printf '   \033[36mhttp://%s:%s\033[0m\n' "$ip" "$ApiPort"
echo
if [ -n "$AdminEmail" ] && [ -n "$AdminPassword" ]; then
    echo "First administrator: $AdminEmail (set at startup - just log in)."
else
    echo "First administrator: open the web address above and REGISTER - the first"
    echo "account created becomes the Administrator. Do this soon, before anyone else."
fi
echo
echo "Data & database live in: $DataDir  (back this folder up)."
echo "Re-run this script anytime to update OpenTrack to the latest code."
echo
