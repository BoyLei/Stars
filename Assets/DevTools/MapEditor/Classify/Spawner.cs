///--------------------------------------------------------------------
/// 文件名   :   Spawner.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/20 18:25:31
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace MapEditor
{
    [HideMonoScript]
    [SelectionBase]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class Spawner : MonoBehaviour
    {
        [LabelText("ID")]
        [OnValueChanged("FreshReference")]
        public int SpawnerID;

        [LabelText("范围")]
        public int Range = 100;

        [HideInInspector]
        [OnInspectorInit("OnModelInit")]
        public GameObject mModel;

        [LabelText("显示颜色")]
        public Color GizmosColor = Color.green;

        [LabelText("是否显示")]
        public bool Ishow;

        private Color color;

        [Button("一键映射引用")]
        public void SetReference()
        {
            FreshReference();
        }

        [Button("一键贴地")]
        public void OnGround()
        {
            Vector3 position = transform.position;
            transform.position = MapEditorUtils.GetGroundPoint(position);
        }



        public void OnModelInit()
        {
            if (mModel == null)
            {
                mModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mModel.hideFlags = HideFlags.HideAndDontSave;
                mModel.transform.SetParent(transform);
                mModel.transform.localPosition = Vector3.zero;
                mModel.transform.localRotation = Quaternion.identity;
                mModel.transform.localScale = Vector3.one;
            }
        }

        void FreshReference()
        {
            GameObject[] goes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (int i = 0; i < goes.Length; i++)
            {
                GameObject go = goes[i];
                if (go.scene.name != null)
                {
                    if (go != gameObject)
                    {
                        Find(go);
                    }
                }
            }
        }

        void Find(GameObject go)
        {
            if(go==null)
            {
                return;
            }
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if(component==null)
                {
                    continue;
                }
                FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                for (int j = 0; j < fields.Length; j++)
                {
                    Spawner value = fields[j].GetValue(component) as Spawner;
                    if (value == this)
                    {
                        FreshRule freshRule = go.GetComponent<FreshRule>();
                        if (freshRule != null)
                        {
                            freshRule.SpawnerID = SpawnerID;
                        }
                    }
                }
            }

            int ChildCount = go.transform.childCount;
            if (ChildCount > 0)
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    var trans = go.transform.GetChild(i);
                    if (trans != null && trans.gameObject != null)
                    {
                        Find(trans.gameObject);
                    }
                }
            }
        }

        protected void OnDrawGizmos()
        {
            if (Ishow)
            {
                color = Handles.color;
                Handles.color = GizmosColor;
                UnityEditor.Handles.DrawWireArc(transform.position, Vector3.up, Vector3.forward, 360, Range * 0.01f);

                UnityEditor.Handles.Label(transform.position + new Vector3(0, 1, 0), transform.name);
                Handles.color = color;
            }
        }
    }
}
#endif