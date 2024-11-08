using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static InputSystem instance ;
    public PlayerInputActions playerInput;
    
    void Awake()
    {
        playerInput = new PlayerInputActions();
        if (instance == null) instance = this;
    }
}
