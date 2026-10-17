using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace HR.GUI{
// Backfills missing/blank translations across every String Table
// Collection in the project, copying from English - so a Key only ever
// needs to be typed once, and adding a brand new Locale later (just create
// the asset, run this) starts every existing Key off readable instead of
// blank, rather than re-typing the same placeholder text per language.
// Never overwrites an entry that already has real text, so running this
// again after translators fill in ja/ko for real is always safe.
public static class LocalizationFillMissing
{
    const string SourceLocaleCode = "en";

    [MenuItem("HoloBomber/Localization/Fill Missing Translations From English")]
    static void FillMissing()
    {
        int filledCount = 0;

        foreach (StringTableCollection collection in LocalizationEditorSettings.GetStringTableCollections())
        {
            StringTable sourceTable = collection.StringTables.FirstOrDefault(t => t.LocaleIdentifier.Code == SourceLocaleCode);
            if (sourceTable == null)
            {
                Debug.LogWarning($"'{collection.TableCollectionName}' has no '{SourceLocaleCode}' table - skipped.");
                continue;
            }

            foreach (SharedTableData.SharedTableEntry sharedEntry in collection.SharedData.Entries)
            {
                StringTableEntry sourceEntry = sourceTable.GetEntry(sharedEntry.Id);
                if (sourceEntry == null || string.IsNullOrEmpty(sourceEntry.Value)) continue; // nothing to copy from either

                foreach (StringTable table in collection.StringTables)
                {
                    if (table == sourceTable) continue;

                    StringTableEntry entry = table.GetEntry(sharedEntry.Id);
                    if (entry != null && !string.IsNullOrEmpty(entry.Value)) continue; // already has real text - don't clobber

                    table.AddEntry(sharedEntry.Id, sourceEntry.Value);
                    EditorUtility.SetDirty(table);
                    filledCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Filled {filledCount} missing translation(s) from '{SourceLocaleCode}'.");
    }
}
}
