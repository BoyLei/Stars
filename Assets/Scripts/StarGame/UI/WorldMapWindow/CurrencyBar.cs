using SGF.UI.Framework;
using StarProject.Game;
using StarProject.Service.Business;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyBar
{
    private const string Path = "Bag/CurrencyBar";

    private Transform Parent;

    private GameObject Root;

    private Animator mAimator;

    private JButton MoreBtn;

    private GameObject CurrencyItem;

    private Transform mGridTrans;

    private Dictionary<long, CurrencyBarItem> CurrencyList = new();

    private bool IsCreareBar = false;

    private List<long> m_Coins = new();

    public void Init(Transform parent, GameObject root = null)
    {
        Parent = parent;
        //if (root == null)
        //{
        //    root = UIRes.LoadPrefab(Path);
        //}
        //Root = root;

        if (root == null)
        {
            Action<GameObject> action = (GameObject gob) =>
            {
                Root = gob;
                InitBind();
            };
            UIRes.LoadPrefabAsync(Path, action);
        }
        else
        {
            Root = root;
            InitBind();
        }
    }

    private void InitBind()
    {
        mAimator = Root.GetComponent<Animator>();

        if (Parent != null)
        {
            Root.transform.SetParent(Parent);
            Root.transform.localPosition = Vector3.zero;
            Root.transform.localScale = Vector3.one;
        }

        MoreBtn = Root.transform.Find("Grid/MoreBtn").GetComponent<JButton>();
        MoreBtn.OnClick += OnClickHandler;
        MoreBtn.gameObject.SetActive(false);
        CurrencyItem = Root.transform.Find("Grid/CurrencyItem").gameObject;
        CurrencyItem.SetActive(false);
        mGridTrans = Root.transform.Find("Grid");

        if (IsCreareBar)
        {
            CreateBar();
        }
    }

    private void OnClickHandler(GameObject arg0)
    {
    }

    public void PlayAnim(bool isEnter)
    {
        if (mAimator == null)
        {
            return;
        }
        if (isEnter)
        {
            mAimator.Play("CurrencyBarAnim_Enter");
        }
        else
        {
            mAimator.Play("CurrencyBarAnim_Exit");
        }
    }

    public void InitBar(List<long> Coins)
    {
        m_Coins = Coins;
        if (CurrencyItem == null)
        {
            IsCreareBar = true;
            return;
        }
        CreateBar();
    }

    private void CreateBar()
    {
        if (m_Coins != null)
        {
            foreach (var coin in m_Coins)
            {
                var item = new CurrencyBarItem();
                var go = GameObject.Instantiate(CurrencyItem, mGridTrans);
                item.OnInit(go, coin);
                CurrencyList.Add(coin, item);
            }
        }

        RefeshBar(string.Empty, null);
        PlayAnim(true);
        IsCreareBar = false;
    }

    private void RefeshBar(string key, object val)
    {
        if (CurrencyList != null)
        {
            foreach (var item in CurrencyList)
            {
                item.Value.RefeshItem();
            }
        }
    }

    private void InitAttribute()
    {
        var M_EntityBase = GameManager.Instance.M_MainPlayerCtrlBase;
        M_EntityBase.Data.RegisterAttribute(StarProjectDef.AOIAttrDefine.Silver, RefeshBar);
        M_EntityBase.Data.RegisterAttribute(StarProjectDef.AOIAttrDefine.Coin, RefeshBar);
        M_EntityBase.Data.RegisterAttribute(StarProjectDef.AOIAttrDefine.ChargeDiamond, RefeshBar);
        M_EntityBase.Data.RegisterAttribute(StarProjectDef.AOIAttrDefine.BindDiamond, RefeshBar);
        M_EntityBase.Data.RegisterAttribute(StarProjectDef.AOIAttrDefine.CollectEnergy, RefeshBar);
    }

    private void DelAttribute()
    {
        var M_EntityBase = GameManager.Instance.M_MainPlayerCtrlBase;
        M_EntityBase.Data.UnRegisterAttribute(StarProjectDef.AOIAttrDefine.Silver, RefeshBar);
        M_EntityBase.Data.UnRegisterAttribute(StarProjectDef.AOIAttrDefine.Coin, RefeshBar);
        M_EntityBase.Data.UnRegisterAttribute(StarProjectDef.AOIAttrDefine.ChargeDiamond, RefeshBar);
        M_EntityBase.Data.UnRegisterAttribute(StarProjectDef.AOIAttrDefine.BindDiamond, RefeshBar);
        M_EntityBase.Data.UnRegisterAttribute(StarProjectDef.AOIAttrDefine.CollectEnergy, RefeshBar);
    }


    private void FreshBar(bool fresh)
    {
        RefeshBar(string.Empty, null);
    }

    public void OnListener()
    {
        StarProject.GlobalEvent.OnRefeshCurrencyBar.RemoveAllListeners();
        StarProject.GlobalEvent.OnRefeshCurrencyBar.AddListener(FreshBar);
        InitAttribute();
    }

    public void OffListener()
    {
        StarProject.GlobalEvent.OnRefeshCurrencyBar.RemoveListener(FreshBar);
        DelAttribute();
    }

    public void OnRelease()
    {
        if (CurrencyList != null)
        {
            foreach (var item in CurrencyList)
            {
                item.Value.OnRelease();
            }

            CurrencyList.Clear();
        }


        MoreBtn.OnClick -= OnClickHandler;
        MoreBtn = null;
        CurrencyItem = null;
        mGridTrans = null;
    }
}

