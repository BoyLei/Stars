using SGF.Unity;
using StarProject.Game;
using StarProject.Service.AtlasManager;
using StarProject.Service.LocalData;
using StarProject.Service.SDK;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SGF.UI.Framework
{
    public class PartnerAppearedWidget : UIWidget
    {
        private string TagFlag = "[PartnerAppearedWidget]";

        private Image Icon;
        private Image Bg;
        private Transform BgPartner;
        private Transform FxPartner;

        private Action<Sprite> loadBgIcon;
        private Action<Sprite> loadIcon;



        protected override void Awake()
        {
            base.Awake();
            Icon = transform.Find("Cut4Cam90/AssemblyRatio2/Anim/Mask/Anim/ImgRole").GetComponent<Image>();
            Bg = transform.Find("Cut4Cam90/AssemblyRatio2/Anim/Bg").GetComponent<Image>();

            BgPartner = transform.Find("Cut4Cam90/AssemblyRatio2/Anim/Fx").transform;
            FxPartner = transform.Find("Cut4Cam90/AssemblyRatio2/Anim/Fx01").transform;
        }

        protected override void OnOpen(object arg = null)
        {
            base.OnOpen(arg);

            int partnerConfID = -1;
            int.TryParse(arg.ToString(), out partnerConfID);
            GameManager.Instance.EventPreNewPlayerEvent($"5_{partnerConfID}");

            PartnerDataCell partnerDataCell = LocalDataManager.Instance.GetPartnerDataCell(partnerConfID);
            if (partnerDataCell != null)
            {
                // 图标
                AvatarDataCell m_AvatarDataCell = LocalDataManager.Instance.GetAvatarDataCell(partnerDataCell.GetAvatarID());
                if (m_AvatarDataCell != null)
                {
                    var headcfg = LocalDataManager.Instance.GetModelHeadDataCell(m_AvatarDataCell.GetHeadID());
                    if (headcfg != null)
                    {
                        Icon.color = Color.clear;
                        string path = $"{headcfg.HalfDrawing}_Head";
                        //string path = $"{headcfg.HalfDrawing}";
                        //Addressables.LoadAssetAsync<Sprite[]>("Assets/Resources/Sprite/Bomberman.png");
                        //StarProject.Service.Resource.ResourceFormalManager.Instance.LoadAssetAllSync<Sprite>()
                        loadIcon = (Sprite sp) =>
                        {
                            if (Icon != null && sp != null)
                            {
                                Icon.sprite = sp;
                                Icon.color = Color.white;
                                PlayAnimation("PartnerAppearedWidget", OnAnimFinish);
                            }
                        };
                        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAtlasAsync(AtlasManager.AtlasPathRoleHead + headcfg.HalfDrawing, path, loadIcon);
                    }
                }
                for (int i = 0; i < BgPartner.childCount; i++)
                {
                    var child = BgPartner.GetChild(i);
                    if (child != null)
                    {
                        child.gameObject.SetActive(i + 1 == partnerDataCell.GetQuality());
                    }
                }
                for (int i = 0; i < FxPartner.childCount; i++)
                {
                    var child = FxPartner.GetChild(i);
                    if (child != null)
                    {
                        child.gameObject.SetActive(i + 1 == partnerDataCell.GetQuality());
                    }
                }
                // 背景
                int quality = partnerDataCell.GetQuality();
                string qualityBgName = $"{AtlasManager.AtlasPathRoleHead}HudRole_Bg";
                switch (quality)
                {
                    case 1:
                        qualityBgName += "N";
                        break;
                    case 2:
                        qualityBgName += "R";
                        break;
                    case 3:
                        qualityBgName += "SR";
                        break;
                    case 4:
                        qualityBgName += "SSR";
                        break;
                    default:
                        break;
                }
                loadBgIcon = (Sprite sp) =>
                {
                    if (Bg != null && sp != null)
                    {
                        Bg.sprite = sp;
                    }
                };
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadSpriteAsync(qualityBgName, loadBgIcon);

            }
            // 保底5秒一定关闭
            DelayInvoker.DelayInvoke(TagFlag, 5, DelayClose, new object[] { });
        }

        protected override void OnClose(object arg = null)
        {
            base.OnClose(arg);
            if (DelayInvoker.ContainInvoke(TagFlag))
            {
                DelayInvoker.CancelInvoke(TagFlag);
            }
        }

        private void DelayClose(object[] args)
        {
            OnAnimFinish();
        }

        private void OnAnimFinish()
        {
            loadBgIcon = null;
            loadIcon = null;
            UIManager.Instance.CloseWidget(UIDef.PartnerAppearedWidget, null, true);
        }

    }
}
