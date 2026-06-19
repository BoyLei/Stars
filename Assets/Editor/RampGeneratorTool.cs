using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using DG.DemiEditor;
using UnityEngine.UIElements;

public class GradientCreator : EditorWindow
{
    [MenuItem("Tools/GradientCreator")]
    private static void ShowWindow()
    {
        var window = GetWindow<GradientCreator>();
        window.titleContent = new GUIContent("GradientCreator");
        window.Show();
    }


    ///<绘制面板>
    private int _GradientWidth = 128;//每一条渐变的宽度
    private int _GradientHeight = 4;//每一条渐变的高度
    public RampGeneratorData _RampGeneratorData;
    private Texture2D _GradientMap;
    private Material _TargetMat;
    private string MatSavePath;
    private bool ifFirstSave = true;
    private void OnGUI()
    {
        _RampGeneratorData = EditorGUILayout.ObjectField("GradientData", _RampGeneratorData, typeof(RampGeneratorData), false) as RampGeneratorData;
        if (_RampGeneratorData)
        {
            _GradientWidth = _RampGeneratorData._GradientWidth;
        }
        else
        {
            _GradientWidth = 128;
        }


        GradientListGUI();

        _GradientMap = Create(_Gradient, _GradientWidth, _Gradient.Count * _GradientHeight);
        _GradientMap.wrapMode = TextureWrapMode.Clamp;
        //无需保存贴图也能传递给shader
        if (_TargetMat != null)
            _TargetMat.SetTexture("_RampTexture", _GradientMap);
        //Shader.SetGlobalTexture("_Gradient", _GradientMap);
        SceneView.RepaintAll();

        Save();
    }

    ///<绘制渐变控制列表>
    [SerializeField]//必须要加
    protected List<Gradient> _Gradient = new List<Gradient>();
    protected List<string> _GradientNameList = new List<string>();
    protected SerializedObject _serializedObject;    //序列化对象
    protected SerializedProperty _assetLstProperty;   //序列化属性
    private int _GradientSize;
    private void GradientListGUI()//绘制列表
    {
       if (_RampGeneratorData)
       {
            _Gradient = _RampGeneratorData._RampList;
            _GradientSize = _RampGeneratorData._RampList.Count;
       }
      
        
       if(GUILayout.Button("初始化所有Ramp"))
        {
            //List<Gradient> gradientListTemp = ();
            int arrange = _Gradient.Count;
            for(int i = 0;i<arrange;i++)
            {
                int alphaKeyLength = _Gradient[i].alphaKeys.Length;
                int colorKeyLength = _Gradient[i].colorKeys.Length;
                for(int j =0; j<alphaKeyLength;j++)
                {
                    _Gradient[i].alphaKeys[j].alpha = 255;
                }
                for(int j = 0; j < colorKeyLength; j++)
                {
                    _Gradient[i].colorKeys[j].color = Color.gray;
                }
            }
            _assetLstProperty = _serializedObject.FindProperty("_Gradient");
        }

        //获取输入的参数
        if (_RampGeneratorData)
        { 
            _GradientSize = EditorGUILayout.IntField("Size", _GradientSize);
            if (_GradientSize < 0)
            {
                _GradientSize = _RampGeneratorData._RampList.Count;
            }
            //若输入的值较小 则遍历删除队尾元素
            if (_GradientSize < _RampGeneratorData._RampList.Count)
            {
                int i = _RampGeneratorData._RampList.Count - _GradientSize;
                for (int j = 0; j < i; j++)
                {
                    int tempCount = _Gradient.Count;
                    Debug.Log(tempCount);
                    _Gradient.Remove(_Gradient[tempCount - 1]);
                }
            }//若输入值较大 则往队尾增加元素
            else if (_GradientSize > _RampGeneratorData._RampList.Count)
            {
                int i = _GradientSize - _RampGeneratorData._RampList.Count;
                for (int j = 0; j < i; j++)
                {
                    _Gradient.Add(new Gradient());
                }
            }
        }

        //顺序输出_Gradient的所有元素
        int gradientCount = _Gradient.Count;
        //_Gradient.Reverse();
        for (int i = _Gradient.Count - 1 ; i >=0; i--)
        {

            _Gradient[i] = EditorGUILayout.GradientField(_Gradient[i]);
            //Debug.Log("输出第" + (_Gradient.Count - 1 - i) + "个ramp");
           // Debug.Log("Grandient有" + _Gradient.Count + "条贴图");
            
        }
        //_Gradient.Reverse();

 /*       _serializedObject.Update();
        //开始检查是否有修改
        EditorGUI.BeginChangeCheck();
        // _Gradient = (List<Gradient>)EditorGUILayout.ObjectField("Gradient", _Gradient, typeof(List<Gradient>));
        EditorGUILayout.PropertyField(_assetLstProperty, true);//显示属性 //第二个参数必须为true，否则无法显示子节点即List内容
        if (EditorGUI.EndChangeCheck())//结束检查是否有修改
        {
            _serializedObject.ApplyModifiedProperties();//提交修改
        }*/
    }
    ///<存储纹理>
    string _GradientName;

