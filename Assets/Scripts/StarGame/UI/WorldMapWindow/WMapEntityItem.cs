using StarProject.Game;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WMapEntityItem : MonoBehaviour
{
    private JButton jBtnGob;
    private Image Icon;
    private Text Name;
    private Text Title;

    private MapShowEntity mapShowEntity = null;
    private Action<MapShowEntity> m_MapShowEntityClick = null;

    private MapShowArea mapShowArea = null;
    private Action<MapShowArea> m_MapShowAreaClick = null;

    private void Awake()
    {
        Icon = transform.Find("Icon").GetComponent<Image>();
        Name = transform.Find("Name").GetComponent<Text>();
        Title = transform.Find("Title").GetComponent<Text>();
        jBtnGob = transform.GetComponent<JButton>();
        jBtnGob.OnClick = (go) =>
        {
            OnClickItem();
        };
    }

    public void SetMapShowEntity(MapShowEntity data, Action<MapShowEntity> click)
    {
        mapShowEntity = data;
        m_MapShowEntityClick = click;
        if (mapShowEntity == null)
        {
            Release();
            return;
        }

        Name.text = data.Name;
        Title.text = data.Title;
        bool isShow = true;
        switch (mapShowEntity.TabType)
        {
            case MapShowTabType.None:
                break;
            case MapShowTabType.Common:
            case MapShowTabType.Function:
            case MapShowTabType.GamePlay:
                {
                    NpcDataCell npc = LocalDataManager.Instance.GetNPCDataCell((uint)mapShowEntity.ConfID);
                    if (npc != null && npc.MapLogo != null && npc.MapLogo != string.Empty && npc.MapLogo != "0")
                    {
                        isShow = BusinessManager.Instance.GetNPCVisiable(mapShowEntity.ConfID);
                        string iconName = $"{npc.MapLogo}";
                        Action<Sprite> cb = (Sprite sp) =>
                        {
                            if (sp != null)
                            {
                                Icon.sprite = sp;
                            }
                        };
                        AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, cb);
                    }
                }
                break;
            case MapShowTabType.Transfer:
                break;
            case MapShowTabType.Task:
                {
                    Action<Sprite> cb = (Sprite sp) =>
                    {
                        if (sp != null)
                        {
                            Icon.sprite = sp;
                        }
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, mapShowEntity.IconPath, cb);
                }
                break;
            case MapShowTabType.Monster:
                {
                    string iconName = "Hud_Map_monster";
                    Action<Sprite> cb = (Sprite sp) =>
                    {
                        if (sp != null)
                        {
                            Icon.sprite = sp;
                        }
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathHud, iconName, cb);
                }
                break;
            default:
                break;
        }

        gameObject.SetActive(isShow);
    }

    public void SetMapShowEntity(MapShowArea data, Action<MapShowArea> click)
    {
        mapShowArea = data;
        m_MapShowAreaClick = click;
        if (mapShowArea == null)
        {
            Release();
            return;
        }

        Name.text = mapShowArea.MapName;
        bool isOpen = GameManager.Instance.CheckMapIsOpen(mapShowArea.MapID);
        gameObject.SetActive(isOpen);
    }

    private void OnClickItem()
    {
        if (mapShowEntity == null && mapShowArea == null)
        {
            return;
        }
        if (mapShowEntity != null)
        {
            m_MapShowEntityClick?.Invoke(mapShowEntity);
        }
        if (mapShowArea != null)
        {
            m_MapShowAreaClick?.Invoke(mapShowArea);
        }
    }

    private void Reset()
    {
        mapShowEntity = null;
        m_MapShowEntityClick = null;
        mapShowArea = null;
        m_MapShowAreaClick = null;
    }

    public void Release()
    {
        Reset();
        gameObject.SetActive(false);
    }

}
