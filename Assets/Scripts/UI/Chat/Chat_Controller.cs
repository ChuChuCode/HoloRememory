using HR.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.Network.Select;
using HR.Network.Game;
using UnityEngine.UI;
using Mirror;
using HR.Network;

public class Chat_Controller : NetworkBehaviour
{
    public static Chat_Controller Instance;
    [SerializeField] TMP_InputField message_feild;
    [SerializeField] Transform messageContent;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] MessageComponent Message_Prefab;
    [SerializeField] bool isGlobal;
    private Network_Manager manager;

    public Network_Manager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = Network_Manager.singleton as Network_Manager;
        }
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    // Send Button
    public void Add_Message()
    {
        // Get Message Text
        string message = message_feild.text;
        // if no message
        if (message == "") return;
        // Get User Name
        string userName = "";
        if (SceneManager.GetActiveScene().name == "Lobby_Scene")
        {
            userName = LobbyController.Instance.LocalPlayerController.PlayerName;
        }
        else if (SceneManager.GetActiveScene().name == "Select_Scene")
        {
            userName = SelectController.Instance.LocalPlayerController.PlayerName;
        }
        else
        {
            userName = GameController.Instance.LocalPlayerController.PlayerName;
        }
        // Call CmdAddMessage through the LocalPlayerObject to ensure authority
        Manager.LocalPlayerObject.CmdAddMessage(userName, message);
        message_feild.text = string.Empty;
    }
    [Command]
    public void CmdAddMessage(string userName, string message)
    {
        // Broadcast the message to all clients
        RpcAddMessage(userName, message);
    }
    [ClientRpc]
    public void RpcAddMessage(string userName, string message)
    {
        if (isGlobal)
        {
            // Client-side logic to add the message
            MessageComponent newMessage = Instantiate(Message_Prefab);
            newMessage.SetString(userName, message);
            // Set Parent
            newMessage.transform.SetParent(messageContent);
            newMessage.transform.localScale = Vector3.one;
            scrollRect.verticalNormalizedPosition = 0;
        }
        else
        {
            int localPlayerTeamID = Manager.LocalPlayerObject.TeamID;
            foreach (PlayerObject player in Manager.PlayersInfoList)
            {
                if (player.TeamID == localPlayerTeamID)
                {
                    // // Client-side logic to add the message for team members
                    // MessageComponent newMessage = Instantiate(Message_Prefab);
                    // newMessage.SetString(userName, message);
                    // // Set Parent
                    // newMessage.transform.SetParent(messageContent);
                    // newMessage.transform.localScale = Vector3.one;
                    TargetRpcAddMessage(player.connectionToClient, userName, message);
                }
            }
        }
    }
    [TargetRpc]
    public void TargetRpcAddMessage(NetworkConnection conn, string userName, string message)
    {
        // Client-side logic to add the message for team members
        MessageComponent newMessage = Instantiate(Message_Prefab);
        newMessage.SetString(userName, message);
        // Set Parent
        newMessage.transform.SetParent(messageContent);
        newMessage.transform.localScale = Vector3.one;
    }
}
