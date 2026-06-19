using SGF.UI.Framework;
using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DeleteHeroWidget : UIWidget
{
    public Image M_RoleIcon;          
    public Image M_JobTypeIcon;
    public Image M_JobTypeBg;
    public Text M_NickName;            
    public Text M_Level;              
    public InputField M_Input;

    public PlayerLoginData M_PlayerLoginData;  // 角色信息

    private Action<Sprite> loadRoleIcon;
    private Action<Sprite> loadJobTypeIcon;
    private Action<Sprite> loadJobTypeBgIcon;

    protected override void Awake()
    {
        {
            JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/Content/BtnCancel").GetComponent<JButton>();
            Btn.OnClick = (go) =>
            {
                Close();
            };
        }



        {
            JButton Btn = transform.Find("Cut4Cam90/AssemblyRatio2/Content/BtnOk").GetComponent<JButton>();
            Btn.OnClick = (go) =>
            {
                string inputText = Convert.ToString(M_Input.text);
                if (!string.IsNullOrEmpty(inputText) && !string.IsNullOrWhiteSpace(inputText))
                {
                    bool isEquals = inputText.Equals(LanguageManager.Instance.GetLanguageByKey("DeletePlayerTips"), StringComparison.CurrentCultureIgnoreCase);
                    if (!isEquals)
                    {
                        Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("DeletePlayerTips2"));
                    }
                    else
                    {
                        Close(true);
                    }
                }
                else
                {
                    Frame.Util.ShowSystemMessage(LanguageManager.Instance.GetLanguageByKey("DeletePlayerTips2"));
                }
            };
        }
    }

    protected override void OnOpen(object arg)
    {
        base.OnOpen(arg);
        M_PlayerLoginData = (PlayerLoginData)arg;
        M_Input.text = "";

        if (M_PlayerLoginData != null)
        {
            JobDataCell M_JobDataCell = LocalDataManager.Instance.GetJobDataCell(M_PlayerLoginData.JobID);
            AvatarDataCell M_AvatarDataCell = null;
            ModelDataCell M_ModelDataCell = null;

            if (M_JobDataCell != null)
            {
                M_AvatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(M_JobDataCell.GetAvatarID());
                if (M_AvatarDataCell != null)
                {
                    M_ModelDataCell = LocalDataManager.Instance.GetModelDataCell(M_AvatarDataCell.GetHighModelId());
                }
            }

            // 头像设置
            {
                ModelHeadDataCell modelHeadDataCell = LocalDataManager.Instance.GetModelHeadDataCell(M_AvatarDataCell.GetHeadID());
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
                int jobshi = Mathf.FloorToInt(M_PlayerLoginData.JobID / 10);
                //-- 职业类型背景
                string jobTypeBgName = $"Icon_ArmBg{jobshi}";
                loadJobTypeBgIcon = (Sprite sp) =>
                {
                    if (M_JobTypeBg != null && sp != null)
                    {
                        M_JobTypeBg.sprite = sp;
                    }
                };
                AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathRole, jobTypeBgName, loadJobTypeBgIcon);
                
                //-- 职业类型
                string jobTypeName = $"Icon_Arm{jobshi}";
                loadJobTypeIcon = (Sprite sp) =>
                {
                    if (M_JobTypeIcon != null && sp != null)
                    {
                        M_JobTypeIcon.sprite = sp;
                    }
                };
                AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathRole, jobTypeName, loadJobTypeIcon);
            
            }

            M_NickName.text = M_PlayerLoginData.NickName;
            //string level = $"{M_PlayerLoginData.Level}级";
            string level = string.Format(LanguageManager.Instance.GetLanguageByKey("LvStr"), M_PlayerLoginData.Level);
            M_Level.text = level;
        }
    }

    protected override void OnClose(object arg)
    {
        loadRoleIcon = null;
        loadJobTypeIcon = null;
        loadJobTypeBgIcon = null;
    }

}
