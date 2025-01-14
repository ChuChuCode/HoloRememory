using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HR.UI{
public class Spell_Icon : MonoBehaviour
{
    [SerializeField] Image skill_Image;
    [SerializeField] Image skill_Image_Gray;
    [SerializeField] TMP_Text coolDown_TIme;
    public Button Level_Up_Button;
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
        float NeedTime = CD_time - Usedtime;
        float DecimalTime = Mathf.Round( NeedTime *10f ) / 10f;
        return DecimalTime.ToString();
    }
}

}