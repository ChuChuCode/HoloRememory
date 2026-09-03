using UnityEngine;

namespace HR.Network{
// Picks which NetworkManager GameObject gets to wake up, based on whether
// Steam is actually available - this has to happen BEFORE either
// NetworkManager's own Awake() runs. Mirror's NetworkManager.Awake()
// destroys the whole GameObject of any NetworkManager that wakes up after
// another one already claimed NetworkManager.singleton, so flipping
// SetActive() on the loser after the fact doesn't work - only disabling it
// before its Awake() ever fires does.
//
// SteamManager.Initialized is safe to read this early: its getter lazily
// creates/initializes a SteamManager the first time anything touches it if
// none exists yet, synchronously, before returning.
[DefaultExecutionOrder(-100)]
public class NetworkTransportSelector : MonoBehaviour
{
    [SerializeField] GameObject steamNetworkManager;
    [SerializeField] GameObject kcpNetworkManager;

    void Awake()
    {
        bool useSteam = SteamManager.Initialized;
        if (steamNetworkManager != null) steamNetworkManager.SetActive(useSteam);
        if (kcpNetworkManager != null) kcpNetworkManager.SetActive(!useSteam);
    }
}
}
