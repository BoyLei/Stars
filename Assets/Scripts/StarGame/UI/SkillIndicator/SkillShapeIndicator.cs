using SGF.Time;
using System.Collections.Generic;
using UnityEngine;
namespace StarProject.UI.SkillIndicator
{
    public class SkillShapeIndicator : MonoBehaviour
    {
        public List<Renderer> shaders;

        private Dictionary<string, SkinnedMeshRenderer> m_SkinnedMeshRenderers = new Dictionary<string, SkinnedMeshRenderer>();

        public GameObject Shape;
        private void Awake()
        {
            Shape = transform.GetChild(0).gameObject;
            InsMaterial();
            //Debug.Log($"指示器 Awake name={transform.name},time={TimeUtils.TimeLogString()}");
        }

        private DictionaryEx<string, List<Material>> key2Materials = new();


        public void InsMaterial()
        {
            // if (shaders != null)
            // {
            //     for (int i = 0; i < shaders.Count; i++)
            //     {
            //         var shader = shaders[i];
            //         var sMaterial = shader.sharedMaterial;
            //         if (shader != null && sMaterial != null)
            //         {
            //             var newMaterial = new Material(sMaterial);
            //             shader.sharedMaterial = newMaterial;
            //         }
            //     }
            // }
            key2Materials.Clear();

            key2Materials.Add("_GlowRange", new());
            key2Materials.Add("_Range", new());
            key2Materials.Add("_RingAlpha", new());
            key2Materials.Add("_Angle", new());
            key2Materials.Add("_MiddleRingSize", new());
            key2Materials.Add("_Length", new());

            if (shaders != null)
            {
                for (int i = 0; i < shaders.Count; i++)
                {
                    var shader = shaders[i];
                    var material = shader.material;
                    foreach (var item in key2Materials)
                    {
                        if (material.HasFloat(item.Key))
                        {
                            item.Value.Add(material);
                        }
                    }
                }
            }
        }

        public void InsEditorMaterial()
        {
            key2Materials.Clear();
            key2Materials.Add("_GlowRange", new());
            key2Materials.Add("_Range", new());
            key2Materials.Add("_RingAlpha", new());
            key2Materials.Add("_Angle", new());
            key2Materials.Add("_MiddleRingSize", new());
            key2Materials.Add("_Length", new());

            if (shaders != null)
            {
                for (int i = 0; i < shaders.Count; i++)
                {
                    var shader = shaders[i];
                    var sMaterial = shader.sharedMaterial;
                    if (shader != null && sMaterial != null)
                    {
                        var newMaterial = new Material(sMaterial);
                        shader.sharedMaterial = newMaterial;
                        foreach (KeyValuePair<string, List<Material>> item in key2Materials)
                        {
                            if (shader.sharedMaterial.HasFloat(item.Key))
                            {
                                item.Value.Add(newMaterial);
                            }
                        }

                    }
                }
            }

        }



        private void Start()
        {

            //Debug.Log($"指示器 Start name={transform.name},time={TimeUtils.TimeLogString()}");
        }

        private void OnEnable()
        {
            //Debug.Log($"指示器 OnEnable name={transform.name},time={TimeUtils.TimeLogString()}");

        }

        private void OnDisable()
        {
            //Debug.Log($"指示器 OnDisable name={transform.name},time={TimeUtils.TimeLogString()}");

        }

        private SkinnedMeshRenderer GetSkinnedMeshRendererByName(string key)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = null;
            if (m_SkinnedMeshRenderers.ContainsKey(key))
            {
                skinnedMeshRenderer = m_SkinnedMeshRenderers[key];
            }
            return skinnedMeshRenderer;
        }

        private void AddSkinnedMeshRenderer(string key, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if (!m_SkinnedMeshRenderers.ContainsKey(key))
            {
                m_SkinnedMeshRenderers[key] = skinnedMeshRenderer;
            }
        }

        // 设置【环扇形】的隐藏
        public void SetShapeActive(bool isShow)
        {
            Shape.transform.localScale = isShow ? Vector3.one : Vector3.zero;
        }

        public void SetGlowRange(float value)
        {

            SetMaterialFloat("_GlowRange", value);

        }

        #region 圆

