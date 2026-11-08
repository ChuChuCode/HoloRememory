using System;

namespace HR.Story{
public enum LevelEventTrigger
{
    // Fires once Network_Manager.Minion_List.Count <= TriggerValue.
    MinionsRemaining,
    // Fires once seconds-since-match-start >= TriggerValue (see
    // Network_Manager.IsEventTriggered for the exact calc).
    TimeElapsed,
    // Tutorial-style triggers - fire once the solo player has performed the
    // named action at least once (CharacterBase.TutorialMovedUp/Down/Left/
    // Right/PlacedBomb, CharacterSkillBase.TutorialUsedSkill). TriggerValue
    // is unused for these.
    MovedUp,
    MovedDown,
    MovedLeft,
    MovedRight,
    PlacedBomb,
    UsedSkill,
    PickedUpItem,
    // TODO: add more trigger types (score, position, etc.) as needed.
}

public enum LevelEventAction
{
    // Uses Dialogue - pauses input (same treatment as the intro dialogue),
    // plays it, resumes once dismissed.
    PlayDialogue,
    // Uses SpawnCount, via MinionSpawner.Instance.SpawnWave() - always
    // reuses the map's own MinionSpawner.minionPrefabs, not a per-event list.
    SpawnMinions,
    // Uses ControlName/PromptTextKey/PromptDuration - a lightweight "press
    // this key" hint (icon via the existing KeyIconSet + short text),
    // doesn't lock input, auto-hides after PromptDuration seconds regardless
    // of whether the player actually did it yet.
    ShowPrompt,
    // Ends the match as a win right now (calls Network_Manager.EndMatch()),
    // regardless of WinCondition/Minion_List - for a tutorial/non-combat
    // level that has no minions to clear at all (Minion_List.Count is
    // trivially 0 for those, so the existing ClearAllMinions win check
    // already reads this as a win with no further changes needed there).
    // Reuses the Dialogue field (optional) for a "congratulations" line
    // played BEFORE ending the match, instead of needing a second event on
    // the same trigger and relying on list-order to sequence them.
    CompleteLevel,
    // TODO: add more action types as needed.
}

// One scripted beat that can fire partway through a solo level - a trigger
// condition plus an action. Each entry fires at most once per match (see
// Network_Manager.MonitorLevelEvents) - a level that wants several waves
// just lists several entries with increasing TriggerValue, rather than this
// supporting a repeat/interval of its own.
[Serializable]
public class LevelEvent
{
    public LevelEventTrigger Trigger;
    public float TriggerValue;
    public LevelEventAction Action;
    public DialogueScript Dialogue;
    public int SpawnCount = 1;
    // Used when Action == ShowPrompt. ControlName matches KeyIconSet's own
    // lookup key (e.g. "upArrow", "w", "space") - same raw control-name
    // format KeyBindingManager.GetControlName produces, not a display string.
    public string ControlName;
    public string PromptTextKey;
    public float PromptDuration = 2.5f;
}
}
