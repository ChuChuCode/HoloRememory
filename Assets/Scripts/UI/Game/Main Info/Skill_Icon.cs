using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HR.Network.Game;
using HR.Object.Player;

namespace HR.UI{
public class Skill_Icon : MonoBehaviour
{
    [SerializeField] Image skill_Image;
    [SerializeField] Image skill_Image_Gray;
    [SerializeField] TMP_Text coolDown_TIme;
    [SerializeField] string skill;
    public Button Level_Up_Button;
    void Start()
    {
        skill = gameObject.name;
    }
    public void Set_Skill_Icon(Sprite Skill_Image)
    {
        skill_Image.sprite = Skill_Image;
        skill_Image_Gray.sprite = Skill_Image;
    }
    public void Set_CoolDown(float timer, float CD_time)
    {
        // Time.time - timer = time since last skill used
        float Usedtime = Time.time - timer;
        if ( Usedtime < CD_time)
        {
            // ShowTime Text
            if (!coolDown_TIme.gameObject.activeSelf) coolDown_TIme.gameObject.SetActive(true);
            // Get Decimal Digit 1 float
            // coolDown_TIme.text = ( Mathf.Round( (CD_time - (Time.time - timer) ) *10f ) / 10f).ToString();
            coolDown_TIme.text = Cal_CoolDown_Text(timer,CD_time);
            skill_Image.fillAmount = Usedtime / CD_time;
        }
        else
        {
            // Hide Time Text
            if (coolDown_TIme.gameObject.activeSelf) coolDown_TIme.gameObject.SetActive(false);
        }
    }
    string Cal_CoolDown_Text(float timer, float CD_time)
    {
        float Usedtime = Time.time - timer;
        float NeedTime = CD_time -Usedtime;
        float DecimalTime = Mathf.Round( NeedTime *10f ) / 10f;
        return DecimalTime.ToString();
    }
    // Skill UI Button UnityAction
    public void Use_Skill()
    {
        switch (skill)
        {
            case "Q":
                GameController.Instance.LocalPlayer.QKeyDown();
                return;
            case "W":
                GameController.Instance.LocalPlayer.WKeyDown();
                return;
            case "E":
                GameController.Instance.LocalPlayer.EKeyDown();
                return;
            case "R":
                GameController.Instance.LocalPlayer.RKeyDown();
                return;
        }
    }
    // Skill Up Button
    public void Skill_Up()
    {
        GameController.Instance.LocalPlayer.Skill_Up(skill);
    }
    public void Show_LevelUp(CharacterSkillBase skillComponent)
    {
        bool canShow = false;
        switch (skill)
        {
            case "Q":
                canShow = skillComponent.Q_Show_LevelUp();
                break;
            case "W":
                canShow = skillComponent.W_Show_LevelUp();
                break;
            case "E":
                canShow = skillComponent.E_Show_LevelUp();
                break;
            case "R":
                canShow = skillComponent.R_Show_LevelUp();
                break;
        }
        Level_Up_Button.gameObject.SetActive(canShow);
    }
}

}