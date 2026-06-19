
using SGF.Module.Framework;
using SGF.Unity;
using StarProject.Game;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using XLua;
using Vector3 = UnityEngine.Vector3;
using SGF.UI.Framework;
using StarProjectDef;
using DeadMosquito.AndroidGoodies.Internal;
using StarProject.Service.Cam;

namespace StarProject.Game
{
    /// <summary>
    /// 在StarSceneManager上層進行具體切換融合邏輯
    /// StarSceneManager是更底層，相當於統籌網絡，進度，加載
    /// 這個更偏重系統業務如何【夥伴】【抽卡】處理界面
    /// </summary>
    [LuaCallCSharp]
    public class MutiScenesMergeManager : ServiceModule<MutiScenesMergeManager>
    {
        public Material cardMat;
        Material orgMat;
        Action<bool> loadFinishEvent = null;
        private SceneInstance Scene;
        public void Init()
        {
            CheckSingleton();
            //orgMat = RenderSettings.skybox;
        }

        public override void Release()
        {
            base.Release();

        }



        //bool loaded = false;
        Scene orgActiveScene;

        GameObject globalVolume;
        GameObject lightObj;
        public void LoadCardScene(int sceneIndex, Action<bool> callback)
        {
            //关闭灯光，关闭volume
            string sceneName = SceneManager.GetActiveScene().name;
            // globalVolume = GameObject.Find($"{sceneName}/Global Volume");
            // if (globalVolume != null)
            // {
            //     globalVolume.SetActive(false);
            // }
            // lightObj = GameObject.Find($"{sceneName}/Light");
            // if (lightObj != null)
            // {
            //     lightObj.SetActive(false);
            // }

            // RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            // RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
            CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).gameObject.SetActive(false);
            AppMain.Instance.UseHighHighHighLevelSetting();
            DynamicUIRoot.EntityUIRoot.gameObject.SetActive(false);
            if (DrawCardUI.Instance == null || DrawCardUI.Instance.gameObject == null)
            {
                orgActiveScene = SceneManager.GetActiveScene();
                loadFinishEvent = callback;

                StarScenesManager.Instance.LoadUISystemActiveScene("sys_gacha_carddis", (ins) =>
                {
                    EntityRoot.Instance.gameObject.SetActive(false);
                    SceneManager.SetActiveScene(ins.Scene);
                    Scene = ins;
                    MonoHelper.StartCoroutine(LoadScene());
                });
            }
            else
            {
                EntityRoot.Instance.gameObject.SetActive(false);
                var scene = SceneManager.GetSceneByName("sys_gacha_carddis");
                SceneManager.SetActiveScene(scene);
                DrawCardUI.Instance.transform.parent.gameObject.SetActive(true);
                if (callback != null)
                {
                    callback(false);
                }
                //RenderSettings.skybox = DrawCardUI.Instance.cardMat;
                //GameInput.Instance.transform.localScale = Vector3.zero;
                //GlobalModules.Instance.transform.localScale = Vector3.zero;
            }
        }

        public void UnloadScene()
        {
            // if (globalVolume != null)
            // {
            //     globalVolume.SetActive(true);
            // }
            // if (lightObj != null)
            // {
            //     lightObj.SetActive(true);
            // }

            AppMain.Instance.SetQualityLevel(GameConfig.MachineQualityLevel);

            DrawCardUI.Instance.transform.parent.gameObject.SetActive(false);
            CameraManager.Instance.GetCamera(E_CameraType.StarWorldCam).gameObject.SetActive(true);
            SceneManager.SetActiveScene(orgActiveScene);
            //SceneManager.UnloadSceneAsync(18);
            //RenderSettings.skybox = orgMat;

            DynamicUIRoot.EntityUIRoot.gameObject.SetActive(true);
            EntityRoot.Instance.gameObject.SetActive(true);

            // if(GameInput.Instance != null)
            // {
            //     GameInput.Instance.transform.localScale = Vector3.one;
            // }
            // GlobalModules.Instance.transform.localScale = Vector3.one;
            //Addressables.UnloadSceneAsync(Scene);
            //loaded = false;
        }

        IEnumerator LoadScene()
        {
            yield return new WaitUntil(() => DrawCardUI.Instance != null);

            DrawCardUI.Instance.transform.parent.gameObject.SetActive(true);
            if (loadFinishEvent != null)
            {
                loadFinishEvent(true);
            }

            //RenderSettings.skybox = DrawCardUI.Instance.cardMat;
            //GameInput.Instance.transform.localScale = Vector3.zero;
            //GlobalModules.Instance.transform.localScale = Vector3.zero;
        }
    }
}