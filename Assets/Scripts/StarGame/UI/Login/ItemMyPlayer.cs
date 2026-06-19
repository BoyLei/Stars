using SGF.Module.Framework;
using SGF.UI.Framework;
using StarProject.Module;
using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemMyPlayer : MonoBehaviour
    {
        private CreateRoleWindow windowInstance;
        public Button M_Btn;
        // 正常
        public GameObject M_NormalGob;
        public Image M_NormalRoleIcon;           //角色icon
        public Text M_NormalNickName;            //昵称
        public Text M_NormalLevel;               //等级
        public Image M_NormalJobBg;           //职业背景
        public Image M_NormalJobIcon;           //职业icon

        // 选中
        public GameObject M_SelectedGob;
        public Image M_SelectedRoleIcon;           //角色icon
        public Text M_SelectedNickName;            //昵称
        public Text M_SelectedLevel;               //等级
        public Image M_SelectedJobBg;           //职业背景
        public Image M_SelectedJob;           //职业icon
        public JButton M_DeleteRole;           //删除角色

        public PlayerLoginData M_PlayerLoginData;  // 角色登录信息
        public JobDataCell M_JobDataCell;  // 创角信息
        public ModelDataCell M_ModelDataCell;  // 模型信息
        public AvatarDataCell M_AvatarDataCell;  // 模型信息
        public CharacterCreateDataCell M_CharacterDataCell;  // 创角信息

        public bool isShow = false;

        private Action<Sprite> loadRoleIcon;
        private Action<Sprite> loadJobTypeIcon;
        private Action<Sprite> loadJobTypeBgIcon;

        private void Awake() 
        {
            M_DeleteRole.OnClick = (go) =>
            {
                windowInstance.OnBtnDelRole();
            };
        }

        public void InitMyPlayerInfo(PlayerLoginData playerLoginData, CreateRoleWindow window)
        {
            windowInstance = window;
            M_PlayerLoginData = playerLoginData;
            if (M_PlayerLoginData == null)
            {
                isShow = false;
                gameObject.SetActive(false);
            }
            else
            {
                isShow = true;
                M_JobDataCell = LocalDataManager.Instance.GetJobDataCell(playerLoginData.JobID);
                if (M_JobDataCell != null)
                {
                    M_CharacterDataCell = LocalDataManager.Instance.GetCharacterCreateDataCellBy(M_JobDataCell.GetBaseJob());
                    M_AvatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(M_JobDataCell.HighAvatarID);
                    if (M_AvatarDataCell != null)
                    {
                        M_ModelDataCell = LocalDataManager.Instance.GetModelDataCell(M_AvatarDataCell.GetHighModelId());
                    }
                }

                if (M_JobDataCell == null || M_AvatarDataCell == null || M_ModelDataCell == null)
                {
                    M_PlayerLoginData = null;
                    isShow = false;
                }

                if (!isShow)
                {
                    gameObject.SetActive(false);
                    return;
                }

                string level = string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), M_PlayerLoginData.Level);
                // 头像设置
                {
                    ModelHeadDataCell modelHeadDataCell = LocalDataManager.Instance.GetModelHeadDataCell(M_AvatarDataCell.GetHeadID());
                    if (modelHeadDataCell != null)
                    {
                        loadRoleIcon = (Sprite sp) =>
                        {
                            if (M_NormalRoleIcon != null && sp != null)
                            {
                                M_NormalRoleIcon.sprite = sp;
                            }
                            if (M_SelectedRoleIcon != null && sp != null)
                            {
                                M_SelectedRoleIcon.sprite = sp;
                            }
                        };
                        AtlasManager.Instance.GetSpriteAsync(modelHeadDataCell.HeadAtlasName, modelHeadDataCell.MinHead, loadRoleIcon);
                    }
                }
                // 职业类型icon设置
                {
                    int jobshi = Mathf.FloorToInt(playerLoginData.JobID / 10);
                    //-- 职业类型背景
                    string jobTypeBgName = $"Icon_ArmBg{jobshi}";
                    loadJobTypeBgIcon = (Sprite sp) =>
                    {
                        if (M_NormalJobBg != null && sp != null)
                        {
                            M_NormalJobBg.sprite = sp;
                        }
                        if (M_SelectedJobBg != null && sp != null)
                        {
                            M_SelectedJobBg.sprite = sp;
                        }
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathRole, jobTypeBgName, loadJobTypeBgIcon);

                    //-- 职业类型
                    string jobTypeName = $"Icon_Arm{jobshi}";
                    loadJobTypeIcon = (Sprite sp) =>
                    {
                        if (M_NormalJobIcon != null && sp != null)
                        {
                            M_NormalJobIcon.sprite = sp;
                        }
                        if (M_SelectedJob != null && sp != null)
                        {
                            M_SelectedJob.sprite = sp;
                        }
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathRole, jobTypeName, loadJobTypeIcon);
                }

                M_NormalNickName.text = M_PlayerLoginData.NickName;
                M_NormalLevel.text = level;
                M_SelectedNickName.text = M_PlayerLoginData.NickName;
                M_SelectedLevel.text = level;

                gameObject.SetActive(true);
            }
            SetSelected(false);
        }

        public void DelMyPlayerInfo()
        {
            M_PlayerLoginData = null;
            M_JobDataCell = null;
            M_ModelDataCell = null;
            M_AvatarDataCell = null;
            M_CharacterDataCell = null;
            gameObject.SetActive(false);
            loadRoleIcon = null;
            loadJobTypeIcon = null;
            loadJobTypeBgIcon = null;
        }

        public void SetSelected(bool isSelect)
        {
            M_NormalGob.SetActive(!isSelect);
            M_SelectedGob.SetActive(isSelect);
            M_DeleteRole.gameObject.SetActive(isSelect);
        }
    }
}
