using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Network.Lobby;


namespace HR.Network.Select{
public class CharacterSelectItem : MonoBehaviour
{
    [SerializeField] Image characterImage;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] AudioSource audioSource;
    AudioClip audioClip;
    public int CharacterID { get; private set; }

    // Called by LobbyController when it builds the character buttons - the
    // one place external code needs to set this button's data.
    public void SetCharacterData(int characterID, string characterName, Sprite sprite, AudioClip clip)
    {
        CharacterID = characterID;
        characterImage.sprite = sprite;
        audioClip = clip;
        if (characterNameText != null) characterNameText.text = characterName;
    }
    // Button click -> LobbyController.UpdatePlayerList (via the CharacterID
    // SyncVar hook) refreshes every button's interactable state and the
    // Ready button, same pattern as the team slot buttons.
    public void Select_Character()
    {
        LobbyController.Instance.LocalPlayerController.CanSetCharacter(CharacterID);
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}

}