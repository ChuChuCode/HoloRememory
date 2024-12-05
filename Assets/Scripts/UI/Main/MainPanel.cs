using UnityEngine;

namespace HR.UI{
public class MainPanel : MonoBehaviour
{
    [SerializeField] GameObject Game_UI;
    [SerializeField] GameObject Setting_UI;
    [SerializeField] GameObject Credit_UI;
    void Start()
    {
        Game_UI.SetActive(false);
        Setting_UI.SetActive(false);
        Credit_UI.SetActive(false);
    }
    // Game Button
    public void Game()
    {
        Game_UI.SetActive(true);
        gameObject.SetActive(false);
    }
    // Setting Button
    public void Setting()
    {
        Setting_UI.SetActive(true);
        gameObject.SetActive(false);
    }
    // Credit Button
    public void Credit()
    {
        Credit_UI.SetActive(true);
        gameObject.SetActive(false);
    }
    // Exit Button
    public void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

}