using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace AssetChecker.Define
{
    public class FileVerInfo
    {
        public string _author;
        public string _orderid;

        public override string ToString()
        {
            return string.Format("author:{0} orderNum:{1}", _author, _orderid);
        }
    }

    public class ExcelTitle
    {
        public string _title;
        public int _width;

        public ExcelTitle(string title, int width)
        {
            this._title = "[ " + title + " ]";
            this._width = width;
        }
    }

    public class TableTitle
    {
        public string _title;
        public string _align;

        public TableTitle(string title)
        {
            this._title = title;
        }

        public TableTitle(string title, string align)
        {
            this._title = title;
            this._align = align;
        }
    }

    public enum MaliGPUType
    {
        MaliG710,
        MaliG610,
        MaliG510,
        MaliG310,
        MaliG78AE,
        MaliG78,
        MaliG77,
        MaliG68,
        MaliG57,
        MaliG76,
        MaliG72,
        MaliG71,
        MaliG52,
        MaliG51,
        MaliG31,
        MaliT880,
        MaliT860,
        MaliT830,
        MaliT820,
        MaliT760,
        MaliT720,
    }

    [Serializable]
    public class MaliGpuInfo
    {
        public MaliGPUType _type;
        public float _complexity;
    }

    public enum ShaderTrunkType
    {
        None,
        Properties,
        Vertex,
        Fragment,
        Main,
    }

    public class ShaderFuncInfo
    {
        public string _assetPath;
        public int _passCount;
        public List<string> _propVars;
        public List<string> _lineCodes;
        public List<string> _mateList;
    }

    public class ShaderTrunkInfo
    {
        public string _trunkFile;
        public int _index;
    }

    public class ShaderKeyworkInfo
    {
        public string _assetPath;
        public string _keyword;
        public List<string> _mateNodes;
    }

    public class ShaderComplexityInfo
    {
        public string _globalKeywords;
        public string _localKeywords;
        public string _keywords;
        public string _trunkFile;
        public string _workRegInfo;         //该shader工作使用的寄存器数量，减少提升性能
        public string _uniformRegCount;     //存储着色器可能需要的常量，所有线程都有共享uniform register
        public string _stackSpilling;       //是否有变量被放置到栈内存中，有的话GPU读取是性能消耗较大
        public string _16Arithmetic;        //以 16 位或更低精度执行的算术运算的百分比。数值越高代表shader性能越好
        public string _A;                   //Arithmetic 在Mali Valhall 架构的GPU实现了两个并行处理单元，算数单元A被拆分为 FMA , CVT , SFU
        public string _LS;                  //Load/Store operation 读取和存储的操作消耗,处理所有非纹理内存访问
        public string _T;                   //Texture operations 所有纹理采样和过滤操作消耗
        public string _V;                   //Varying operations 在shader中不同单位插值的消耗
        public string _bound;               //循环计数最高的功能单元，识别瓶颈单元。
        public string _complexity;          //Shader生成的所有指令的累积执行周期数

        public override string ToString()
        {
            var format = @"
_trunkFile:>{0} 
_globalKeywords:>{1} 
_localKeywords:>{2} 
_keywords:>{3} 
_workRegInfo:>{4} 
_uniformRegCount:>{5} 
_stackSpilling:>{6} 
_16Arithmetic:>{7} 
_A:>{8} 
_LS:>{9} 
_T:>{10} 
_V:>{11} 
_bound:>{12} 
_complexity:>{13} 
";
            return string.Format(format, 
                Path.GetFileName(_trunkFile), 
                _globalKeywords, 
                _localKeywords, 
                _keywords,
                _workRegInfo,
                _uniformRegCount,
                _stackSpilling,
                _16Arithmetic,
                _A,
                _LS,
                _T,
                _V,
                _bound,
                _complexity
                );
        }
    }

    public class ShaderCompiledInfo
    {
        public string _assetPath;
        public int _variantNum;
        public List<string> _codeLines;
        public List<string> _minLines;
        public List<string> _maxLines;
    }

    public class AssetCheckDataDefine
    {
        public const string ShaderCheckCfgName = "ShaderCheckerDefine";
        public const string ShaderCheckCfgFile = ShaderCheckCfgName + ".asset";
    }

    [System.Serializable]
    public class DepABInfo
    {
        public string _name;
    }

    public class ABTreeNode
    {
        public string _name;
        public ABTreeNode _parent;
    }

    public enum FindAbType
    {
        Print,
        Pull,
    }

    public enum DataSourceType //数据源
    {
        ManifestFile,   //manifest文件
        MemDependent,     //字典列表
    }
}
