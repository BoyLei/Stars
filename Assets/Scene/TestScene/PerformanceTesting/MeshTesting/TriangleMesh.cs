using System.Collections.Generic;
using UnityEngine;


public class TriangleMesh : MonoBehaviour
{
    private Mesh mesh;

    private Vector3[] initVertices;
    private int[] initTrangles;
    private Vector3[] origVertices;
    private int[] origTrangles;

    public int verts;
    public int tris;

    // Use this for initialization
    void Start()
    {
        mesh = transform.GetComponent<MeshFilter>().mesh;
        initVertices = mesh.vertices;
        initTrangles = mesh.triangles;

        GetAllObjects();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            OnIncrese();
        }
    }

    public void OnEmpty()
    {
        mesh.vertices = initVertices;
        mesh.triangles = initTrangles;

        mesh.RecalculateBounds();
        //由于normal没有增加，导致表面看起来不平滑(如果要重新计算normals参考顶点的计算)
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        GetAllObjects();

    }

    public void OnIncrese()
    {
        origVertices = mesh.vertices;
        origTrangles = mesh.triangles;
        Dictionary<Vector3, int> verticesResultDic = new Dictionary<Vector3, int>();
        List<int> tranglesResultList = new List<int>();
        //计算三角面的个数
        int k = origTrangles.Length / 3;
        int index = 0;
        for (int i = 0; i < k; i++)
        {
            //取出一个三角面（的顶点）
            Vector3[] trangle = new Vector3[3] { origVertices[origTrangles[i * 3]], origVertices[origTrangles[i * 3 + 1]], origVertices[origTrangles[i * 3 + 2]] };

            //通过取三条边的中心点
            //原来三个顶点，变成六个顶点 (4倍)
            Vector3[] result = new Vector3[6];

            Vector3 v01 = (trangle[0] + trangle[1]) * 0.5f;
            Vector3 v12 = (trangle[1] + trangle[2]) * 0.5f;
            Vector3 v02 = (trangle[0] + trangle[2]) * 0.5f;


            if (AddVertices(verticesResultDic, trangle[0], index)) index++;
            if (AddVertices(verticesResultDic, trangle[1], index)) index++;
            if (AddVertices(verticesResultDic, trangle[2], index)) index++;

            if (AddVertices(verticesResultDic, v01, index)) index++;
            if (AddVertices(verticesResultDic, v12, index)) index++;
            if (AddVertices(verticesResultDic, v02, index)) index++;


            // 将原三角面分成新的四个三角面
            // 注意左手法则，逆时针顺序
            //三角形数组存储的是顶点在顶点数组中的序号

            tranglesResultList.Add(verticesResultDic[trangle[0]]);
            tranglesResultList.Add(verticesResultDic[v01]);
            tranglesResultList.Add(verticesResultDic[v02]);

            tranglesResultList.Add(verticesResultDic[v01]);
            tranglesResultList.Add(verticesResultDic[trangle[1]]);
            tranglesResultList.Add(verticesResultDic[v12]);

            tranglesResultList.Add(verticesResultDic[trangle[2]]);
            tranglesResultList.Add(verticesResultDic[v02]);
            tranglesResultList.Add(verticesResultDic[v12]);

            tranglesResultList.Add(verticesResultDic[v02]);
            tranglesResultList.Add(verticesResultDic[v01]);
            tranglesResultList.Add(verticesResultDic[v12]);

        }


        mesh.vertices = GetReusltVertices(verticesResultDic);
        mesh.triangles = tranglesResultList.ToArray();

        mesh.RecalculateBounds();
        //由于normal没有增加，导致表面看起来不平滑(如果要重新计算normals参考顶点的计算)
        mesh.RecalculateNormals();
        GetAllObjects();

    }

    bool AddVertices(Dictionary<Vector3, int> verticesResultDic, Vector3 vertice, int index)
    {
        if (verticesResultDic.ContainsValue(index) || verticesResultDic.ContainsKey(vertice))
            return false;

        verticesResultDic.Add(vertice, index);
        return true;
    }

    Vector3[] GetReusltVertices(Dictionary<Vector3, int> verticesResultDic)
    {
        int length = verticesResultDic.Keys.Count;
        Vector3[] result = new Vector3[length];
        List<Vector3> temp = new List<Vector3>(verticesResultDic.Keys);
        for (int i = 0; i < length; i++)
        {
            result[i] = temp[i];
        }

        return result;

    }



    /// <summary>
    /// 得到场景中所有的GameObject
    /// </summary>
    void GetAllObjects()
    {
        verts = 0;
        tris = 0;
        // 获取所有节点的  顶点数 和 面数
        //GameObject[] ob = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        //foreach (GameObject obj in ob)
        //{
        //    GetAllVertsAndTris(obj);
        //}

        // 只获取自己的
        GetAllVertsAndTris(gameObject);
    }
    //得到三角面和顶点数
    void GetAllVertsAndTris(GameObject obj)
    {
        Component[] filters;
        filters = obj.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter f in filters)
        {
            tris += f.sharedMesh.triangles.Length / 3;
            verts += f.sharedMesh.vertexCount;
        }
    }
    void OnGUI()
    {
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;    //这是设置背景填充的
        bb.normal.textColor = new Color(1.0f, 0.5f, 0.0f);   //设置字体颜色的
        bb.fontSize = 40;       //当然，这是字体大小
        string vertsdisplay = verts.ToString("#,##0 verts-顶点数");
        GUILayout.Label(vertsdisplay, bb);
        string trisdisplay = tris.ToString("#,##0 tris-面数");
        GUILayout.Label(trisdisplay, bb);

    }
}