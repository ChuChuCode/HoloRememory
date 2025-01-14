using HR.UI;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using HR.Network.Lobby;
using HR.Network.Select;
using HR.Network.Game;
using UnityEngine.UI;

public class Chat_Controller : MonoBehaviour
{
    public static Chat_Controller Instance;
    [SerializeField] TMP_InputField message_feild;
    [SerializeField] Transform messageContent;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] MessageComponent Message_Prefab;
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
        string message = message_feild.text;
        // if no message
        if (message == "") return;
        MessageComponent newMessage = Instantiate(Message_Prefab);
        newMessage.SetString(userName,message);
        // Set Parent
        newMessage.transform.SetParent(messageContent);
        newMessage.transform.localScale = Vector3.one;
        // Reset
        message_feild.text = "";
        Canvas.ForceUpdateCanvases();
        Canvas.ForceUpdateCanvases();
    }
}