        // 设置【圆】的半径
        public void SetCircleRange(float value)
        {
            if (value <= 0)
            {
                SetShapeActive(false);
                return;
            }
            SetMaterialFloat("_Range", value);

            for (int i = 0; i < shaders.Count; i++)
            {
                //float val = i == 0 ? value : value - 1;

                //scale.x = value * 2;
                //scale.y = value * 2;
                //scale.z = value * 2;
                //shaders[i].transform.localScale = scale;


                // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                if (i == 0)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(shaders[i].transform.name);
                    if (skinnedMeshRenderer == null)
                    {
                        skinnedMeshRenderer = shaders[i].GetComponent<SkinnedMeshRenderer>();
                        AddSkinnedMeshRenderer(shaders[i].transform.name, skinnedMeshRenderer);
                    }
                    Bounds bounds = skinnedMeshRenderer.localBounds;
                    bounds.extents = Vector3.one * value;
                    skinnedMeshRenderer.localBounds = bounds;
                }
            }
        }

        // 设置【圆】的边框透明度 0-1
        public void SetCircleRingAlpha(float value)
        {

            SetMaterialFloat("_RingAlpha", value);

            Shape.SetActive(value > 0);
        }

        // 设置【圆】的光晕 0-2
        public void SetCircleGlowRange(float value)
        {

            SetMaterialFloat("_GlowRange", value);

        }

        #endregion

        #region 扇形

        // 设置【扇形】的半径
        public void SetSectorRange(float value)
        {
            SetMaterialFloat("_Range", value);

            foreach (var item in shaders)
            {
                //scale.x = value * 2;
                //scale.y = value * 2;
                //scale.z = value * 2;
                //item.transform.localScale = scale;

                // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(item.transform.name);
                if (skinnedMeshRenderer == null)
                {
                    skinnedMeshRenderer = item.GetComponent<SkinnedMeshRenderer>();
                    AddSkinnedMeshRenderer(item.transform.name, skinnedMeshRenderer);
                }
                Bounds bounds = skinnedMeshRenderer.localBounds;
                bounds.extents = Vector3.one * value / 100.0f;
                skinnedMeshRenderer.localBounds = bounds;
            }
        }

        // 设置【扇形】的角度
        public void SetSectorAngle(float value)
        {

            SetMaterialFloat("_Angle", value);

        }

        // 设置【扇形】的边框透明度 0-1
        public void SetSectorRingAlpha(float value)
        {

            SetMaterialFloat("_RingAlpha", value);

            Shape.SetActive(value > 0);
        }

        // 设置【扇形】的光晕 0-2
        public void SetSectorGlowRange(float value)
        {

            SetMaterialFloat("_GlowRange", value);

        }

        #endregion

        #region 环扇形

        // 设置【环扇形】的半径
        public void SetRingFanRange(float value)
        {
            SetMaterialFloat("_Range", value);

            foreach (var item in shaders)
            {
                //scale.x = value * 2;
                //scale.y = value * 2;
                //scale.z = value * 2;
                //item.transform.localScale = scale;

                // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(item.transform.name);
                if (skinnedMeshRenderer == null)
                {
                    skinnedMeshRenderer = item.GetComponent<SkinnedMeshRenderer>();
                    AddSkinnedMeshRenderer(item.transform.name, skinnedMeshRenderer);
                }
                Bounds bounds = skinnedMeshRenderer.localBounds;
                bounds.extents = Vector3.one * value / 100.0f;
                skinnedMeshRenderer.localBounds = bounds;
            }
        }

        // 设置【环扇形】的角度
        public void SetRingFanAngle(float value)
        {

            SetMaterialFloat("_Angle", value);

        }

        // 设置【环扇形】的小角度
        public void SetRingFanMiddleRingSize(float value)
        {

            SetMaterialFloat("_MiddleRingSize", value);

        }

        // 设置【环扇形】的边框透明度 0-1
        public void SetRingFanRingAlpha(float value)
        {
            SetMaterialFloat("_RingAlpha", value);


            Shape.SetActive(value > 0);
        }

        // 设置【环扇形】的光晕 0-2
        public void SetRingFanGlowRange(float value)
        {
            SetMaterialFloat("_GlowRange", value);
        }

        #endregion

        #region 矩形

        // 设置【矩形】的高
        public void SetRectRange(float value)
        {
            SetMaterialFloat("_Range", value, true);

            foreach (var item in shaders)
            {
                //scale.x = value;
                //scale.y = 1;
                //scale.z = 1;
                ////item.transform.localScale = scale;

                // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(item.transform.name);
                if (skinnedMeshRenderer == null)
                {
                    skinnedMeshRenderer = item.GetComponent<SkinnedMeshRenderer>();
                    AddSkinnedMeshRenderer(item.transform.name, skinnedMeshRenderer);
                }
                Bounds bounds = skinnedMeshRenderer.localBounds;
                Vector3 extentsVector3 = Vector3.zero;
                extentsVector3.x = value / 2 / 100.0f;
                bounds.extents = extentsVector3;
                skinnedMeshRenderer.localBounds = bounds;
                break;
            }
        }

        public void SetWarningRectRange(float value)
        {
            SetMaterialFloat("_Range", value, true);

            foreach (var item in shaders)
            {
                //scale.x = value;
                //scale.y = 1;
                //scale.z = 1;
                ////item.transform.localScale = scale;

                // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(item.transform.name);
                if (skinnedMeshRenderer == null)
                {
                    skinnedMeshRenderer = item.GetComponent<SkinnedMeshRenderer>();
                    AddSkinnedMeshRenderer(item.transform.name, skinnedMeshRenderer);
                }
                Bounds bounds = skinnedMeshRenderer.localBounds;
                Vector3 extentsVector3 = Vector3.zero;
                extentsVector3.z = value / 2 / 100.0f;
                bounds.extents = extentsVector3;
                skinnedMeshRenderer.localBounds = bounds;
                break;
            }
        }


        public void SetRectLength(float value)
        {
            SetMaterialFloat("_Length", value);

            for (int i = 0; i < shaders.Count; i++)
            {
                ////scale.x = value;
                ////scale.y = value;
                //scale.z = value;
                //shaders[i].transform.localScale = scale;

                if (i == 0)
                {
                    // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                    SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(shaders[i].transform.name);
                    if (skinnedMeshRenderer == null)
                    {
                        skinnedMeshRenderer = shaders[i].GetComponent<SkinnedMeshRenderer>();
                        AddSkinnedMeshRenderer(shaders[i].transform.name, skinnedMeshRenderer);
                    }
                    Bounds bounds = skinnedMeshRenderer.localBounds;
                    Vector3 extentsVector3 = Vector3.zero;
                    extentsVector3.z = value / 2 / 100.0f;
                    extentsVector3.x = bounds.extents.x;
                    bounds.extents = extentsVector3;

                    Vector3 centerVector3 = Vector3.zero;
                    centerVector3.z = -extentsVector3.z;
                    bounds.center = centerVector3;
                    skinnedMeshRenderer.localBounds = bounds;
                }
            }
        }

        public void SetWarningRectLength(float value)
        {
            SetMaterialFloat("_Length", value);
            for (int i = 0; i < shaders.Count; i++)
            {
                ////scale.x = value;
                ////scale.y = value;
                //scale.z = value;
                //shaders[i].transform.localScale = scale;

                if (i == 0)
                {
                    // 设置SkinnedMeshRenderer的Bounds边界的大小和中心点
                    SkinnedMeshRenderer skinnedMeshRenderer = GetSkinnedMeshRendererByName(shaders[i].transform.name);
                    if (skinnedMeshRenderer == null)
                    {
                        skinnedMeshRenderer = shaders[i].GetComponent<SkinnedMeshRenderer>();
                        AddSkinnedMeshRenderer(shaders[i].transform.name, skinnedMeshRenderer);
                    }
                    Bounds bounds = skinnedMeshRenderer.localBounds;
                    Vector3 extentsVector3 = Vector3.zero;
                    extentsVector3.x = value / 2 / 100.0f;
                    extentsVector3.z = bounds.extents.z;
                    bounds.extents = extentsVector3;

                    Vector3 centerVector3 = Vector3.zero;
                    centerVector3.x = extentsVector3.x - 0.05f;
                    bounds.center = centerVector3;
                    skinnedMeshRenderer.localBounds = bounds;
                }
            }
        }

        // 设置【矩形】的边框透明度 0-1
        public void SetRectRingAlpha(float value)
        {
            SetMaterialFloat("_RingAlpha", value);
            Shape.SetActive(value > 0);
        }

        // 设置【矩形】的光晕 0-2
        public void SetRectGlowRange(float value)
        {
            SetMaterialFloat("_GlowRange", value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="isFirstBreak">曲爷神奇的代码只要设置第一个shader, 所以此处专们加个 变量 用来只执行第一个</param>
        private void SetMaterialFloat(string key, float value, bool isFirstBreak = false)
        {
            List<Material> materials = key2Materials[key];
            if (materials == null)
            {
                return;
            }
            foreach (var item in materials)
            {
                item.SetFloat(key, value);
                if (isFirstBreak)
                {
                    break;
                }
            }
        }

        #endregion

        // 设置光晕扩散的类型 true:中间扩散 --- false: 开始从一侧向另一侧扩散
        public void SetGlowType(bool isMiddle)
        {
            string type = isMiddle ? "_FROMMIDDLE" : "_FROMSIDE";
            foreach (var item in shaders)
            {
                item.material.EnableKeyword(type);
            }
        }

    }
}