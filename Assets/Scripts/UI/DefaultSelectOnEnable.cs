using UnityEngine;
using UnityEngine.EventSystems;

namespace HR.UI{
// Drop onto any panel and point FirstSelected at whichever Button/Selectable
// should be highlighted the moment it opens. The Input System's Navigate
// action only moves relative to EventSystem.current's existing
// currentSelectedGameObject - without this, arrow keys/gamepad d-pad do
// nothing on a panel until the player first clicks something with the mouse.
public class DefaultSelectOnEnable : MonoBehaviour
{
    [SerializeField] GameObject FirstSelected;

    void OnEnable()
    {
        if (FirstSelected == null || EventSystem.current == null) return;
        // Clearing first forces OnSelect to actually fire even if
        // FirstSelected was already the selected object from before this
        // panel was last closed.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(FirstSelected);
    }
}
}
