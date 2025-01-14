using UnityEngine;
using TMPro;

namespace HR.UI{
public class MessageComponent : MonoBehaviour
{
    [SerializeField] TMP_Text Message_Text;
    string userName;
    string message;
    public void SetString(string userName, string message)
    {
        Message_Text.text = $"{userName} : {message}";
    }
}

}