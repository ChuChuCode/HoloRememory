using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public static InputSystem instance {get; private set;}
    public PlayerInputActions playerInput;
    
    void Awake()
    {
        instance = this;
        playerInput = new PlayerInputActions();
    }
}
