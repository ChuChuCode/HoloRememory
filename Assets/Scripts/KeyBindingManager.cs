using UnityEngine;
using UnityEngine.InputSystem;

namespace HR.Global{
public enum BindableKey { MoveUp, MoveDown, MoveLeft, MoveRight, Bomb, Skill }

public enum RebindResult { Success, Cancelled, Duplicate }

// Rebinding only ever touches the "KeyBoard and Mouse" group bindings -
// Move's WASD composite for the 4 directions, plus Bomb/Skill's single
// keyboard binding. The Arrow-key composite and all Controller-group
// bindings are left alone as fixed fallback schemes.
public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance;
    const string PrefsKey = "KeyBindingOverrides";

    // The one rebind that can be in progress at a time - tracked so Reset
    // (or starting a different row's rebind) can cancel it instead of
    // leaving its action disabled and its row stuck on "Press key".
    InputActionRebindingExtensions.RebindingOperation activeRebind;

    // KeyBindPanel checks this before letting Esc close the whole panel -
    // while a rebind is waiting on a key press, Esc should only cancel that
    // rebind (via WithCancelingThrough below), not also back out of the panel.
    public bool IsRebinding => activeRebind != null;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Looks up the (action, bindingIndex) pair a BindableKey currently maps
    // to - resolved by composite/part name or control scheme group rather
    // than a hardcoded index, since those shift if the .inputactions asset
    // ever gets more bindings added.
    public (InputAction action, int bindingIndex) Resolve(BindableKey key)
    {
        var player = InputComponent.instance.playerInput.Player;
        switch (key)
        {
            case BindableKey.MoveUp: return (player.Move, FindCompositePart(player.Move, "WASD", "up"));
            case BindableKey.MoveDown: return (player.Move, FindCompositePart(player.Move, "WASD", "down"));
            case BindableKey.MoveLeft: return (player.Move, FindCompositePart(player.Move, "WASD", "left"));
            case BindableKey.MoveRight: return (player.Move, FindCompositePart(player.Move, "WASD", "right"));
            case BindableKey.Bomb: return (player.Bomb, FindKeyboardBinding(player.Bomb));
            case BindableKey.Skill: return (player.Skill, FindKeyboardBinding(player.Skill));
            default: return (null, -1);
        }
    }

