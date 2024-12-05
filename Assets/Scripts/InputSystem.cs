using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        if (instance == null) instance = this;
    }
    void Start()
    {
        #if UNITY_EDITOR
            Cursor.SetCursor(PlayerSettings.defaultCursor, Vector2.zero, CursorMode.ForceSoftware);
        #endif
    }
}

}