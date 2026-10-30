using UnityEngine;
using HR.Map;

namespace HR.Network{
// Auto-loaded from Assets/Resources/Data/Map (same pattern as
// CharacterSelectComponent) - drop a new asset in that folder to add a map,
// no code or array editing needed.
[CreateAssetMenu(fileName = "MapConfig", menuName = "HoloBomber/Map Config")]
public class MapConfig : ScriptableObject
{
    // Stable key - this is what GameSettings.MapName actually holds now
    // (used to be a literal scene name; see GameSettings.MapName's own
    // comment). Network_Manager.ChangeScene looks a MapConfig up by this.
    public string MapId;
    // Shown on the map select UI.
    public string DisplayName;
    // Not used yet - reserved for a future thumbnail/card-style map picker.
    public Sprite Thumbnail;

    // Legacy path: a map fully baked into its own Scene. Only read when
    // MapFile below is null - Network_Manager.ChangeScene loads this scene
    // directly instead of the shared Game_Map_Runtime scene.
    public string SceneName;

    // New path: a text map (MapTextParser format) loaded at runtime into the
    // shared Game_Map_Runtime scene by MapLoader. Takes priority over
    // SceneName when set.
    public TextAsset MapFile;
    // Optional - null uses MapLoader's own default palette.
    public MapBlockPalette PaletteOverride;
}
}
