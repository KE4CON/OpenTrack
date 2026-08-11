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

using OpenTrack.Core.Enums;
using OpenTrack.Core.Validation;

namespace OpenTrack.Core.Tests;

/// <summary>Pure validation/normalization of custom-field values by type — the rules both the
/// definition CRUD and the value write path rely on.</summary>
public class CustomFieldValidationTests
{
    [Theory]
    [InlineData("", null)]
    [InlineData("   ", null)]   // blank of any kind normalizes to "no value"
    public void Blank_NormalizesToNull_ForAnyType(string raw, string? expected)
    {
        foreach (var type in Enum.GetValues<CustomFieldType>())
        {
            var r = CustomFieldValidation.ValidateValue(type, "A\nB", raw);
            Assert.True(r.Ok);
            Assert.Equal(expected, r.Normalized);
        }
    }

    [Theory]
    [InlineData("42", true)]
    [InlineData("-3.14", true)]
    [InlineData("1,000", true)]      // group separators accepted by NumberStyles.Number
    [InlineData("twelve", false)]
    [InlineData("3 apples", false)]
    public void Number_ParsesDecimals(string raw, bool ok)
    {
        var r = CustomFieldValidation.ValidateValue(CustomFieldType.Number, null, raw);
        Assert.Equal(ok, r.Ok);
    }

    [Fact]
    public void Date_NormalizesToIso()
    {
        var r = CustomFieldValidation.ValidateValue(CustomFieldType.Date, null, "2026-03-09");
        Assert.True(r.Ok);
        Assert.Equal("2026-03-09", r.Normalized);

        Assert.False(CustomFieldValidation.ValidateValue(CustomFieldType.Date, null, "not-a-date").Ok);
    }

    [Fact]
    public void Enum_MustMatchAnOption_AndStoresCanonicalCasing()
    {
        var options = "High\nMedium\nLow";
        var r = CustomFieldValidation.ValidateValue(CustomFieldType.Enum, options, "medium");
        Assert.True(r.Ok);
        Assert.Equal("Medium", r.Normalized); // canonical casing, not the user's

        Assert.False(CustomFieldValidation.ValidateValue(CustomFieldType.Enum, options, "Critical").Ok);
    }

    [Fact]
    public void ParseEnumOptions_TrimsAndDropsBlankLines()
    {
        var options = CustomFieldValidation.ParseEnumOptions("  A \n\n  B\n \nC ");
        Assert.Equal(new[] { "A", "B", "C" }, options);
    }
}
