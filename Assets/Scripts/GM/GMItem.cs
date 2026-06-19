using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using UnityEngine.UI;
using System;

public class GMItem : MonoBehaviour
{
    public Text descText;
    public GMDataCell gMDataCell;

    public Action<GMDataCell> ActionOnGmSelect;

    public void Init(GMDataCell dataCell)
    {
        gMDataCell = dataCell;

        if (descText != null)
        {
            descText.text = dataCell.Name;
        }
    }

    public void OnGmSelect()
    {
        ActionOnGmSelect.Invoke(gMDataCell);
    }
}
