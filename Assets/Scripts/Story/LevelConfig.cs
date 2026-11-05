using UnityEngine;
using HR.Network;
using HR.Network.Select;

namespace HR.Story{
public enum LevelWinCondition
{
    ClearAllMinions,
    // TODO: add SurviveTimer / ReachExit / Escort / etc. here as future solo
    // modes are designed - Network_Manager.CheckSoloWin() needs a branch per
    // new value when they're built.
}

// Auto-loaded from Assets/Resources/Data/Level (same pattern as
// CharacterSelectComponent/MapConfig) - drop a new asset in that folder to
// add a level, no code or array editing needed.
[CreateAssetMenu(fileName = "LevelConfig", menuName = "HoloBomber/Story/Level Config")]
public class LevelConfig : ScriptableObject
{
    // Unlock key used against SaveSystem: $"level:{LevelId}".
    public string LevelId;
    // Localization key (Story table) shown on the Level Select screen.
    public string DisplayNameKey;
    // Reuses the existing PvP arena map system as-is - a solo level is just
    // an existing arena played alone against Minions.
    public MapConfig Map;
    // Which character this level is played as - per-level, not a single
    // fixed default, so different levels can star different characters
    // (e.g. once dialogue content exists for more than just Watame).
    public CharacterSelectComponent Character;
    // 0 = unlocked by default.
    public int UnlockCost;
    // "" = no prerequisite; otherwise must already be unlocked first.
    public string RequiredLevelId;
    public LevelWinCondition WinCondition = LevelWinCondition.ClearAllMinions;
    // Optional - null skips straight to gameplay, no dialogue screen.
    public DialogueScript IntroDialogue;
}
}
