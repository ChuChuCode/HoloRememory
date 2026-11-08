using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace HR.GUI{
// Pulls a shared Google Sheet (two tabs, "Story" and "UI" by default) via
// the real Google Sheets API v4 - see GoogleSheetsAuth for the auth side -
// and writes the rows straight into the matching Localization String Table
// Collections (also named "Story"/"UI"). Upsert only: a key present in a
// table but missing from the Sheet is left untouched, never deleted - so an
// accidentally near-empty Sheet can't wipe out existing translations the
// way a CSV full-overwrite used to (that's exactly what happened once
// during development, with the CSV-file version of this tool, before this
// was switched to write Tables directly). To retire a key, remove it from
// the Table by hand in the Localization Tables window.
public static class LocalizationGoogleSheetImport
{
    static readonly HttpClient Http = new HttpClient();

    [MenuItem("HoloBomber/Localization/Import From Google Sheet")]
    static void Import()
    {
        GoogleSheetImportSettings settings = GoogleSheetImportSettings.Find();
        if (settings == null || string.IsNullOrEmpty(settings.SpreadsheetId))
        {
            Debug.LogError("No GoogleSheetImportSettings asset found (or SpreadsheetId is blank) - " +
                "create one via Create > HoloBomber > Localization > Google Sheet Import Settings and fill in SpreadsheetId.");
            return;
        }

        if (!EditorUtility.DisplayDialog("Import Localization From Google Sheet",
            "This will add/update entries in the Story and UI Localization tables to match the Sheet's current content. " +
            "Keys that exist in a table but aren't in the Sheet are left untouched. Continue?",
            "Import", "Cancel"))
        {
            return;
        }

        string accessToken = GoogleSheetsAuth.GetAccessToken();
        if (string.IsNullOrEmpty(accessToken)) return; // GoogleSheetsAuth already logged why

        var results = new List<string>();
        bool anyFailed = false;

        anyFailed |= !ImportSheet(settings.SpreadsheetId, settings.StorySheetName, "Story", accessToken, results);
        anyFailed |= !ImportSheet(settings.SpreadsheetId, settings.UISheetName, "UI", accessToken, results);

        AssetDatabase.SaveAssets();

        string summary = string.Join("\n", results);
        if (anyFailed)
        {
            Debug.LogError($"Google Sheet import finished with errors:\n{summary}");
            EditorUtility.DisplayDialog("Import Finished With Errors", summary + "\n\nSee the Console for details.", "OK");
        }
        else
        {
            Debug.Log($"Google Sheet import complete:\n{summary}");
            EditorUtility.DisplayDialog("Import Complete", summary, "OK");
        }
    }

    // Returns false (and appends an error line to results) on failure,
    // rather than throwing - a problem with one sheet (e.g. a typo'd tab
    // name) shouldn't stop the other from importing.
    static bool ImportSheet(string spreadsheetId, string sheetName, string collectionName, string accessToken, List<string> results)
    {
        List<string[]> rows;
        try
        {
            rows = FetchSheetValues(spreadsheetId, sheetName, accessToken);
        }
        catch (Exception e)
        {
            results.Add($"'{sheetName}' -> {collectionName}: FAILED to fetch ({e.Message})");
            return false;
        }

        return LocalizationTableWriter.WriteRows(collectionName, rows, results, $"'{sheetName}'");
    }

    static List<string[]> FetchSheetValues(string spreadsheetId, string sheetName, string accessToken)
    {
        string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{Uri.EscapeDataString(sheetName)}?majorDimension=ROWS";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = Http.SendAsync(request).GetAwaiter().GetResult();
        string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Sheets API returned {(int)response.StatusCode}: {body}");
        }

        JObject json = JObject.Parse(body);
        JArray values = json["values"] as JArray;
        var rows = new List<string[]>();
        if (values == null) return rows;

        foreach (JToken rowToken in values)
        {
            rows.Add(rowToken.Select(cell => cell.ToString()).ToArray());
        }
        return rows;
    }
}
}
