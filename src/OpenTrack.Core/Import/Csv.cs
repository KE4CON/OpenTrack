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

namespace OpenTrack.Core.Import;

/// <summary>A small, dependency-free RFC 4180-style CSV reader: comma-separated, double-quoted fields with
/// embedded commas / newlines / doubled quotes ("") supported. Returns one string[] per record.</summary>
public static class Csv
{
    public static List<string[]> Parse(string? text)
    {
        var records = new List<string[]>();
        if (string.IsNullOrEmpty(text)) return records;

        // Strip a UTF-8 BOM if present.
        var i = 0;
        if (text[0] == '﻿') i = 1;

        var field = new System.Text.StringBuilder();
        var record = new List<string>();
        var inQuotes = false;
        var sawAny = false;

        void EndField() { record.Add(field.ToString()); field.Clear(); }
        void EndRecord()
        {
            EndField();
            records.Add(record.ToArray());
            record = new List<string>();
        }

        for (; i < text.Length; i++)
        {
            var c = text[i];
            sawAny = true;
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; } // escaped quote
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                switch (c)
                {
                    case '"': inQuotes = true; break;
                    case ',': EndField(); break;
                    case '\r': break; // handled by the \n case (or a lone \r ends nothing visible)
                    case '\n': EndRecord(); break;
                    default: field.Append(c); break;
                }
            }
        }
        // Flush the final record if the file didn't end with a newline (or had trailing content).
        if (field.Length > 0 || record.Count > 0 || (sawAny && records.Count == 0))
            EndRecord();

        return records;
    }
}
