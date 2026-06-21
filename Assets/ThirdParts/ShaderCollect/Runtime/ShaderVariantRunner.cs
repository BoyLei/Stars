using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameDLL
{
    public class ShaderVariantRunner : MonoBehaviour
    {
        enum Stage
        {
            Sleep = 0,
            Normal,
            PrepareInstancing,
            Instancing,
            Finished,
        }
        private Renderer[] batchRenders;
        public Camera renderingCamera;
        public float distance;
        const float fov = 90f;
        const float sphereRadius = 0.1f;
        const int grid = 10;
        private Mesh instancingMesh = null;
        private Queue<Material> instancingMaterialQueue;
        private List<Material> instabcingMaterialList;
        private Queue<Material> materialQueue;
        private Stage stage;
        public List<Material> targetMaterials;
        public Action callback;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("ShaderVariantRunner Start targetMaterials.Count=" + targetMaterials.Count);

            renderingCamera.transform.position = new Vector3(0, 0, -distance);
            renderingCamera.fieldOfView = fov;
            //90度张角保证渲染平面的宽度比较好算
            float minx = -distance, miny = -distance, maxx = distance, maxy = distance;
            float slice = (2 * distance) / (grid + 1);
            float x = minx + slice;
            List<Renderer> rendererList = new List<Renderer>();
            for (int gx = 0; gx < grid; ++gx)
            {
                float y = miny + slice;
                for (int gy = 0; gy < grid; ++gy)
                {
                    GameObject prototype = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    prototype.transform.position = new Vector3(x, y, 0);
                    Renderer prototypeRenderer = prototype.GetComponent<Renderer>();
                    MeshFilter mf = prototype.GetComponent<MeshFilter>();
                    instancingMesh = mf.sharedMesh;
                    rendererList.Add(prototypeRenderer);
                    y += slice;
                }
                x += slice;
            }
            batchRenders = rendererList.ToArray();
            materialQueue = new Queue<Material>(targetMaterials);
            instancingMaterialQueue = new Queue<Material>();
            instabcingMaterialList = new List<Material>();
            stage = Stage.Normal;

            Camera camera = GetComponent<Camera>();
            if(camera==null)
            {
                Debug.LogError("ShaderVariantRunner camera not found");
            }
            else
            {
                //GLog.Log("ShaderVariantRunner camera found");
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (stage)
            {
                case Stage.Normal:
                    UpdateNormal();
                    break;
                case Stage.PrepareInstancing:
                    PrepareInstancing();
                    break;
                case Stage.Instancing:
                    UpdateInstancing();
                    break;
                case Stage.Finished:
                    stage = Stage.Sleep;
                    QuitRunning();
                    break;
            }
        }

        void UpdateNormal()
        {
            int index = 0;
            //GLog.LogFormat("ShaderVariantRunner UpdateNormal materialQueue.Count={0}", materialQueue.Count);
            while (index < batchRenders.Length)
            {
                if (materialQueue.Count == 0)
                {
                    stage = Stage.PrepareInstancing;
                    break;
                }
                var mat = materialQueue.Dequeue();
                batchRenders[index].sharedMaterial = mat;
                if (mat != null && mat.enableInstancing)
                {
                    instancingMaterialQueue.Enqueue(mat);
                    instabcingMaterialList.Add(mat);
                }
                ++index;
            }
        }

        void PrepareInstancing()
        {
            //Debug.Log("ShaderVariantRunner PrepareInstancing");
            foreach (var r in batchRenders)
            {
                r.enabled = false;
            }
            stage = Stage.Instancing;
        }

        void UpdateInstancing()
        {
            int index = 0;
            //GLog.LogFormat("ShaderVariantRunner UpdateInstancing instancingMaterialQueue.Count={0}", instancingMaterialQueue.Count);
            while (index < batchRenders.Length)
            {
                if (instancingMaterialQueue.Count == 0)
                {
                    stage = Stage.Finished;
                    break;
                }
                var mat = instancingMaterialQueue.Dequeue();
                /*
                if(index >= instabcingMaterialList.Count)
                {
                    break;
                }
                var mat = instabcingMaterialList[index];
                */
                Matrix4x4[] instancingMatrix = new Matrix4x4[] { batchRenders[index].transform.localToWorldMatrix };
                Graphics.DrawMeshInstanced(instancingMesh, 0, mat, instancingMatrix, instancingMatrix.Length, null, UnityEngine.Rendering.ShadowCastingMode.On, false, 0);

                ++index;
            }

        }

        protected virtual void QuitRunning()
        {
            //GLog.LogTagFormat(GLog.LogTypeGameDLL, "ShaderVariantRunner QuitRunning");
            Debug.Log("ShaderVariantRunner QuitRunning");
        }
    }
}
