using Sirenix.OdinInspector;
using StarProject.Game;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapAreaItem : MonoBehaviour
{
    [LabelText("地图ID")]
    public int MapID = 0;
    [HideInInspector]
    public RectTransform MapRect = null;
    [HideInInspector]
    public Image MapImg = null;
    [HideInInspector]
    public Image MapBuildImg = null;
    [HideInInspector]
    public Text AreaText = null;
    [HideInInspector]
    public Text UnLockLevel = null;
    [HideInInspector]
    public Image Lock = null;
    [HideInInspector]
    public bool isUnlock = false;

    private readonly Color colorGray = Color.gray;
    private readonly Color colorWhite = Color.white;

    private Action<WorldMapAreaItem> m_Click;
    private MapCfgData m_SceneMapDataCell;

    private void Init()
    {
        MapRect = transform.GetComponent<RectTransform>();
        MapImg = transform.GetComponent<Image>();
        {
            var gob = transform.Find("icon");
            if (gob != null)
            {
                MapBuildImg = gob.GetComponent<Image>();
            }
        }
        AreaText = transform.Find("di/Layout/AreaText").GetComponent<Text>();
        {
            var gob = transform.Find("Lv");
            if (gob != null)
            {
                UnLockLevel = gob.GetComponent<Text>();
            }
        }
        Lock = transform.Find("di/Layout/Lock").GetComponent<Image>();

        Button btn = transform.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickAreaBtn);
    }

    private void OnClickAreaBtn()
    {
        if (!isUnlock)
        {
            return;
        }
        m_Click?.Invoke(this);
    }

    public void InitWorldMapAreaItem(Action<WorldMapAreaItem> cb)
    {
        Init();
        m_Click = cb;
        m_SceneMapDataCell = LocalDataManager.Instance.GetMapCfgData(MapID);
        if (m_SceneMapDataCell != null)
        {
            AreaText.text = m_SceneMapDataCell.MapName;
            if (UnLockLevel != null)
            {
                //UnLockLevel.text = $"{m_SceneMapDataCell.GetOpenRoleLv()}级";
                //UnLockLevel.text = string.Format(GameConfig.LocalStr["LvStr"], m_SceneMapDataCell.GetOpenRoleLv());
                UnLockLevel.text = string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), m_SceneMapDataCell.OpenRoleLv);
            }
            else
            {
                SGF.Debuger.LogWarning($"WorldMapAreaItem InitWorldMapAreaItem() MapID={MapID},name={transform.name}");
            }
        }
    }

    public void CheckLock()
    {
        if (MapID == 0 || MapID == 999)
        {
            return;
        }
        if (m_SceneMapDataCell != null)
        {
            isUnlock = GameManager.Instance.CheckMapIsOpen(MapID);

            if (Lock != null)
            {
                Lock.gameObject.SetActive(!isUnlock);
            }

            if (MapImg != null)
            {
                MapImg.color = isUnlock ? colorWhite : colorGray;
            }

            if (MapBuildImg != null)
            {
                MapBuildImg.color = isUnlock ? colorWhite : colorGray;
            }
        }
    }
}

