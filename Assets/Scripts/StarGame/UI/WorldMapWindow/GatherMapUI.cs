using System.Collections;
using System.Collections.Generic;
using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Utlis;
using StarProject;
using StarProject.Game;
using StarProject.Game.Map;
using StarProject.Service.AtlasManager;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine.UI;
using UnityEngine;
using StarProject.Service.Language;

public class GatherMapUI
{
    public int MapID { get; private set; }
    public GameObject Root { get; private set; }

    private GameObject mTemplete;
    private Transform mParent;

    private JButton mCloseBtn;

    //池
    private List<GameObject> Pools = new List<GameObject>(5);
    private Dictionary<int, GatherMapUIItem> m_Items = new Dictionary<int, GatherMapUIItem>();
    private System.Action<int,int> SelectCallBack = null;
    private System.Action OnCloseCallBack = null;

    public GatherMapUI(GameObject go,System.Action cb)
    {
        Root = go;
        OnCloseCallBack = cb;
        var transform = Root.transform;
        mCloseBtn = transform.Find("Close").GetComponent<JButton>();
        mTemplete = transform.Find("Root/ItemGrid").gameObject;
        mParent =  transform.Find("Root/Scroll View/Viewport/Content");
        mCloseBtn.OnClick += OnCloseHandler;
    }

    private void OnCloseHandler(GameObject go)
    {
        OnClose();
    }

    public void OnOpen(int mapID, System.Action<int,int> cb, Dictionary<long /*唯一ID*/, MineJsonData> Mines)
    {
        MapID = mapID;
        SelectCallBack = cb;
        //LastSelect = 0;
        Dictionary<long, (int,int)> mines = new Dictionary<long, (int,int)>();
        foreach (var item in Mines)
        {
            var mine = LocalDataManager.Instance.GetInteractDataCell(item.Value.MineID);
            if (mine != null)
            {
                if (mine.GetMineID() > 0)
                {
                    if (!mines.ContainsKey(mine.GetMineID()))
                    {
                        var gather = LocalDataManager.Instance.GetLifeSkillMineDataCell((int)mine.GetMineID());
                        if (gather != null)
                        {
                            mines.Add(mine.GetMineID(), (gather.GetSkillID(),gather.GetID()));
                        }
                    }
                }
            }
        }

        CleanItems();

        List<(int, int, int)> list = new();
        foreach (var mine in mines)
        {
            list.Add(((int)mine.Key, mine.Value.Item1, mine.Value.Item2));
        }
        list.Sort((t1, t2) =>
        {
            if (t1.Item2 < t2.Item2)
            {
                return 1;
            }
            else  if (t1.Item2 == t2.Item2)
            {
                if (t1.Item3 < t2.Item3)
                {
                    return 1;
                }
            }
            return 0;
        });
        foreach (var mine in list)
        {
            CreateGatherMapUIItem(mine.Item1);
        }
        Root.SetActive(true);
        OnOpenSelect(GatherMapSelect.GetMapSelect(MapID));
    }

    private void CreateGatherMapUIItem(int mineID)
    {
        GameObject go = null;
        if (Pools != null && Pools.Count > 0)
        {
            go = Pools[0];
            Pools.RemoveAt(0);
        }
        else
        {
            go = GameObject.Instantiate(mTemplete);
            go.transform.SetParent(mParent);
 
        }
        go.transform.localScale = Vector3.one;
        var item = new GatherMapUIItem(go);
        item.SetData(MapID,mineID, OnSelect);
        m_Items.Add(mineID,item);
    }

    private void CleanItems()
    {
        if (m_Items == null)
        {
            m_Items = new Dictionary<int, GatherMapUIItem>();
        }

        if (m_Items != null && m_Items.Count > 0)
        {
            foreach (var item in m_Items)
            {
                item.Value.OnRelease();
                Pools.Add(item.Value.Root);
            }
        }

        m_Items.Clear();
    }

    public void OnOpenSelect(int mineid)
    {
        SelectCallBack?.Invoke(MapID,mineid);
        foreach (var item in m_Items)
        {
            item.Value.OnSelectCallBack(mineid);
        }
    }
    
    public void OnSelect(int mineid)
    {
        if ( GatherMapSelect.GetMapSelect(MapID) == mineid)
        {
            mineid = 0;
        }

        GatherMapSelect.UpdateMapSelect(MapID,mineid);
        SelectCallBack?.Invoke(MapID,mineid);
        foreach (var item in m_Items)
        {
            item.Value.OnSelectCallBack(mineid);
        }
    }

    public void OnClose()
    {
        
        MapID = 0;
        //LastSelect = 0;
        SelectCallBack = null;
        CleanItems();
        Root.SetActive(false);
        OnCloseCallBack?.Invoke();
    }
}


