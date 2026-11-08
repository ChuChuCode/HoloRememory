using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace HR.GUI{
// Shared "rows -> Table Collection" upsert logic used by both
// LocalizationGoogleSheetImport (rows from the Sheets API) and
// LocalizationCsvImport (rows from a local CSV file) - the two just differ
// in where the rows come from, not in how they get written. Upsert only: a
// key already in the Table but missing from the source rows is left
// untouched, never deleted (see LocalizationGoogleSheetImport's own comment
// for why that matters). To retire a key, remove it by hand in the
// Localization Tables window.
static class LocalizationTableWriter
{
    // Writes `rows` (row 0 = header, looked up by column NAME so source
    // column order doesn't matter) into the named String Table Collection.
    // Appends a one-line result to `results` either way; returns false only
    // on a failure that stopped the write entirely.
    public static bool WriteRows(string collectionName, List<string[]> rows, List<string> results, string sourceLabel)
    {
        if (rows.Count == 0)
        {
            results.Add($"{sourceLabel} -> {collectionName}: 0 row(s) (source is empty - nothing changed)");
            return true;
        }

        Dictionary<string, int> columnIndex = MapColumns(rows[0]);
        if (columnIndex == null)
        {
            string[] required = LocalizationSheetSchema.RequiredColumns();
            string missing = string.Join(", ", required.Where(c => !rows[0].Contains(c)));
            results.Add($"{sourceLabel} -> {collectionName}: FAILED (header is missing column(s): {missing})");
            return false;
        }

        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(collectionName);
        if (collection == null)
        {
            results.Add($"{sourceLabel} -> {collectionName}: FAILED (no '{collectionName}' String Table Collection in this project)");
            return false;
        }

        SharedTableData sharedData = collection.SharedData;
        Dictionary<string, StringTable> tablesByLocale = collection.StringTables.ToDictionary(t => t.LocaleIdentifier.Code, t => t);

        int written = 0;
        for (int r = 1; r < rows.Count; r++)
        {
            string[] row = rows[r];
            string key = Field(row, columnIndex, LocalizationSheetSchema.KeyColumn);
            if (string.IsNullOrWhiteSpace(key)) continue; // skip blank rows

            SharedTableData.SharedTableEntry sharedEntry = sharedData.GetEntry(key) ?? sharedData.AddKey(key);
            SetComment(sharedEntry.Metadata, Field(row, columnIndex, LocalizationSheetSchema.SharedCommentsColumn));

            foreach (var locale in LocalizationSheetSchema.Locales)
            {
                if (!tablesByLocale.TryGetValue(locale.LocaleCode, out StringTable table))
                {
                    Debug.LogWarning($"'{collectionName}' has no table for locale '{locale.LocaleCode}' - skipping that column for key '{key}'.");
                    continue;
                }

                StringTableEntry entry = table.AddEntry(sharedEntry.Id, Field(row, columnIndex, locale.ValueColumn));
                SetComment(entry, Field(row, columnIndex, locale.CommentColumn));
                EditorUtility.SetDirty(table);
            }

            written++;
        }

        EditorUtility.SetDirty(sharedData);
        EditorUtility.SetDirty(collection);
        results.Add($"{sourceLabel} -> {collectionName}: {written} entr{(written == 1 ? "y" : "ies")} added/updated");
        return true;
    }

    // Mutates an existing Comment metadata in place if there is one,
    // otherwise adds a new one - MetadataCollection (shared entries) and
    // StringTableEntry (per-locale entries) both expose the same
    // GetMetadata<T>/AddMetadata shape, so one overload covers both.
    static void SetComment(MetadataCollection metadata, string text)
    {
        Comment existing = metadata.GetMetadata<Comment>();
        if (existing != null) existing.CommentText = text;
        else metadata.AddMetadata(new Comment { CommentText = text });
    }

    static void SetComment(StringTableEntry entry, string text)
    {
        Comment existing = entry.GetMetadata<Comment>();
        if (existing != null) existing.CommentText = text;
        else entry.AddMetadata(new Comment { CommentText = text });
    }

    static string Field(string[] row, Dictionary<string, int> columnIndex, string column)
    {
        int i = columnIndex[column];
        return i >= 0 && i < row.Length ? row[i] : "";
    }

    // Maps each required column name to its index in the header row - or
    // null if the header is missing any of them.
    static Dictionary<string, int> MapColumns(string[] header)
    {
        var map = new Dictionary<string, int>();
        foreach (string column in LocalizationSheetSchema.RequiredColumns())
        {
            int index = System.Array.IndexOf(header, column);
            if (index < 0) return null;
            map[column] = index;
        }
        return map;
    }
}
}
