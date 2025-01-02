using HR.Object.Player;
using UnityEngine;

namespace HR.Object.Equipment{
public class ItemUse_UI : MonoBehaviour
{
    public CharacterBase characterBase;
    [SerializeField] GameObject ItemUse_Prfab;
    [SerializeField] int range;


    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(characterBase.transform.position, characterBase.mouseProject);
        if (distance > range)
        {
            Vector3 direction = (characterBase.mouseProject - characterBase.transform.position).normalized;
            transform.position = characterBase.transform.position + direction * range;
            return;
        }
        transform.position = characterBase.mouseProject;
    }
    public void Spawn_ItemUse()
    {
        Instantiate(ItemUse_Prfab,transform.position + new Vector3(0,5,0),Quaternion.identity);
    }
}

}