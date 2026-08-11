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

namespace OpenTrack.Infrastructure.Attachments;

/// <summary>Raised when an upload exceeds the configured maximum size.</summary>
public sealed class AttachmentTooLargeException(long maxBytes)
    : Exception($"Attachment exceeds the maximum allowed size of {maxBytes} bytes.")
{
    public long MaxBytes { get; } = maxBytes;
}

/// <summary>
/// Physical storage for issue attachments, kept OUT of any web root. The stored key is an opaque,
/// server-generated identifier — never a client-supplied path or filename — so a malicious upload
/// name (e.g. "../../appsettings.json") can never influence where a file is written or read.
/// </summary>
public interface IAttachmentStorage
{
    /// <summary>
    /// Streams <paramref name="content"/> to storage, enforcing the configured size limit, and
    /// returns the opaque storage key to persist in the database. Throws
    /// <see cref="AttachmentTooLargeException"/> if the limit is exceeded.
    /// </summary>
    Task<string> SaveAsync(Stream content, CancellationToken ct = default);

    /// <summary>Opens the stored file for reading, or returns null if the key is invalid or missing.</summary>
    Task<Stream?> OpenAsync(string storageKey, CancellationToken ct = default);

    /// <summary>Deletes the stored file (no-op if already gone).</summary>
    Task DeleteAsync(string storageKey, CancellationToken ct = default);

    /// <summary>The configured maximum upload size, in bytes.</summary>
    long MaxBytes { get; }
}
