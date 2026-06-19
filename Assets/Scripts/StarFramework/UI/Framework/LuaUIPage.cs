///--------------------------------------------------------------------
/// 文件名   :   LuaUIPage
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/08/17 18:48:06
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using Sirenix.OdinInspector;
using StarProject.Service.Lua;
using System;
using XLua;

namespace SGF.UI.Framework
{
    public class LuaUIPage : UIPage
    {
        [LabelText("Lua路径")]
        [FilePath(AbsolutePath = false, Extensions = ".txt", ParentFolder = "Assets")]
        public string LuaFilePath;
        private string LuaName;
        private LuaTable scriptEnv;
        private LuaPanel _luaPanel;


        public LuaPanel GetLuaPanel()
        {

            return _luaPanel;
        }


        /// <summary>
        /// 当UI打开时，会响应这个函数
        /// </summary>
        /// <param name="arg"></param>
        protected override void OnOpen(object arg = null)
        {
            _luaPanel.Execute("OnOpen", arg);
        }

        /// <summary>
        /// 当UI关闭时，会响应这个函数
        /// 该函数在重写时，需要支持可重复调用
        /// </summary>
        protected override void OnClose(object arg = null)
        {
            _luaPanel.Execute("OnClose", arg);
        }


        protected override void Awake()
        {
            InitLuaEnv();
            _luaPanel.Execute("Awake");
        }

        protected override void Start()
        {
            _luaPanel.Execute("Start");
        }
        protected override void OnEnable()
        {
            _luaPanel.Execute("OnEnable");
        }
        protected override void OnDisable()
        {
            _luaPanel.Execute("OnDisable");
        }
        protected override void OnDestroy()
        {
            _luaPanel.Execute("OnDestroy");
            _luaPanel.OnRelease();
            _luaPanel = null;
            scriptEnv?.Dispose();
            scriptEnv = null;
        }

        private void InitLuaEnv()
        {
            scriptEnv = LuaManager.Instance.NewTable();
            LuaTable meta = LuaManager.Instance.NewTable();
            meta.Set("__index", LuaManager.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            scriptEnv.Set("self", this);
            scriptEnv.Set("prefab", this.gameObject);
            LuaName = System.IO.Path.GetFileNameWithoutExtension(LuaFilePath);

            LuaFilePath = LuaFilePath.Replace(".txt", "");
            LuaFilePath = LuaFilePath.Replace("Resources/", "");
            LuaFilePath = LuaFilePath.Replace("Res/", "");

            //var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadLuaAssetAsync(LuaFilePath);
            //var luaText = textasset.bytes;
            //var results = LuaManager.Instance.DoString(luaText, LuaName, scriptEnv);
            //_luaPanel = new LuaPanel();
            //var tb = results.Get<LuaTable>(0);
            //tb.Set("prefab", this.gameObject);
            //tb.Set("AtlasPath",AtlasPath);
            //tb.Set("onCloseDestroy", SetCloseDestroyAction);
            //_luaPanel.BindCall(tb);

            //Action<UnityEngine.TextAsset> cb = (UnityEngine.TextAsset textasset) =>
            //{
            //    if (textasset != null)
            //    {
            //        var results = LuaManager.Instance.DoString(textasset.bytes, LuaName, scriptEnv);
            //        _luaPanel = new LuaPanel();
            //        var tb = results.Get<LuaTable>(0);
            //        tb.Set("prefab", this.gameObject);
            //        tb.Set("AtlasPath", AtlasPath);
            //        tb.Set("onCloseDestroy", SetCloseDestroyAction);
            //        _luaPanel.BindCall(tb);
            //    }
            //};
            //StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTextAssetAsync(LuaFilePath, cb);

            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(LuaFilePath);
            var results = LuaManager.Instance.DoString(textasset.bytes, LuaName, scriptEnv);
            _luaPanel = new LuaPanel();
            var tb = results.Get<LuaTable>(0);
            tb.Set("prefab", this.gameObject);
            tb.Set("AtlasPath", AtlasPath);
            tb.Set("onCloseDestroy", SetCloseDestroyAction);
            _luaPanel.BindCall(tb);
        }
    }
}
