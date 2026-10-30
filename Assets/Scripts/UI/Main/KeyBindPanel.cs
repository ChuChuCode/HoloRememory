using UnityEngine;
using UnityEngine.InputSystem;
using HR.Global;

namespace HR.UI.Main{
// Opened from a button on SettingPanel. Spawns one KeyBindComponent row per
// BindableKey the first time it's enabled, then just refreshes them on
// every later open (in case bindings changed elsewhere, e.g. Reset).
public class KeyBindPanel : MonoBehaviour
{
    [SerializeField] KeyBindComponent rowPrefab;
    [SerializeField] Transform rowParent;
    [SerializeField] GameObject settingsPanel;

    static readonly (BindableKey key, string labelKey)[] Rows =
    {
        (BindableKey.MoveUp, "keybind_move_up"),
        (BindableKey.MoveDown, "keybind_move_down"),
        (BindableKey.MoveLeft, "keybind_move_left"),
        (BindableKey.MoveRight, "keybind_move_right"),
        (BindableKey.Bomb, "keybind_bomb"),
        (BindableKey.Skill, "keybind_skill"),
    };

    void OnEnable()
    {
        if (rowParent.childCount == 0)
        {
            foreach (var (key, labelKey) in Rows)
            {
                KeyBindComponent row = Instantiate(rowPrefab, rowParent);
                row.Setup(key, labelKey);
            }
        }
        else
        {
            foreach (Transform child in rowParent)
            {
                KeyBindComponent row = child.GetComponent<KeyBindComponent>();
                if (row != null) row.RefreshDisplay();
            }
        }
    }

    // Hooked up to a "Reset to Default" button in the Inspector.
    public void ResetToDefault()
    {
        KeyBindingManager.Instance.ResetToDefault();
        foreach (Transform child in rowParent)
        {
            KeyBindComponent row = child.GetComponent<KeyBindComponent>();
            if (row != null) row.RefreshDisplay();
        }
    }

    // Polled directly against Keyboard.current (rather than the Player
    // action map's Option action) since this panel can be open from the
    // Main Menu/Lobby before any player/character exists to have that map
    // enabled. Skipped while a row is actively waiting on a key press, so
    // that Esc only cancels the rebind (see KeyBindingManager.StartRebind's
    // WithCancelingThrough) instead of also closing the whole panel.
    void Update()
    {
        if (KeyBindingManager.Instance != null && KeyBindingManager.Instance.IsRebinding) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Back();
    }

    // Hooked up to this panel's Back button in the Inspector.
    public void Back()
    {
        gameObject.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}
}