public class CurrencyBarItem
{
    private GameObject mRoot;
    private long BaseID;
    private Text Num;
    private Image Icon;
    private GameObject go_Add;
    private JButton btnClick;

    public void OnInit(GameObject root, long id)
    {
        mRoot = root;
        BaseID = id;
        mRoot.SetActive(true);
        Num = mRoot.transform.Find("num").GetComponent<Text>();
        Icon = mRoot.transform.Find("Icon").GetComponent<Image>();
        go_Add = mRoot.transform.Find("add").gameObject;
        btnClick = mRoot.transform.GetComponent<JButton>();
        btnClick.OnClick += OnItemClick;
    }

    private void OnItemClick(GameObject go)
    {
        //点击货币
        if (BaseID == 3)
        {
            //打开充值
            //UIManager.Instance.OpenWidget("Shop/BuyCashWidget", false, null, null, StarProjectDef.MainPageCommond.HideNone, false, false);
            UIManager.Instance.OpenWidgetAsync(UIDef.BuyCashWidget, null, false, null, null, StarProjectDef.MainPageCommond.HideNone, false, false);
        }
    }

    public void RefeshItem()
    {
        var cfg = LocalDataManager.Instance.GetItemDataCell(BaseID);

        //道具图标
        //LoadIcon(self.Icon,cfg.Icon);
        string path = cfg.Icon;
        //var sp = StarProject.ResourceHelperMono.LoadSprite(path);
        //MedicineJB.GetComponent<Image>().sprite = StarProject.Service.Resource.ResourceManager.Instance.LoadSprite(path);
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>(path,
            (Sprite img) =>
            {
                if (img == null || Icon == null)
                {
                    return;
                }

                Icon.sprite = img;
            });


        //货币
        if (BaseID <= 4 || BaseID == 116)
        {
            Num.text = "" + BusinessManager.Instance.GetCurrencyNum(BaseID);
        }
        else
        {
            var it = BusinessManager.Instance.GetItemByItemBaseId(BaseID);
            if (it != null)
            {
                Num.text = "" + BusinessManager.Instance.GetItemByItemBaseId(BaseID).Num;
            }
            else
            {
                Num.text = "0";
            }
        }

        if (BusinessManager.Instance.SystemIsOpen(StarProjectDef.SystemOpenType.FirstTopUp))
        {
            go_Add.SetActive(BaseID == 3);
        }

        else
        {
            go_Add.SetActive(false);
        }
    }

    public void OnRelease()
    {
        BaseID = 0;
        Num = null;
        Icon = null;
        go_Add = null;
        btnClick.OnClick -= OnItemClick;
        btnClick = null;
        GameObject.Destroy(mRoot);
    }
}