using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace HR.GUI{
// One-shot seeding of UI Keys from the main menu/lobby screens, English
// text only - run "Fill Missing Translations From English" afterward to
// backfill zh-TW/ja/ko placeholders, then hand those off for real
// translation. Skips any Key that already exists (by name), so re-running
// this after adding more entries to the list below is always safe and
// never touches a Key someone's already hand-edited.
public static class LocalizationSeedUIStrings
{
    const string TableName = "UI";
    const string SourceLocaleCode = "en";

    static readonly (string key, string text)[] Entries =
    {
        ("press_any_key", "PRESS ANY KEY"),

        ("menu_single", "Single"),
        ("menu_multi", "Multi"),
        ("menu_setting", "Setting"),
        ("menu_credit", "Credit"),
        ("menu_exit", "Exit"),

        ("multi_host", "Host"),
        ("multi_room_list", "Room List"),
        ("multi_quick_match", "Quick Match"),

        ("settings_audio", "Audio"),
        ("settings_music", "Music"),
        ("settings_sfx", "SFX"),
        ("settings_voice", "Voice"),
        ("settings_show_log", "Show LOG"),

        ("room_name_label", "Room Name"),
        ("password_label", "Password"),
        ("password_placeholder", "Enter text..."),
        ("show_password_button", "Show"),
        ("room_visual_label", "Room Visual"),
        ("game_mode_label", "Game Mode"),

        ("room_visual_public", "Public"),
        ("room_visual_friends_only", "Friends Only"),
        ("room_visual_invite_only", "Invite Only"),

        // Matches GameModeConfig.DisplayName on the 3 assets under
        // Assets/Resources/Data/Game Mode/ - keep these in sync by hand if
        // those ever change, they're not read from the asset automatically.
        ("game_mode_one_life", "One Life"),
        ("game_mode_multi_life", "Multi Life"),
        ("game_mode_health_mode", "Health Mode"),

        // Shared across multiple screens (Multiplayer menu, Settings panel,
        // Create Room dialog) - one Key, reused everywhere "Back"/"Save"/
        // etc. appears, instead of a separate near-duplicate per screen.
        ("back", "Back"),
        ("save", "Save"),
        ("cancel", "Cancel"),
        ("create", "Create"),
    };

    [MenuItem("HoloBomber/Localization/Seed UI Strings (English)")]
    static void Seed()
    {
        StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollections()
            .FirstOrDefault(c => c.TableCollectionName == TableName);
        if (collection == null)
        {
            Debug.LogError($"No String Table Collection named '{TableName}' found.");
            return;
        }

        StringTable sourceTable = collection.StringTables.FirstOrDefault(t => t.LocaleIdentifier.Code == SourceLocaleCode);
        if (sourceTable == null)
        {
            Debug.LogError($"'{TableName}' has no '{SourceLocaleCode}' table.");
            return;
        }

        int added = 0;
        foreach ((string key, string text) in Entries)
        {
            if (collection.SharedData.Contains(key)) continue; // already exists - leave it alone

            collection.SharedData.AddKey(key);
            sourceTable.AddEntry(key, text);
            added++;
        }

        EditorUtility.SetDirty(collection.SharedData);
        EditorUtility.SetDirty(sourceTable);
        AssetDatabase.SaveAssets();
        Debug.Log($"Added {added} new Key(s) to '{TableName}' (English only - run 'Fill Missing Translations From English' next to backfill zh-TW/ja/ko placeholders).");
    }
}
}
