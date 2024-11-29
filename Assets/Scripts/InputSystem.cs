using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public static InputSystem instance ;
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