    string[] MapFormat = { "TGA", "PNG", "JPG" };
    int FormatIndex = 0;

    //是否第一次点击存储
    private bool onFirstSave()
    {
        if (ifFirstSave)
        {
            ifFirstSave = false;
            return true;
        }

        return false;
    }
    private void Save()
    {
        EditorGUI.BeginChangeCheck();
        _TargetMat =(Material) EditorGUILayout.ObjectField("目标材质球", _TargetMat,typeof(Material));
        if (EditorGUI.EndChangeCheck())
        {
            if (_TargetMat!=null)
            {
                MatSavePath = AssetDatabase.GetAssetPath(_TargetMat);
                MatSavePath = MatSavePath.Replace("Assets", "");
                MatSavePath = MatSavePath.Replace("/" + _TargetMat.name + ".mat", "");
                MatSavePath = MatSavePath.Replace("/Material", "");
                MatSavePath = Application.dataPath + MatSavePath + "/Texture/Ramp";
                Debug.Log("材质球的存储路径为" + MatSavePath + "1");
                //Debug.Log("材质球的存储路径为" + MatSavePath + "1");
            }
        }

        if (_RampGeneratorData)
        {
            _RampGeneratorData._GradientWidth = EditorGUILayout.IntField("每条渐变宽度(像素)", _GradientWidth);
            _GradientHeight = _RampGeneratorData._GradientHeight;
            _RampGeneratorData._GradientHeight = EditorGUILayout.IntField("每条渐变高度(像素)", _GradientHeight);
        }
        else
        {
            _GradientWidth = EditorGUILayout.IntField("每条渐变宽度(像素)", _GradientWidth);
            _GradientHeight = EditorGUILayout.IntField("每条渐变高度(像素)", _GradientHeight);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("纹理名称", GUILayout.Width(100));
        if (_RampGeneratorData)
        {
            _GradientName = _RampGeneratorData._GradientName;
            _RampGeneratorData._GradientName = EditorGUILayout.TextArea(_GradientName);
        }
        else
        {
            _GradientName = EditorGUILayout.TextArea(_GradientName);

        }

        FormatIndex = EditorGUILayout.Popup(FormatIndex, MapFormat, GUILayout.Width(100));
        string _Format = ".tga";
        if (FormatIndex == 0)
            _Format = ".tga";
        if (FormatIndex == 1)
            _Format = ".tga";
        if (FormatIndex == 2)
            _Format = ".jpg";
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save"))
        {
            Debug.Log("材质球的存储路径为" + MatSavePath + "2");
            string path = EditorUtility.SaveFolderPanel("Select an output path", MatSavePath, "");
            
            if (!path.IsNullOrEmpty())
            {
                 MatSavePath = path;
            }
           
            Debug.Log("材质球的存储路径为" + MatSavePath + "3");
            
            if (_RampGeneratorData)
            {
                _RampGeneratorData._RampList = this._Gradient;
            }

            byte[] pngData = _GradientMap.EncodeToPNG();
            File.WriteAllBytes(path + "/" + _GradientName + _Format, pngData);
            EditorUtility.SetDirty(_RampGeneratorData);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
    }

    ///<序列化属性列表>
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);//使用当前类初始化
        _assetLstProperty = _serializedObject.FindProperty("_Gradient");
    }

    ///<生成纹理函数>
    Texture2D Create(List<Gradient> Gradient, int width = 32, int height = 1)
    {
        var _GradientMap = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _GradientMap.filterMode = FilterMode.Bilinear;
        float inv = 1f / (width - 1);

        int eachHeight = height / 1;
        if (Gradient.Count != 0)
        {
            eachHeight = height / Gradient.Count;
        }

        int howMany = 0;
        while (howMany != Gradient.Count)
        {
            int start = height - eachHeight * howMany - 1;
            int end = start - eachHeight;
            for (int y = start; y > end; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var t = x * inv;
                    Color col = Gradient[howMany].Evaluate(t);
                    _GradientMap.SetPixel(x, y, col);
                }
            }
            howMany++;
        }
        _GradientMap.Apply();
        return _GradientMap;
    }
}