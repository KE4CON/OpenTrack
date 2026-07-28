// OpenTrack - an open-source bug and issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

namespace OpenTrack.UI;

/// <summary>
/// Marker type with no behavior of its own — exists only so other projects (notably
/// OpenTrack.Web's Router) have a concrete type to point at when they need
/// "the OpenTrack.UI assembly" via typeof(AssemblyMarker).Assembly, since Blazor's
/// Router.AdditionalAssemblies needs an actual Type, and _Imports.razor doesn't produce one.
/// </summary>
public sealed class AssemblyMarker;
