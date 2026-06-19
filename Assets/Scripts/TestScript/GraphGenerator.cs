#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using StarProjectDef;
using StarProject.Service.LocalData;
using UnityEditor;
using GameDLL.Hdg;
using UnityEngine.UIElements;

public class GraphGenerator : MonoBehaviour
{
    public GameObject bigNode;
    public GameObject smallNode;
    public GameObject rootNode;
    public GameObject lineObj;

    Dictionary<int, List<AttriTTNodeDataCell>> allNodes = new Dictionary<int, List<AttriTTNodeDataCell>>();
    public int rootNumber = 0;
    public int disx = 100;
    public int disy = 200;

    public Dictionary<int, GraphNode> allObjs = new Dictionary<int, GraphNode>();

    [Button("生成")]
    public void Generate()
    {
        if (rootNumber == 0)
        {
            UnityEditor.EditorUtility.DisplayDialog("错误", "请输入天赋树根节点id", "确定");
            return;
        }
        allNodes.Clear();
        var nodeData = LocalDataManager.Instance.M_AttriTTNodeData.StaticAttriTTNodeDatas;
        foreach(var node in nodeData)
        {
            int treeNumber = node.Value.GetFromTree();
            if (!allNodes.ContainsKey(treeNumber))
            {
                allNodes[treeNumber] = new List<AttriTTNodeDataCell>();
            }
            allNodes[treeNumber].Add(node.Value);
        }

        if(!allNodes.ContainsKey(rootNumber))
        {
            UnityEditor.EditorUtility.DisplayDialog("错误", "天赋树根节点输入有误，请重新输入", "确定");
            return;
        }

        var treeNodes = allNodes[rootNumber];
        List<int> allPredeNodes = new List<int>();
        foreach(var node in treeNodes)
        {
            allPredeNodes.AddRange(node.PredeNode);
        }
        List<int> leafNodes = new List<int>();
        foreach(var node in treeNodes) 
        {
            if(!allPredeNodes.Contains(node.GetID()))
            {
                leafNodes.Add(node.GetID());
            }
        }
        
        foreach(var item in allObjs)
        {
            if(item.Value != null)
            {
                GameObject.DestroyImmediate(item.Value.gameObject);
            }
        }
        allObjs.Clear();
        GenerateTree(leafNodes);

        foreach(var item in allObjs)
        {
            if (!nodeData.ContainsKey(item.Key))
            {
                item.Value.SetPreNodes(item.Key, null);
                continue;
            }
            var l = nodeData[item.Key].PredeNode;
            List<GraphNode> preNodes = new List<GraphNode>();
            foreach(var node in l)
            {
                preNodes.Add(allObjs[node]);
            }
            item.Value.SetPreNodes(item.Key, preNodes);
        }

        foreach (var item in allObjs)
        {
            var obj = item.Value;
            List<GraphNode> childNodes = new List<GraphNode>();
            foreach (var node in allObjs)
            {
                if (node.Value.parentNodes != null && node.Value.parentNodes.Contains(obj))
                {
                    childNodes.Add(node.Value);
                }
            }
            item.Value.SetNextNodes(childNodes);
        }

        Refresh();
    }

    [Button("清理")]
    public void Clear()
    {
        foreach (var item in allObjs)
        {
            if (item.Value != null)
            {
                GameObject.DestroyImmediate(item.Value.gameObject);
            }
        }
        allObjs.Clear();
    }

    [Button("影藏line")]
    public void HideLine()
    {
        var lrs = transform.GetComponentsInChildren<LineRenderer>();
        foreach (var node in lrs)
        {
            node.enabled = !node.enabled;
        }
    }

