using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.ArtHelper;
using StarProject.Game;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.DisplayProcess;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class CreateRoleWindow : UIWindow
    {
        private string LOG_TAG = "[CreateRoleWindow]";

        private LoginModule m_Mymodule;
        // 创建角色
        public Text M_Title;
        public Transform M_WidgetRoot;
        public GameObject M_CreatorRolePanel;
        public DisplayRawimg M_Rawimg;
        public Transform M_JobRoot;
        public GameObject M_JobPrefab;   // 职业item
        public Image M_SelectJobBackIcon;
        public Text M_SelectJob;
        public Text M_JobDes;
        public Transform M_JobLabelRoot;
        public GameObject M_JobLabelPrefab; // 职业标签
        public Transform M_JobSwitchRoot;
        public GameObject M_JobSwitchPrefab;    // 职业转职
        public Transform M_JobCapacityRoot;
        public GameObject M_JobCapacityPrefab;

        public JButton JBtnMan;
        public JButton JBtnWoman;

        public Transform JobTabContent;
        public GameObject BtnCreateGo;
        public GameObject UnCreateGo;

        //---------  选中角色界面
        public GameObject M_SelectRolePanel;
        public ScrollViewNevigation M_SelectRoleScrollView;
        public Transform M_SelectRoleRoot;
        public GameObject M_MyPlayerPrefab;
        private GameObject CreatRoleRoot;

        // 3DUI 面片节点
        private GameObject CreateRoleWindowBgRootGo;
        private GameObject _SelectRoleRoot;
        private GameObject _CreateRoleRoot;
        private Material _CreateRoleBgMat;
        private Material _CreateRoleFrontMat;

        private ItemJob m_curItemJob;   // 当前选中的职业
        private int MyPlayerCount = 0;
        private ItemMyPlayer m_MyPlayerItem;    // 当前选中的角色
        private PlayerLoginData CheckPlayerData;    // 当前选中的角色信息
        private bool m_isClickStartGame = true;
        //------------

        private int m_CurrentCheckSex = -1;  // 当前选中的性别
        private ModelDataCell m_ModelDataCell;  // 模型信息
        private AvatarDataCell m_AvatarDataCell;
        private CharacterCreateDataCell m_CharacterCreateDataCell;  // 创角信息
        private RoleViewDisplay m_ModelViewDisplay = null;
        private string m_ModelPath = "";
        private UIAsyncLoadState m_uIAsyncLoadState = UIAsyncLoadState.None;

        private bool m_IsSelectRolePanel = false;
        private bool m_IsHavePlayers = false;
        private bool m_IsHaveRecordLoginPID = false;
        private bool m_IsDefaultCheck = false;

        private const string DelayKey = "CreateRole";

        protected override void Awake()
        {
            base.Awake();


            CreatRoleRoot = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/LeftCenter/TabContext1/Scroll View/Viewport/ContentRoot/CreatRoleRoot").gameObject;

            // 按钮绑定事件
            {
                // 创建按钮
                {
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/CreatorRolePanel/RoleInfoContent/RightBottom/BtnCreate").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnClickBename();
                    };
                }
                // 男
                {
                    JBtnMan.OnClick = OnJbtnClickMan;
                }
                // 女
                {
                    JBtnWoman.OnClick = OnJbtnClickWoman;
                }
                // 关闭
                {
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/BtnClose").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClose();
                    };
                }

                //------------------------ 选择角色界面
                // 进入游戏
                {
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/RightBottom/BtnStartGame").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnStartGame();
                    };
                }
                // 创建角色
                {
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/LeftCenter/TabContext1/Scroll View/Viewport/ContentRoot/CreatRoleRoot/BtnCreateNewRole").GetComponent<JButton>();
                    //JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/LeftBottom/BtnCreateNewRole").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnClickCreateNewRole();
                    };
                }
                //删除角色
                {
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/LeftBottom/BtnDelRole(back)").GetComponent<JButton>();
                    //JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/LeftBottom/BtnDelRole").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnDelRole();
                    };
                }

                //删除全部角色
                {
                    JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/SelectRolePanel/LeftBottom/BtnDelAllRole").GetComponent<JButton>();
                    Btn.OnClick = (go) =>
                    {
                        OnBtnDelAllRole();
                    };
                    Btn.gameObject.SetActive(AppConfig.IsDev());
                }
            }
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);
            UIManager.Instance.CloseWidget(UIDef.AnnouncementWidget);
            onCloseDestroy = true;
            transform.SetAsLastSibling();
            PickSrvAck gla = (PickSrvAck)arg;
            m_Mymodule = ModuleManager.Instance.GetModule(ModuleDef.Name.LoginModule) as LoginModule;
            m_Mymodule.SetPageActive(false, false);
            // 判断所选区服，有无角色信息
            m_IsHavePlayers = gla != null && gla.PlayerData != null && gla.PlayerData.Count > 0;
            m_IsHaveRecordLoginPID = false;
            m_IsDefaultCheck = false;
            bool isShowCreateRoleBtn = false;
            MyPlayerCount = 0;
            CreateRoleWindowBgRoot();
            if (m_IsHavePlayers)
            {
                if (m_Mymodule.M_RecordLoginPID != 0)
                {
                    m_IsHaveRecordLoginPID = gla.PlayerData.FindIndex(x => x.PID == m_Mymodule.M_RecordLoginPID) != -1;
                }
                MyPlayerCount = gla.PlayerData.Count;
                int childCount = M_SelectRoleRoot.childCount;
                int maxLength = Mathf.Max(childCount, MyPlayerCount);
                for (int i = 0; i < maxLength; i++)
                {
                    PlayerLoginData playerLoginData = MyPlayerCount > i ? gla.PlayerData[i] : null;
                    if (childCount > i)
                    {
                        Transform rawCell = M_SelectRoleRoot.GetChild(i);
                        SetMyPlayerItem(rawCell.gameObject, playerLoginData, i, maxLength);
                    }
                    else
                    {
                        AddMyPlayerItem(playerLoginData, i, maxLength);
                    }
                }
                // 这里要遍历一下，再找一下，有多少个真实可用的数量（有些jobid是旧的不可用的）
                int realMyPlayerCount = 0;
                for (int i = 0; i < M_SelectRoleRoot.childCount; i++)
                {
                    var child = M_SelectRoleRoot.GetChild(i);
                    if (child.gameObject.activeInHierarchy)
                    {
                        realMyPlayerCount++;
                    }
                }

                isShowCreateRoleBtn = realMyPlayerCount < SystemConstConfigs.CreateCharacterLimit;
            }
            CreatRoleRoot.SetActive(isShowCreateRoleBtn);
            M_SelectRolePanel.SetActive(m_IsHavePlayers);
            M_CreatorRolePanel.SetActive(!m_IsHavePlayers);

            if (!m_IsHavePlayers)
            {
                SetDefaultSelectJobItem();
                SetDefaultSelectJobTabItem();
            }
            //SetTitleText(m_IsHavePlayers ? GameConfig.LocalStr["CreateRoleTitle1"] : GameConfig.LocalStr["CreateRoleTitle2"]);
            SetTitleText(m_IsHavePlayers ? LanguageManager.Instance.GetLanguageByKey("CreateRoleTitle1") : LanguageManager.Instance.GetLanguageByKey("CreateRoleTitle2"));

            m_IsSelectRolePanel = m_IsHavePlayers;
            //InitJob();
            BindingAllJobTab();
        }

        private void CreateRoleWindowBgRoot()
        {
            //资源加载
            StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>("UI/Login/Prefab/CreateRoleWindowBgRoot",
                (GameObject go) =>
                {
                    if (go == null)
                    {
                        return;
                    }

                    if (this == null || transform == null)
                    {
                        return;
                    }

                    CreateRoleWindowBgRootGo = GameObject.Instantiate<GameObject>(go);
                    if (CreateRoleWindowBgRootGo != null)
                    {
                        _SelectRoleRoot = CreateRoleWindowBgRootGo.transform.Find("SelectRoleRoot").gameObject;
                        _CreateRoleRoot = CreateRoleWindowBgRootGo.transform.Find("CreateRoleRoot").gameObject;
                        _CreateRoleBgMat = _CreateRoleRoot.transform.Find("CreateRoleBg/Bg").GetComponent<MeshRenderer>().sharedMaterial;
                        _CreateRoleFrontMat = _CreateRoleRoot.transform.Find("CreateRoleBg/Front").GetComponent<MeshRenderer>().sharedMaterial;

                        SetCreateRoleWindowBgRootActive(m_IsHavePlayers);

                        SetCreateRole3DBgTextures();
                    }
                });
        }

        private void SetCreateRoleWindowBgRootActive(bool isShowSelect)
        {
            _SelectRoleRoot?.SetActive(isShowSelect);
            _CreateRoleRoot?.SetActive(!isShowSelect);
        }

        private void InitJob()
        {
            Dictionary<int, CharacterCreateDataCell> StaticCharacterDatas = LocalDataManager.Instance.M_CharacterCreateData.StaticCharacterCreateDatas;
            var list = StaticCharacterDatas.KToList();
            int count = list.Count;
            int childCount = M_JobRoot.childCount;
            int maxLength = Mathf.Max(childCount, count);
            for (int i = 0; i < maxLength; i++)
            {
                CharacterCreateDataCell data = count > i ? list[i].Value : null;
                if (childCount > i)
                {
                    Transform item = M_JobRoot.GetChild(i);
                    SetJobItem(item.gameObject, data, i);
                }
                else
                {
                    AddJobItem(data, i);
                }
            }
        }

        private void BindingAllJobTab()
        {
            Dictionary<int, CharacterCreateDataCell> StaticCharacterDatas = LocalDataManager.Instance.M_CharacterCreateData.StaticCharacterCreateDatas;
            int i = 0;
            foreach (var item in StaticCharacterDatas)
            {
                int jobId = item.Value.GetJobID();
                Transform tr = JobTabContent.Find("ItemJob" + item.Value.GetJobID());
                if (tr != null)
                {
                    bool canCreate = item.Value.GetIsCanBuilder();
                    tr.gameObject.SetActive(canCreate);
                    if (canCreate)
                    {
                        SetJobItem(tr.gameObject, item.Value, i);
                        i++;
                    }
                }
            }
        }

        private void AddJobItem(CharacterCreateDataCell data, int idx)
        {
            var gob = Instantiate<GameObject>(M_JobPrefab);
            gob.transform.SetParent(M_JobRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            SetJobItem(gob, data, idx);
        }

        private void SetJobItem(GameObject gob, CharacterCreateDataCell data, int idx)
        {
            ItemJob sp = gob.GetComponent<ItemJob>();
            sp.InitItemJobInfo(data);
            if (data != null)
            {
                sp.M_Btn.OnClick = (go) =>
                {
                    HandleJobItem(sp);
                };

            }
            // 默认选刺客
            if (idx == 1 && !m_IsSelectRolePanel)
            {
                HandleJobItem(sp);
            }
        }

        private void HandleJobItem(ItemJob itemJob)
        {
            // 隐藏上次的tab
            if (m_curItemJob != null)
            {
                m_curItemJob.SetSelected(false);
            }
            // 开启当前的tab
            m_curItemJob = itemJob;
            if (m_curItemJob != null)
            {
                m_curItemJob.SetSelected(true);
                M_SelectJob.text = m_curItemJob.M_JobDataCell.Name;
                //M_SelectJobBackIcon
                int jobshi = m_curItemJob.M_CharacterDataCell.GetJobID() / 10;
                bool canRealCreate = jobshi != 1;
                BtnCreateGo.SetActive(canRealCreate);
                UnCreateGo.SetActive(!canRealCreate);
                string iconPath = $"{AtlasManager.AtlasPathRoleHead}Login_LogoChara{jobshi}";
                Action<Sprite> loadIcon = (Sprite sp) =>
                {
                    if (M_SelectJobBackIcon != null && sp != null)
                    {
                        M_SelectJobBackIcon.sprite = sp;
                    }
                };
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAsync(iconPath, loadIcon);

                M_JobDes.text = m_curItemJob.M_CharacterDataCell.Desc;

                //SetJobLabel();
                SetJobSwitch();
                SetJobCapacity();

                // 设置模型
                m_ModelDataCell = m_curItemJob.M_ModelDataCell;
                m_AvatarDataCell = m_curItemJob.M_AvatarDataCell;
                m_CharacterCreateDataCell = m_curItemJob.M_CharacterDataCell;
                SetModel();

                SetJobSexBtn();
                SetCreateRole3DBgTextures();
            }
        }

        private void SetCreateRole3DBgTextures()
        {
            if (m_curItemJob == null) return;

            if (_CreateRoleBgMat != null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Texture>(m_curItemJob.M_CharacterDataCell.BgPath,
                (UnityEngine.Texture img) =>
                {
                    if (img == null)
                    {
                        return;
                    }
                    _CreateRoleBgMat.SetTexture("_MainTex", img);
                });
            }

            if (_CreateRoleFrontMat != null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Texture>(m_curItemJob.M_CharacterDataCell.FgPath,
                (UnityEngine.Texture img) =>
                {
                    if (img == null)
                    {
                        return;
                    }
                    _CreateRoleFrontMat.SetTexture("_MainTex", img);
                });
            }
        }

        private void SetJobSexBtn()
        {
            bool isContainsMan = CheckIsContainsSex(1);
            JBtnMan.gameObject.GetComponent<CanvasGroup>().alpha = isContainsMan ? 1f : 0.3f;

            bool isContainsWoman = CheckIsContainsSex(2);
            JBtnWoman.gameObject.GetComponent<CanvasGroup>().alpha = isContainsWoman ? 1f : 0.3f;

            if (m_CurrentCheckSex == -1)
            {
                m_CurrentCheckSex = isContainsMan ? 1 : 2;
            }
            else if (m_CurrentCheckSex == 1 && !isContainsMan)
            {
                m_CurrentCheckSex = 2;
            }
            else if (m_CurrentCheckSex == 2 && !isContainsWoman)
            {
                m_CurrentCheckSex = 1;
            }
            SelectSexBtn(JBtnMan.transform, m_CurrentCheckSex == 1);
            SelectSexBtn(JBtnWoman.transform, m_CurrentCheckSex == 2);
        }

        private void SetDefaultSelectJobItem()
        {
            int count = M_JobRoot.childCount;
            for (var i = 0; i < count; i++)
            {
                Transform child = M_JobRoot.GetChild(i);
                ItemJob itemJob = child.GetComponent<ItemJob>();
                if (itemJob != null && itemJob.M_CharacterDataCell != null && i == 1) //i==1 老大要剑盾展示不创建，还要放在最后一个位置显示，默认选中刺客，曲子你别怪我代码为什么写这么粗暴
                {
                    HandleJobItem(itemJob);
                    break;
                }
            }
        }

        private void SetDefaultSelectJobTabItem()
        {
            int count = JobTabContent.childCount;
            for (var i = 0; i < count; i++)
            {
                Transform child = JobTabContent.GetChild(i);
                ItemJob itemJob = child.GetComponent<ItemJob>();
                if (child.gameObject.activeSelf && itemJob != null && itemJob.M_CharacterDataCell != null && i == 1)//i==1老大要剑盾展示不创建，还要放在最后一个位置显示，默认选中刺客，曲子你别怪我代码为什么写这么粗暴
                {
                    HandleJobItem(itemJob);
                    break;
                }
            }
        }

        private void SetModel()
        {
            if (m_ModelDataCell == null)
            {
                return;
            }

            if (m_AvatarDataCell == null)
            {
                return;
            }

            string path = m_ModelDataCell.ModelsPath;
            path = path.Replace("Assets/Res/", string.Empty);

            if (path != m_ModelPath)
            {
                if (m_ModelViewDisplay == null)
                {
                    if (m_uIAsyncLoadState == UIAsyncLoadState.InLoaded)
                    {
                        return;
                    }
                    m_uIAsyncLoadState = UIAsyncLoadState.InLoaded;
                    StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(UIDef.DisplayModelPath, (go) =>
                    {
                        CreateDisplayCB(go);
                    });
                }
                else
                {
                    SetRoleViewDisplayData(path);
                }
                #region 旧的

                //m_ModelPath = path;

                //StarProject.Service.Resource.ResourceFormalManager.Instance.PopGameObject(UIDef.DisplayModelPath, (go) =>
                //{
                //    if (go != null)
                //    {
                //        string anim = "idle_01";
                //        if (m_CharacterCreateDataCell != null)
                //        {
                //            if (string.IsNullOrEmpty(m_CharacterCreateDataCell.AppreaceAnim) || string.IsNullOrWhiteSpace(m_CharacterCreateDataCell.AppreaceAnim))
                //            {
                //                SGF.Debuger.LogWarning($"{LOG_TAG} SetModel() Jobid={m_CharacterCreateDataCell.GetJobID()},AppreaceAnim=null");
                //            }
                //            else
                //            {
                //                anim = m_CharacterCreateDataCell.AppreaceAnim;
                //            }
                //        }

                //        UIManager.Instance.SetModel2UI2(
                //            go,
                //            m_AvatarDataCell,
                //            ThreeDModelPos.None,
                //            scale: Vector3.one,
                //            WorleRotate: Vector3.zero,
                //            localRotate: Vector3.zero,
                //            localPos: Vector3.zero,
                //            anim,
                //            false,
                //            E_LayerType.Entity
                //        );
                //        go.SetActive(true);
                //        if (m_ModelViewDisplay != null)
                //        {
                //            PushDisplayGob();
                //            m_ModelPath = path;
                //        }
                //        m_ModelViewDisplay = go.GetComponent<RoleViewDisplay>();
                //        M_Rawimg.SetModelView(m_ModelViewDisplay);
                //        // 转向归0
                //        Quaternion rotation = Quaternion.Euler(0, 0, 0);
                //        m_ModelViewDisplay.SetRotation(rotation);
                //        M_Rawimg.SetModelView(m_ModelViewDisplay);
                //    }
                //});

                //{
                //    //GameObject gob = Service.Resource.ResourceManager.Instance.RecursionLoadGameObject(DisplayModelPath, E_AssetType.Roles);
                //    //if (gob != null)
                //    //{
                //    //    gob.transform.SetParent(EntityRoot.Instance.RemoveRoot.transform);
                //    //    m_ModelViewDisplay = gob.GetComponent<RoleViewDisplay>();
                //    //    m_ModelViewDisplay.InitData(m_AvatarDataCell, "idle_01", "Entity");
                //    //    M_Rawimg.SetModelView(m_ModelViewDisplay);

                //    //    // 一： UI相机渲染，3D模型
                //    //    //ModelDataCell modelDataCell = LocalDataManager.Instance.GetModelDataCell(m_curItemJob.M_CharacterDataCell.GetModelId());
                //    //    //string animPath = $"{modelDataCell.AnimsPath}/idle_01";
                //    //    //UIManager.Instance.SetModel2UI(
                //    //    //    m_ModelViewDisplay.gameObject,
                //    //    //    ThreeDModelPos.Front,
                //    //    //    scale: new Vector3(4.5f, 4.5f, 4.5f),
                //    //    //    WorleRotate: Vector3.zero,
                //    //    //    localRotate: new Vector3(0, 180, 0),
                //    //    //    localPos: new Vector3(0, -4f, 0),
                //    //    //    animPath
                //    //    //    );

                //    //    // 二： Raw相机渲染，3D模型
                //    //    //ModelDataCell modelDataCell = LocalDataManager.Instance.GetModelDataCell(modelId);
                //    //    //string animPath = $"{modelDataCell.AnimsPath}/idle_01";
                //    //    //UIManager.Instance.SetModel2UI(
                //    //    //    m_ModelViewDisplay.gameObject,
                //    //    //    ThreeDModelPos.Raw,
                //    //    //    scale: Vector3.one,
                //    //    //    WorleRotate: Vector3.zero,
                //    //    //    localRotate: Vector3.zero,
                //    //    //    localPos: Vector3.zero,
                //    //    //    animPath
                //    //    //    );

                //    //    // 设置相机为默认角度
                //    //}
                //}
                #endregion
            }
        }

        private void CreateDisplayCB(GameObject go)
        {
            if (go != null)
            {
                if (m_AvatarDataCell == null)
                {
                    Service.Resource.ResourceFormalManager.Instance.PushGameObject(UIDef.DisplayModelPath, go);
                    return;
                }
                m_uIAsyncLoadState = UIAsyncLoadState.LoadFinish;

                string path = m_ModelDataCell.ModelsPath;
                path = path.Replace("Assets/Res/", string.Empty);
                m_ModelPath = path;

                string anim = "idle_01";
                if (m_CharacterCreateDataCell != null)
                {
                    if (string.IsNullOrEmpty(m_CharacterCreateDataCell.AppreaceAnim) || string.IsNullOrWhiteSpace(m_CharacterCreateDataCell.AppreaceAnim))
                    {
                        SGF.Debuger.LogWarning($"{LOG_TAG} SetModel() Jobid={m_CharacterCreateDataCell.GetJobID()},AppreaceAnim=null");
                    }
                    else
                    {
                        anim = m_CharacterCreateDataCell.AppreaceAnim;
                    }
                }

                UIManager.Instance.SetModel2UI2(
                    go,
                    m_AvatarDataCell,
                    ThreeDModelPos.None,
                    scale: Vector3.one,
                    WorleRotate: Vector3.zero,
                    localRotate: Vector3.zero,
                    localPos: Vector3.zero,
                    anim,
                    true,
                    E_LayerType.Entity
                );
                go.SetActive(true);

                m_ModelViewDisplay = go.GetComponent<RoleViewDisplay>();
                // 转向归0
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                m_ModelViewDisplay.SetRotation(rotation);
                M_Rawimg.SetModelView(m_ModelViewDisplay);
            }
            else
            {
                m_uIAsyncLoadState = UIAsyncLoadState.None;
            }
        }

        private void SetRoleViewDisplayData(string path)
        {
            if (m_ModelViewDisplay == null)
            {
                return;
            }

            m_ModelPath = path;

            string anim = "idle_01";
            if (m_CharacterCreateDataCell != null)
            {
                if (string.IsNullOrEmpty(m_CharacterCreateDataCell.AppreaceAnim) || string.IsNullOrWhiteSpace(m_CharacterCreateDataCell.AppreaceAnim))
                {
                    SGF.Debuger.LogWarning($"{LOG_TAG} SetModel() Jobid={m_CharacterCreateDataCell.GetJobID()},AppreaceAnim=null");
                }
                else
                {
                    anim = m_CharacterCreateDataCell.AppreaceAnim;
                }
            }
            //m_ModelViewDisplay.SetIsPoolsLoad(false);
            m_ModelViewDisplay.InitData(m_AvatarDataCell, anim, E_LayerType.Entity.ToString(), true);

            // 转向归0
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            m_ModelViewDisplay.SetRotation(rotation);
        }

        private void SetJobLabel()
        {
            string label = m_curItemJob.M_CharacterDataCell.Label;
            string[] labels = label.Split(",");
            int count = labels.Length;
            int childCount = M_JobLabelRoot.childCount;
            int maxLength = Mathf.Max(childCount, count);
            for (int i = 0; i < maxLength; i++)
            {
                string child = count > i ? labels[i] : "";
                if (childCount > i)
                {
                    Transform item = M_JobLabelRoot.GetChild(i);
                    ItemRoleLabel sp = item.GetComponent<ItemRoleLabel>();
                    sp.InitItemRoleLabelInfo(child);
                }
                else
                {
                    AddJobLabel(child);
                }
            }
        }

        private void AddJobLabel(string label)
        {
            var gob = GameObject.Instantiate<GameObject>(M_JobLabelPrefab);
            gob.transform.SetParent(M_JobLabelRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            ItemRoleLabel sp = gob.GetComponent<ItemRoleLabel>();
            sp.InitItemRoleLabelInfo(label);
        }

        private void SetJobSwitch()
        {
            List<string> iconPaths = m_curItemJob.M_CharacterDataCell.Switch;
            int switchCount = iconPaths.Count;
            List<string> jobNames = m_curItemJob.M_TransferJobName;
            int transferJobNameCount = jobNames.Count;
            int TransferJobCount = Mathf.Min(switchCount, transferJobNameCount);
            int childCount = M_JobSwitchRoot.childCount;
            int maxLength = Mathf.Max(childCount, TransferJobCount);
            for (int i = 0; i < maxLength; i++)
            {
                string iconPath = TransferJobCount > i ? iconPaths[i] : "";
                string jobName = TransferJobCount > i ? jobNames[i] : "";
                if (childCount > i)
                {
                    Transform item = M_JobSwitchRoot.GetChild(i);
                    ItemRoleSwitch sp = item.GetComponent<ItemRoleSwitch>();
                    sp.InitItemRoleSwitch(jobName, iconPath);
                }
                else
                {
                    AddJobSwitch(jobName, iconPath);
                }
            }
        }

        private void AddJobSwitch(string jobName, string iconPath)
        {
            var gob = GameObject.Instantiate<GameObject>(M_JobSwitchPrefab);
            gob.transform.SetParent(M_JobSwitchRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            ItemRoleSwitch sp = gob.GetComponent<ItemRoleSwitch>();
            sp.InitItemRoleSwitch(jobName, iconPath);
        }

        private void SetJobCapacity()
        {
            List<int> powers = m_curItemJob.M_CharacterDataCell.Power_Value;
            string label = m_curItemJob.M_CharacterDataCell.Label;
            string[] labels = label.Split(",");
            int count = labels.Length;
            int childCount = M_JobCapacityRoot.childCount;
            int maxLength = Mathf.Max(childCount, count);
            for (int i = 0; i < maxLength; i++)
            {
                string child = count > i ? labels[i] : "";
                int power = count > i ? powers[i] : 0;
                if (childCount > i)
                {
                    Transform item = M_JobCapacityRoot.GetChild(i);
                    ItemRoleCapacity sp = item.GetComponent<ItemRoleCapacity>();
                    sp.InitItemRoleCapacity(child, power);
                }
                else
                {
                    AddJobCapacity(child, power);
                }
            }
        }

        private void AddJobCapacity(string label, int power)
        {
            var gob = GameObject.Instantiate<GameObject>(M_JobCapacityPrefab);
            gob.transform.SetParent(M_JobCapacityRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            ItemRoleCapacity sp = gob.GetComponent<ItemRoleCapacity>();
            sp.InitItemRoleCapacity(label, power);
        }

        private void OnClickBename()
        {
            if (m_curItemJob != null)
            {
                bool canRealCreate = m_curItemJob.M_CharacterDataCell.GetJobID() != 10;
                if (!canRealCreate) { return; }

                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_STAR_ENTER_GAME);

                //UIWidget wnd = UIManager.Instance.OpenWidget(UIDef.BenameWidget, true, m_curItemJob.M_CharacterDataCell, this.M_WidgetRoot, MainPageCommond.HideNone, false, false);

                //wnd.onClose += (arg) =>
                //{
                //    string name = Convert.ToString(arg);
                //    if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
                //    {
                //        // 确定创角
                //        if (m_curItemJob != null)
                //        {
                //            m_Mymodule.CreateAccount(name, 1, Convert.ToInt32(m_curItemJob.M_CharacterDataCell.GetJobID()));
                //        }
                //    }
                //};
                Action<UIWidget> action = (UIWidget ui) =>
                {
                    if (ui != null)
                    {
                        BenameWidget widget = (BenameWidget)ui;
                        widget.SetSureCB(BenameWidgetcb);
                        //ui.onClose += (arg) =>
                        //{
                        //    string name = Convert.ToString(arg);
                        //    if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
                        //    {
                        //        // 确定创角
                        //        if (m_curItemJob != null)
                        //        {
                        //            m_Mymodule.CreateAccount(name, 1, Convert.ToInt32(m_curItemJob.M_CharacterDataCell.GetJobID()));
                        //        }
                        //    }
                        //};
                    }
                };
                UIManager.Instance.OpenWidgetAsync(UIDef.BenameWidget, action, true, null, M_WidgetRoot, MainPageCommond.HideNone, false, false);
            }
        }

        private void BenameWidgetcb(string name)
        {
            if (m_curItemJob != null)
            {
                SDKManager.Instance.EventPreEvent(E_SDK_PreEvent.CLICK_CONFIRM_ENTER_GAME);

                m_Mymodule.CreateAccount(name, 1, Convert.ToInt32(m_curItemJob.M_CharacterDataCell.GetJobID()), CloseBenameWidget);
            }
        }

        private void CloseBenameWidget(bool isClose)
        {
            if (isClose)
            {
                UIManager.Instance.CloseWidget(UIDef.BenameWidget, M_WidgetRoot);
            }
        }

        private void OnBtnClickCreateNewRole()
        {
            SetDefaultSelectJobItem();
            SetDefaultSelectJobTabItem();
            m_IsSelectRolePanel = false;
            M_CreatorRolePanel.SetActive(true);
            M_SelectRolePanel.SetActive(false);
            SetCreateRoleWindowBgRootActive(false);
            //SetTitleText(GameConfig.LocalStr["CreateRoleTitle2"]);
            SetTitleText(LanguageManager.Instance.GetLanguageByKey("CreateRoleTitle2"));
        }

        private void OnJbtnClickMan(GameObject god)
        {
            // 判断当前职业是否有男性职业
            bool isContains = CheckIsContainsSex(1);
            if (isContains)
            {
                SelectSexBtn(JBtnMan.transform, true);
                SelectSexBtn(JBtnWoman.transform, false);

                m_CurrentCheckSex = 1;
            }
            else
            {
                //Frame.Util.ShowSystemMessage(GameConfig.LocalStr["CreateRoleTips"]);
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("CreateRoleTips"));
            }
        }

        private void OnJbtnClickWoman(GameObject god)
        {
            // 判断当前职业是否有女性职业
            bool isContains = CheckIsContainsSex(2);
            if (isContains)
            {
                SelectSexBtn(JBtnMan.transform, false);
                SelectSexBtn(JBtnWoman.transform, true);

                m_CurrentCheckSex = 2;
            }
            else
            {
                //Frame.Util.ShowSystemMessage(GameConfig.LocalStr["CreateRoleTips2"]);
                Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("CreateRoleTips2"));
            }
        }

        private void SelectSexBtn(Transform JbtnTran, bool isSelect)
        {
            JbtnTran.Find("noselected").gameObject.SetActive(!isSelect);
            JbtnTran.Find("selected").gameObject.SetActive(isSelect);
        }

        // 男：1 女：2
        private bool CheckIsContainsSex(int sexInt)
        {
            bool isContains = false;
            if (m_curItemJob != null)
            {
                CharacterCreateDataCell characterDataCell = m_curItemJob.M_CharacterDataCell;
                if (characterDataCell != null)
                {
                    List<int> sexList = characterDataCell.SexSelect;
                    if (sexList.Contains(sexInt))
                    {
                        isContains = true;
                    }
                }
            }
            return isContains;
        }


        private void OnBtnClose()
        {
            if (!m_IsSelectRolePanel && m_IsHavePlayers)
            {
                SetDefaultSelectRoleItem();
                m_IsSelectRolePanel = true;
                M_CreatorRolePanel.SetActive(false);
                M_SelectRolePanel.SetActive(true);
                SetCreateRoleWindowBgRootActive(true);
                //SetTitleText(GameConfig.LocalStr["CreateRoleTitle1"]);
                SetTitleText(LanguageManager.Instance.GetLanguageByKey("CreateRoleTitle1"));
            }
            else
            {
                m_Mymodule.SetPageActive(true, false);
                GameManager.Instance.IsLoginInitState = true;
                SGF.Debuger.Log($"[公告] UICreateRoleWindow OnBtnClose set IsLoginInitState : {GameManager.Instance.IsLoginInitState}");
                this.Close();
            }
        }

        private void AddMyPlayerItem(PlayerLoginData playerLoginData, int idx, int maxLength)
        {
            var gob = GameObject.Instantiate<GameObject>(M_MyPlayerPrefab);
            gob.transform.SetParent(M_SelectRoleRoot);
            gob.transform.localPosition = Vector3.zero;
            gob.transform.SetLocalScale(Vector3.one);
            SetMyPlayerItem(gob, playerLoginData, idx, maxLength);
        }

        private void SetMyPlayerItem(GameObject gob, PlayerLoginData playerLoginData, int idx, int maxLength)
        {
            ItemMyPlayer sp = gob.GetComponent<ItemMyPlayer>();
            sp.InitMyPlayerInfo(playerLoginData, this);
            sp.M_Btn.onClick.RemoveAllListeners();
            if (playerLoginData != null)
            {
                sp.M_Btn.onClick.AddListener(
                    () =>
                    {
                        HandleMyPlayerItem(sp);
                    }
                );
            }
            // 默认选择
            if (m_IsHaveRecordLoginPID)
            {
                if (playerLoginData != null && playerLoginData.PID == m_Mymodule.M_RecordLoginPID)
                {
                    if (sp.isShow)
                    {
                        HandleMyPlayerItem(sp);
                        //M_SelectRoleScrollView.Nevigate2(gob.GetComponent<RectTransform>());
                        DelayInvoker.DelayInvoke(DelayKey, 0.1f, DelayShowModel, new object[] { gob.GetComponent<RectTransform>() });
                        //M_SelectRoleScrollView.CenterPoint(gob.GetComponent<RectTransform>());
                    }
                }
            }
            else
            {
                if (!m_IsDefaultCheck && sp.M_JobDataCell != null)
                {
                    if (gob.activeInHierarchy)
                    {
                        HandleMyPlayerItem(sp);
                        m_IsDefaultCheck = true;
                    }
                    else
                    {
                        // 清空旧的模型
                        PushDisplayGob();
                        m_ModelDataCell = null;
                        m_AvatarDataCell = null;
                        m_CharacterCreateDataCell = null;
                    }
                }
                if (idx + 1 == maxLength && !m_IsDefaultCheck)
                {
                    // 清空旧的模型
                    PushDisplayGob();
                    m_ModelDataCell = null;
                    m_AvatarDataCell = null;
                    m_CharacterCreateDataCell = null;
                    m_IsHavePlayers = false;
                    M_CreatorRolePanel.SetActive(!m_IsHavePlayers);
                    M_SelectRolePanel.SetActive(m_IsHavePlayers);
                    SetCreateRoleWindowBgRootActive(m_IsHavePlayers);
                    if (!m_IsHavePlayers)
                    {
                        SetDefaultSelectJobItem();
                        SetDefaultSelectJobTabItem();
                    }
                }
            }
        }

        private void DelayShowModel(object[] args)
        {
            RectTransform gob = (RectTransform)args[0];
            if (gob == null)
            {
                return;
            }
            M_SelectRoleScrollView.Nevigate(gob);
        }

        private void ClearDelay()
        {
            if (!DelayInvoker.ContainInvoke(DelayKey))
            {
                return;
            }
            DelayInvoker.CancelInvoke(DelayKey);
        }

        private void HandleMyPlayerItem(ItemMyPlayer playerItem)
        {
            // 隐藏上次的tab
            if (m_MyPlayerItem != null)
            {
                m_MyPlayerItem.SetSelected(false);
            }
            // 开启当前的tab
            m_MyPlayerItem = playerItem;
            if (m_MyPlayerItem != null)
            {
                m_MyPlayerItem.SetSelected(true);
                CheckPlayerData = m_MyPlayerItem.M_PlayerLoginData;
            }

            // 设置模型
            m_ModelDataCell = m_MyPlayerItem.M_ModelDataCell;
            m_AvatarDataCell = m_MyPlayerItem.M_AvatarDataCell;
            m_CharacterCreateDataCell = m_MyPlayerItem.M_CharacterDataCell;

            SetModel();
        }

        private void ChossHeroAckCallBack(bool isSuccess)
        {
            if (isSuccess)
            {
                //PushDisplayGob();
            }
            else
            {
                m_isClickStartGame = true;
            }
        }

        private void OnBtnStartGame()
        {
            if (CheckPlayerData != null && m_isClickStartGame)
            {
                m_isClickStartGame = false;
                m_Mymodule.ChoseHeroAck(CheckPlayerData.PID, ChossHeroAckCallBack);
            }
            else if (!m_isClickStartGame)
            {
                DisplayProcessDispenser.Instance.AddSpecialMessage(LanguageManager.Instance.GetLanguageByKey("RequestTimeoutTips"));
            }
            else
            {
                //DisplayProcessDispenser.Instance.AddSpecialMessage("点击响应点击响应点击响应点击响应点击响应点击响应...");
            }
        }

        public void OnBtnDelRole()
        {
            if (m_MyPlayerItem != null)
            {
                //UIWidget wnd = UIManager.Instance.OpenWidget(UIDef.DeleteHeroWidget, true, m_MyPlayerItem.M_PlayerLoginData, this.M_WidgetRoot, MainPageCommond.HideNone, false, false);
                //wnd.onClose += (arg) =>
                //{
                //    string name = Convert.ToString(arg);
                //    if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
                //    {
                //        m_Mymodule.DeleteHeroAck(m_MyPlayerItem.M_PlayerLoginData.PID, GameLoginInfo.M_PickGroupReq.GroupID, OnDeleteHeroAck);
                //    }
                //};

                Action<UIWidget> action = (UIWidget ui) =>
                {
                    if (ui != null)
                    {
                        ui.onClose += (arg) =>
                        {
                            string name = Convert.ToString(arg);
                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
                            {
                                m_Mymodule.DeleteHeroAck(m_MyPlayerItem.M_PlayerLoginData.PID, GameLoginInfo.M_PickGroupReq.GroupID, OnDeleteHeroAck);
                            }
                        };
                    }
                };
                UIManager.Instance.OpenWidgetAsync(UIDef.DeleteHeroWidget, action, true, m_MyPlayerItem.M_PlayerLoginData, this.M_WidgetRoot, MainPageCommond.HideNone, false, false);
            }
        }

        private void OnBtnDelAllRole()
        {
            for (int i = M_SelectRoleRoot.childCount - 1; i >= 0; i--)
            {
                var item = M_SelectRoleRoot.GetChild(i);
                var sp = item.GetComponent<ItemMyPlayer>();
                if (sp.M_PlayerLoginData != null)
                {
                    m_Mymodule.DeleteHeroAck(sp.M_PlayerLoginData.PID, GameLoginInfo.M_PickGroupReq.GroupID, null);

                }
            }

            OnBtnClose();
        }

        private void OnDeleteHeroAck(bool isSuccess)
        {
            if (!isSuccess)
            {
                return;
            }
            if (m_MyPlayerItem != null)
            {
                m_MyPlayerItem.DelMyPlayerInfo();

                SetDefaultSelectRoleItem();
                MyPlayerCount--;
                int realMyPlayerCount = 0;
                for (int i = 0; i < M_SelectRoleRoot.childCount; i++)
                {
                    var child = M_SelectRoleRoot.GetChild(i);
                    if (child.gameObject.activeInHierarchy)
                    {
                        realMyPlayerCount++;
                    }
                }
                bool isShowCreateRoleBtn = realMyPlayerCount < SystemConstConfigs.CreateCharacterLimit;
                CreatRoleRoot.SetActive(isShowCreateRoleBtn);
            }
        }

        private void SetDefaultSelectRoleItem()
        {
            bool isHaveMyPlayer = false;
            int count = M_SelectRoleRoot.childCount;
            for (var i = 0; i < count; i++)
            {
                Transform child = M_SelectRoleRoot.GetChild(i);
                ItemMyPlayer itemMyPlayer = child.GetComponent<ItemMyPlayer>();
                if (itemMyPlayer != null && itemMyPlayer.M_PlayerLoginData != null)
                {
                    HandleMyPlayerItem(itemMyPlayer);
                    isHaveMyPlayer = true;
                    break;
                }
            }
            if (!isHaveMyPlayer)
            {
                m_IsHavePlayers = false;
                OnBtnClickCreateNewRole();
            }
        }

        private void SetTitleText(string text)
        {
            M_Title.text = text;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            GameManager.Instance.IsLoginInitState = false;
            SGF.Debuger.Log($"[公告] UICreateRoleWindow OnEnable set IsLoginInitState : {GameManager.Instance.IsLoginInitState}");
        }



        protected override void OnClose(object arg = null)
        {
            _SelectRoleRoot = null;
            _CreateRoleRoot = null;
            _CreateRoleBgMat = null;
            _CreateRoleFrontMat = null;
            if (CreateRoleWindowBgRootGo != null)
            {
                GameObject.DestroyImmediate(CreateRoleWindowBgRootGo);
            }
            CreateRoleWindowBgRootGo = null;
            //TODO：曲 这里都要处理空---
            ClearDelay();
            PushDisplayGob();
            M_Rawimg.SetModelView(null);

            m_curItemJob = null;
            MyPlayerCount = 0;
            m_MyPlayerItem = null;    // 当前选中的角色
            CheckPlayerData = null;    // 当前选中的角色信息
            m_isClickStartGame = true;
            //------------
            m_CurrentCheckSex = -1;  // 当前选中的性别
            m_ModelDataCell = null;  // 模型信息
            m_AvatarDataCell = null;
            m_CharacterCreateDataCell = null;  // 创角信息
            m_ModelViewDisplay = null;
            m_ModelPath = "";
            m_uIAsyncLoadState = UIAsyncLoadState.None;

            m_IsSelectRolePanel = false;
            m_IsHavePlayers = false;
            m_IsHaveRecordLoginPID = false;
            m_IsDefaultCheck = false;

            base.OnClose(arg);
        }

        private void PushDisplayGob()
        {
            if (m_ModelViewDisplay != null)
            {
                Service.Resource.ResourceFormalManager.Instance.PushGameObject(UIDef.DisplayModelPath, m_ModelViewDisplay.gameObject);
                m_ModelViewDisplay = null;
                M_Rawimg.SetModelView(m_ModelViewDisplay);
                m_ModelPath = "";
            }
            m_uIAsyncLoadState = UIAsyncLoadState.Remove;
            SGF.Debuger.LogWarning("----------------------------- 删除创角的模型");
        }

    }
}
