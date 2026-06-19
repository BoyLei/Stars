using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
public class GameObjectPool<T> where T : Component
{
    private readonly Stack<T> m_Stack = new Stack<T>();
    private readonly Action<T> m_ActionOnGet;
    private readonly Action<T> m_ActionOnRelease;
    public GameObject Temp;
    public int countAll { get; private set; }
    public int countActive { get { return countAll - countInactive; } }
    public int countInactive { get { return m_Stack.Count; } }

    public GameObjectPool(GameObject _go, Action<T> actionOnGet, Action<T> actionOnRelease, Transform parent = null)
    {
        Temp = _go;
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        mParent = parent;
    }
    public Transform mParent;
    public T Get()
    {
        return Get(mParent);
    }
    public T Get(Transform parent)
    {
        T element;
        if (m_Stack.Count == 0)
        {
            //Debug.Log("Instantiate:" + Temp.name);
            GameObject go = GameObject.Instantiate(Temp, parent);
            //go.name = Temp.name + "_" + countAll;
            element = go.GetComponent<T>();
            if (element == null)
                element = go.AddComponent<T>();
            countAll++;
        }
        else
        {
            element = m_Stack.Pop();
        }
        if (m_ActionOnGet != null)
            m_ActionOnGet(element);
        return element;
    }

    public void Release(T element)
    {
        //if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
        //    Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
        if (m_ActionOnRelease != null)
            m_ActionOnRelease(element);
        m_Stack.Push(element);
    }
}

/// <summary>
/// 一个超级简单的 节点池,后续看情况再拓展
/// </summary>
public static class NodePool
{
    /// <summary>
    /// 节点池的类型
    /// </summary>
    public enum NodePoolType
    {
        /// <summary>
        /// 特效的根节点
        /// </summary>
        FxRoot,
        /// <summary>
        /// 特效节点
        /// </summary>
        Fx,

    }
    public static StringBuilder sb = new StringBuilder();

    public static Dictionary<NodePoolType, Stack<GameObject>> m_StackDic = new Dictionary<NodePoolType, Stack<GameObject>>();

    public static GameObject Get(NodePoolType nodePoolTypeePool)
    {
        GameObject go;
        Stack<GameObject> m_Stack = GetStack(nodePoolTypeePool);
        if (m_Stack.Count > 0)
        {
            go = m_Stack.Pop();
        }
        else
        {
            go = new GameObject();
        }
        go.SetActive(true);
        return go;
    }

    private static Transform GetNodePoolParent(NodePoolType nodePoolType)
    {

        string name = sb.Clear().Append("NodePool_").Append(nodePoolType).ToString();

        Transform root = EntityRoot.Instance.DotRemoveRoot.transform.Find(name);
        if (root == null)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = EntityRoot.Instance.DotRemoveRoot.transform;
            root = go.transform;
        }
        return root;
    }

    private static Stack<GameObject> GetStack(NodePoolType nodePoolType)
    {
        if (m_StackDic.TryGetValue(nodePoolType, out Stack<GameObject> stack))
        {
            return stack;
        }
        stack = new Stack<GameObject>();
        m_StackDic.Add(nodePoolType, stack);
        return stack;
    }

    private static void Put(GameObject go, NodePoolType nodePoolType, bool putStack = true)
    {
        go.transform.gameObject.SetActive(false);

        go.transform.parent = GetNodePoolParent(nodePoolType);

        if (putStack)
        {
            GetStack(nodePoolType).Push(go);
        }
    }

    /// <summary>
    /// 放入 对于类型的 节点池中, 并存入 stack。 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="nodePoolType"></param>
    /// <param name="putStack">是否将 数据 入栈</param>
    public static void Put(Transform go, NodePoolType nodePoolType, bool putStack = true)
    {
        Put(go.gameObject, nodePoolType, putStack);
    }


}