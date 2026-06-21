#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yoka.Galaxy.Mono
{
    public class UnityMono : MonoBehaviour
    {
        private IMono _monoModule = null;

        public IMono MonoModule
        {
            get
            {
                return _monoModule;
            }

            set
            {
                _monoModule = value;
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(MonoModule != null)
                MonoModule.InvokeUpdate();
        }

        void LateUpdate()
        {
            if (MonoModule != null)
                MonoModule.InvokeLateUpdate();
        }

        void OnDestroy()
        {
            if (MonoModule != null)
                MonoModule.InvokeDestroy();
        }
    }

}
#endif