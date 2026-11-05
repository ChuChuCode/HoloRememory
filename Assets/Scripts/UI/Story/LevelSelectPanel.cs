using System.Collections.Generic;
using UnityEngine;
using HR.Story;
using HR.Network;

namespace HR.UI{
// Sibling panel to ModeSelectPanel/LobbyListManager in Main_Scene -
// ModeSelectPanel.Solo() opens this instead of the old SoloUI placeholder.
public class LevelSelectPanel : MonoBehaviour
{
    [SerializeField] GameObject previousUI;
    [SerializeField] Transform content;
    [SerializeField] LevelSelectItem itemPrefab;

    readonly List<LevelSelectItem> spawnedItems = new List<LevelSelectItem>();

    // Called by ModeSelectPanel.Solo() and by PressAnyKey-style return flows.
    public void Open()
    {
        gameObject.SetActive(true);
        if (previousUI != null) previousUI.SetActive(false);
        Populate();
    }

    // Back Button.
    public void Back()
    {
        gameObject.SetActive(false);
        if (previousUI != null) previousUI.SetActive(true);
    }

    void Populate()
    {
        foreach (LevelSelectItem item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();

        if (content == null || itemPrefab == null) return;
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return;

        foreach (LevelConfig level in manager.levelConfigList)
        {
            if (level == null) continue;
            LevelSelectItem item = Instantiate(itemPrefab, content);
            item.SetLevel(level, this);
            spawnedItems.Add(item);
        }
    }

    // Called by LevelSelectItem.Select() once a level is confirmed unlocked.
    // Any intro dialogue plays AFTER arriving in the level, not here - see
    // Network_Manager.WaitForAllPlayersLoadedThenStartMatch.
    public void SelectLevel(LevelConfig level)
    {
        if (level == null) return;
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return;
        // Covers the brief Lobby_Scene pass-through StartHost() causes -
        // Network_Manager.WaitForAllPlayersLoadedThenStartMatch() calls
        // TransitionScene.FadeOut() once the arena (and any intro dialogue)
        // is ready to reveal.
        TransitionScene.Instance?.FadeIn();
        manager.StartSoloLevel(level);
    }
}
}
