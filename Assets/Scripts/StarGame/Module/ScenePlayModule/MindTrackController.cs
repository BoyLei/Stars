///--------------------------------------------------------------------
/// 文件名   :   MindTrackController
/// 内  容   :   心灵追踪控制器
/// 说  明   :  
/// 创建日期 :   2024/08/21 16:30:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using Cysharp.Threading.Tasks;
using StarProject.Service.LocalData;
using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using ModuleManager = SGF.Module.Framework.ModuleManager;

namespace StarProject.Module
{
    public class MindTrackController : MonoBehaviour
    {
        public GameObject mRoot;
        public Image mImage;
        public Image mMaskImage;
        public Text mText;
        public JButton TrackButton;
        private int PlayType;
        private int PlayID;
        private int TaskID;
        private float Duration;

        private void Awake()
        {
            TrackButton.OnClick += OnClickHandler;
            GlobalEvent.OnShowScenePlay.AddListener(OnShowScenePlayHandler);
            Duration = 0;
            ScenePlayModule scenePlayModule =
                ModuleManager.Instance.GetModule(ModuleDef.Name.ScenePlayModule) as ScenePlayModule;
            if (scenePlayModule != null)
            {
                OnShowScenePlayHandler((int)scenePlayModule.CurrentScenePlayType, scenePlayModule.CurrentPlayID, scenePlayModule.TaskID);
            }
        }

        private float MaxDuration = 5;
        private void OnClickHandler(GameObject go)
        {
            if (Duration > 0)
            {
                return;
            }
            if (LocalDataManager.Instance.M_TrackPlayData.StaticTrackPlayDatas.TryGetValue(PlayID, out var playData))
            {
                Duration = playData.GetDuration() * 0.001f;
                MaxDuration = Duration;
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ScenePlayModule, "OnPlayEffect",
                    new object[] { PlayType, TaskID });
                mText.text = Duration.ToString();
                mMaskImage.fillAmount = 0;
                mMaskImage.gameObject.SetActive(true);
                mText.gameObject.SetActive(true);
                _ = SecondUpdate();
            }
        }

        private async UniTaskVoid SecondUpdate()
        {
            while (Duration > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1));
                Duration--;
                mText.text = Duration.ToString();
                mMaskImage.fillAmount = Duration / MaxDuration;
            }
            mMaskImage.gameObject.SetActive(false);
            mText.gameObject.SetActive(false);
        }

        private void OnShowScenePlayHandler(int playtype, int id, int taskID)
        {
            PlayType = playtype;
            PlayID = id;
            TaskID = taskID;
            if (playtype > 0 && id > 0)
            {
                mRoot.SetActive(true);
            }
            else
            {
                Duration = 0;
                mMaskImage.gameObject.SetActive(false);
                mText.gameObject.SetActive(false);
                mRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            GlobalEvent.OnShowScenePlay.RemoveListener(OnShowScenePlayHandler);
            TrackButton.OnClick -= OnClickHandler;
        }
    }
}