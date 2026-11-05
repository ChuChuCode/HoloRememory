using System.Collections.Generic;
using UnityEngine;

namespace HR.Story{
// A linear sequence of DialogueLines - referenced directly by
// LevelConfig.IntroDialogue (not auto-loaded from Resources like
// CharacterSelectComponent/MapConfig, since it's only ever reached through
// that reference). No branching/choices yet - see LevelConfig's own TODO
// for future expansion.
[CreateAssetMenu(fileName = "DialogueScript", menuName = "HoloBomber/Story/Dialogue Script")]
public class DialogueScript : ScriptableObject
{
    public List<DialogueLine> Lines = new List<DialogueLine>();
}
}
