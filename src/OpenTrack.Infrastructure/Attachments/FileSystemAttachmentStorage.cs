// OpenTrack — open-source issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version. This program is distributed WITHOUT ANY WARRANTY; without even
// the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License <https://www.gnu.org/licenses/> for
// more details.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace OpenTrack.Infrastructure.Attachments;

/// <summary>
/// Stores attachments as files on disk under a configured root directory that is OUTSIDE any web
/// root. Each file's name is a fresh 32-hex-character key (a GUID with no dashes) with no extension,
/// so a client can never influence the path (no traversal) and the file can never be executed by the
/// web server. The client's original filename lives only in the database as display metadata.
///
/// Config:
///   OpenTrack:Attachments:Path      — storage root (default: %LocalAppData%/OpenTrack/attachments)
///   OpenTrack:Attachments:MaxBytes  — max upload size   (default: 10 MiB)
/// </summary>
public sealed partial class FileSystemAttachmentStorage : IAttachmentStorage
{
    private readonly string _root;
    public long MaxBytes { get; }

    public FileSystemAttachmentStorage(IConfiguration configuration)
    {
        _root = configuration["OpenTrack:Attachments:Path"] is { Length: > 0 } configured
            ? configured
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OpenTrack", "attachments");
        Directory.CreateDirectory(_root);

        MaxBytes = long.TryParse(configuration["OpenTrack:Attachments:MaxBytes"], out var m) && m > 0
            ? m
            : 10L * 1024 * 1024;
    }

    [GeneratedRegex("^[0-9a-f]{32}$")]
    private static partial Regex StorageKeyPattern();

    public async Task<string> SaveAsync(Stream content, CancellationToken ct = default)
    {
        var key = Guid.NewGuid().ToString("N");
        var path = Path.Combine(_root, key);

        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        var buffer = new byte[81920];
        long total = 0;
        int read;
        while ((read = await content.ReadAsync(buffer, ct)) > 0)
        {
            total += read;
            if (total > MaxBytes)
            {
                // Abort and clean up a partial file before anyone can reference it.
                await file.DisposeAsync();
                TryDelete(path);
                throw new AttachmentTooLargeException(MaxBytes);
            }
            await file.WriteAsync(buffer.AsMemory(0, read), ct);
        }
        return key;
    }

    public Task<Stream?> OpenAsync(string storageKey, CancellationToken ct = default)
    {
        // Only ever open a file whose name is exactly a generated key — never anything derived from
        // client input. This is the guard against path traversal on the read path.
        if (!StorageKeyPattern().IsMatch(storageKey))
            return Task.FromResult<Stream?>(null);

        var path = Path.Combine(_root, storageKey);
        if (!File.Exists(path))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        if (StorageKeyPattern().IsMatch(storageKey))
            TryDelete(Path.Combine(_root, storageKey));
        return Task.CompletedTask;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (IOException) { /* best-effort cleanup */ }
        catch (UnauthorizedAccessException) { /* best-effort cleanup */ }
    }
}
