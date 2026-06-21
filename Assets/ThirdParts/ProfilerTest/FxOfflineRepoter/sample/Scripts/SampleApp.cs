#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yoka.Galaxy.Framework;
using Cysharp.Threading.Tasks;


public class SampleApp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        App game = new IOCApp();
        game.Init();
        game.Register<Yoka.Galaxy.Mono.IMono, Yoka.Galaxy.Mono.Mono>(true);
        game.Register<Yoka.Galaxy.Profile.IProfiler, Yoka.Galaxy.Profile.Profiler>(true);

        _ = UniTask.RunOnThreadPool(Boot);

    }

    async static void Boot()
    {
        await UniTask.SwitchToMainThread();
        App game = App.GetInstance();
        var profiler = game.Resolve<Yoka.Galaxy.Profile.IProfiler>();
        var task = profiler.Begin(Application.persistentDataPath + "/profile.txt", null);
        await task;
        profiler.End();
        await UniTask.Delay(1000);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif

    }
}
#endif