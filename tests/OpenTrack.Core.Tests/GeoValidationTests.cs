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

using OpenTrack.Core.Validation;

namespace OpenTrack.Core.Tests;

/// <summary>Client-supplied issue coordinates are accepted only when finite and in range; anything else
/// (out of range, NaN, infinity) is dropped to null rather than stored.</summary>
public class GeoValidationTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(45.5, -122.6)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void InRange_IsKept(double lat, double lon)
    {
        Assert.Equal(lat, GeoValidation.Latitude(lat));
        Assert.Equal(lon, GeoValidation.Longitude(lon));
    }

    [Theory]
    [InlineData(90.1)]
    [InlineData(-90.1)]
    [InlineData(9999)]
    public void LatitudeOutOfRange_IsNull(double lat) => Assert.Null(GeoValidation.Latitude(lat));

    [Theory]
    [InlineData(180.1)]
    [InlineData(-180.1)]
    [InlineData(9999)]
    public void LongitudeOutOfRange_IsNull(double lon) => Assert.Null(GeoValidation.Longitude(lon));

    [Fact]
    public void NonFiniteOrNull_IsNull()
    {
        Assert.Null(GeoValidation.Latitude(double.NaN));
        Assert.Null(GeoValidation.Longitude(double.PositiveInfinity));
        Assert.Null(GeoValidation.Latitude(null));
        Assert.Null(GeoValidation.Longitude(null));
    }
}
