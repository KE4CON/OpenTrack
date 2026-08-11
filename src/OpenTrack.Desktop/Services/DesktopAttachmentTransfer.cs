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

using System.Net.Http.Headers;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using OpenTrack.UI.Services;

namespace OpenTrack.Desktop.Services;

/// <summary>
/// Desktop-host attachment transfer: streams files to/from the API over the authenticated HttpClient
/// (the bearer token is attached by AuthTokenHandler) and opens a downloaded file with the OS default
/// handler. Used only in the interactive BlazorWebView host.
/// </summary>
public sealed class DesktopAttachmentTransfer(HttpClient http) : IAttachmentTransfer
{
    public ClientHostKind Host => ClientHostKind.Desktop;

    // Desktop downloads go through DownloadAndOpenAsync (needs the bearer token), not a bare link.
    public string DownloadUrl(int attachmentId) => string.Empty;

    public async Task UploadAsync(int issueId, string fileName, string contentType, Stream content, CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(content);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        form.Add(fileContent, "file", fileName);

        var resp = await http.PostAsync($"/api/issues/{issueId}/attachments", form, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DownloadAndOpenAsync(int attachmentId, string fileName, CancellationToken ct = default)
    {
        var bytes = await http.GetByteArrayAsync($"/api/attachments/{attachmentId}/download", ct);

        // Save under the app cache using only the file-name portion (never a client path), then hand
        // the OS the local file to open with its default handler.
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName)) safeName = $"attachment-{attachmentId}";
        var path = Path.Combine(FileSystem.CacheDirectory, $"{attachmentId}-{safeName}");
        await File.WriteAllBytesAsync(path, bytes, ct);

        await Launcher.Default.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
    }
}
