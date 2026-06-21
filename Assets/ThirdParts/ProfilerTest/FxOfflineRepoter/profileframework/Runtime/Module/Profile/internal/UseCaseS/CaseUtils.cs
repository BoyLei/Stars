// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/16 9:36:52)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yoka.Galaxy.Profile.Utils
{
    public delegate UniTask TaskAction<T> (T t);
    public delegate UniTask TaskAction<T1, T2> (T1 t1, T2 t2);

    [Serializable]
    public class InstanceMatrix
    {
        [HorizontalGroup("Params", LabelWidth = 20)]
        public int r;

        [HorizontalGroup("Params")]
        public float i;

        [HorizontalGroup("Params")]
        public Vector3 c;

        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public async UniTask ForEach(TaskAction<Vector3> action)
        {
            int halfX = r;
            int halfZ = r;
            int numX = halfX * 2 + 1, numZ = numX;
            _count = 0;
            for (int x = 0; x < numX; x++)
            {
                for (int z = 0; z < numZ; z++)
                {
                    float x1 = (float)(x - halfX) * i;
                    float z1 = (float)(z - halfZ) * i;
                    Vector3 position = new Vector3(c.x + x1, c.y, c.z + z1);
                    await action(position);
                    ++_count;
                }
            }
        }

        public async UniTask ForEach(TaskAction<Vector3, int> action)
        {
            int halfX = r;
            int halfZ = r;
            int numX = halfX * 2 + 1, numZ = numX;
            int index = 0;
            _count = 0;
            for (int x = 0; x < numX; x++)
            {
                for (int z = 0; z < numZ; z++)
                {
                    float x1 = (float)(x - halfX) * i;
                    float z1 = (float)(z - halfZ) * i;
                    Vector3 position = new Vector3(c.x + x1, c.y, c.z + z1);
                    await action(position, index);
                    ++index;
                    ++_count;
                }
            }
        }
    }


    [Serializable]
    public class DebugId<T>
    {
        public bool debugOn;
        [ShowIf("debugOn")]
        public List<T> activeIdList;

        private HashSet<T> _fastLookupSet = null;

        public void Init()
        {
            if(debugOn)
            {
                _fastLookupSet = new HashSet<T>(activeIdList);
            }
        }

        public void Dispose()
        {

        }

        public bool AcceptId(T tester)
        {
            if (!debugOn)
                return true;
            if (_fastLookupSet.Contains(tester))
                return true;
            return false;
        }
    }


    public class CaseUtils
    {

    }

}
#endif