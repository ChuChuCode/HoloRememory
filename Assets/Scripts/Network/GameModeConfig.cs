using UnityEngine;

namespace HR.Network{
[CreateAssetMenu(fileName = "GameModeConfig", menuName = "HoloBomber/Game Mode Config")]
public class GameModeConfig : ScriptableObject
{
    public GameMode mode;
    // Shown on the mode select button - separate from the enum name so it
    // can read however you want (e.g. localized) without touching code.
    public string DisplayName;

    [Tooltip("On: force everyone to maxHealth (OneLife/MultiLife = 1, instant death). " +
             "Off: each character keeps its own prefab maxHealth (HealthBar mode - tankier/frailer characters still differ).")]
    public bool overrideHealth = true;
    public int maxHealth = 1;

    [Tooltip("How many times a character can die before being permanently eliminated. " +
             "OneLife = 1, MultiLife = 5 (or however many), HealthBar = 1 (the bar itself is the multi-hit buffer).")]
    public int lives = 1;
}
}
