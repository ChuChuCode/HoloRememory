using UnityEngine;
using Steamworks;

// TODO(localization): Unity Localization package is installed but not set
// up yet. Still needed:
//   1. Project Settings > Localization - create Locales for en, zh-Hant,
//      ja, ko (matching Language_Type below).
//   2. Create a String Table Collection for UI text.
//   3. Convert SkillData/CharacterSelectComponent's plain string fields
//      (SkillName, Description, CharacterName, etc.) to LocalizedString so
//      they pull from that table.
//   4. Bridge CurrentLanguage here to Unity's own
//      LocalizationSettings.SelectedLocale (they're two separate systems
//      right now - this component doesn't touch Localization at all yet).
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
