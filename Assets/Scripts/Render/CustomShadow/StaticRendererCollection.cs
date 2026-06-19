using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.Universal.Internal.CustomShadowMgr;

[ExecuteAlways]
public class StaticRendererCollection : MonoBehaviour
{
    [SerializeField]
    //[HideInInspector]
    List<RendererData> m_RendererDataList = new List<RendererData>();
    public void Collect()
    {
        m_RendererDataList.Clear();

        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            bool needAdd = false;
            if (renderer.name.Contains("Lod1_Stc"))
                needAdd = true;

            if (renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                needAdd = true;

            if (needAdd)
            {
                var filter = renderer.GetComponent<MeshFilter>();
                if (filter == null)
                    continue;

                RendererData data = new RendererData(filter.sharedMesh, renderer.sharedMaterial, renderer.transform.localToWorldMatrix);
                m_RendererDataList.Add(data);
            }
        }

        UnityEngine.Rendering.Universal.Internal.CustomShadowMgr.Instance.AddRendererData(m_RendererDataList);
    }

    public void Clear()
    {
        UnityEngine.Rendering.Universal.Internal.CustomShadowMgr.Instance.RemoveRendererData(m_RendererDataList);
        m_RendererDataList.Clear();
    }

    void OnEnable()
    {
        UnityEngine.Rendering.Universal.Internal.CustomShadowMgr.Instance.AddRendererData(m_RendererDataList);
    }

    void OnDisable()
    {
        Clear();
    }
}
