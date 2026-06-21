using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SgameGrass
{
    [CustomEditor(typeof(GrassColorMap))]
    public class GrassColorMapEditor : Editor
    {
        //public  List<Material> GrassMaterialList;  //存储草材质球的List

        //public List<GameObject> TerrainObject;    //需要融合颜色的地面模型

        private GrassColorMap script;
        //序列化对象
        protected SerializedObject _serializedObject;
        protected SerializedObject _serializedObject2;
        //序列化属性
        protected SerializedProperty _assetLstProperty;
        protected SerializedProperty _assetLstProperty2;
        

        private static string colorMapUVname = "_ColorMapUV";

        public void OnEnable()
        {
            //初始化
             script = (GrassColorMap)target;
             _serializedObject = new SerializedObject(target);
             //获取当前类中可序列化的属性
             _assetLstProperty = _serializedObject.FindProperty("TerrainObject");
             _assetLstProperty2 = _serializedObject.FindProperty("GrassMaterialList");
            //throw new NotImplementedException();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _assetLstProperty = serializedObject.FindProperty("TerrainObject");
            _assetLstProperty2 = serializedObject.FindProperty("GrassMaterialList");
            EditorGUILayout.LabelField("此脚本用来设置草地材质球采样地面颜色所需要的世界空间UV,设置完毕删除即可.如果地表Terrain没有移动位置则不用再设置");
            EditorGUILayout.PropertyField(_assetLstProperty, true);
            EditorGUILayout.Space(20);
            EditorGUILayout.PropertyField(_assetLstProperty2, true);
            EditorGUILayout.Space(20);
            serializedObject.ApplyModifiedProperties();
            GUIContent buttonContent = new GUIContent("Set Material UV");
            if (GUILayout.Button(buttonContent))
            {
                setMaterialUV();
            }
            
            
        }

        public void setMaterialUV()
        {
            Bounds bounds = GetTerrainBounds(script.TerrainObject);
            Vector4 uv = BoundsToUV(bounds);
            EditorGUILayout.LabelField("ColorMapUV:"+uv.ToString());
            Debug.Log("ColorMapUV:"+uv.ToString());
            foreach (var v in script.GrassMaterialList)
            {
                v.SetVector(colorMapUVname,uv);
            }
        }
        public static Vector4 BoundsToUV(Bounds b)
        {
            Vector4 uv = new Vector4();

            //Origin position
            uv.x = b.min.x;
            uv.y = b.min.z;
            //Scale factor
            uv.z = 1f / b.size.x;
            uv.w = 0f;

            return uv;
        }
         public static Bounds GetTerrainBounds(List<GameObject> terrainObjects)
        {
            Vector3 minSum = Vector3.one * Mathf.Infinity;
            Vector3 maxSum = Vector3.one * Mathf.NegativeInfinity;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            
            foreach (GameObject item in terrainObjects)
            {
                if (item == null) continue;

                Terrain t = item.GetComponent<Terrain>();
                MeshRenderer r = t ? null : item.GetComponent<MeshRenderer>();

                if (t)
                {
                    //Min/max bounds corners in world-space
                    min = t.GetPosition(); //Doesn't exactly represent the minimum bounds value, but doesn't have to be
                    max = t.GetPosition() + t.terrainData.size; //Note, size is slightly more correct in height than bounds
                }

                if (r)
                {
                    //World-space bounds corners
                    min = r.bounds.min;
                    max = r.bounds.max;
                }
                
                minSum = Vector3.Min(minSum, min);
                
                //Must handle each axis separately, terrain may be further away, but not necessarily higher
                maxSum.x = Mathf.Max(maxSum.x, max.x);
                maxSum.y = Mathf.Max(maxSum.y, max.y);
                maxSum.z = Mathf.Max(maxSum.z, max.z);
            }

            Bounds b = new Bounds(Vector3.zero, Vector3.zero);

            b.SetMinMax(minSum, maxSum);

            //Increase bounds height for flat terrains
            if (b.size.y < 2f)
            {
                b.Encapsulate(new Vector3(b.center.x, b.center.y + 1f, b.center.z));
                b.Encapsulate(new Vector3(b.center.x, b.center.y - 1f, b.center.z));
            }

            //Ensure bounds is always square
            b.size = new Vector3(Mathf.Max(b.size.x, b.size.z), b.size.y, Mathf.Max(b.size.x, b.size.z));
            b.center = Vector3.Lerp(b.min, b.max, 0.5f);

            return b;
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}
