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

using OpenTrack.Core.Text;

namespace OpenTrack.Core.Tests;

/// <summary>The safe Markdown renderer. The security-critical property is that no user text ever
/// becomes live markup — everything is HTML-encoded first — while the intended formatting still works.</summary>
public class SafeMarkdownTests
{
    [Fact]
    public void RawHtml_IsEncoded_NotEmitted()
    {
        var html = SafeMarkdown.ToHtml("<script>alert('x')</script> and <b>hi</b>");
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("<b>hi</b>", html);   // the user's <b> is inert, encoded
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void ImgOnerror_XssPayload_IsNeutralized()
    {
        var html = SafeMarkdown.ToHtml("<img src=x onerror=alert(1)>");
        Assert.DoesNotContain("<img", html);
        Assert.Contains("&lt;img", html);
    }

    [Fact]
    public void JavascriptLink_IsNotLinkified()
    {
        var html = SafeMarkdown.ToHtml("[click](javascript:alert(1))");
        // Unsafe scheme → never becomes an anchor; it stays as inert, visible text (harmless).
        Assert.DoesNotContain("<a ", html);
        Assert.DoesNotContain("href=", html);
    }

    [Fact]
    public void HttpLink_IsLinkified_WithSafeRel()
    {
        var html = SafeMarkdown.ToHtml("see [the docs](https://example.com/a)");
        Assert.Contains("<a href=\"https://example.com/a\"", html);
        Assert.Contains("rel=\"noopener noreferrer\"", html);
    }

    [Fact]
    public void FencedCodeBlock_PreservesContent_Encoded()
    {
        var html = SafeMarkdown.ToHtml("```\nvar x = a < b && c > d;\n```");
        Assert.Contains("<pre><code>", html);
        Assert.Contains("a &lt; b &amp;&amp; c &gt; d", html);
        Assert.DoesNotContain("<strong>", html);   // markers inside code are literal
    }

    [Fact]
    public void Emphasis_InlineCode_AndLists_Render()
    {
        Assert.Contains("<strong>bold</strong>", SafeMarkdown.ToHtml("this is **bold**"));
        Assert.Contains("<em>it</em>", SafeMarkdown.ToHtml("this is *it*"));
        Assert.Contains("<code>Foo&lt;T&gt;</code>", SafeMarkdown.ToHtml("call `Foo<T>` now"));

        var list = SafeMarkdown.ToHtml("- one\n- two");
        Assert.Contains("<ul><li>one</li><li>two</li></ul>", list);
    }


    [Fact]
    public void EmphasisMarkersInsideInlineCode_AreNotInterpreted()
    {
        var html = SafeMarkdown.ToHtml("use `a * b * c` here");
        Assert.Contains("<code>a * b * c</code>", html);
        Assert.DoesNotContain("<em>", html);
    }

    [Fact]
    public void Empty_ReturnsEmpty()
    {
        Assert.Equal("", SafeMarkdown.ToHtml(null));
        Assert.Equal("", SafeMarkdown.ToHtml(""));
    }
}
