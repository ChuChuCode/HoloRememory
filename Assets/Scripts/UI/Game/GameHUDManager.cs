using UnityEngine;
using HR.Network;

namespace HR.UI{
// Drives the 8-slot top player bar during a match. Event-driven, not
// polled - CharacterBase calls RefreshAll() whenever something this bar
// actually shows changes (a player is added to Player_List, or their
// PlayerName/lives/isDead changes), instead of re-reading everyone every
// frame for 8 mostly-static slots.
public class GameHUDManager : MonoBehaviour
{
    public static GameHUDManager Instance;

    [SerializeField] GameHUDPlayerSlot[] slots;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void RefreshAll()
    {
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            if (i < manager.Player_List.Count && manager.Player_List[i] != null)
            {
                slots[i].SetPlayer(manager.Player_List[i]);
            }
            else
            {
                slots[i].SetEmpty();
            }
        }
    }
}
}
