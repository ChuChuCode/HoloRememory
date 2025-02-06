using UnityEngine;
using UnityEngine.UI;
using HR.Object.Player;
using TMPro;

namespace HR.UI{

public class Result_Component : MonoBehaviour
{
    public CharacterBase characterBase;
    public Image CharacterImage;
    public TMP_Text Kill_Text;
    public TMP_Text Death_Text;
    public TMP_Text Assist_Text;
    public Image[] Equipment_Images = new Image[6];
    // Start is called before the first frame update
    public void Initial(Sprite sprite)
    {
        // Set Character Image
        CharacterImage.sprite = sprite;
        // KDA
        Kill_Text.text = characterBase.kill.ToString();
        Death_Text.text = characterBase.death.ToString();
        Assist_Text.text = characterBase.assist.ToString();
        for (int i = 0 ; i < Equipment_Images.Length ; i++)
        {
            Equipment_Images[i].sprite = characterBase.EquipmentSlots[i]?.EquipmentImage;
        }
    }
}

}