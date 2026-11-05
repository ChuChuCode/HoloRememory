using System;
using UnityEngine;

namespace HR.Story{
// One beat of a DialogueScript. Deliberately per-line (not fixed on the
// script itself) so swapping narrator/speaker mid-scene, or reusing this
// template for a different character later, is just data - no code change.
[Serializable]
public class DialogueLine
{
    // Placeholder sprite for now - only Watame has any character art at all
    // right now, and even hers isn't portrait-proportioned. Swap in real
    // portraits per character/expression later without touching this class.
    public Sprite Portrait;
    // Localization keys into the "Story" table (Assets/Localization/CSV/Story.csv) -
    // same GetLocalizedString("Story", key) pattern already used for the "UI"
    // table elsewhere in the project. Empty SpeakerNameKey = no name shown
    // (e.g. narration).
    public string SpeakerNameKey;
    public string TextKey;
}
}
