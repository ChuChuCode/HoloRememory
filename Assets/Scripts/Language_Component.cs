using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Steamworks;

namespace HR.Global{
public enum Language_Type
{
    EN,
    ZH_TW,
    JA,
    KO
}
public class Language_Component : MonoBehaviour
{
    public static Language_Component Instance;
    public Language_Type CurrentLanguage;
    const string LANGUAGE_KEY = "Language";

    // Unity's own Locale identifier codes (see the 4 assets under
    // Assets/Localization/) - matched by code rather than array/enum index,
    // so reordering or adding Locale assets in Project Settings never
    // silently desyncs from this enum.
    static readonly string[] LocaleCodes = { "en", "zh-TW", "ja", "ko" };

    const string UITableName = "UI";
    const string NativeNameKey = "language_native_name";

    // Each language's own name, written in itself - for UI language pickers
    // (e.g. SettingPanel's dropdown). Pulled from the UI table like any
    // other text, but NOT through the normal LocalizedString/
    // LocalizeStringEvent path - that only ever resolves whatever locale is
    // CURRENTLY selected, whereas a picker needs all 4 locales' own
    // versions of this one Key at once (so you can always find "日本語" to
    // switch to it, not have it get translated INTO whatever's currently
    // selected). GetLocalizedString's explicit `locale` parameter is what
    // makes that possible - it fetches one specific locale's entry
    // regardless of SelectedLocale.
    public static string GetNativeName(Language_Type language)
    {
        string code = LocaleCodes[(int)language];
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale == null) return code; // Locale asset missing - fall back to the raw code rather than crash/blank

        return LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, NativeNameKey, locale);
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GetDataFromPlayerPrefs();
        StartCoroutine(ApplyLocaleWhenReady());
    }

    public void GetDataFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey(LANGUAGE_KEY))
        {
            CurrentLanguage = (Language_Type)PlayerPrefs.GetInt(LANGUAGE_KEY);
        }
        else
        {
            // First launch - no saved preference yet, so pick a default
            // from whatever the platform tells us instead of always
            // landing on EN.
            CurrentLanguage = DetectLanguage();
            SaveDataToPlayerPrefs();
        }
    }

    public void SaveDataToPlayerPrefs()
    {
        PlayerPrefs.SetInt(LANGUAGE_KEY, (int)CurrentLanguage);
    }

    // Called by the Settings page's language option. Updates the saved
    // preference AND Unity's own LocalizationSettings.SelectedLocale in one
    // call - that single assignment is what cascades the change out to
    // every live LocalizeStringEvent in the game, no manual UI refresh loop
    // needed.
    public void SetLanguage(Language_Type language)
    {
        CurrentLanguage = language;
        SaveDataToPlayerPrefs();
        ApplyLocale();
    }

    // Live-preview only - same visual effect as SetLanguage (every
    // LocalizeStringEvent updates immediately) but doesn't touch
    // PlayerPrefs. Lets SettingPanel show the change as you pick it in the
    // dropdown while still being able to revert cleanly on Back (by calling
    // this again with whatever CurrentLanguage was before you opened the
    // panel) without having prematurely saved a choice you didn't commit.
    public void PreviewLanguage(Language_Type language)
    {
        CurrentLanguage = language;
        ApplyLocale();
    }

    // LocalizationSettings does its own async init (reading Locales via
    // Addressables) before SelectedLocale can be set - assigning it any
    // earlier gets silently overwritten once that init actually finishes.
    IEnumerator ApplyLocaleWhenReady()
    {
        yield return LocalizationSettings.InitializationOperation;
        ApplyLocale();
    }

    void ApplyLocale()
    {
        string code = LocaleCodes[(int)CurrentLanguage];
        Locale locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Debug.LogWarning($"No Locale found for code '{code}' (Language_Type.{CurrentLanguage}) - check the Locale assets under Assets/Localization.");
        }
    }

    // Only ever consulted once, on the very first launch (see
    // GetDataFromPlayerPrefs) - after that the player's own PlayerPrefs
    // choice always wins, even if they later switch their Steam client
    // language. SteamManager.Initialized is false on the non-Steam KCP
    // transport path (see NetworkTransportSelector), so that build just
    // falls through to EN.
    Language_Type DetectLanguage()
    {
        if (SteamManager.Initialized)
        {
            string steamLang = SteamApps.GetCurrentGameLanguage();
            switch (steamLang)
            {
                case "tchinese": return Language_Type.ZH_TW;
                case "schinese": return Language_Type.ZH_TW;
                case "japanese": return Language_Type.JA;
                case "koreana": return Language_Type.KO;
            }
        }
        return Language_Type.EN;
    }
}
}
