using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

/// <summary>
    /// 给选中的特效添加脚本的
    /// </summary>
    //[InitializeOnLoad]
public static class TestParticleEffect
    {
        private const string RequestTestKey = "TestParticleEffectRquestTest";
        private static bool _hasPlayed;
        static bool isRestart = false;
        private static GameObject _particleEffect;
        private static string _particleEffectName;
        private static int[] _originalLayers;
        private static string ifEffectSelectedBegin = "ifEffectSelectedBegin";
        
        [MenuItem("GameObject/特效/测试", false, 11)]
        private static void Test()
        {
            //已经在播放状态，使其重新开始
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                isRestart = true;
            }
            else
            {
                EditorApplication.isPlaying = true;
            }
            
            var go = Selection.activeGameObject;
            _particleEffect = Selection.activeGameObject;
            _particleEffectName = Selection.activeGameObject.name;
            PlayerPrefs.SetString("ParticleEffect", Selection.activeGameObject.name);
            PlayerPrefs.SetString(ifEffectSelectedBegin, "On");
            var particleSystemRenderer = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
    
            if (particleSystemRenderer.Length == 0)
            {
                Debug.LogError("不是特效无法测试！");
                return;
            }
    
            EditorPrefs.SetBool(RequestTestKey, true);
    
            
            
            
            // var particleEffectScript = go.GetComponentsInChildren<ParticleEffectScript>(true);
            // if (particleEffectScript.Length == 0)
            // {
            //     go.AddComponent<ParticleEffectScript>();
            // }

         
            //ParticleEffectScript.OnGameExit += OnQuitting;
            
            
            
            //监听事件 回调函数
        }
    
        static TestParticleEffect()
        {
            EditorApplication.update += Update;
            EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
            //EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        }
    
        private static void Update()
        {
            if (EditorPrefs.HasKey(RequestTestKey) && !_hasPlayed &&
                EditorApplication.isPlaying &&
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorPrefs.DeleteKey(RequestTestKey);
                _hasPlayed = true;
            }
        }
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {

            
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                Debug.Log("进入游戏");
                if (PlayerPrefs.GetString(ifEffectSelectedBegin) == "On")
                {
                    var EffectName = PlayerPrefs.GetString("ParticleEffect");
                    _particleEffect = GameObject.Find(EffectName);
                    //批量把Go的子物体都换成TransparentFX的layer
                    //RestoreOriginalLayers();
                    var renderers = _particleEffect.GetComponentsInChildren<Renderer>(true);
                    
                    _originalLayers = new int[renderers.Length];
                    for (int i = 0; i < renderers.Length; i++)
                    {
                        _originalLayers[i] = renderers[i].gameObject.layer;
                        renderers[i].gameObject.layer = LayerMask.NameToLayer("TransparentFX");
                    }
                    
                    //关闭场景中的所有灯光,如果灯光存在会影响Batches的数量
                    Light[] lights = GameObject.FindObjectsOfType<Light>();
                    foreach (var VARIABLE in lights)
                    {
                        VARIABLE.enabled = false;
                    }
                    
                    var particleEffectScript = _particleEffect.GetComponentsInChildren<ParticleEffectScript>(true);
                    if (particleEffectScript.Length == 0)
                    {
                        _particleEffect.AddComponent<ParticleEffectScript>();
                    }

                }
                
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefs.SetString(ifEffectSelectedBegin,"Off");
                
                var EffectName = PlayerPrefs.GetString("ParticleEffect");
                var go = Selection.activeGameObject;
                Debug.Log("退出游戏");
                if (EffectName != null)
                {
                    //关闭的时候本脚本内存都没了，需要被editor来处理
                    if (_particleEffect == null)
                    {
                        _particleEffect = GameObject.Find(EffectName);
                    }
                }
                if (_particleEffect != null)
                {
                    RestoreOriginalLayers();
                    var particleEffectScript = _particleEffect.GetComponent<ParticleEffectScript>();
                    if (particleEffectScript != null)
                    {
                        Object.DestroyImmediate(particleEffectScript);
                    }
                }
                
            }
        }

        private static void PlaymodeStateChanged()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.Log("EditorApplication.isPlaying :  1" +  EditorApplication.isPlaying);
                if (_particleEffect != null)
                {
                    
                    Debug.Log("EditorApplication.isPlaying :  3" +  EditorApplication.isPlaying);
                }
            }
            if (!EditorApplication.isPlaying)
            {
                _hasPlayed = false;
                Debug.Log("EditorApplication.isPlaying :  2" +  EditorApplication.isPlaying);
            }
    
            if (isRestart)
            {
                EditorApplication.isPlaying = true;
                isRestart = false;
            }
        }
    
        private static void QuitGame()
        {
            Debug.Log("退出游戏");
         
        }
        private static void RestoreOriginalLayers()
        {
            if (_particleEffect != null && _originalLayers != null)
            {
               
                var renderers = _particleEffect.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].gameObject.layer = _originalLayers[i];
                }
                _originalLayers = null;
            }
        }
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoadRuntimeMethod()
        {
            //_particleEffect = Selection.activeGameObject;
            if (_particleEffect != null)
            {
                var renderers = _particleEffect.GetComponentsInChildren<Renderer>(true);
                _originalLayers = new int[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    _originalLayers[i] = renderers[i].gameObject.layer;
                    renderers[i].gameObject.layer = LayerMask.NameToLayer("TransparentFX");
                }
            }
        }
        
        // // 游戏退出时调用
        // //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // private static void OnQuitting()
        // {
        //     // 执行退出游戏时的操作
        //     if (_particleEffect != null)
        //     {
        //          RestoreOriginalLayers();
        //         var particleEffectScript = _particleEffect.GetComponent<ParticleEffectScript>();
        //         if (particleEffectScript != null)
        //         {
        //             Object.DestroyImmediate(particleEffectScript);
        //         }
        //     }
        // }
    }


