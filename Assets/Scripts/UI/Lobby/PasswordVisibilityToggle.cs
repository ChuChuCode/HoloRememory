using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace HR.Network.Lobby{
// Press-and-hold button: shows the input field's raw text while held,
// masks it again the instant it's released - not a toggle. Attach this
// directly to the button GameObject (it needs a Graphic + Raycast Target,
// which a Button already has) and assign the password field it controls.
// Reusable for both the Create Room screen's password field and the Join
// password prompt's - just add one instance per field.
public class PasswordVisibilityToggle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] TMP_InputField passwordInput;

    public void OnPointerDown(PointerEventData eventData)
    {
        SetVisible(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        SetVisible(false);
    }
    void SetVisible(bool visible)
    {
        if (passwordInput == null) return;
        passwordInput.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        // contentType alone doesn't repaint the already-rendered text -
        // this forces it to re-mask/re-reveal immediately.
        passwordInput.ForceLabelUpdate();
    }
}
}
