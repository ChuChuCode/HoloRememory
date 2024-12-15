using UnityEngine;

namespace HR.UI{
public class CharacterInfoPanel : MonoBehaviour
{
    public static CharacterInfoPanel Instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        gameObject.SetActive(false);
    }
}

}