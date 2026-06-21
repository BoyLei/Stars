#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yoka.Galaxy.Profile;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using Yoka.Galaxy.Profile.Utils;
//using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.Analytics;

namespace Sample
{
    public class SampleUsage : ProfilerUseCase
    {
        [VerticalGroup("Detail")]
        public int idle;

        [VerticalGroup("Detail")]
        public GameObject prefab;

        [VerticalGroup("Detail")]
        public InstanceMatrix instanceMatrix;

        protected int Count
        {
            get
            {
                return instanceMatrix.Count;
            }
        }

        public async override UniTask Run()
        {
            await UniTask.SwitchToMainThread();

            List<GameObject> instancelist = new List<GameObject>();
            await instanceMatrix.ForEach(async (pos, index) =>
            {
                var instance = GameObject.Instantiate(prefab);
                instance.transform.position = pos;
                instancelist.Add(instance);
            });
            await UniTask.Delay(idle * 1000);

            foreach(var instance in instancelist)
            {
                GameObject.Destroy(instance);
            }
        }

        public override string ReportStatus(IProfilerOutputFormatter formatter)
        {
            return $"{tag} cout:{Count}";
        }
    }

}
#endif