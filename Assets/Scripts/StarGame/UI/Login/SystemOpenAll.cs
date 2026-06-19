using System.Collections;
using System.Collections.Generic;
using SGF.UI.Framework;
using StarProject.Service.SystemOpen;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

public class SystemOpenAll : MonoBehaviour
{

    public Text text;

    public JButton jButton;

    public bool IsOpenAll = false;

    private void Awake()
    {

        bool isShow = false;

#if (UNITY_EDITOR || STAR_DEV)
        isShow = true;
#endif

        if (!isShow)
        {
            gameObject.SetActive(false);
            return;
        }


        bool isExists = SaveManager.Instance.KeyExists(GameConfig.SYSTEM_OPEN_All, "");
        if (isExists)
        {
            var localCahce = SaveManager.Instance.Load<bool>(GameConfig.SYSTEM_OPEN_All, "");
            RefreshIsOpenAll(localCahce, false);
        }
        else
        {
            RefreshIsOpenAll(false, false);
        }

        jButton.OnClick += OnClick;
    }

    private void RefreshIsOpenAll(bool isOpenAlll, bool saveCache)
    {
        IsOpenAll = isOpenAlll;

        text.text = isOpenAlll ? "系统条件开放" : "系统全开";

        SystemOpenManager.Instance.SetOpenAll(IsOpenAll);
        SGF.Debuger.Log($"isOpenAll : {IsOpenAll}");
        if (saveCache)
        {
            SaveManager.Instance.Save<bool>(GameConfig.SYSTEM_OPEN_All, IsOpenAll, "");
        }
    }

    public void OnClick(GameObject go)
    {
        RefreshIsOpenAll(!IsOpenAll, true);
    }
}
