using Sirenix.OdinInspector;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapGveItem : MonoBehaviour
{


    public Image Icon;

    public Text Title ;

    public Text Time;

    public Text Name;

    public void Show(bool b)
    {
        gameObject.SetActive(b);
    }

    public void UpdateTime(long leftTime)
    {
        if(Time.gameObject.activeSelf)
        {
            if (leftTime > 0)
            {
                Time.text = SGF.Time.TimeUtils.GetTimeStringV2("%mm:%ss", leftTime);
            }
            else
            {
                Time.text = "";
            }
        }
    }

    public void SetBossInfo(string bossName,Sprite sprite)
    {
        Show(true);
        //Title.text = GameConfig.LocalStr["Defending"];
        Title.text = LanguageManager.Instance.GetLanguageByKey("Defending");
        Name.text = bossName;
        Icon.sprite = sprite;
        Time.gameObject.SetActive(false);
        Name.gameObject.SetActive(true);
    }

    public void SetNullBoss(Sprite sprite)
    {
        Show(true);
        //Title.text = GameConfig.LocalStr["Countdown"];
        Title.text = LanguageManager.Instance.GetLanguageByKey("Countdown");
        Name.gameObject.SetActive(false);
        Time.gameObject.SetActive(true);
        Icon.sprite = sprite;
    }

}

