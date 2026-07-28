using HR.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
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
        // FFA has no teams to restrict chat to (every TeamID is a unique
        // player slot, so the old team-only path would only ever reach the
        // sender) - everyone just gets the message.
        MessageComponent newMessage = Instantiate(Message_Prefab);
        newMessage.SetString(userName, message);
        // Set Parent
        newMessage.transform.SetParent(messageContent);
        newMessage.transform.localScale = Vector3.one;
        scrollRect.verticalNormalizedPosition = 0;
    }
}
