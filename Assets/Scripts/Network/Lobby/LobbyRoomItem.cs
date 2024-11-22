using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using TMPro;

public class LobbyRoomItem : MonoBehaviour
{
    [Header("Data")]
    public CSteamID lobbyID;
    public string lobbyName;
    public TMP_Text lobbyNameText;
    public void SetLobbyData()
    {
        if (lobbyName == "")
        {
            lobbyNameText.text = "Empty Lobby";
        }
        else
        {
            lobbyNameText.text = lobbyName;
        }
    }
    // Join Button    
    public void JoinLobby()
    {
        SteamLobby.Instance.JoinLobby(lobbyID);
    }
}

