using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using TMPro;

namespace HR.Network.Lobby{
public class LobbyRoomItem : MonoBehaviour
{
    [Header("Data")]
    public CSteamID lobbyID;
    public string lobbyName;
    public string mapName;
    public string mode;
    public int currentPlayers;
    public int maxPlayers;
    public TMP_Text lobbyNameText;
    public TMP_Text mapText;
    public TMP_Text modeText;
    public TMP_Text playerCountText;
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
        if (mapText != null) mapText.text = mapName;
        if (modeText != null) modeText.text = mode;
        if (playerCountText != null) playerCountText.text = $"{currentPlayers} / {maxPlayers}";
    }
    // Join Button    
    public void JoinLobby()
    {
        SteamLobby.Instance.JoinLobby(lobbyID);
    }
}

}