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

namespace OpenTrack.Core.Validation;

/// <summary>Validates the browser-supplied issue location. Latitude/longitude come from a client the
/// server doesn't control, so out-of-range or non-finite values are rejected (→ null) rather than stored
/// and rendered into a map link.</summary>
public static class GeoValidation
{
    /// <summary>Returns the latitude if it's a finite value within [-90, 90], else null.</summary>
    public static double? Latitude(double? value) =>
        value is { } v && double.IsFinite(v) && v is >= -90 and <= 90 ? v : null;

    /// <summary>Returns the longitude if it's a finite value within [-180, 180], else null.</summary>
    public static double? Longitude(double? value) =>
        value is { } v && double.IsFinite(v) && v is >= -180 and <= 180 ? v : null;
}
