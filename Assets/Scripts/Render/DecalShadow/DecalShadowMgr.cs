using UnityEngine;

public class DecalShadowMgr 
{
    Material m_Material;
    Vector3 m_PosOffset = new Vector3(0, -0.2f, 0);
    Vector3 m_ScaleOffset = new Vector3(1, 0.5f, 1);
    int m_DecalShadowMax = 100;

    Mesh m_Mesh;
    Matrix4x4 m_OffsetTrans;
    int m_TargetCount = 0;
    public bool enable { get; set; }
    public static DecalShadowMgr instance
    {
        get
        {
            if (s_Instance == null)
                s_Instance = new DecalShadowMgr();
            return s_Instance;
        }
    }

    static DecalShadowMgr s_Instance;
    Matrix4x4[] m_Matrices;

    public void AddDecalTarget(Vector3 target)
    {
        if (m_TargetCount > m_DecalShadowMax || !enable)
            return;

        m_Matrices[m_TargetCount] = Matrix4x4.Translate(target) * m_OffsetTrans;
        ++m_TargetCount;
    }

    public void Draw()
    {
        if (!enable) 
            return;

        Graphics.DrawMeshInstanced(m_Mesh, 0, m_Material, m_Matrices, m_TargetCount);
        m_TargetCount = 0;
    }
    DecalShadowMgr()
    {
        m_Matrices = new Matrix4x4[m_DecalShadowMax];
        m_Mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        m_Material = Resources.Load<Material>("decalShadowMat");
        m_OffsetTrans = Matrix4x4.TRS(m_PosOffset, Quaternion.identity, m_ScaleOffset);
    }
}
