using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Network_SelectPlayer : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayerSteamID;
    public TMP_Text PlayerNameText;
    public Image PlayerIcon;
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
