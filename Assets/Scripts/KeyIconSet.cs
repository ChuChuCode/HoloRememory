using System.Collections.Generic;
using UnityEngine;

namespace HR.Global{
[System.Serializable]
public class KeyIconEntry
{
    // Raw control path segment as Unity's Input System reports it, e.g.
    // "e", "space", "escape", "upArrow", "leftShift" - matches
    // KeyBindingManager.GetControlName's output, not a display string.
    public string ControlName;
    public Sprite Icon;
}

// Kenney's keyboard key prompt sprites (Input Prompts/Keyboard & Mouse),
// looked up by raw control name so KeyBindComponent can show an icon
// instead of text when one exists, falling back to text otherwise. Loaded
// via Resources.Load("KeyIconSet") rather than wired per-prefab.
[CreateAssetMenu(fileName = "KeyIconSet", menuName = "HoloBomber/Key Icon Set")]
public class KeyIconSet : ScriptableObject
{
    public List<KeyIconEntry> Icons = new List<KeyIconEntry>();

    Dictionary<string, Sprite> lookup;

    public Sprite GetIcon(string controlName)
    {
        if (lookup == null)
        {
            lookup = new Dictionary<string, Sprite>();
            foreach (KeyIconEntry entry in Icons)
            {
                if (!string.IsNullOrEmpty(entry.ControlName) && entry.Icon != null)
                {
                    lookup[entry.ControlName] = entry.Icon;
                }
            }
        }
        return lookup.TryGetValue(controlName, out Sprite sprite) ? sprite : null;
    }
}
}
