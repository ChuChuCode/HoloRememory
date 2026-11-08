using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HR.GUI{
// Pulls Assets/Localization/CSV/Story.csv and UI.csv into the matching
// Localization String Table Collections - the CSV-sourced counterpart to
// LocalizationGoogleSheetImport (same upsert semantics, see
// LocalizationTableWriter). Useful during development when new keys get
// added straight to a CSV (by hand, or by another tool such as
// LocalizationSeedUIStrings) rather than through the Sheet, and need
// pulling into the live Tables the game actually reads.
public static class LocalizationCsvImport
{
    [MenuItem("HoloBomber/Localization/Import From CSV")]
    static void Import()
    {
        if (!EditorUtility.DisplayDialog("Import Localization From CSV",
            "This will add/update entries in the Story and UI Localization tables from Story.csv and UI.csv. " +
            "Keys that exist in a table but aren't in the CSV are left untouched. Continue?",
            "Import", "Cancel"))
        {
            return;
        }

        var results = new List<string>();
        bool anyFailed = false;

        anyFailed |= !ImportFile("Assets/Localization/CSV/Story.csv", "Story", results);
        anyFailed |= !ImportFile("Assets/Localization/CSV/UI.csv", "UI", results);

        AssetDatabase.SaveAssets();

        string summary = string.Join("\n", results);
        if (anyFailed)
        {
            Debug.LogError($"CSV import finished with errors:\n{summary}");
            EditorUtility.DisplayDialog("Import Finished With Errors", summary + "\n\nSee the Console for details.", "OK");
        }
        else
        {
            Debug.Log($"CSV import complete:\n{summary}");
            EditorUtility.DisplayDialog("Import Complete", summary, "OK");
        }
    }

    static bool ImportFile(string path, string collectionName, List<string> results)
    {
        if (!File.Exists(path))
        {
            results.Add($"'{path}' -> {collectionName}: FAILED (file not found)");
            return false;
        }

        List<string[]> rows = ParseCsv(File.ReadAllText(path));
        return LocalizationTableWriter.WriteRows(collectionName, rows, results, $"'{path}'");
    }

    // Minimal RFC4180-style CSV parser - handles quoted fields, "" as an
    // escaped quote inside a quoted field, and commas/newlines inside
    // quotes. Needed because the existing CSVs already have commas inside
    // quoted "Shared Comments" fields, so a naive Split(',') would misalign
    // columns.
    static List<string[]> ParseCsv(string text)
    {
        var rows = new List<string[]>();
        var fields = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;
        int i = 0;

        void EndField() { fields.Add(field.ToString()); field.Clear(); }
        void EndRow() { EndField(); rows.Add(fields.ToArray()); fields = new List<string>(); }

        while (i < text.Length)
        {
            char c = text[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i += 2; continue; }
                if (c == '"') { inQuotes = false; i++; continue; }
                field.Append(c);
                i++;
                continue;
            }

            if (c == '"') { inQuotes = true; i++; continue; }
            if (c == ',') { EndField(); i++; continue; }
            if (c == '\r') { i++; continue; }
            if (c == '\n') { EndRow(); i++; continue; }
            field.Append(c);
            i++;
        }
        if (field.Length > 0 || fields.Count > 0) EndRow();

        return rows;
    }
}
}
