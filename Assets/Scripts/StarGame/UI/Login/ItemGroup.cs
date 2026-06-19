using StarProject.Service.AtlasManager;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace StarProject.UI.Login
{
    public class ItemGroup : MonoBehaviour
    {
        public Button M_Btn;
        public Image M_GroupState;              // 服务器状态标识（推荐/新/无）
        public Text M_GroupStateText;              // 服务器状态标识（推荐/新/无）
        public Image M_GroupLoadState;          // 服务器负载状态标识（火爆：红/拥挤：黄/正常：绿/维护中：灰） 
        public Text M_GroupName;                // 服务器名
        public Text M_GroupCreateTime;          // 服务器创建时间（时间戳 秒）

        private Action<Sprite> loadGroupIcon;

        public GroupInfo M_GroupInfo;

        public void InitItemGroupInfo(GroupInfo groupInfo)
        {
            M_GroupInfo = groupInfo;
            if (M_GroupInfo == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                // 设置推荐/新状态
                {
                    //string groupStateIconPath = string.Empty;
                    string groupState = string.Empty;
                    if (M_GroupInfo.isRecommend)
                    {
                        //groupStateIconPath = $"UI/LoginPage/Textures/Group_State_Recommend";
                        groupState = LanguageManager.Instance.GetLanguageByKey("RecommondStr");
                    }
                    else if (M_GroupInfo.isNew)
                    {
                        //groupStateIconPath = $"UI/LoginPage/Textures/Group_State_New";
                        groupState = LanguageManager.Instance.GetLanguageByKey("NewStr");
                    }
                    M_GroupStateText.text = groupState;

                    M_GroupState.gameObject.SetActive(!string.IsNullOrEmpty(groupState));
                }
                // 设置服务器负载状态
                {
                    //int state = 1;
                    //if (M_GroupInfo.isRecommend)
                    //{
                    //    state = 3;
                    //}
                    //else if (M_GroupInfo.groupLoad == 0)
                    //{
                    //    state = 0;
                    //}
                    //string iconPath = $"UI/LoginPage/Textures/Group_State_{state}";

                    //var img = AtlasManager.Instance.GetSprite(AtlasPath, "Signin_Explosivedegree_fluent");
                    //if (img != null && M_GroupLoadState != null)
                    //{
                    //    M_GroupLoadState.sprite = img;
                    //}

                    string stateIconName = "Signin_Explosivedegree_fluent";
                    if (M_GroupInfo.groupLoad < 0)
                    {
                        stateIconName = "Signin_Explosivedegree_Undermaintenance";
                    }
                    loadGroupIcon = (Sprite sp) =>
                    {
                        if (M_GroupLoadState != null && sp != null)
                        {
                            M_GroupLoadState.sprite = sp;
                        }
                        loadGroupIcon = null;
                    };
                    AtlasManager.Instance.GetSpriteAsync(AtlasManager.AtlasPathLogin, stateIconName, loadGroupIcon);
                }

                bool isClick = M_GroupInfo.groupLoad >= 0;
                if (isClick)
                {
                    M_GroupName.color = new(0.286f, 0.282f, 0.282f);
                }
                else
                {
                    M_GroupName.color = new(0.75f, 0.75f, 0.75f);
                }
                M_GroupName.text = M_GroupInfo.groupName;

                DateTime dateTime = SGF.Time.TimeUtils.GetDateTime((int)M_GroupInfo.CreateTime);
                M_GroupCreateTime.text = dateTime.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);
                gameObject.SetActive(true);
            }
        }
    }
}
