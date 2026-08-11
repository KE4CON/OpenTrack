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

namespace OpenTrack.UI.Services;

public enum ClientHostKind { Web, Desktop }

/// <summary>
/// Host-specific file transfer for attachments. Uploading/downloading a file can't go through the
/// JSON data seam and can't be done the same way in both hosts: the web app is static-SSR (no
/// interactivity → it uses plain HTML forms + anchor links to its own endpoints), while the desktop
/// app is an interactive BlazorWebView with no local server (→ it streams over HttpClient to the API
/// and opens the saved file with the OS). The shared issue page branches on <see cref="Host"/> to
/// render the right controls; only the matching implementation's methods are ever invoked.
/// </summary>
public interface IAttachmentTransfer
{
    ClientHostKind Host { get; }

    /// <summary>The URL an anchor/link should point at to download an attachment (web host only).</summary>
    string DownloadUrl(int attachmentId);

    /// <summary>Uploads a selected file to the given issue (desktop host).</summary>
    Task UploadAsync(int issueId, string fileName, string contentType, Stream content, CancellationToken ct = default);

    /// <summary>Downloads an attachment and opens it with the OS default handler (desktop host).</summary>
    Task DownloadAndOpenAsync(int attachmentId, string fileName, CancellationToken ct = default);
}
