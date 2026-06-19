using System.Collections;
using System.Collections.Generic;
using StarProjectDef;
using UnityEngine;

public class EventTimeWidget : MonoBehaviour
{
    public EventTimeItem eventTimeItemCopy;

    private Dictionary<E_EventTimeType, EventTimeItem> type2EventItem = new();

    /// <summary>
    /// 理论上 只有一个 相同类型的 活动倒计时预告
    /// </summary>
    /// <param name="e_EventTimeType"></param>
    /// <param name="leastTime"></param>
    public void AddEventTime(E_EventTimeType e_EventTimeType, int leastTime)
    {
        if (!type2EventItem.TryGetValue(e_EventTimeType, out var eventTypeItem))
        {
            eventTypeItem = GetEventTime();
        }

        eventTypeItem.UpdateData(e_EventTimeType, leastTime);
    }

    private EventTimeItem GetEventTime()
    {
        EventTimeItem eventTimeItem = null;

        // 如果有已经关闭的 活动倒计时预告, 那就重用旧的, 同时从父节点 移除
        foreach (var item in type2EventItem)
        {
            if (!item.Value.IsActive)
            {
                eventTimeItem = item.Value;
                type2EventItem.Remove(item.Key);
                eventTimeItem.transform.SetParent(null);
                break;
            }
        }

        if (eventTimeItem == null)
        {
            eventTimeItem = GameObject.Instantiate(eventTimeItemCopy.gameObject).GetComponent<EventTimeItem>();

        }
        eventTimeItem.transform.SetParent(transform);
        return eventTimeItem;
    }

    public void StopEventTime(E_EventTimeType e_EventTimeType)
    {
        if (type2EventItem.TryGetValue(e_EventTimeType, out var eventTypeItem))
        {
            eventTypeItem.Close();
        }
    }

    public void Update()
    {
        foreach (var item in type2EventItem)
        {
            if (item.Value.IsActive)
            {
                item.Value.OnUpdate();
            }
        }

    }


}
