///--------------------------------------------------------------------
/// 文件名   :   UIEditor
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/07/18 14:59:17
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class UIEditor : EditorWindow
{
    public UIPanelType panelType;

    [MenuItem("Tools/UIEditor")]
    static public void OpenWindow()
    {
        UIEditor window = GetWindow<UIEditor>("UIEditor");
        CsSavePath = Application.dataPath + "/Scripts/StarGame/UI";
        LuaSavePath = Application.dataPath + "/Resources/LuaScripts/View";
        window.Show();
    }

    public Vector2 ScrollView = Vector2.zero;
    private GameObject mPrefab;
    private List<UIElement> uIElements = new List<UIElement>();
    static private string CsSavePath;
    static private string LuaSavePath;

    public string mTemplate;
    public string mLuaTemplate;


    private void OnGUI()
    {
        GameObject go = (GameObject)EditorGUILayout.ObjectField("界面预制件", mPrefab, typeof(GameObject), true);
        if (go != mPrefab)
        {
            mPrefab = go;
            Init();
            Repaint();
        }

        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.TextField("Cs文件路径", CsSavePath);
        if (GUILayout.Button("选择"))
        {
            string path = EditorUtility.OpenFolderPanel("Cs文件路径", "Open Folder", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (CsSavePath != path)
                {
                    CsSavePath = path;
                }
            }
        }

        GUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.TextField("Lua文件路径", LuaSavePath);
        if (GUILayout.Button("选择"))
        {
            string path = EditorUtility.OpenFolderPanel("Lua文件路径", "Open Folder", "");
            if (!string.IsNullOrEmpty(path))
            {
                if (LuaSavePath != path)
                {
                    LuaSavePath = path;
                }
            }
        }

        GUILayout.EndHorizontal();

        if (mPrefab == null)
        {
            return;
        }

        panelType = (UIPanelType)EditorGUILayout.EnumPopup("界面类型", panelType);
        EditorGUILayout.Space(20);
        ScrollView = GUILayout.BeginScrollView(ScrollView);
        if (uIElements != null && uIElements.Count > 0)
        {
            List<int> removes = new List<int>();
            int index = 0;
            foreach (var item in uIElements)
            {
                bool removed = DrawUIElement(item);
                if (removed)
                {
                    break;
                }

                index++;
            }

            if (index >= 0 && index < uIElements.Count)
            {
                uIElements.RemoveAt(index);
                index = 0;
                Repaint();
            }
        }

        if (GUILayout.Button("Add"))
        {
            UIElement uI = new UIElement();
            uIElements.Add(uI);
        }

        GUILayout.EndScrollView();
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("生成CS文件"))
        {
            Execute(false);
        }

        if (GUILayout.Button("生成Lua文件"))
        {
            Execute(true);
        }

        GUILayout.EndHorizontal();
    }

    private bool DrawUIElement(UIElement uIElement)
    {
        if (uIElement == null)
        {
            return false;
        }

        GUILayout.BeginHorizontal("box");
        uIElement.UIName = EditorGUILayout.TextField("节点名称", uIElement.UIName);
        GameObject go = (GameObject)EditorGUILayout.ObjectField("节点对象", uIElement.Object, typeof(GameObject), true);
        if (uIElement.Object != go)
        {
            uIElement.Object = go;
            if (uIElement.Object == null)
            {
                uIElement.UIName = "";
                uIElement.Type = "";
            }
            else
            {
                uIElement.UIName = uIElement.Object.name.Replace(" ", "");
                if (uIElement.UIName.Contains("("))
                {
                    int index = uIElement.UIName.IndexOf('(');
                    uIElement.UIName = uIElement.UIName.Substring(0, index);
                }

                uIElement.Components.Clear();
                uIElement.Components.Add("GameObject");
                uIElement.Components.Add("Transform");
                var types = uIElement.Object.GetComponents<Component>();
                if (types != null)
                {
                    foreach (var item in types)
                    {
                        uIElement.Components.Add(item.GetType().Name);
                    }
                }
            }
        }

        uIElement.index = EditorGUILayout.Popup("节点类型", uIElement.index, uIElement.Components.ToArray());
        if (GUILayout.Button("删除"))
        {
            return true;
        }

        GUILayout.EndHorizontal();
        return false;
    }

    private void Init()
    {
        uIElements.Clear();
        int childCount = mPrefab.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform go = mPrefab.transform.GetChild(i);
            if (go != null)
            {
                UIElement uIElement = new UIElement();
                uIElement.Object = go.gameObject;
                uIElement.UIName = uIElement.Object.name.Replace(" ", "");
                if (uIElement.UIName.Contains("("))
                {
                    int index = uIElement.UIName.IndexOf('(');
                    uIElement.UIName = uIElement.UIName.Substring(0, index);
                }

                uIElement.Components.Clear();
                uIElement.Components.Add("GameObject");
                uIElement.Components.Add("Transform");
                var types = uIElement.Object.GetComponents<Component>();
                if (types != null)
                {
                    foreach (var item in types)
                    {
                        uIElement.Components.Add(item.GetType().Name);
                    }
                }

                uIElements.Add(uIElement);
            }
        }
    }

    public void Execute(bool isLua)
    {
        if (isLua)
        {
            WriteLua();
        }
        else
        {
            WriteCs();
        }

        AssetDatabase.Refresh();
    }

    private string GetCsProperty()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            builder.Append($"    private {item.Components[item.index]} m_{item.UIName};\n");
        }

        return builder.ToString();
    }

    private string GetLuaProperty()
    {
        string className = mPrefab.name;
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            builder.Append($"{className}.m_{item.UIName}=nil;\n");
        }

        foreach (var item in uIElements)
        {
            if (item.IsButton || item.IsJButton || item.IsSlider)
            {
                builder.Append($"local {item.UIName}Func=nil;\n");
            }
        }

        return builder.ToString();
    }

    private void CalPath(Transform go, ref string path)
    {
        if (go != null)
        {
            if (go != mPrefab.transform)
            {
                path += go.name + "/";
            }

            if (go != mPrefab.transform)
            {
                CalPath(go.parent, ref path);
            }
        }
    }

    public string reverseCharArrays(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            StringBuilder builder = new StringBuilder();
            string[] array = s.Split('/');
            if (array != null && array.Length > 0)
            {
                string path = string.Empty;
                for (int i = array.Length - 1; i > -1; i--)
                {
                    if (!string.IsNullOrEmpty(array[i]))
                    {
                        path += ("/" + array[i]);
                    }
                }

                path = path.Remove(0, 1);
                return path;
            }
        }

        return s;
    }


    private string GetCsAwake()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            string path = string.Empty;
            CalPath(item.Object.transform, ref path);
            string finalName = reverseCharArrays(path);
            if (item.IsGameObject)
            {
                builder.Append($"m_{item.UIName}=transform.Find(\"{finalName}\").gameObject;\n");
            }
            else if (item.IsTransfom)
            {
                builder.Append($"m_{item.UIName}=transform.Find(\"{finalName}\");\n");
            }
            else
            {
                builder.Append(
                    $"m_{item.UIName}=transform.Find(\"{finalName}\").GetComponent<{item.Components[item.index]}>();\n");
            }
        }

        return builder.ToString();
    }

    private string GetLuaAwake()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            string path = string.Empty;

            CalPath(item.Object.transform, ref path);
            string finalName = reverseCharArrays(path);
            if (item.IsGameObject)
            {
                builder.Append($"\tself.m_{item.UIName}=self:Find(\"{finalName}\").gameObject;\n");
            }
            else if (item.IsTransfom)
            {
                builder.Append($"\tself.m_{item.UIName}=self:Find(\"{finalName}\");\n");
            }
            else
            {
                builder.Append(
                    $"\tself.m_{item.UIName}=self:Find(\"{finalName}\"):GetComponent(typeof({item.Components[item.index]}));\n");
            }
        }

        foreach (var item in uIElements)
        {
            if (item.IsButton || item.IsJButton)
            {
                builder.Append($"\t{item.UIName}Func=function() self:OnClick{item.UIName}(); end;\n");
            }

            if (item.IsSlider)
            {
                builder.Append($"\t{item.UIName}Func=function() self:OnValueChange{item.UIName}(); end;\n");
            }
        }

        return builder.ToString();
    }


    private string GetCsStart()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            if (item.IsButton)
            {
                builder.Append($"m_{item.UIName}.onClick.AddListener(OnClick{item.UIName});\n");
            }

            if (item.IsSlider)
            {
                builder.Append($"m_{item.UIName}.onValueChanged.AddListener(OnValueChange{item.UIName});\n");
            }

            if (item.IsJButton)
            {
                builder.Append($"m_{item.UIName}.OnClick=OnClick{item.UIName};\n");
            }
        }

        return builder.ToString();
    }

    private string GetLuaStart()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            if (item.IsButton)
            {
                builder.Append($"\tself.m_{item.UIName}.onClick:AddListener({item.UIName}Func);\n");
            }

            if (item.IsSlider)
            {
                builder.Append($"\tself.m_{item.UIName}.onValueChanged:AddListener({item.UIName}Func);\n");
            }

            if (item.IsJButton)
            {
                builder.Append($"\tself.m_{item.UIName}.OnClick= {item.UIName}Func;\n");
            }
        }

        return builder.ToString();
    }


    private string GetCsFunction()
    {
        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            if (item.IsButton)
            {
                builder.Append($"private void OnClick{item.UIName}()\n");
                builder.Append("{\n");
                builder.Append("}\n");
            }

            if (item.IsSlider)
            {
                builder.Append($"private void OnValueChange{item.UIName}(float vaule)\n");
                builder.Append("{\n");
                builder.Append("}\n");
            }

            if (item.IsJButton)
            {
                builder.Append($"private void OnClick{item.UIName}(GameObject target)\n");
                builder.Append("{\n");
                builder.Append("}\n");
            }
        }

        return builder.ToString();
    }

    private string GetLuaFunction()
    {
        string className = mPrefab.name;

        StringBuilder builder = new StringBuilder();
        foreach (var item in uIElements)
        {
            if (item.IsButton || item.IsJButton)
            {
                builder.Append($"function {className}:OnClick{item.UIName}()\n");
                builder.Append("\n");
                builder.Append("end\n");
                builder.Append("\n");
            }

            if (item.IsSlider)
            {
                builder.Append($"function {className}:OnValueChange{item.UIName}(vaule)\n");
                builder.Append("\n");
                builder.Append("end\n");
                builder.Append("\n");
            }
        }

        return builder.ToString();
    }

    private void WriteCs()
    {
        if (!Directory.Exists(CsSavePath))
        {
            Directory.CreateDirectory(CsSavePath);
        }

        string mContent = mTemplate;
        string className = mPrefab.name;
        mContent = mContent.Replace("#CLASSNAME#", className);
        mContent = mContent.Replace("#UITYPE#", panelType.ToString());
        mContent = mContent.Replace("#UIPROPERTY#", GetCsProperty());
        mContent = mContent.Replace("#AWAKE#", GetCsAwake());
        mContent = mContent.Replace("#START#", GetCsStart());
        mContent = mContent.Replace("#FUNCTION#", GetCsFunction());
        mContent = mContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        mContent = mContent.Replace("#AUTHOR#", System.Environment.UserName);
        string filePath = CsSavePath + "/" + className + ".cs";
        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(mContent);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
    }

    private void WriteLua()
    {
        if (!Directory.Exists(LuaSavePath))
        {
            Directory.CreateDirectory(LuaSavePath);
        }

        string mContent = mLuaTemplate;
        string className = mPrefab.name;
        mContent = mContent.Replace("#CLASSNAME#", className);
        mContent = mContent.Replace("#UIPROPERTY#", GetLuaProperty());
        mContent = mContent.Replace("#AWAKE#", GetLuaAwake());
        mContent = mContent.Replace("#START#", GetLuaStart());
        mContent = mContent.Replace("#FUNCTION#", GetLuaFunction());
        mContent = mContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        mContent = mContent.Replace("#AUTHOR#", System.Environment.UserName);
        string filePath = LuaSavePath + "/" + className + ".lua.txt";
        FileStream fs = new FileStream(filePath, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(mContent);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
    }

    public void OnEnable()
    {
        mTemplate = File.ReadAllText("Assets/DevTools/UIEditor/UITemplate.cs.txt", System.Text.Encoding.UTF8);
        mLuaTemplate = File.ReadAllText("Assets/DevTools/UIEditor/UILuaTemplate.lua.txt", System.Text.Encoding.UTF8);
    }

    public void OnDestroy()
    {
        mPrefab = null;
        uIElements.Clear();
        mTemplate = string.Empty;
        mLuaTemplate = string.Empty;
    }
}

public class UIElement
{
    public List<string> Components = new List<string>();
    public string UIName;
    public GameObject Object;
    public string Type;
    public int index;

    public bool IsGameObject
    {
        get { return Components[index] == "GameObject"; }
    }

    public bool IsTransfom
    {
        get { return Components[index] == "Transform"; }
    }

    public bool IsButton
    {
        get { return Components[index] == "Button"; }
    }

    public bool IsJButton
    {
        get { return Components[index] == "JButton"; }
    }

    public bool IsSlider
    {
        get { return Components[index] == "Slider"; }
    }
}

public enum UIPanelType
{
    UIWidget = 0,
    UIWindow = 1,
    UIPage = 2
}

#endif