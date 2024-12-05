using UnityEngine;

namespace HR.UI{
public class CreditPanel : MonoBehaviour
{
    [SerializeField] GameObject Main_UI;
    // Back Button
    public void Back()
    {
        Main_UI.SetActive(true);
        gameObject.SetActive(false);
    }
    // Social Media Button
    public void SocialButton(string url)
    {
        Application.OpenURL(url);
    }
}

}