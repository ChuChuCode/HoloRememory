using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace HR.Network.Select{
public class Network_SelectPlayer : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayerSteamID;
    public TMP_Text PlayerNameText;
    public Image PlayerIcon;
    public Image Spell_1;
    public Image Spell_2;
    public int TeamID;
    public bool isSelecet;
    void Start()
    {
        
    }
    public void SetPlayerValues()
    {
        PlayerNameText.text = PlayerName;
    }
    public void SetCharacterImage(Sprite sprite)
    {
        PlayerIcon.sprite = sprite;
    }
}

}