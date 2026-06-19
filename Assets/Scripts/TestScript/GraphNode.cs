#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using StarProject.Service.LocalData;

[ExecuteInEditMode]
public class GraphNode : MonoBehaviour
{
    public Text text;
    public List<GraphNode> parentNodes = new List<GraphNode>();
    public List<GraphNode> childNodes = new List<GraphNode>();
    int myNode;

    public void SetPreNodes(int id, List<GraphNode> preNodes)
    {
        text.text = id.ToString();
        parentNodes = preNodes;

        
        myNode = id;
    }

    public void SetNextNodes(List<GraphNode> nextNodes)
    {
        childNodes = nextNodes;
    }

    private void Update()
    {
        if (childNodes != null)
        {
            var lr = GetComponent<LineRenderer>();
            List<Vector3> arr = new List<Vector3>();

            var nodeData = LocalDataManager.Instance.M_AttriTTNodeData.StaticAttriTTNodeDatas;

            var w = GetComponent<RectTransform>().rect.width;
            var h = GetComponent<RectTransform>().rect.height;
            foreach (var item in childNodes)
            {
                var start = new Vector3(0, 0, -0.001f);//new Vector3(transform.localPosition.x, transform.position.y, 0);
                var end = item.transform.localPosition - transform.localPosition;//new Vector3(item.transform.position.x, item.transform.position.y, 0);
                end = new Vector3(end.x, end.y, -0.001f);

                if (!nodeData.ContainsKey(myNode))
                {
                    start += new Vector3(w/2, 0, 0);
                }
                else if(nodeData[myNode].GetNodeType() == 1)
                {
                    start += new Vector3(w / 2, 0, 0);
                }
                else
                {
                    start += new Vector3(w / 2, 0, 0);
                }

                var w2 = item.GetComponent<RectTransform>().rect.width;
                var h2 = item.GetComponent<RectTransform>().rect.height;
                if (!nodeData.ContainsKey(item.myNode))
                {
                    end -= new Vector3(w2/2, 0, 0);
                }
                else if (nodeData[item.myNode].GetNodeType() == 1)
                {
                    end -= new Vector3(w2/2, 0, 0);
                }
                else
                {
                    end -= new Vector3(w2/2, 0, 0);
                }

                arr.Add(start);
                arr.Add(end);
            }
            lr.positionCount= arr.Count;
            lr.SetPositions(arr.ToArray());
        }
    }
}
#endif