public class GatherMapUIItem
{
    public GameObject Root { get; private set; }
    private GameObject m_Selected;
    private Image m_BgRare;
    private Image m_Icon;
    private Text m_Name;
    private Text m_Level;
    private Text m_JobName;
    private JButton m_Btn;
    private JButton m_GoBtn;
    private JButton m_IconBtn;
    private System.Action<int> OnSelect;
    private int CurrentMapID;
    private int MineID;
    private long ItemID;
    private int JobID;
    public GatherMapUIItem(GameObject go)
    {
        Root = go;

        var transform = Root.transform;
        m_Selected = transform.Find("Selected").gameObject;
        m_BgRare = transform.Find("BgRare").GetComponent<Image>();
        m_Icon = transform.Find("Icon").GetComponent<Image>();
        m_Name = transform.Find("Name").GetComponent<Text>();
        m_JobName = transform.Find("JobName").GetComponent<Text>();
        m_Level = transform.Find("Name/Level").GetComponent<Text>();
        m_Btn = transform.GetComponent<JButton>();
        m_GoBtn = transform.Find("GotoBtn").GetComponent<JButton>();
        m_IconBtn = transform.Find("Icon").GetComponent<JButton>();
        
        m_Btn.OnClick += OnClickSelectHandler;
        m_GoBtn.OnClick += OnClickGotoHandler;
        m_IconBtn.OnClick += OnClickItemandler;

    }

    public void OnRelease()
    {
        Root.SetActive(false);
        m_Btn.OnClick -= OnClickSelectHandler;
        m_GoBtn.OnClick -= OnClickGotoHandler;
        MineID = 0;
        m_Selected.SetActive(false);
        OnSelect = null;
    }

    public void SetData(int mapid,int mineid, System.Action<int> cb)
    {
        CurrentMapID = mapid;
        MineID = mineid;
        OnSelect = cb;
        
        var config = LocalDataManager.Instance.GetLifeSkillMineDataCell(MineID);
        if (config != null)
        {
            ItemID = config.GetBasicOutput();
            JobID = config.GetSkillID();
            //m_Level.text = config.GetLevel() + "级";
            //m_Level.text = string.Format(GameConfig.LocalStr["LvStr"], config.GetLevel());
            m_Level.text = string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), config.GetLevel());

            var itemcfg = LocalDataManager.Instance.GetItemDataCell(ItemID);
            if (itemcfg != null)
            {
                var quality = itemcfg.GetQuality();
                m_Name.text = ColorDefine.Instance.SetTextColorByQuality(config.Name, quality);
                AtlasManager.Instance.GetSpriteAsync("ui/common/atlas/commonitem", "Bag_Grid_sq" + quality,
                    (sp) => { m_BgRare.sprite = sp; });
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAsync(itemcfg.Icon,
                    (sp) => { m_Icon.sprite = sp; });
                

            }

            var jobcfg = LocalDataManager.Instance.GetLifeSkillJobDataCell(config.GetSkillID(), config.GetLevel());
            if (jobcfg != null)
            {
                m_JobName.text = jobcfg.SkillName;
            }
        }
        Root.SetActive(true);
  
    }

    
    public void OnSelectCallBack(int mineID)
    {
        m_Selected.SetActive(mineID== MineID);
    }

    private void OnClickItemandler(GameObject go)
    {
        ModuleManager.Instance.SendMessage(ModuleDef.Name.LivingSkillsModule, "OpenLivingSkillsWindow", 
            new object[] { MineID,JobID,1});
        
        /*ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnOpenTips", 
            new object[] { ItemID, false, 0, m_Icon.transform, 0, null, null, null, 0 });*/
    }
    
    private void OnClickGotoHandler(GameObject go)
    {
        Debug.LogError($"前往 {MineID}");
        var config = LocalDataManager.Instance.GetLifeSkillMineDataCell(MineID);
        if (config != null)
        {
            System.Action<int, Vector3> cb = OnFindCallBack;
            ModuleManager.Instance.SendMessage(ModuleDef.Name.LivingSkillsModule,"OnGetMinePosition",
                new object[] {MineID,config.GetPersonReserve(),cb});
        }
    }

    private void OnFindCallBack(int mapID, Vector3 position)
    {
        if (mapID == CurrentMapID)
        {
            //相同地图
            TaskHelper.FindPostion(mapID,position);
            UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);
        }
        else
        {
            string ItemName = "";
            var config = LocalDataManager.Instance.GetLifeSkillMineDataCell(MineID);
            if (config != null)
            {
                ItemName = config.Name;
            }

            UIAPI.ShowMsgBox(54, (EventName) =>
            {
                if (EventName == "SURE")
                {
                    TaskHelper.FindPostion(mapID,position);
                    UIManager.Instance.CloseWindow(UIDef.MiniMapWindow);
                }
            },ItemName);
        }
            

    }
    private void OnClickSelectHandler(GameObject go)
    {
        OnSelect?.Invoke(MineID);
    }
}

public static class GatherMapSelect
{
    private static Dictionary<int, int> Maps = new Dictionary<int, int>();

    public static int GetMapSelect(int mapID)
    {
        if (Maps.ContainsKey(mapID))
        {
            return Maps[mapID];
        }

        return 0;
    }

    public static void UpdateMapSelect(int mapID, int selectID)
    {
        if (Maps.ContainsKey(mapID))
        {
             Maps[mapID]=selectID;
        }
        else
        {
            Maps.Add(mapID, selectID);
        }
       GlobalEvent.OnGratherSelectChange
            ?.Invoke(mapID, selectID);
    }
}  