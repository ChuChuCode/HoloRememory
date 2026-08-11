using UnityEngine;
using UnityEngine.InputSystem;

namespace HR.UI{
// Lives on Title - Title is active exactly as long as this should be
// listening, and stops running the moment ModeSelectPanel.Open() disables it.
public class PressAnyKey : MonoBehaviour
{
    [SerializeField] ModeSelectPanel modeSelectPanel;

    // Title is always active at Main_Scene startup, unlike Mode Select UI -
    // this is the only reliable place to catch the "came back from Lobby"
    // flag on scene load.
    void Start()
    {
        modeSelectPanel.CheckReturnToGameUI();
    }
    void Update()
    {
        bool pressed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.allControls.Count > 0 && Gamepad.current[0].IsPressed());

        if (pressed) modeSelectPanel.Open();
    }
}

}
