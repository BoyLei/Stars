///--------------------------------------------------------------------
/// 文件名   :   PlayMindTracking
/// 内  容   :   心灵追踪
/// 说  明   :  
/// 创建日期 :   2024/08/21 16:30:35
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Resources;
using SGF.Unity;
using StarProject.Game;
using StarProject.Game.Map;
using StarProject.Module;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;

namespace StarProject.Module
{
    /**
     * PlayMindTracking 心灵追踪玩法
     * 玩家自身坐标到目标的的连线
     * 屏幕的后处理
     */
    public class PlayMindTracking : BaseScenePlay
    {
        public PlayMindTracking() : base(ScenePlayType.MindTracking)
        {
            if (m_LineEffect == null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<GameObject>(EffectPath,
                    E_AssetType.Effects,
                    (_go) =>
                    {
                        var go = GameObject.Instantiate(_go);
                        m_LineEffect = go;

                        go.SetActive(false);
                        go.transform.position = Vector3.zero;
                        go.transform.rotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;
                        GameObject.DontDestroyOnLoad(go);
                    }
                );
            }

            if (m_Vignette == null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.RecursionLoadAsset<GameObject>(VignettePath,
                    E_AssetType.Effects,
                    (_go) =>
                    {
                        var go = GameObject.Instantiate(_go);
                        m_Vignette = go;
                        go.SetActive(false);
                        go.transform.position = Vector3.zero;
                        go.transform.rotation = Quaternion.identity;
                        go.transform.localScale = Vector3.one;
                        GameObject.DontDestroyOnLoad(go);
                    }
                );
            }
        }

        //特效路径
        public const string EffectPath = "Effects/Scene/Cm/FX_XinL_ZZ_New";


        public const string VignettePath = "Effects/Scene/Cm/Vignettte";

        private GameObject m_Vignette;

        //特效对象
        private GameObject m_LineEffect;

        public override void OnPlayBegine(int playId)
        {
            if (GameManager.Instance.M_MainPlayerCtrlBase == null ||
                GameManager.Instance.M_MainPlayerCtrlBase.M_Curr == null)
            {
                return;
            }

            var playerPostion = GameManager.Instance.M_MainPlayerCtrlBase.M_Curr.Position() + new Vector3(0, 1, 0);


            var configs = LocalDataManager.Instance.M_TrackPlayData;
            if (configs != null)
            {
                if (configs.StaticTrackPlayDatas.TryGetValue(playId, out var playData))
                {
                    if (GameMap.sceneJsonData.Spawners.TryGetValue(playData.GetSpawnerID(),
                            out var spawnerJsonData))
                    {
                        var end = spawnerJsonData.Position.Convert() + new Vector3(0, 1, 0);
                        var dir = (end - playerPostion).normalized;
                        //玩家前方2米的位置
                        m_LineEffect.transform.position = playerPostion+dir*2f;
                        m_LineEffect.transform.forward = dir;

                        m_Vignette.SetActive(true);
                        m_LineEffect.gameObject.SetActive(true);
                    }

                    DelayInvoker.DelayInvoke(playData.GetEffectCleanTime() * 0.001f, (a) => { OnPlayEnd(); }, null);
                }
            }
        }

        public override void OnPlayEnd()
        {
            m_Vignette?.SetActive(false);
            m_LineEffect?.SetActive(false);
        }
    }
}