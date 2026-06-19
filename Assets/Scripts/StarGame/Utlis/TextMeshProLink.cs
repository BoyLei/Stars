using StarProject.Service.Business;
using StarProject.Service.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextMeshProLink : MonoBehaviour, IPointerClickHandler
{

    public List<string> LinkID2URL = new();
    private static readonly string key = "014789a23b56cdef"; // AES加密的密钥，需要保密
    private TextMeshProUGUI m_TextMeshPro;
    void Start()
    {
        m_TextMeshPro = transform.GetComponent<TextMeshProUGUI>();
        m_TextMeshPro.text = m_TextMeshPro.text.Replace("\\", "");
    }

    public void SetUrl(List<string> urls)
    {
        LinkID2URL.Clear();
        LinkID2URL.AddRange(urls);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enabled)
        {
            return;
        }
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, eventData.pressEventCamera);
        if (linkIndex == -1)
        {
            return;
        }
        TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];
        // RectTransformUtility.ScreenPointToLocalPointInRectangle(m_TextMeshPro.rectTransform, eventData.position, eventData.pressEventCamera, out var worldPointInRectangle);
        string des = linkInfo.GetLinkID();
        switch (des)
        {
            case "id_01":
                {
                    string url = LinkID2URL[0];
                    Debug.Log($"点击了id：id_01的超链接 : {url}");
                    SDKManager.Instance.RequestOpenWebView(url);
                    // Application.OpenURL(url);
                }

                break;
            case "id_02":
                {
                    string url = LinkID2URL[1];
                    Debug.Log($"点击了id：id_02的超链接 : {url}");
                    SDKManager.Instance.RequestOpenWebView(url);

                    // Application.OpenURL(url);
                }
                break;
            default:
                if (des.Contains("survey"))
                {
                    string ext = EncryptString(string.Format("{0}_{1}", BusinessManager.Instance.GetUserPID(), BusinessManager.Instance.GetAreaID()), key);
                    string urlEncoded = System.Net.WebUtility.UrlEncode(ext);
                    SDKManager.Instance.RequestOpenWebView(des + urlEncoded);
                }
                break;
        }
    }

    static string EncryptString(string plainText, string keyString)
    {
        byte[] key = Encoding.UTF8.GetBytes(keyString);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = new byte[16]; // AES block size is 16 bytes

            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    byte[] encrypted = msEncrypt.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }
    }
}
