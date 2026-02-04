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
    public TMP_Text Kill_Text;
    public TMP_Text Death_Text;
    public TMP_Text Assist_Text;
    public Image[] Equipment_Images = new Image[6];
    
    public void Initial(CharacterBase character)
    {
        characterBase = character;
        // Set Character Image
        CharacterImage.sprite = characterBase.CharacterImage;
        // KDA
        Kill_Text.text = characterBase.kill.ToString();
        Death_Text.text = characterBase.death.ToString();
        Assist_Text.text = characterBase.assist.ToString();
    }
    public void UpdateInfo()
    {
        Kill_Text.text = characterBase.kill.ToString();
        Death_Text.text = characterBase.death.ToString();
        Assist_Text.text = characterBase.assist.ToString();
    }
}

}