using System;
using System.Collections.Generic;

namespace HR.Save{
// Plain data, no Unity/Mirror dependency - serialized with Newtonsoft
// (com.unity.nuget.newtonsoft-json, already a package dependency) by
// SaveSystem, both to the local save file and to Steam Cloud.
[Serializable]
public class SaveData
{
    public int SchemaVersion = 1;
    public int Currency = 0;
    // Namespaced so characters/skins/collectibles can share one list
    // without colliding, e.g. "character:4", "collectible:xyz". No
    // consumer other than SaveSystem needs to parse the prefix.
    public List<string> UnlockedIds = new List<string>();
}
}
