using UnityEngine.InputSystem;
public interface ICharacter
{
    void OnRKeyInput(InputAction.CallbackContext context);
    void OnRightMouseClick(InputAction.CallbackContext context);
    void OnQKeyClick(InputAction.CallbackContext context);
    void OnYKeyClick(InputAction.CallbackContext context);
    void OnSpaceKeyClick(InputAction.CallbackContext context);
    void Passive();
}