    static int FindCompositePart(InputAction action, string compositeName, string partName)
    {
        var bindings = action.bindings;
        bool inComposite = false;
        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i].isComposite)
            {
                inComposite = bindings[i].name == compositeName;
                continue;
            }
            if (!bindings[i].isPartOfComposite)
            {
                inComposite = false;
                continue;
            }
            if (inComposite && bindings[i].name == partName) return i;
        }
        return -1;
    }

    static int FindKeyboardBinding(InputAction action)
    {
        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            if (!bindings[i].isComposite && !bindings[i].isPartOfComposite
                && bindings[i].groups != null && bindings[i].groups.Contains("KeyBoard and Mouse"))
            {
                return i;
            }
        }
        return -1;
    }

    // Starts an interactive rebind for the given key - waits for the next
    // keyboard press, rejects it if it would collide with any other
    // bindable key's current binding, and persists on success.
    public void StartRebind(BindableKey key, System.Action<RebindResult> onFinished)
    {
        // Only one interactive rebind can run at a time - cancel whatever
        // row was mid-rebind before starting this one.
        CancelActiveRebind();

        (InputAction action, int bindingIndex) = Resolve(key);
        if (action == null || bindingIndex < 0)
        {
            onFinished?.Invoke(RebindResult.Cancelled);
            return;
        }

        action.Disable();
        activeRebind = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            // Escape is reserved for cancel (both here and backing out of
            // panels) - never offer it as a new binding for anything else,
            // which would make it ambiguous with the cancel-through below.
            .WithControlsExcluding("<Keyboard>/escape")
            .WithCancelingThrough("<Keyboard>/escape")
            .WithExpectedControlType("Button")
            .OnMatchWaitForAnother(0.05f)
            .OnCancel(op =>
            {
                activeRebind = null;
                op.Dispose();
                action.Enable();
                onFinished?.Invoke(RebindResult.Cancelled);
            })
            .OnComplete(op =>
            {
                activeRebind = null;
                op.Dispose();
                action.Enable();

                if (IsDuplicate(key, action, bindingIndex))
                {
                    action.RemoveBindingOverride(bindingIndex);
                    onFinished?.Invoke(RebindResult.Duplicate);
                    return;
                }

                SaveOverrides();
                onFinished?.Invoke(RebindResult.Success);
            })
            .Start();
    }

    // Cancels whatever rebind is currently waiting for a key press, if any -
    // this runs that same action's OnCancel callback above, which
    // re-enables the action and reports RebindResult.Cancelled back to
    // whichever row started it, so its button resets normally.
    public void CancelActiveRebind()
    {
        activeRebind?.Cancel();
    }

    static bool IsDuplicate(BindableKey justChanged, InputAction changedAction, int changedBindingIndex)
    {
        string newPath = changedAction.bindings[changedBindingIndex].effectivePath;
        foreach (BindableKey other in System.Enum.GetValues(typeof(BindableKey)))
        {
            if (other == justChanged) continue;
            (InputAction action, int bindingIndex) = Instance.Resolve(other);
            if (action == null || bindingIndex < 0) continue;
            if (action.bindings[bindingIndex].effectivePath == newPath) return true;
        }
        return false;
    }

    // The raw control path segment (e.g. "e", "space", "upArrow") - used
    // both as the key into KeyIconSet and as input to GetKeyDisplayName's
    // text fallback. Kept separate from effectivePath's full "<Keyboard>/e"
    // form so callers never need to know that format.
    public static string GetControlName(InputAction action, int bindingIndex)
    {
        string path = action.bindings[bindingIndex].effectivePath;
        return path.Substring(path.LastIndexOf('/') + 1);
    }

    // GetBindingDisplayString() resolves the OS's current keyboard layout -
    // on a system with a CJK IME/layout active (e.g. Zhuyin on Taiwan
    // keyboards), that shows the IME's symbol for the key instead of the
    // plain English letter. Building the display name straight from the
    // binding path instead keeps it as a fixed "E"/"Space"/etc. regardless
    // of the player's OS keyboard layout or active IME. Only used as a
    // fallback when KeyIconSet has no icon for this control.
    public static string GetKeyDisplayName(string control)
    {
        switch (control)
        {
            case "space": return "Space";
            case "upArrow": return "Up Arrow";
            case "downArrow": return "Down Arrow";
            case "leftArrow": return "Left Arrow";
            case "rightArrow": return "Right Arrow";
            case "escape": return "Esc";
            case "leftShift": return "L Shift";
            case "rightShift": return "R Shift";
            case "leftCtrl": return "L Ctrl";
            case "rightCtrl": return "R Ctrl";
            case "leftAlt": return "L Alt";
            case "rightAlt": return "R Alt";
            case "tab": return "Tab";
            case "enter": return "Enter";
            case "backspace": return "Backspace";
            default:
                return control.Length <= 1 ? control.ToUpperInvariant() : char.ToUpperInvariant(control[0]) + control.Substring(1);
        }
    }

    public void SaveOverrides()
    {
        string json = InputComponent.instance.playerInput.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(PrefsKey, json);
        PlayerPrefs.Save();
    }

    public static void LoadOverrides(PlayerInputActions playerInput)
    {
        if (PlayerPrefs.HasKey(PrefsKey))
        {
            playerInput.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PrefsKey));
        }
    }

    public void ResetToDefault()
    {
        CancelActiveRebind();
        InputComponent.instance.playerInput.Player.Get().RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(PrefsKey);
        PlayerPrefs.Save();
    }
}
}
