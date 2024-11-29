using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

namespace HR.Network.Lobby{
public class LobbyListManager : MonoBehaviour
{
    public static LobbyListManager instance;
    public GameObject lobbyMenu;
    public LobbyRoomItem lobbyroomPrefab;
    public Transform ListContent;
    public GameObject Main_UI;
    public List<LobbyRoomItem> listOfLobbies = new List<LobbyRoomItem>();
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // 
    public void GetListOfLobbies()
    {
        // UI Set
        Main_UI.SetActive(false);
        lobbyMenu.SetActive(true);

        SteamLobby.Instance.GetLobbyList();
    }
    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result)
    {
        for(int i = 0; i < lobbyIDs.Count ; i++)
        {
            if (lobbyIDs[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                LobbyRoomItem lobbyRoomTemp = Instantiate(lobbyroomPrefab);
                lobbyRoomTemp.lobbyID = (CSteamID)lobbyIDs[i].m_SteamID;
                lobbyRoomTemp.lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID,"name");
                lobbyRoomTemp.SetLobbyData();
                lobbyRoomTemp.transform.SetParent(ListContent);
                lobbyRoomTemp.transform.localScale = Vector3.one;
                // Add to List
                listOfLobbies.Add(lobbyRoomTemp);
            }
        }
    }
    public void DestroyLobbies()
    {
        foreach(LobbyRoomItem lobbyRoomItem in listOfLobbies)
        {
            Destroy(lobbyRoomItem.gameObject);
        }
        listOfLobbies.Clear();
    }
    public void BackToMain()
    {
        DestroyLobbies();
        // UI Set
        Main_UI.SetActive(true);
        lobbyMenu.SetActive(false);
        
    }
}

}