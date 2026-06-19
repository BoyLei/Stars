using StarProject.Service.Resource;
using System;
using System.Resources;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;
[RequireComponent(typeof(UnityEngine.UI.Text))]
public class ScrollText : UIBehaviour
{
    static readonly string s_scrollpaths = "Prefabs/UI/ScrollView";
    Text m_text;
    Color m_origintextcolor;
    [SerializeField]
    Text m_LinkText;
    [SerializeField]
    bool m_isbind = false;
    [SerializeField]
    bool m_ishorizonl = false;
    [SerializeField]
    bool m_isvertical = true;
    protected override void Awake()
    {
       
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_text = GetComponent<Text>();
        if (m_isbind)
        {
            BindTextChange(m_LinkText);
            return;
        }
        m_origintextcolor = m_text.color;
        m_text.color = Color.clear;
        this.enabled = false;
        StartCoroutine(loadScroll());
    }
#if UNITY_EDITOR
    /// <summary>
    /// 仅编辑器下使用
    /// </summary>
    public void ReplaceScrollView()
    {
        m_text = GetComponent<Text>();
        if (m_isbind)
        {
            return;
        }
        m_isbind = true;
        m_origintextcolor = m_text.color;
        m_text.color = Color.clear;
      //  this.enabled = false;
        var tempgo = this.gameObject;
        var clonego = GameObject.Instantiate(tempgo);
        var go = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/Prefabs/UI/ScrollView.prefab");
        var clonetext = CreateScrollLink(tempgo, clonego, go);
    }
#endif

    IEnumerator loadScroll()
    {
        yield return null;
        var tempgo = this.gameObject;
        var clonego = GameObject.Instantiate(tempgo);
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.GameObject>(s_scrollpaths,
        (UnityEngine.GameObject go) =>
        {
            var clonetext = CreateScrollLink(tempgo,clonego,go);
            BindTextChange(clonetext);
        });
    }

    void BindTextChange(Text listentext)
    {
        m_text.RegisterDirtyLayoutCallback(() =>
        {
            listentext.text = m_text.text;
        });
    }

    Text CreateScrollLink(GameObject tempgo,GameObject clonego,GameObject go)
    {
        ScrollRect rect = go.GetComponent<ScrollRect>();
        rect.vertical = m_isvertical;
        rect.horizontal = m_ishorizonl;
        var scrollgo = GameObject.Instantiate(go, tempgo.transform, false);
        var content = scrollgo.transform.Find("Viewport/Content");
        clonego.transform.SetParent(content.transform, false);
        var clonetext = clonego.GetComponent<Text>();
        clonetext.color = m_origintextcolor;
        m_LinkText = clonetext;
        ScrollText scroll = clonego.GetComponent<ScrollText>();
        if(scroll != null)
        {
            MonoBehaviour.DestroyImmediate(scroll);
        }
        return clonetext;
    }
}