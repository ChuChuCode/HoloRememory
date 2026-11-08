using System.Collections.Generic;

namespace HR.GUI{
// Column layout shared by the Google Sheet <-> Localization Table import
// and export tools - one place to change if a locale is ever added/removed,
// so the two directions can't drift apart from each other.
static class LocalizationSheetSchema
{
    public const string KeyColumn = "Key";
    public const string IdColumn = "Id";
    public const string SharedCommentsColumn = "Shared Comments";

    public static readonly (string LocaleCode, string ValueColumn, string CommentColumn)[] Locales =
    {
        ("zh-TW", "Chinese (Traditional)(zh-TW)", "Chinese (Traditional)(zh-TW) Comments"),
        ("en", "English(en)", "English(en) Comments"),
        ("ja", "Japanese(ja)", "Japanese(ja) Comments"),
        ("ko", "Korean(ko)", "Korean(ko) Comments"),
    };

    // Full header row, including the (write-only, otherwise-ignored) Id
    // column - kept so the Sheet's own header stays stable/readable rather
    // than shrinking a column the first time someone runs an export.
    public static string[] Header()
    {
        var columns = new List<string> { KeyColumn, IdColumn, SharedCommentsColumn };
        foreach (var locale in Locales)
        {
            columns.Add(locale.ValueColumn);
            columns.Add(locale.CommentColumn);
        }
        return columns.ToArray();
    }

    // Columns a source (Sheet or CSV) must have for an import to work - Id
    // is deliberately excluded, since it's read-ignored (see LocalizationGoogleSheetImport).
    public static string[] RequiredColumns()
    {
        var columns = new List<string> { KeyColumn, SharedCommentsColumn };
        foreach (var locale in Locales)
        {
            columns.Add(locale.ValueColumn);
            columns.Add(locale.CommentColumn);
        }
        return columns.ToArray();
    }
}
}
