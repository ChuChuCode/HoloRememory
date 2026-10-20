using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Network.Lobby;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


namespace HR.Network.Select{
public class CharacterSelectItem : MonoBehaviour
{
    const string UITableName = "UI";

    [SerializeField] Image characterImage;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] AudioSource audioSource;
    AudioClip audioClip;
    public int CharacterID { get; private set; }

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }
    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
    void OnLocaleChanged(Locale locale)
    {
        RefreshNameText();
    }
    void RefreshNameText()
    {
        if (characterNameText != null) characterNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString(UITableName, $"character_{CharacterID}_name");
    }

    // Called by LobbyController when it builds the character buttons - the
    // one place external code needs to set this button's data.
    public void SetCharacterData(int characterID, Sprite sprite, AudioClip clip)
    {
        CharacterID = characterID;
        characterImage.sprite = sprite;
        audioClip = clip;
        RefreshNameText();
    }
    // Button click -> LobbyController.UpdatePlayerList (via the CharacterID
    // SyncVar hook) refreshes every button's interactable state and the
    // Ready button, same pattern as the team slot buttons.
    public void Select_Character()
    {
        // LocalPlayerController isn't set yet until the local player's own
        // PlayerObject.OnStartAuthority() runs - a same-frame ordering race
        // with LobbyController starting up can let a click land before that
        // happens, which otherwise NPEs here.
        PlayerObject localPlayer = LobbyController.Instance?.LocalPlayerController;
        if (localPlayer == null) return;

        localPlayer.CanSetCharacter(CharacterID);
        CharacterDetailPanel.Instance?.ShowCharacter(CharacterID);
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}

}