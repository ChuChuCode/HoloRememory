using UnityEngine;
using HR.Object.Player;

namespace HR.UI{

[CreateAssetMenu(fileName = "Potion",menuName = "HoloRememory/Game/Potion",order = 1)]
public class Potion_ScriptableObject : Equipment_ScriptableObject
{
    public int gain_health;
    public override void ItemKeyDown(CharacterBase characterBase)
    {
        characterBase.HealthHeal(gain_health);
    }
}

}