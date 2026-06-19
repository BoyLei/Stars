using System;
using System.Collections.Generic;
using System.Reflection;
using System.Timers;
using GameTechTools.CommonLibs.CommonExtends;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CasualEngine.ProjectScanTool
{

    public enum EnumParticleEffectAnalysisType
    {
        eParticlesCnt = 1
    }

    public struct CheckInfo<U> where U : CustomCheckDetail
    {
        public string path;
        public U checkDetail;
    }

    public class ParticleEffectAnalysis<T, U> where T : CustomRule where U : CustomCheckDetail
    {
        private static int checkIndex = 0; //当前正在检查第几项
        private static Timer ticker;
        private static float m_iTick;
        private static float m_iRoundTick; //当前检查的特效播放一轮所有的最长时间
        private static T currentCheckRule;
        private static EnumParticleEffectAnalysisType curAnalysisType;
        private static List<CheckInfo<U>> checkInfos = new List<ProjectScanTool.CheckInfo<U>>();

        //开始检查项目
        public static void Begin(T customRule, EnumParticleEffectAnalysisType analysisType, string methodName)
        {
            if (customRule == null)
            {
                return;
            }
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("正在运行中，请先暂停运行再重新执行检查");
                return;
            }

#if UNITY_2017_1_OR_NEWER
            m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.Instance | BindingFlags.NonPublic);
#else
        m_CalculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CountSubEmitterParticles", BindingFlags.Instance | BindingFlags.NonPublic);
#endif
            currentCheckRule = customRule;
            curAnalysisType = analysisType;
            GTEditorCoroutineRunner.StartEditorCoroutine(LoadSceneAndStartTicker());
        }

        //加载一个场景用来执行特效分析数据
        private static System.Collections.IEnumerator LoadSceneAndStartTicker()
        {
            if (EditorApplication.isPlaying)
                yield break;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.OpenScene(ProjectScanGlobalConfig.test_scene_path, OpenSceneMode.Single);
            yield return null;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode modee)
        {
            Debug.LogError("OnSceneOpened");
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
            EditorApplication.isPlaying = true;
        }

        private static void OnPlaymodeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                RemoveTicker();
                return;
            }
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }
            Debug.LogError("OnPlaymodeStateChanged");
            InitCheckInfos();
            DoParticleEffectCheck();
        }

        //初始化要处理的数据
        private static void InitCheckInfos()
        {
            checkInfos.Clear();
            switch (curAnalysisType)
            {
                case EnumParticleEffectAnalysisType.eParticlesCnt:
                    foreach (var checkDetail in (currentCheckRule as ParticleEffectParticlesCntCheck).checkDetailList)
                    {
                        string[] paths = ProjectScanHelper.GetAssetPathsByType(currentCheckRule, checkDetail, AssetType.prefab);
                        foreach (var path in paths)
                        {
                            checkInfos.Add(new CheckInfo<U>()
                            {
                                path = path,
                                checkDetail = checkDetail as U
                            });
                        }
                    }
                    break;
            }
        }

        //添加定时器
        private static void AddTicker()
        {
            Debug.LogError("AddTicker");
            m_iTick = 0;
            ticker = new Timer();
            ticker.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            ticker.Interval = 100f; //每帧收集一次
            ticker.Enabled = true;
            ticker.AutoReset = false;
            ticker.Start();
        }

        //移除定时器
        private static void RemoveTicker()
        {
            Debug.LogError("RemoveTicker");
            if (ticker != null)
            {
                ticker.Close();
                ticker = null;
            }
        }

        //每帧更新
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Debug.LogError(m_iTick + "_____" + m_iRoundTick);
            m_iTick += 200f;
            // LateUpdate();
            if (m_iTick >= m_iRoundTick)
            {
                Debug.LogError("xxxxxxxxxxxxx        " + m_MaxParticleCount);
                checkIndex++;
                if (checkIndex >= checkInfos.Count)
                {
                    RemoveTicker();
                    EditorApplication.isPlaying = false;
                    Debug.LogError("xxxxxxxxxxxxx        " + m_MaxParticleCount);
                }
                else
                {
                    DoParticleEffectCheck();
                }

            }
        }



        private static ParticleSystem[] m_ParticleSystems;
        private static int m_ParticleCount = 0;
        private static int m_MaxParticleCount = 0;
        private static MethodInfo m_CalculateEffectUIDataMethod;
        //轮询检查资源
        private static void DoParticleEffectCheck()
        {
            try
            {
                var tmpCheckInfo = checkInfos[checkIndex++];
                var go = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(tmpCheckInfo.path));
                m_ParticleSystems = go.GetComponentsInChildren<ParticleSystem>();
                m_ParticleCount = m_MaxParticleCount = 0;
                //拿到最长时间
                m_iRoundTick = 0;
                foreach (var ps in m_ParticleSystems)
                {
                    float lifeTime = 0;
                    if (ps.main.startLifetime.mode == ParticleSystemCurveMode.Constant)
                    {
                        lifeTime = Mathf.Max(ps.main.startLifetime.constant, lifeTime);
                    }
                    else if (ps.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants)
                    {
                        lifeTime = Mathf.Max(ps.main.startLifetime.constantMin, lifeTime);
                        lifeTime = Mathf.Max(ps.main.startLifetime.constantMax, lifeTime);
                    }
                    m_iRoundTick = Math.Max(m_iRoundTick, ps.main.duration + lifeTime);
                }
                m_iRoundTick *= 1000;
                AddTicker();
            }
            catch (Exception ex)
            {
                RemoveTicker();
                EditorApplication.isPlaying = false;
                Debug.LogError(ex.ToString());
            }
        }

        //每帧收集数据
        private static void LateUpdate()
        {
            RecordParticleCount();
        }

        ///计算同时存在最大粒子数
        private static void RecordParticleCount()
        {
            m_ParticleCount = 0;
            foreach (var ps in m_ParticleSystems)
            {
                int count = 0;
#if UNITY_2017_1_OR_NEWER
                object[] invokeArgs = { count, 0.0f, Mathf.Infinity };
                m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
                count = (int)invokeArgs[0];
#else
            object[] invokeArgs = { count };
            m_CalculateEffectUIDataMethod.Invoke(ps, invokeArgs);
            count = (int)invokeArgs[0];
            count += ps.particleCount;
#endif
                m_ParticleCount += count;
            }
            if (m_MaxParticleCount < m_ParticleCount)
            {
                m_MaxParticleCount = m_ParticleCount;
            }
            Debug.LogError("xxxxxxxxxxxxx        " + m_MaxParticleCount);
        }

    }
}