///--------------------------------------------------------------------
/// 文件名   :   LuaFancyCell.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/24 17:50:22
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using FancyScrollView;
using Sirenix.OdinInspector;
using StarProject.Service.Lua;
using StarProject.Service.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    public class LuaFancyCell : FancyCell<object>
    {
        [LabelText("Lua路径")]
        [FilePath(AbsolutePath = false, Extensions = ".txt", ParentFolder = "Assets/Res")]
        public string LuaFilePath;

        private string LuaName;
        private LuaTable scriptEnv;


        public LuaUICallCSharp1 InitializeEvent;

        public LuaUICallCSharp2 UpdateContentEvent;

        public LuaUICallCSharp2 UpdatePositionEvent;

        public override void Initialize()
        {
            InitLuaEnv();
            InitializeEvent?.Invoke(scriptEnv);
        }

        public override void UpdateContent(object itemData)
        {
            UpdateContentEvent?.Invoke(scriptEnv, itemData);
        }

        public override void UpdatePosition(float position)
        {
            UpdatePositionEvent?.Invoke(scriptEnv, position);
        }

        public void Find(string path, Transform parent, out Transform result)
        {
            result = null;
            if (parent != null)
            {
                result = parent.Find(path);
            }
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
            Debug.LogError("LuaName" + LuaName);
            // var text = Resources.Load<TextAsset>(LuaFilePath);

            //var textasset = ResourceFormalManager.Instance.LoadLuaAssetAsync(LuaFilePath);
            //var luaText = textasset.bytes;
            //var results = LuaManager.Instance.DoString(luaText, LuaName, scriptEnv);
            //var tb = results.Get<LuaTable>(0);
            //tb.Set("prefab", this.gameObject);

            //Action<TextAsset> cb = (TextAsset textasset) =>
            //{
            //    if (textasset != null)
            //    {
            //        var results = LuaManager.Instance.DoString(textasset.bytes, LuaName, scriptEnv);
            //        var tb = results.Get<LuaTable>(0);
            //        tb.Set("prefab", this.gameObject);
            //    }
            //};
            //StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTextAssetAsync(LuaFilePath, cb);

            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(LuaFilePath);
            var luaText = textasset.bytes;
            var results = LuaManager.Instance.DoString(luaText, LuaName, scriptEnv);
            var tb = results.Get<LuaTable>(0);
            tb.Set("prefab", this.gameObject);

        }
    }
}
