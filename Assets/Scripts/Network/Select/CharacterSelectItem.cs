using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        SelectController.Instance.LocalPlayerController.CanSetCharacter(CharacterID);
        // SelectController.Instance.UpdatePlayerUI();
        audioSource.clip = audioClip;
        audioSource.Play();
    }
}
