using System;
using HR.Network.Select;

namespace HR.Story{
// One beat of a DialogueScript. Deliberately per-line (not fixed on the
// script itself) so swapping narrator/speaker mid-scene, or reusing this
// template for a different character later, is just data - no code change.
[Serializable]
public class DialogueLine
{
    // Portrait is resolved as Character.GetPortrait(Expression) at playback
    // time (see DialogueUI.ShowLine) rather than wiring a raw Sprite here -
    // the same art asset gets reused across every line/script that needs
    // it instead of being re-dragged into each one, and swapping art later
    // only means editing CharacterSelectComponent.Portraits in one place.
    // Null Character = no portrait shown (e.g. narration).
    public CharacterSelectComponent Character;
    // Name into Character.Portraits (e.g. "Normal", "Happy") - "" or no
    // match falls back to that character's first portrait entry.
    public string Expression;
    // Localization keys into the "Story" table (Assets/Localization/CSV/Story.csv) -
    // same GetLocalizedString("Story", key) pattern already used for the "UI"
    // table elsewhere in the project. Empty SpeakerNameKey = no name shown
    // (e.g. narration).
    public string SpeakerNameKey;
    public string TextKey;
}
}
