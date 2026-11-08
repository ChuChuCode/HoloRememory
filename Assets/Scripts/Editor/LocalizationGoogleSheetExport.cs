using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace HR.GUI{
// The reverse of LocalizationGoogleSheetImport: pushes the project's
// current "Story"/"UI" Localization Table content up into the matching
// tabs of the shared Google Sheet, overwriting whatever is there. Unlike
// the import (which only adds/updates and never deletes a Table entry),
// this intentionally replaces each tab's full contents - the Table is the
// single source of truth being published outward, and Google Sheets' own
// built-in version history (File > Version history) is the safety net if
// an export clobbers someone's in-progress edit in the Sheet.
//
// Requires the service account to have Editor (not just Viewer) access on
// the Sheet - see the Share dialog on the Sheet itself.
public static class LocalizationGoogleSheetExport
{
    static readonly HttpClient Http = new HttpClient();

    [MenuItem("HoloBomber/Localization/Export To Google Sheet")]
    static void Export()
    {
        GoogleSheetImportSettings settings = GoogleSheetImportSettings.Find();
        if (settings == null || string.IsNullOrEmpty(settings.SpreadsheetId))
        {
            Debug.LogError("No GoogleSheetImportSettings asset found (or SpreadsheetId is blank) - " +
                "create one via Create > HoloBomber > Localization > Google Sheet Import Settings and fill in SpreadsheetId.");
            return;
        }

        if (!EditorUtility.DisplayDialog("Export Localization To Google Sheet",
            "This will overwrite the 'Story' and 'UI' tabs in the Sheet with this project's current Localization table content. " +
            "Anything currently in those tabs will be replaced. Continue?",
            "Export", "Cancel"))
        {
            return;
        }

        string accessToken = GoogleSheetsAuth.GetAccessToken();
        if (string.IsNullOrEmpty(accessToken)) return; // GoogleSheetsAuth already logged why

        var results = new List<string>();
        bool anyFailed = false;

        anyFailed |= !ExportCollection(settings.SpreadsheetId, settings.StorySheetName, "Story", accessToken, results);
        anyFailed |= !ExportCollection(settings.SpreadsheetId, settings.UISheetName, "UI", accessToken, results);

        string summary = string.Join("\n", results);
        if (anyFailed)
        {
            Debug.LogError($"Google Sheet export finished with errors:\n{summary}");
            EditorUtility.DisplayDialog("Export Finished With Errors", summary + "\n\nSee the Console for details.", "OK");
        }
        else
        {
            Debug.Log($"Google Sheet export complete:\n{summary}");
            EditorUtility.DisplayDialog("Export Complete", summary, "OK");
        }
    }

    static bool ExportCollection(string spreadsheetId, string sheetName, string collectionName, string accessToken, List<string> results)
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(collectionName);
        if (collection == null)
        {
            results.Add($"{collectionName} -> '{sheetName}': FAILED (no '{collectionName}' String Table Collection in this project)");
            return false;
        }

        Dictionary<string, StringTable> tablesByLocale = collection.StringTables.ToDictionary(t => t.LocaleIdentifier.Code, t => t);

        var rows = new List<string[]> { LocalizationSheetSchema.Header() };
        foreach (SharedTableData.SharedTableEntry sharedEntry in collection.SharedData.Entries)
        {
            var fields = new List<string>
            {
                sharedEntry.Key,
                "", // Id column - write-side leaves it blank, see LocalizationSheetSchema
                sharedEntry.Metadata.GetMetadata<Comment>()?.CommentText ?? "",
            };

            foreach (var locale in LocalizationSheetSchema.Locales)
            {
                StringTableEntry entry = tablesByLocale.TryGetValue(locale.LocaleCode, out StringTable table)
                    ? table.GetEntry(sharedEntry.Id)
                    : null;
                fields.Add(entry?.Value ?? "");
                fields.Add(entry?.GetMetadata<Comment>()?.CommentText ?? "");
            }

            rows.Add(fields.ToArray());
        }

        try
        {
            WriteSheetValues(spreadsheetId, sheetName, rows, accessToken);
        }
        catch (Exception e)
        {
            results.Add($"{collectionName} -> '{sheetName}': FAILED to write ({e.Message})");
            return false;
        }

        results.Add($"{collectionName} -> '{sheetName}': {rows.Count - 1} row(s) written");
        return true;
    }

    static void WriteSheetValues(string spreadsheetId, string sheetName, List<string[]> rows, string accessToken)
    {
        string encodedSheet = Uri.EscapeDataString(sheetName);

        // Clear the whole tab first so a shrinking row count (a key got
        // removed from the Table since the last export) doesn't leave
        // stale rows behind below the new, shorter data.
        string clearUrl = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{encodedSheet}:clear";
        SendJson(HttpMethod.Post, clearUrl, "{}", accessToken);

        var body = new JObject
        {
            ["range"] = sheetName,
            ["majorDimension"] = "ROWS",
            ["values"] = new JArray(rows.Select(row => new JArray(row.Select(cell => (JToken)cell)))),
        };
        string updateUrl = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{encodedSheet}!A1?valueInputOption=RAW";
        SendJson(HttpMethod.Put, updateUrl, body.ToString(Newtonsoft.Json.Formatting.None), accessToken);
    }

    static void SendJson(HttpMethod method, string url, string json, string accessToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = Http.SendAsync(request).GetAwaiter().GetResult();
        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Sheets API returned {(int)response.StatusCode}: {responseBody}");
        }
    }
}
}
