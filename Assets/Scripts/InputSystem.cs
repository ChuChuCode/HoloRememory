using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace HR.Global{
public class InputComponent : MonoBehaviour
{
    public static InputComponent instance ;
    public PlayerInputActions playerInput;
    
    void Awake()
    {
        DontDestroyOnLoad(this);
        playerInput = new PlayerInputActions();
        KeyBindingManager.LoadOverrides(playerInput);
        // Unlike Player (only enabled while a character exists - see
        // CharacterBase.OnEnable/OnDisable), UI navigation needs to work
        // from the Main Menu onward, before any character is ever spawned.
        playerInput.UI.Enable();
        if (instance == null) instance = this;
        if (instance != this) Destroy(gameObject);
    }
    void Start()
    {
        #if UNITY_EDITOR
            Cursor.SetCursor(PlayerSettings.defaultCursor, Vector2.zero, CursorMode.ForceSoftware);
        #endif
    }
    public void Reset()
    {
        playerInput.Dispose();
        playerInput = new PlayerInputActions();
        KeyBindingManager.LoadOverrides(playerInput);
        playerInput.Enable();
    }
}

}