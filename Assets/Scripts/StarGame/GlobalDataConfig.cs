using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IpConfig
{
    [LabelText("ip类型")]
    public string E_IP;

    [LabelText("ip地址")]
    public string ip;
}

[CreateAssetMenu(menuName = "Assets/Create GlobalDataConfig")]
public class GlobalDataConfig : ScriptableObject
{
    [LabelText("IP地址列表")]
    [TableList]
    public List<IpConfig> Ips;

    [LabelText("IP地址")]
    [ValueDropdown("_ips")]
    public string ipType;

    public IEnumerable _ips()
    {
        List<string> vs = new List<string>();
        foreach (var item in Ips)
        {
            vs.Add(item.E_IP);
        }
        return vs;
    }

    public string GetUrl()
    {
        string ip = "";
        foreach (var item in Ips)
        {
            if (item.E_IP == ipType)
            {
                ip = item.ip;
                break;
            }
        }
        string url = $"{ip}";
        return url;
    }

    private static GlobalDataConfig globalDataConfig;
    public static GlobalDataConfig Instance
    {
        get
        {
            if (globalDataConfig == null)
            {
                globalDataConfig = Resources.Load<GlobalDataConfig>("GlobalDataConfig");
            }
            return globalDataConfig;
        }
    }



}




