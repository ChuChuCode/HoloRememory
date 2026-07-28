using UnityEngine;
using UnityEngine.UI;
using HR.Object.Player;
using TMPro;

namespace HR.UI{

public class PlayerInfo_Component : MonoBehaviour
{
    public CharacterBase characterBase;
    public int PlayerID;
    public Image CharacterImage;
    public Image[] Equipment_Images = new Image[6];

    public void Initial(CharacterBase character)
    {
        characterBase = character;
        // Set Character Image
        CharacterImage.sprite = characterBase.CharacterImage;
    }
    public void UpdateInfo()
    {
    }
}

}