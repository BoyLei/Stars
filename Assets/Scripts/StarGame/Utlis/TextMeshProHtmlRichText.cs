using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;

using Html2UnityRich;
using TMPro;
[XLua.LuaCallCSharp]
public class TextMeshProHtmlRichText : MonoBehaviour
{

    public TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        textMeshPro = transform.GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("输入的 HTML 内容为空！");
            return;
        }

        try
        {
            var rootNode = Html2UnityRichMgr.CreateHtmlRootNode(content).ToPropNode().ToUnityRichNode();
            textMeshPro.text = rootNode.ToTextProRichText();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"在设置文本时出现错误：{ex.Message}");
        }
    }
}
