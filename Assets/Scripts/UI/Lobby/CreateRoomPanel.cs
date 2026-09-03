using UnityEngine;

namespace HR.Network.Lobby{
// Lives on the Create Room panel itself. SteamLobby.Start() runs once,
// early, quite possibly before this panel has ever been SetActive(true) -
// populating the dropdowns/default room name only there can silently do
// nothing. OnEnable fires every time this panel is opened instead, so the
// options/default are always current.
public class CreateRoomPanel : MonoBehaviour
{
    [SerializeField] SteamLobby steamLobby;

    void OnEnable()
    {
        if (steamLobby == null) return;
        steamLobby.RefreshCreateRoomDefaults();
    }
}
}
