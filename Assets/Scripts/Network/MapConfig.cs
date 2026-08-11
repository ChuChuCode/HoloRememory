using UnityEngine;

namespace HR.Network{
// Auto-loaded from Assets/Resources/Data/Map (same pattern as
// CharacterSelectComponent) - drop a new asset in that folder to add a map,
// no code or array editing needed.
[CreateAssetMenu(fileName = "MapConfig", menuName = "HoloBomber/Map Config")]
public class MapConfig : ScriptableObject
{
    // Actual Unity scene name - this is what GameSettings.MapName/
    // ServerChangeScene actually use.
    public string SceneName;
    // Shown on the map select UI.
    public string DisplayName;
    // Not used yet - reserved for a future thumbnail/card-style map picker.
    public Sprite Thumbnail;
}
}
