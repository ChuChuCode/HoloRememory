using UnityEngine.InputSystem;
public interface ICharacter
{
    void OnRKeyInput(InputAction.CallbackContext context);
    void OnRightMouseClick(InputAction.CallbackContext context);
    void OnQKeyClick(InputAction.CallbackContext context);
    void Passive();
}
