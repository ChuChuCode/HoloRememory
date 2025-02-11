using UnityEngine;
using UnityEngine.UI;


namespace HR.Network.Select{
public class CharacterSelectItem : MonoBehaviour
{
    public Image characterImage;
    public AudioClip audioClip;
    public int CharacterID;
    [SerializeField] AudioSource audioSource;
    public void SetImage(Sprite sprite)
    {
        characterImage.sprite = sprite;
    }
    public void SetCharaterID()
    {

    }
    //Update UI
    public void Select_Character()
    {
        // Set Old interactable true
        SelectController.Instance.Character_Interactable(SelectController.Instance.LocalPlayerController.CharacterID,true);
        // Local Player Select
        SelectController.Instance.LocalPlayerController.CanSetCharacter(CharacterID);
        // SelectController.Instance.LocalPlayerController.CanSetCharacter(CharacterID);
        // SelectController.Instance.UpdatePlayerUI();

        // Set New interactable false
        SelectController.Instance.Character_Interactable(CharacterID,false);

        // Check Ready Button
        // SelectController.Instance.ReadyButton.interactable = true;
        SelectController.Instance.Check_ReadyButton();
        
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}

}