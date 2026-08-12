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

using OpenTrack.Infrastructure.Ai;
using OpenTrack.Infrastructure.Export;

namespace OpenTrack.API.Tests;

/// <summary>Regression tests for audit-hardening fixes: CSV formula-injection neutralization and the
/// AI-prompt input cap.</summary>
public class AuditHardeningTests
{
    [Theory]
    [InlineData("=1+1")]
    [InlineData("+cmd")]
    [InlineData("-2+3")]
    [InlineData("@SUM(A1)")]
    public void CsvExport_NeutralizesFormulaLeadingChars(string value)
    {
        var cell = ExportBuilder.Csv(value);
        Assert.StartsWith("'", cell.TrimStart('"')); // a leading apostrophe forces text, not a formula
    }

    [Fact]
    public void CsvExport_LeavesOrdinaryValuesAlone()
    {
        Assert.Equal("hello", ExportBuilder.Csv("hello"));
        Assert.Equal("\"a,b\"", ExportBuilder.Csv("a,b")); // still RFC-4180 quoted for the comma
    }

    [Fact]
    public void AiText_CapsLongInput()
    {
        Assert.Equal(new string('x', 500), AiText.Cap(new string('x', 5000), 500));
        Assert.Equal("short", AiText.Cap("short", 500));
        Assert.Equal("", AiText.Cap(null, 500));
    }
}
