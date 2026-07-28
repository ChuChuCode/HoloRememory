using UnityEngine;
using UnityEngine.UI;
using HR.Object.Player;
using TMPro;
using HR.Network;

namespace HR.UI{

public class Result_Component : MonoBehaviour
{
    public PlayerObject playerObject;
    public Image CharacterImage;
    public Image[] Equipment_Images = new Image[6];
    // Start is called before the first frame update
    public void Initial(Sprite sprite)
    {
        // Set Character Image
        CharacterImage.sprite = sprite;
        // for (int i = 0 ; i < Equipment_Images.Length ; i++)
        // {
        //     Equipment_Images[i].sprite = characterBase.EquipmentSlots[i]?.EquipmentImage;
        // }
    }
}

}