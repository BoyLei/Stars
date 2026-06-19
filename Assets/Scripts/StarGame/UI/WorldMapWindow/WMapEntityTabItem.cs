using StarProject.Module;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WMapEntityTabItem : MonoBehaviour
{
    private Transform Content;
    private JButton JBtnGob;
    private Text Title;
    private GameObject UpArrow;
    private GameObject DownArrow;

    private GameObject wMapEntityPrefab;

    public MapShowTabType TabMapShowTabType;
    public MapNationType TabNationType;

    private List<WMapEntityItem> wMapEntityItems = new();
    // 小地图
    private List<MapShowEntity> mapShowEntities = new();
    private Action<MapShowEntity> m_ClickMapShowEntityItem = null;
    // 世界地图
    private List<MapShowArea> mapShowAreaEntities = new();
    private Action<MapShowArea> m_ClickMapShowAreaItem = null;

    private Action<WMapEntityTabItem> m_ClickTabItem = null;
    private bool m_IsCheck = false;

    private void Awake()
    {
        Content = transform.Find("List").transform;
        Title = transform.Find("Title/Text").GetComponent<Text>();
        UpArrow = transform.Find("Title/UpArrow").gameObject;
        DownArrow = transform.Find("Title/DownArrow").gameObject;
        JBtnGob = transform.Find("Title").GetComponent<JButton>();
        JBtnGob.OnClick = (go) =>
        {
            OnClickItem();
        };
    }

    public void InitMiniMap(MapShowTabType tabType, GameObject prefab, Action<WMapEntityTabItem> clickTab, Action<MapShowEntity> clickItem)
    {
        TabMapShowTabType = tabType;
        wMapEntityPrefab = prefab;
        m_ClickTabItem = clickTab;
        m_ClickMapShowEntityItem = clickItem;

        string title = "";
        switch (tabType)
        {
            case MapShowTabType.Common:
                title = LanguageManager.Instance.GetLanguageByKey("MapShowTabTypeCommon");
                break;
            case MapShowTabType.GamePlay:
                title = LanguageManager.Instance.GetLanguageByKey("MapShowTabTypeGamePlay");
                break;
            case MapShowTabType.Function:
                title = LanguageManager.Instance.GetLanguageByKey("MapShowTabTypeFun");
                break;
            case MapShowTabType.Transfer:
                title = LanguageManager.Instance.GetLanguageByKey("MapShowTabTypeTransfer");
                break;
            case MapShowTabType.Task:
                title = LanguageManager.Instance.GetLanguageByKey("TaskLabel");
                break;
            case MapShowTabType.Monster:
                title = LanguageManager.Instance.GetLanguageByKey("MapShowTabTypeMonster");
                break;
            default:
                break;
        }
        Title.text = title;
    }

    public void InitWorldMap(MapNationType tabType, GameObject prefab, Action<WMapEntityTabItem> clickTab, Action<MapShowArea> clickItem)
    {
        TabNationType = tabType;
        wMapEntityPrefab = prefab;
        m_ClickTabItem = clickTab;
        m_ClickMapShowAreaItem = clickItem;

        string title = "";
        switch (TabNationType)
        {
            case MapNationType.None:
                break;
            case MapNationType.FenDe:
                {
                    title = LanguageManager.Instance.GetLanguageByKey("MapNationTypeFenDe");
                }
                break;
            case MapNationType.HeiTieZhiSen:
                {
                    title = LanguageManager.Instance.GetLanguageByKey("MapNationTypeHeiTieZhiSen");
                }
                break;
            case MapNationType.SiBanSai:
                {
                    title = LanguageManager.Instance.GetLanguageByKey("MapNationTypeSiBanSai");
                }
                break;
            default:
                break;
        }
        Title.text = title;
    }

    public void SetMapShowEntitys(List<MapShowEntity> list)
    {
        mapShowEntities = list;

        int count = mapShowEntities.Count;
        gameObject.SetActive(count > 0);

        int childCount = wMapEntityItems.Count;
        int maxLength = Mathf.Max(childCount, count);
        for (int i = 0; i < maxLength; i++)
        {
            MapShowEntity mapShowEntity = count > i ? mapShowEntities[i] : null;
            if (childCount > i)
            {
                WMapEntityItem rawCell = wMapEntityItems[i];
                SetMapShowEntityItem(rawCell, mapShowEntity);
            }
            else
            {
                AddEntity(mapShowEntity);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

    public void SetMapShowEntitys(List<MapShowArea> list)
    {
        mapShowAreaEntities = list;

        int count = mapShowAreaEntities.Count;
        gameObject.SetActive(count > 0);

        int childCount = wMapEntityItems.Count;
        int maxLength = Mathf.Max(childCount, count);
        for (int i = 0; i < maxLength; i++)
        {
            MapShowArea mapShowEntity = count > i ? mapShowAreaEntities[i] : null;
            if (childCount > i)
            {
                WMapEntityItem rawCell = wMapEntityItems[i];
                SetMapShowEntityItem(rawCell, mapShowEntity);
            }
            else
            {
                AddEntity(mapShowEntity);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

    private void OnClickItem()
    {
        if (TabMapShowTabType == MapShowTabType.None && TabNationType == MapNationType.None)
        {
            return;
        }
        //m_ClickTabItem?.Invoke(this);
        CheckTab(!m_IsCheck);
    }

    private void Reset()
    {
        TabMapShowTabType = MapShowTabType.None;
        TabNationType = MapNationType.None;
        mapShowEntities.Clear();
        mapShowAreaEntities.Clear();
        m_ClickMapShowEntityItem = null;
        m_ClickMapShowAreaItem = null;
        m_ClickTabItem = null;
        CheckTab(false);
        HideItem();
    }

    public void Release()
    {
        Reset();
        gameObject.SetActive(false);
    }

    public void HideItem()
    {
        if (TabMapShowTabType == MapShowTabType.Common)
        {
            // 常用的不关闭
            return;
        }
        ClearWMapEntityItems();
        gameObject.SetActive(false);
    }

    private void AddEntity(MapShowEntity data)
    {
        var gob = GameObject.Instantiate<GameObject>(wMapEntityPrefab);
        gob.transform.SetParent(Content);
        gob.transform.localPosition = Vector3.zero;
        gob.transform.localScale = Vector3.one;
        WMapEntityItem sp = gob.GetComponent<WMapEntityItem>();
        SetMapShowEntityItem(sp, data);
        wMapEntityItems.Add(sp);
    }

    private void AddEntity(MapShowArea data)
    {
        var gob = GameObject.Instantiate<GameObject>(wMapEntityPrefab);
        gob.transform.SetParent(Content);
        gob.transform.localPosition = Vector3.zero;
        gob.transform.localScale = Vector3.one;
        WMapEntityItem sp = gob.GetComponent<WMapEntityItem>();
        SetMapShowEntityItem(sp, data);
        wMapEntityItems.Add(sp);
    }

    private void SetMapShowEntityItem(WMapEntityItem sp, MapShowEntity data)
    {
        sp.SetMapShowEntity(data, m_ClickMapShowEntityItem);
    }

    private void SetMapShowEntityItem(WMapEntityItem sp, MapShowArea data)
    {
        sp.SetMapShowEntity(data, m_ClickMapShowAreaItem);
    }

    public void ClearWMapEntityItems()
    {
        for (int i = 0; i < wMapEntityItems.Count; i++)
        {
            var child = wMapEntityItems[i];
            if (child != null)
            {
                child.Release();
            }
        }
        CheckTab(false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

    public void CheckTab(bool isCheck)
    {
        if (m_IsCheck == isCheck)
        {
            return;
        }
        m_IsCheck = isCheck;
        UpArrow.SetActive(m_IsCheck);
        DownArrow.SetActive(!m_IsCheck);
        Content.gameObject.SetActive(m_IsCheck);
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.transform.GetComponent<RectTransform>());
    }

}
