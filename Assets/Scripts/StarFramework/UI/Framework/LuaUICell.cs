///--------------------------------------------------------------------
/// 文件名   :   LuaCell.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/10/24 15:48:30
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
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
    public class LuaUICell : UICell
    {
        [LabelText("Lua路径")]
        [FilePath(AbsolutePath = false, Extensions = ".txt", ParentFolder = "Assets/Res")]
        public string LuaFilePath;

        private string LuaName;
        private LuaTable scriptEnv;

        public LuaTable LuaTable { get; private set; }

        /// <summary>
        /// 更新Lua
        /// </summary>
        public LuaUICallCSharp2 OnUpdateContentEvent;

        /// <summary>
        /// 设置Item尺寸
        /// </summary>
        public LuaUICallCSharp3 SetItemSizeEvent;

        /// <summary>
        /// Awake方法
        /// </summary>
        public LuaUICallCSharp1 AwakeEvent;

        private bool HasInit = false;

        /*
                public AnimationClip clip;
                [ContextMenu("播动画")]
                public void PlayAniamtion()
                {
                    Animator animation = transform.GetComponent<Animator>();
                    animation.Play("Close3");
                }

                [ContextMenu("播动画1")]
                public void PlayAniamtion1()
                {
                    Animation animation = transform.GetComponent<Animation>();
                    animation.Play("Close3");
                }*/

        /// <summary>
        /// 当UI打开时，会响应这个函数
        /// </summary>
        /// <param name="arg"></param>
        public override void UpdateContent(object arg)
        {
            OnInitialized();
            OnUpdateContentEvent?.Invoke(LuaTable, arg);
        }


        public override Vector2 SetItemSize()
        {
            OnInitialized();
            if (SetItemSizeEvent != null)
            {
                return (Vector2)SetItemSizeEvent.Invoke(LuaTable);
            }
            return Vector2.one;
        }

        private void OnInitialized()
        {
            if (!HasInit)
            {
                InitLuaEnv();
                AwakeEvent?.Invoke(LuaTable);
                HasInit = true;
            }
        }

        protected override void Awake()
        {
            OnInitialized();
        }


        //----------------------工具类-----------------------
        public void Find(string path, out Transform result)
        {
            result = null;
            if (transform != null)
            {
                result = transform.Find(path);
            }
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

            /// scriptEnv.Set("self", this);
            /// scriptEnv.Set("prefab", this.gameObject);
            LuaName = System.IO.Path.GetFileNameWithoutExtension(LuaFilePath);
            LuaFilePath = LuaFilePath.Replace(".txt", "");
            LuaFilePath = LuaFilePath.Replace("Resources/", "");
            LuaFilePath = LuaFilePath.Replace("Res/", "");

            //var luaText = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadLuaAssetAsync(LuaFilePath);
            //var luaText = luaText.bytes;
            //var results = LuaManager.Instance.DoString(luaText, LuaName, scriptEnv);
            //var tb = results.Get<LuaTable>(0);
            //LuaTable = tb;
            //LuaTable.Set("prefab", this.gameObject);
            //LuaTable.Set("self", tb);
            //AwakeEvent = LuaTable.Get<LuaUICallCSharp1>("Awake");
            //SetItemSizeEvent = LuaTable.Get<LuaUICallCSharp3>("SetItemSize");
            //OnUpdateContentEvent = LuaTable.Get<LuaUICallCSharp2>("OnUpdateContent");

            //Action<UnityEngine.TextAsset> cb = (UnityEngine.TextAsset textasset) =>
            //{
            //    if (textasset != null)
            //    {
            //        var results = LuaManager.Instance.DoString(textasset.bytes, LuaName, scriptEnv);
            //        var tb = results.Get<LuaTable>(0);
            //        LuaTable = tb;
            //        LuaTable.Set("prefab", this.gameObject);
            //        LuaTable.Set("self", tb);
            //        AwakeEvent = LuaTable.Get<LuaUICallCSharp1>("Awake");
            //        SetItemSizeEvent = LuaTable.Get<LuaUICallCSharp3>("SetItemSize");
            //        OnUpdateContentEvent = LuaTable.Get<LuaUICallCSharp2>("OnUpdateContent");
            //    }
            //};
            //StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTextAssetAsync(LuaFilePath, cb);

            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(LuaFilePath);
            var results = LuaManager.Instance.DoString(textasset.bytes, LuaName, scriptEnv);
            var tb = results.Get<LuaTable>(0);
            LuaTable = tb;
            LuaTable.Set("prefab", this.gameObject);
            LuaTable.Set("self", tb);
            AwakeEvent = LuaTable.Get<LuaUICallCSharp1>("Awake");
            SetItemSizeEvent = LuaTable.Get<LuaUICallCSharp3>("SetItemSize");
            OnUpdateContentEvent = LuaTable.Get<LuaUICallCSharp2>("OnUpdateContent");
        }

    }

}