    [Button("生成线")]
    public void GenerateLine()
    {
        var lines = transform.Find("Lines");
        if(lines == null)
        {
            var obj = new GameObject();
            var newObj = GameObject.Instantiate(obj);
            newObj.transform.parent = transform;
            lines = newObj.transform;
            lines.localPosition = Vector3.zero;
            lines.localScale= Vector3.one;
            lines.name = "Lines";
            lines.SetAsFirstSibling();
        }
        else
        {
            lines.DestroyChildrenImmediate();
        }
        var gns = transform.GetComponentsInChildren<GraphNode>();
        foreach (var node in gns)
        {
            var starPos = node.transform.localPosition;

            foreach (var cnode in node.childNodes)
            {
                var endPos = cnode.transform.localPosition;
                var dir = endPos - starPos;
                float angle = GetAngle(dir, Vector3.right, new Vector3(0,0,-1));
                
                var obj = GameObject.Instantiate(lineObj);
                obj.transform.parent = lines;
                obj.transform.localRotation = Quaternion.Euler(0, 0, angle);
                obj.transform.localPosition = (starPos + endPos)/2;
                obj.transform.localScale = Vector3.one;
                var rt = obj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(dir.magnitude, 7);
                obj.SetActive(true);
                obj.name = node.text.text + "-" + cnode.text.text;
            }
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    [Button("清理线")]
    public void ClearLine()
    {
        var lines = transform.Find("Lines");
        if (lines != null)
        {
            lines.DestroyChildrenImmediate();
        }
    }

    private float GetAngle(Vector3 a, Vector3 b, Vector3 dir)
    {
        float angle1 = Vector3.Angle(a, b);
        Vector3 normal = Vector3.Cross(a, b);
        angle1 *= Mathf.Sign(Vector3.Dot(normal, dir));
    
        return angle1;
    }

    void Refresh()
    {
        GraphNode rootNode = null;
        foreach (var node in allObjs)
        {
            if (node.Value.parentNodes == null || node.Value.parentNodes.Count == 0)
            {
                rootNode = node.Value;
                rootNode.transform.localPosition = Vector3.zero;
                break;
            }
        }

        CalculatePos(rootNode, new Vector3(-790, 0, 0));
    }

    void CalculatePos(GraphNode node, Vector3 pos)
    {
        node.transform.localPosition = pos;
        int count = node.childNodes.Count;
        if(count == 0)
        {
            return;
        }
        if (count % 2 == 0)
        {
            for (int index = 0; index < count; index++)
            {
                var newpos = pos + new Vector3(disx, disy / count / 2 + disy / count * (count / 2 - index - 1));
                CalculatePos(node.childNodes[index], newpos);
            }
        }
        else
        {
            for (int index = 0; index < count; index++)
            {
                var newpos = pos + new Vector3(disx, disy / count * (count / 2 - index));
                CalculatePos(node.childNodes[index], newpos);
            }
        }
    }

    public void GenerateTree(List<int> nodes)
    {
        var nodeData = LocalDataManager.Instance.M_AttriTTNodeData.StaticAttriTTNodeDatas;
        foreach(var item in nodes) 
        {
            if(allObjs.ContainsKey(item))
            {
                continue;
            }
            if(nodeData.ContainsKey(item))
            {
                var node = nodeData[item];
                GameObject obj = null;
                if (node.GetNodeType() == 1)
                {
                    obj = GameObject.Instantiate(smallNode);
                }
                else
                {
                    obj = GameObject.Instantiate(bigNode);
                }
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);
                obj.name = item.ToString();
                allObjs[node.GetID()] = obj.GetComponent<GraphNode>();

                GenerateTree(node.PredeNode);
            }
            else
            {
                var obj = GameObject.Instantiate(rootNode);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);
                obj.name = item.ToString();
                allObjs[item] = obj.GetComponent<GraphNode>();
            }
        }
    }
}

/*[CustomEditor(typeof(GraphGenerator))]
public class ExampleEditor : Editor
{
    public void OnSceneGUI()
    {
        var gg = GameObject.Find("GraphGenerator").GetComponent<GraphGenerator>();
        var gos = gg.allObjs;

        foreach (var t in gos) 
        {
            foreach (var item in t.Value.childNodes)
            {
                Handles.DrawLine(t.Value.transform.position, item.transform.position);
            }
        }
    }
}*/
#endif