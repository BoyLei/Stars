using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemMyGroup : MonoBehaviour
    {
        public Image M_RoleIcon;           //角色icon
        public Text M_NickName;            //昵称
        public Text M_Level;               //等级
        public Text M_AreaName;            //所属大区
        public Image M_StateIcon;          //服务器状态icon
        public Image M_NormalJobBg;           //职业背景
        public Image M_NormalJobIcon;           //职业icon

        public OwnerGroupInfo M_OwnerGroupInfo;

        private Action<Sprite> loadRoleIcon;
        private Action<Sprite> loadJobTypeIcon;
        private Action<Sprite> loadJobTypeBgIcon;
        private Action<Sprite> loadStateIcon;

        public void InitItemOwnerGroupInfo(OwnerGroupInfo ownerGroupInfo)
        {
            M_OwnerGroupInfo = ownerGroupInfo;
            if (M_OwnerGroupInfo == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                // 加载角色Icon
                {
                    JobDataCell jobDataCell = LocalDataManager.Instance.GetJobDataCell((int)M_OwnerGroupInfo.heroID);
                    if (jobDataCell != null)
                    {
                        AvatarDataCell avatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(jobDataCell.GetAvatarID());
                        if (avatarDataCell != null)
                        {
                            ModelHeadDataCell modelHeadDataCell = LocalDataManager.Instance.GetModelHeadDataCell(avatarDataCell.GetHeadID());
                            if (modelHeadDataCell != null)
                            {
                                loadRoleIcon = (Sprite sp) =>
                                {
                                    if (M_RoleIcon != null && sp != null)
                                    {
                                        M_RoleIcon.sprite = sp;
                                    }
                                };
                                AtlasManager.Instance.GetSpriteAsync(modelHeadDataCell.HeadAtlasName, modelHeadDataCell.MinHead, loadRoleIcon);
                            }
                        }

                        // 职业类型icon设置
                        {
                            int job = jobDataCell.GetID();
                            int jobshi = Mathf.FloorToInt(job / 10);
                            //-- 职业类型背景
                            string jobTypeBgName = $"Icon_ArmBg{jobshi}";
                            loadJobTypeBgIcon = (Sprite sp) =>
                            {
                                if (M_NormalJobBg != null && sp != null)
                                {
                                    M_NormalJobBg.sprite = sp;
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
                            };
                            AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathRole, jobTypeName, loadJobTypeIcon);
                        }
                    }
                }

                M_AreaName.text = M_OwnerGroupInfo.groupName;
                M_NickName.text = M_OwnerGroupInfo.nickName;
                //M_Level.text = $"{M_OwnerGroupInfo.level}级";
                //M_Level.text = string.Format(GameConfig.LocalStr["LvStr"], M_OwnerGroupInfo.level);
                M_Level.text = string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), M_OwnerGroupInfo.level);
                gameObject.SetActive(true);
            }
        }

        public void RefreshGroupLoad(int groupLoad)
        {
            if (M_OwnerGroupInfo != null)
            {
                M_OwnerGroupInfo.groupLoad = groupLoad;
                RefreshGroupData();
            }
        }

        private void RefreshGroupData()
        {
            // 设置服务器负载状态
            {
                string stateIconName = "Signin_Explosivedegree_fluent";
                if (M_OwnerGroupInfo.groupLoad < 0)
                {
                    stateIconName = "Signin_Explosivedegree_Undermaintenance";
                }
                loadStateIcon = (Sprite sp) =>
                {
                    if (M_StateIcon != null && sp != null)
                    {
                        M_StateIcon.sprite = sp;
                    }
                };
                AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathLogin, stateIconName, loadStateIcon);
            }

            M_AreaName.text = M_OwnerGroupInfo.groupName;
            bool isClick = M_OwnerGroupInfo.groupLoad >= 0;
            if (isClick)
            {
                M_AreaName.color = new(0.286f, 0.282f, 0.282f);
            }
            else
            {
                M_AreaName.color = new(0.75f, 0.75f, 0.75f);
            }
        }
    }
}
