using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Network.Game;

public class EquipmentSlot : MonoBehaviour
{
    enum SlotIndex
    {
        _1 = 0,
        _2 = 1,
        _3 = 2,
        _4 = 3,
        _5 = 4,
        _6 = 5
    }
    [SerializeField] SlotIndex slotIndex;
    [SerializeField] Image Equipment_Image;
    [SerializeField] Image Equipment_Image_Gray;
    [SerializeField] TMP_Text coolDown_TIme;
    public void UpdateImage(Sprite image)
    {
        Equipment_Image.sprite = image;
        Equipment_Image_Gray.sprite = image;
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
            Equipment_Image.fillAmount = Usedtime / CD_time;
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
    public void Use_Skill()
    {
        GameController.Instance.LocalPlayer.UseEquipmentKeyDown( (int)slotIndex );
    }
}
