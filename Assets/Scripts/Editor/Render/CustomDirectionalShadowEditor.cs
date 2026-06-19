using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CustomDirectionalShadow))]
public class CustomDirectionalShadowEditor : Editor
{
    GameObject m_PreviewObj;
    Material m_Material;
    bool m_Init;
    bool m_TargetEnable = true;

    private void OnEnable()
    {
        CustomDirectionalShadow script = target as CustomDirectionalShadow;

        // Create a temporary object in the scene.
        m_PreviewObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        m_PreviewObj.hideFlags = HideFlags.HideAndDontSave;

        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");

        // Also copy the default material (we don't want to change that for all Unity default objects)
        m_Material = new Material(m_PreviewObj.GetComponent<Renderer>().sharedMaterial)
        //material = new Material(unlit)
        {
            hideFlags = HideFlags.HideAndDontSave,
            //mainTexture = script.shadowCameraTexture,
        };
        m_Material.shader = unlit;
        m_PreviewObj.GetComponent<Renderer>().sharedMaterial = m_Material;

        // Rotate to the 2D plane, assuming the texture UV's are flipped in this case.
        m_PreviewObj.transform.Rotate(0f, 0, 0f);
    }

    private void OnDisable()
    {
        if (m_PreviewObj != null)
            DestroyImmediate(m_PreviewObj);

        if (m_Material != null)
            DestroyImmediate(m_Material);

        m_Init = false;
    }

    public void OnSceneGUI()
    {
        return;

        CustomDirectionalShadow t = target as CustomDirectionalShadow;
        if (t == null)
            return;
        
        if (m_TargetEnable != t.enabled && t.enabled)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            SceneView.lastActiveSceneView.Repaint();

            CustomDirectionalShadow script = target as CustomDirectionalShadow;
            if (script.shadowCameraTexture != null)
            {
                m_Material.mainTexture = script.shadowCameraTexture;
                m_PreviewObj.GetComponent<Renderer>().sharedMaterial = m_Material;
                m_Init = true;
            }
        }
        m_TargetEnable = t.enabled;

        if (t.enabled && m_PreviewObj)
        {
            Camera sceneCamera = SceneView.lastActiveSceneView.camera;
            Vector3 pos = sceneCamera.transform.position + sceneCamera.transform.forward * 5;
            m_PreviewObj.transform.position = pos;
            m_PreviewObj.transform.LookAt(sceneCamera.transform);
            var delta = 1.0f / (sceneCamera.nearClipPlane * sceneCamera.nearClipPlane);
            pos = sceneCamera.ViewportToWorldPoint(new Vector3(1, 0, sceneCamera.nearClipPlane * (1 + delta)));
            m_PreviewObj.transform.position = pos - sceneCamera.transform.right * 0.5f + sceneCamera.transform.up * 0.5f;
        }
    }
}
