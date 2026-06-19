using System.Collections;
using System.Collections.Generic;
using SGF.Time;
using StarProjectDef;
using UnityEngine;

public class EventTimeItem : MonoBehaviour
{
    private bool isActive = false;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
    }

    public E_EventTimeType eventTimeType;

    private long endTime = 0;

    public void UpdateData(E_EventTimeType e_EventTimeType, int leastTime)
    {
        eventTimeType = e_EventTimeType;
        endTime = TimeUtils.ClientNowStampMilli + leastTime;
        isActive = true;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        isActive = false;
        endTime = 0;
        gameObject.SetActive(false);
    }

    public void OnUpdate()
    {
        if (!IsActive)
        {
            return;
        }

        if (TimeUtils.ClientNowStampMilli >= endTime)
        {
            Close();
        }

    }
}
