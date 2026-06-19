using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GMTagItem : MonoBehaviour
{
    public Transform content;

    public Text tagText;

    public Toggle toggle;


    public GameObject gmItemCopy;

    private List<GMDataCell> gMDataCells;

    public Action<GMDataCell> ActionOnSelectGM;

    private Dictionary<int, string> tagTextDesc = new Dictionary<int, string>();

    private void Awake()
    {
        toggle.onValueChanged.AddListener(onValueChanged);
        toggle.isOn = false;

        tagTextDesc.Add(1, "战斗");
        tagTextDesc.Add(2, "关卡");
        tagTextDesc.Add(3, "系统");
        tagTextDesc.Add(4, "数值");
        tagTextDesc.Add(5, "活动");
        tagTextDesc.Add(6, "显示");
        tagTextDesc.Add(7, "自输入");
    }

    public void InitDatas(int tag, List<GMDataCell> datas)
    {
        gMDataCells = datas;

        if (tagTextDesc.ContainsKey(tag))
        {
            tagText.text = tagTextDesc[tag];
        }
    }

    private void onValueChanged(bool result)
    {
        content.gameObject.SetActive(result);
        if (result && content.childCount == 0)
        {
            InitGMItems();
        }
        if (result && content.childCount > 0)
        {
            content.GetChild(0).GetComponent<GMItem>().OnGmSelect();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponentInParent<RectTransform>());
    }

    private void InitGMItems()
    {
        for (int i = 0; i < gMDataCells.Count; i++)
        {
            GameObject gmItemGob = Instantiate(gmItemCopy, content);
            gmItemGob.SetActive(true);

            GMItem gmItem = gmItemGob.GetComponent<GMItem>();

            gmItem.ActionOnGmSelect = ActionOnSelectGM;
            gmItem.Init(gMDataCells[i]);
        }
    }
}
