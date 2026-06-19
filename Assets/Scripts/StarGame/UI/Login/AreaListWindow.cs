using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Game;
using StarProject.Module;
using StarProject.Service.Language;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    /// <summary>
    /// TODO：
    /// 1，Tab之后做成通用组件。
    /// 2，外部脚本需要内部Window来实现，或通用，需识别其价值。
    /// 3，ScrollView需拓展，如各Tab功能相同TabContext需复用，ScrollViewEx需添加池化数据即Cell的复用。
    /// 4，可滑动窗口背景必须有不可滑动的部分，不然穿透，就需要处理很多按钮的重复点击逻辑。
    /// </summary>
	public class AreaListWindow : UIWindow
    {
        private readonly string LOG_TAG = "[AreaListWindow]";

        private LoginModule m_Mymodule;

        // 大区
        public Transform M_AreaRoot;
        public GameObject M_AreaPrefab;

        //小区组
        // --- 我的小区
        public ScrollRect M_MyGroupRoot;   // 我的服务器
        public GameObject M_MyGroupPrefab;  // 我的服务器预设
        // -- 其他小区
        public ScrollRect M_GroupRoot;
        public GameObject M_GroupPrefab;

        // 上次登入信息
        public GameObject M_LastLoginPanel;
        public Image M_LastGroupLoadState;          // 服务器负载状态标识（火爆：红/拥挤：黄/正常：绿/维护中：灰） 
        public Text M_LastGroupName;                // 服务器名

        // 大区Item信息
        private List<ItemArea> m_ItemAreaList = new();
        private ItemArea m_curItemArea;     // 当前选中的大区

        protected override void Awake()
        {
            base.Awake();
            // 按钮绑定事件
            {
                // 关闭
                {
                    Button BtnClose = transform.Find("Cut4Cam90/AssemblyRatio2/BtnClose").GetComponent<Button>();
                    BtnClose.onClick.AddListener(OnBtnClose);
                }
                //删除全部角色
                {
                    Button btnDelRole = transform.Find("Cut4Cam90/AssemblyRatio2/Area_Hint/All_area_state/BtnDelAllRole").GetComponent<Button>();
                    btnDelRole.onClick.AddListener(OnBtnDelAllRole);
                    btnDelRole.gameObject.SetActive(AppConfig.IsDev());
                }
            }
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            transform.SetAsLastSibling();
            UIManager.Instance.CloseWidget(UIDef.AnnouncementWidget);

            m_Mymodule = ModuleManager.Instance.GetModule(ModuleDef.Name.LoginModule) as LoginModule;
            m_Mymodule.SetPageActive(false, false);
            m_Mymodule.RegisterRefreshGroupListAction(RefreshGroupList);

            GetGroupListAck gla = (GetGroupListAck)arg;
            //这里目前受限于1Addressable做好--->2ABLoad----->3不村Perfab--->4分开遍历--->5双方准确idx指定发生关联---->6服务器数据处理更健壮
            if (gla != null)
            {
                var m_AreaList = gla.areaList;

                #region 设置大区
                //-- 优先创建我的服务器列
                InitMyArea();
                ////-- 创建正常的服务器列
                ////--- 包含推荐服信息，[0]下标为0的始终是推荐服数据
                for (int i = 0; i < m_AreaList.Count; i++)
                {
                    AreaInfo areaInfo = m_AreaList[i];
                    AddOtherAreaPrefab(areaInfo);
                }
                #endregion
            }
            else
            {
                DelayShoAreaListLoadingUI();
            }

            // 获取上次登录的信息
            GroupInfo groupInfo = m_Mymodule.M_GroupInfo;
            if (groupInfo != null)
            {
                M_LastGroupName.text = $"{groupInfo.groupName}";
                //M_LastGroupLoadState
            }
            M_LastLoginPanel.SetActive(groupInfo != null);

            GameManager.Instance.IsLoginInitState = false;
            //SGF.Debuger.Log($"[公告] areaListWindow OnOpen set IsLoginInitState : {GameManager.Instance.IsLoginInitState}");
        }

        private void DelayShoAreaListLoadingUI()
        {
            Action onOutAction = () =>
            {
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
                OnBtnClose();
            };
            UIAPI.ShowRequestLoading2(SystemConstConfigs.Login_ReqWaitTimeout * 1000, onOutAction, OnBtnClose, LanguageManager.Instance.GetLanguageByKey("RequestLoadingTips_2"), LoadingWidgetTypeEnum.AwaitResponse);
        }

        private void InitMyArea()
        {
            var gob = Instantiate<GameObject>(M_AreaPrefab);
            gob.transform.SetParent(M_AreaRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            ItemArea myAreaSp = gob.GetComponent<ItemArea>();
            AreaInfo myAreaInfo = new();
            myAreaInfo.AreaID = ulong.MaxValue;
            //myAreaInfo.AreaName = GameConfig.LocalStr["AreaListTabMyPlayer"];
            myAreaInfo.AreaName = LanguageManager.Instance.GetLanguageByKey("AreaListTabMyPlayer");
            myAreaSp.InitItemAreaInfo(myAreaInfo, E_AreaTabType.MyArea);
            myAreaSp.SetSelected(true);
            myAreaSp.M_Btn.OnClick = (go) =>
            {
                HandleArea(myAreaSp);
            };

            m_ItemAreaList.Add(myAreaSp);

            // 初始化我的服务器有号信息
            InitMyGroupList();
            // 默认选中【我的服务器】
            HandleArea(myAreaSp);
        }

        private void InitMyGroupList()
        {
            GetOwnerGroupListAck getOwnerGroupListAck = GameLoginInfo.M_GetOwnerGroupListAck;
            if (getOwnerGroupListAck != null)
            {
                List<OwnerGroupInfo> ownergroupList = getOwnerGroupListAck.ownergroupList;
                // 列表顺序改为按最后登录时间排序，越新越靠前
                ownergroupList.Sort((x, y) => -x.LoginTime.CompareTo(y.LoginTime));
                int count = ownergroupList.Count;
                int childCount = M_MyGroupRoot.content.childCount;
                int maxLength = Mathf.Max(childCount, count);
                for (int i = 0; i < maxLength; i++)
                {
                    OwnerGroupInfo data = count > i ? ownergroupList[i] : null;
                    if (childCount > i)
                    {
                        Transform item = M_MyGroupRoot.content.GetChild(i);
                        SetMyGroup(item.gameObject, data);
                    }
                    else
                    {
                        AddMyGroup(data);
                    }
                }
            }
        }

        private void AddMyGroup(OwnerGroupInfo ownerGroupInfo)
        {
            var gob = Instantiate<GameObject>(M_MyGroupPrefab);
            gob.transform.SetParent(M_MyGroupRoot.content);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            SetMyGroup(gob, ownerGroupInfo);
        }

        private void SetMyGroup(GameObject gob, OwnerGroupInfo data)
        {
            ItemMyGroup mygroupSp = gob.GetComponent<ItemMyGroup>();
            mygroupSp.InitItemOwnerGroupInfo(data);
            JButton btn = gob.GetComponent<JButton>();
            btn.OnClick = (go) =>
            {
                if (data.groupLoad >= 0)
                {
                    HandleMyGroup(mygroupSp);
                }
            };
            //// 维护中的服务器不让进游戏,,不接受点击事件
            btn.DoScale = data.groupLoad >= 0;
        }

        private void AddOtherAreaPrefab(AreaInfo areaInfo)
        {
            var gob = Instantiate<GameObject>(M_AreaPrefab);
            gob.transform.SetParent(M_AreaRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            ItemArea areaSp = gob.GetComponent<ItemArea>();
            areaSp.InitItemAreaInfo(areaInfo, E_AreaTabType.OtherArea);
            areaSp.M_Btn.OnClick = (go) =>
            {
                HandleArea(areaSp);
            };

            m_ItemAreaList.Add(areaSp);
        }

        private void SetGroupListByAreaInfo(AreaInfo areaInfo)
        {
            if (areaInfo != null)
            {
                List<GroupInfo> groupList = areaInfo.GroupList;
                // 列表排序，1.推荐服在最前面 2.大号服务器（新服）放列表前序
                // 开服》推荐》开服时间
                groupList.Sort(m_Mymodule.CompareToRecommedTime);
                int count = groupList.Count;
                int childCount = M_GroupRoot.content.childCount;
                int maxLength = Mathf.Max(childCount, count);
                for (int i = 0; i < maxLength; i++)
                {
                    GroupInfo groupInfo = count > i ? groupList[i] : null;
                    if (childCount > i)
                    {
                        Transform item = M_GroupRoot.content.GetChild(i);
                        SetOtherGroup(item.gameObject, groupInfo);
                    }
                    else
                    {
                        AddOtherGroup(groupInfo);
                    }
                }
            }
        }

        private void AddOtherGroup(GroupInfo groupInfo)
        {
            var gob = Instantiate<GameObject>(M_GroupPrefab);
            gob.transform.SetParent(M_GroupRoot.content);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            SetOtherGroup(gob, groupInfo);
        }

        private void SetOtherGroup(GameObject gob, GroupInfo data)
        {
            ItemGroup sp = gob.GetComponent<ItemGroup>();
            sp.InitItemGroupInfo(data);
            sp.M_Btn.onClick.RemoveAllListeners();
            if (data != null)
            {
                sp.M_Btn.onClick.AddListener(
                    () =>
                    {
                        if (data.groupLoad >= 0)
                        {
                            m_Mymodule.SelectServiceId(sp.M_GroupInfo.groupID);
                            m_Mymodule.M_GroupInfo = sp.M_GroupInfo;
                            OnBtnClose();
                        }
                        else
                        {
                            ////2024/3/15
                            //// 维护中的服务器不让进游戏,,不接受点击事件
                            //sp.M_Btn.interactable = data.groupLoad >= 0;
                            // 2024/4/25 新需求，弹出提示
                            //Frame.Util.ShowMessage(GameConfig.LocalStr["GroupClose"]);
                            Frame.Util.ShowMessage(LanguageManager.Instance.GetLanguageByKey("GroupClose"));
                        }
                    }
                ); 
            }
        }

        private void HandleArea(ItemArea itemArea)
        {
            // 隐藏上次的tab
            if (m_curItemArea != null)
            {
                m_curItemArea.SetSelected(false);
                if (m_curItemArea.M_AreaTabType == E_AreaTabType.MyArea)
                {
                    M_MyGroupRoot.gameObject.SetActive(false);
                }
                else
                {
                    M_GroupRoot.gameObject.SetActive(false);
                }
            }
            // 开启当前的tab
            m_curItemArea = itemArea;
            if (m_curItemArea != null)
            {
                m_curItemArea.SetSelected(true);
                if (m_curItemArea.M_AreaTabType == E_AreaTabType.MyArea)
                {
                    M_MyGroupRoot.gameObject.SetActive(true);
                }
                else
                {
                    SetGroupListByAreaInfo(m_curItemArea.M_AreaInfo);
                    M_GroupRoot.gameObject.SetActive(true);
                }
            }
        }

        private void HandleMyGroup(ItemMyGroup itemMyGroup)
        {
            if (itemMyGroup.M_OwnerGroupInfo.groupLoad < 0)
            {
                // 维护中的服务器不让进游戏
                return;
            }
            Action<bool> cb = (isSuccess) =>
            {
                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_SERVER_AREA_ENTER_GAME);
                GroupInfo groupInfo = new();
                groupInfo.groupName = itemMyGroup.M_OwnerGroupInfo.groupName;    //小服名
                groupInfo.groupID = (uint)itemMyGroup.M_OwnerGroupInfo.groupID;      //小服id
                groupInfo.groupLoad = itemMyGroup.M_OwnerGroupInfo.groupLoad;       //小服负载      火爆/拥挤/流畅/维护中（传的是负数）
                groupInfo.areaID = itemMyGroup.M_OwnerGroupInfo.areaID;       //所属大区id
                groupInfo.Pid = itemMyGroup.M_OwnerGroupInfo.PID;          //这个是服务器在数据库里面的主键(暂时无效）
                groupInfo.isRecommend = false;    // 是否推荐服
                groupInfo.isNew = false;          // 是否新服
                m_Mymodule.M_GroupInfo = groupInfo;
                OnBtnClose();
                m_Mymodule.ChoseHeroAck(itemMyGroup.M_OwnerGroupInfo.PID, null);
            };
            m_Mymodule.LoginGroupId((uint)itemMyGroup.M_OwnerGroupInfo.groupID, cb);
        }

        private void RefreshGroupList()
        {
            // 刷新大区
            if (m_ItemAreaList != null)
            {
                foreach (var item in m_ItemAreaList)
                {
                    if (item != null && item.M_AreaTabType != E_AreaTabType.MyArea && item.M_AreaInfo != null)
                    {
                        AreaInfo areaInfo = GetAreaDataByID(item.M_AreaInfo.AreaID);
                        if (areaInfo != null)
                        {
                            item.RefreshAreaData(areaInfo);
                        }
                    }
                }
            }
            // 刷新小区
            if (m_curItemArea != null)
            {
                if (m_curItemArea.M_AreaTabType == E_AreaTabType.MyArea)
                {
                    foreach (var item in M_MyGroupRoot.content.GetChildren())
                    {
                        if (item != null)
                        {
                            ItemMyGroup mygroupSp = item.GetComponent<ItemMyGroup>();
                            if (mygroupSp != null && mygroupSp.M_OwnerGroupInfo != null)
                            {
                                int groupLoad = GetGroupLoad(mygroupSp.M_OwnerGroupInfo.areaID, mygroupSp.M_OwnerGroupInfo.groupID, out bool isFind);
                                if (isFind)
                                {
                                    mygroupSp.RefreshGroupLoad(groupLoad);
                                }
                            }
                        }
                    }
                }
                else
                {
                    SetGroupListByAreaInfo(m_curItemArea.M_AreaInfo);
                }
            }
        }

        private AreaInfo GetAreaDataByID(UInt64 areaID)
        {
            if (GameLoginInfo.M_GetGroupListAck == null || GameLoginInfo.M_GetGroupListAck.areaList == null || GameLoginInfo.M_GetGroupListAck.areaList.Count <= 0)
            {
                return null;
            }
            var m_AreaList = GameLoginInfo.M_GetGroupListAck.areaList;
            foreach (var item in m_AreaList)
            {
                if (item != null && item.AreaID == areaID)
                {
                    return item;
                }
            }
            return null;
        }

        private int GetGroupLoad(UInt64 areaID, int groupID, out bool isFind)
        {
            isFind = false;
            if (GameLoginInfo.M_GetGroupListAck == null || GameLoginInfo.M_GetGroupListAck.areaList == null || GameLoginInfo.M_GetGroupListAck.areaList.Count <= 0)
            {
                return -1;
            }
            var m_AreaList = GameLoginInfo.M_GetGroupListAck.areaList;
            foreach (var item in m_AreaList)
            {
                if (item != null && item.AreaID == areaID)
                {
                    foreach (var item2 in item.GroupList)
                    {
                        if (item2.groupID == groupID)
                        {
                            isFind = true;
                            return item2.groupLoad;
                        }
                    }
                }
            }
            return -1;
        }

        public void OnBtnClose()
        {
            m_Mymodule.SetPageActive(true, false);


            GameManager.Instance.IsLoginInitState = true;
            SGF.Debuger.Log($"[公告] areaListWindow OnBtnClose set IsLoginInitState : {GameManager.Instance.IsLoginInitState}");

            this.Close();
        }

        private void OnBtnDelAllRole()
        {
            GetOwnerGroupListAck getOwnerGroupListAck = GameLoginInfo.M_GetOwnerGroupListAck;
            if (getOwnerGroupListAck != null)
            {
                int count = getOwnerGroupListAck.ownergroupList.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        OwnerGroupInfo ownerGroupInfo = getOwnerGroupListAck.ownergroupList[i];
                        m_Mymodule.DeleteHeroAck(ownerGroupInfo.PID, (UInt32)ownerGroupInfo.groupID, null);
                    }
                    OnBtnClose();
                }
            }
        }

        private void DelGroupItem()
        {
            for (int i = m_ItemAreaList.Count - 1; i >= 0; i--)
            {
                var child = m_ItemAreaList[i];
                Destroy(child.gameObject);
            }
            m_ItemAreaList.Clear();
        }

        protected override void OnClose(object arg = null)
        {
            M_MyGroupRoot.gameObject.SetActive(false);
            M_GroupRoot.gameObject.SetActive(false);

            m_Mymodule.UpdateUILoginPageAreaData();
            m_Mymodule.UnRegisterRefreshGroupListAction(RefreshGroupList);

            SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_RETURN_LOGIN);

            DelGroupItem();
            base.OnClose(arg);
        }

    }
}
