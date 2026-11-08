using UnityEditor;
using UnityEngine;

namespace HR.GUI{
// Configures LocalizationGoogleSheetImport/Export - create one via Create >
// HoloBomber > Localization > Google Sheet Import Settings and fill in
// SpreadsheetId (the long ID segment in the Sheet's URL,
// .../spreadsheets/d/<THIS>/edit). The actual credentials (service account
// JSON key) are NOT stored here - see GoogleSheetsAuth's own comment for
// where that file lives and why.
[CreateAssetMenu(fileName = "GoogleSheetImportSettings", menuName = "HoloBomber/Localization/Google Sheet Import Settings")]
public class GoogleSheetImportSettings : ScriptableObject
{
    public string SpreadsheetId;
    public string StorySheetName = "Story";
    public string UISheetName = "UI";

    public static GoogleSheetImportSettings Find()
    {
        string[] guids = AssetDatabase.FindAssets("t:GoogleSheetImportSettings");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GoogleSheetImportSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}
}
