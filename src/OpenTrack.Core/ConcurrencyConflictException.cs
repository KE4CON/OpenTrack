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

namespace OpenTrack.Core;

/// <summary>
/// Raised when a save is rejected because the record was changed by someone else since it was
/// loaded (optimistic-concurrency conflict). Both data-service implementations surface this so the
/// UI can tell the user to reload rather than silently overwriting the other person's edit.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message) { }

    public const string DefaultMessage =
        "This item was changed by someone else after you opened it. Reload the page and re-apply your changes.";
}
