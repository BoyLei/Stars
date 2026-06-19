using SGF.Module.Framework;
using StarProject.Game;
using StarProject.Game.Player;
using StarProject.Service.AtlasManager;
using StarProject.Service.Business;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NormalTreasure : MonoBehaviour
{
    public Text mMapName;
    public Text mPosition;
    public Text mBtnTex;
    public Image mIcon;
    public Image mDiIcon;
    public JButton mGoto;
    public JButton mClose;

    private int _mType;
    private int _mapID;
    private Vector3 _position;
    private int _index;
    private ulong _entityID;
    private bool IsAdvance = true;

    private Action<Sprite> loadDiIcon;


    public void Awake()
    {
        mGoto.OnClick+=OnGotoHandler;
        mClose.OnClick += OnCloseHandler;
    }

    private void OnCloseHandler(GameObject arg0)
    {
        if (GameManager.Instance.M_MainPlayerCtrlBase != null)
        {
            var player = GameManager.Instance.M_MainPlayerCtrlBase as PlayerCtrlGroup;
            if (player != null)
            {
                player.BreakFindPath();
                player.BreakFollowDynamicEnity();
            }
        }
        ModuleManager.Instance.SendMessage(ModuleDef.Name.TreasureModule, "OnBreakDigTreasureReq");
        gameObject.SetActive(false);
    }

    public bool IsUsing()
    {
        return gameObject.activeInHierarchy && _mType == 2;
    }

    private void OnGotoHandler(GameObject go)
    {
        if (_mType == 1)
        {
            TaskHelper.FindPostion(_mapID, _position, 1, (result) =>
            {
                if (result)
                {
                    var entityid = GameManager.Instance.GetInterEntityID(_index);
                    if (entityid > 0)
                    {
                        ModuleManager.Instance.SendMessage(ModuleDef.Name.InterActionModule, "QueryInter", entityid);
                    }
                }
            });
        }

        if (_mType == 2)
        {
            ModuleManager.Instance.SendMessage(ModuleDef.Name.TreasureModule, "OnOpenTreasureMapReq", _entityID);
            if (IsAdvance)
            {
                //Frame.Util.ShowMessage(GameConfig.LocalStr["UserCrystalBall"]);
                Frame.Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("UserCrystalBall"));
                gameObject.SetActive(false);
            }
            else
            {
                //Frame.Util.ShowMessage(GameConfig.LocalStr["UserCrystalBall2"]);
                Frame.Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("UserCrystalBall2"));
            }
        }
    }

    public void SetData(int type, ulong entityid, bool isadvance = false)
    {
        _mType = type;
        _entityID = entityid;
        var active = false;
        IsAdvance = isadvance;
        string iconPath = string.Empty;
        string diPath ="Bag_Grid_sq3";
        var item = BusinessManager.Instance.GetItemByItemEntityId(entityid);
        if (item != null)
        {
            _mapID = item.TData.TreasureMapID;
            _position = new Vector3(item.TData.TreasurePos.X, item.TData.TreasurePos.Y, item.TData.TreasurePos.Z);
            _index = item.TData.TreasureInterID;

            var itemcfg = LocalDataManager.Instance.GetItemDataCell(item.BaseID);
            if (itemcfg != null)
            {
                iconPath = itemcfg.Icon;
                diPath = "Bag_Grid_sq"+itemcfg.GetQuality();
            }
            if (type == 1)
            {
                //mBtnTex.text = GameConfig.LocalStr["Goto"];
                mBtnTex.text = LanguageManager.Instance.GetLanguageByKey("Goto");
                active=true;
            }
            if (type == 2)
            {
                //mBtnTex.text = GameConfig.LocalStr["User"];
                mBtnTex.text = LanguageManager.Instance.GetLanguageByKey("User");
                active=true;
            }

            var mapconfig = LocalDataManager.Instance.GetMapCfgData(_mapID);
            if (mapconfig != null && mMapName != null)
            {
                mMapName.text = mapconfig.MapName;
            }

            if (mPosition != null)
            {
                if (isadvance)
                {
                    mPosition.text = "";
                }
                else
                {
                    mPosition.text = $"X:{(int)_position.x}   Y:{(int)_position.z}";
                }

            }
        }

        if (mIcon != null)
        {
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<Sprite>(iconPath,
            (Sprite img) =>
            {
                if (img != null && mIcon != null)
                {
                    mIcon.sprite = img;
                }
            });
        }

        if (!string.IsNullOrEmpty(diPath) && mDiIcon != null)
        {
            loadDiIcon = (Sprite sp) =>
            {
                if (mDiIcon != null && sp != null)
                {
                    mDiIcon.sprite = sp;
                }
            };
            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathCommonItem, diPath, loadDiIcon);
        }
        gameObject.SetActive(active);
    }
}