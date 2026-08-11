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

using OpenTrack.UI.Services;

namespace OpenTrack.Web.Services;

/// <summary>
/// Web-host attachment transfer: uploads and downloads happen through plain HTML forms / anchor links
/// to the cookie-authenticated web endpoints (see AttachmentWebEndpoints), because the web app is
/// static SSR. The interactive Upload/DownloadAndOpen methods are therefore never called here.
/// </summary>
public sealed class WebAttachmentTransfer : IAttachmentTransfer
{
    public ClientHostKind Host => ClientHostKind.Web;

    public string DownloadUrl(int attachmentId) => $"/attachments/{attachmentId}/download";

    public Task UploadAsync(int issueId, string fileName, string contentType, Stream content, CancellationToken ct = default) =>
        throw new NotSupportedException("The web host uploads via a multipart form, not this method.");

    public Task DownloadAndOpenAsync(int attachmentId, string fileName, CancellationToken ct = default) =>
        throw new NotSupportedException("The web host downloads via an anchor link, not this method.");
}
