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

using System.Text;
using Microsoft.Extensions.Configuration;
using OpenTrack.Infrastructure.Attachments;

namespace OpenTrack.API.Tests;

/// <summary>
/// Security tests for attachment storage: opaque server-generated keys (no client filename ever
/// touches the path → no traversal), an enforced size limit, and round-trip integrity. These guard
/// the controls the audit's security reviewer flagged before attachments were implemented.
/// </summary>
public sealed class AttachmentStorageTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "opentrack-attach-" + Guid.NewGuid().ToString("N"));

    private FileSystemAttachmentStorage NewStorage(long maxBytes = 1024)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTrack:Attachments:Path"] = _dir,
                ["OpenTrack:Attachments:MaxBytes"] = maxBytes.ToString(),
            })
            .Build();
        return new FileSystemAttachmentStorage(config);
    }

    [Fact]
    public async Task Save_ReturnsOpaqueHexKey_AndRoundTrips()
    {
        var storage = NewStorage();
        var bytes = Encoding.UTF8.GetBytes("hello attachment");

        string key;
        using (var ms = new MemoryStream(bytes))
            key = await storage.SaveAsync(ms);

        Assert.Matches("^[0-9a-f]{32}$", key); // GUID-N: no extension, no client input

        await using var read = await storage.OpenAsync(key);
        Assert.NotNull(read);
        using var reader = new StreamReader(read!);
        Assert.Equal("hello attachment", await reader.ReadToEndAsync());
    }

    [Theory]
    [InlineData("../secret")]
    [InlineData("..\\secret")]
    [InlineData("/etc/passwd")]
    [InlineData("abc")]                       // not 32 hex
    [InlineData("00000000000000000000000000000000/../x")]
    public async Task Open_RejectsNonKeyPaths(string malicious)
    {
        var storage = NewStorage();
        Assert.Null(await storage.OpenAsync(malicious)); // never resolves outside the key namespace
    }

    [Fact]
    public async Task Save_EnforcesSizeLimit()
    {
        var storage = NewStorage(maxBytes: 8);
        using var ms = new MemoryStream(new byte[9]); // one over the limit
        await Assert.ThrowsAsync<AttachmentTooLargeException>(() => storage.SaveAsync(ms));

        // No partial file must survive an over-limit upload.
        Assert.Empty(Directory.GetFiles(_dir));
    }

    [Fact]
    public async Task Delete_RemovesFile()
    {
        var storage = NewStorage();
        string key;
        using (var ms = new MemoryStream(new byte[4]))
            key = await storage.SaveAsync(ms);

        await storage.DeleteAsync(key);
        Assert.Null(await storage.OpenAsync(key));
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true); }
        catch (IOException) { }
    }
}
