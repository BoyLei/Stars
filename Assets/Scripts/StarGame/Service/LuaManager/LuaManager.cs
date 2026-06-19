using SGF.Module.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using XLua;
using UnityEngine;
using Yoka.UnityString.Core;
namespace StarProject.Service.Lua
{
    [CSharpCallLua]
    public delegate void OnModuleCreate(object arg);

    public class LuaManager : ServiceModule<LuaManager>
    {
        /// <summary>
        /// 是否开启缓存模式，默认true，首次执行将把执行结果table存起来；在非缓存模式下，也可以通过编辑器的Reload来进行强制刷新缓存
        /// 对实时性重载要求高的，可以把开关设置成false，长期都进行Lua脚本重载，理论上会消耗额外的性能用于语法解析
        /// 
        /// 一般的脚本语言，如Python, NodeJS中，其import, require关键字都会对加载过的模块进行缓存(包括Lua原生的require)；如果不缓存，要注意状态的保存问题
        /// 该值调用频繁，就不放ini了
        /// </summary>
        public static bool CacheMode = false;

        private const string GlobalLuaPath = "LuaScripts/Global.lua";

        /// <summary>
        /// Import result object caching
        /// </summary>
        Dictionary<string, object> _importCache = new();

        /// <summary>
        /// 3全局LuaEnv虚拟机，生成且返回
        /// </summary>
        private readonly LuaEnv _luaEnv;

        ///// <summary>
        ///// 实例
        ///// </summary>
        //public static LuaModule Instance = new LuaModule();
        /// <summary>
        /// 1虚拟机全局获取器
        /// 2构造
        /// </summary>
        public static LuaTable Global
        {
            get { return Instance._luaEnv.Global; }
        }


        public bool IsInited { get; private set; }

        private double _initProgress = 0;

        /// <summary>
        /// 获取请去Get
        /// </summary>
        private Dictionary<string, LuaModule> LuaModuleCache;

        public double InitProgress
        {
            get { return _initProgress; }
        }

        public LuaTable NewTable()
        {
            return _luaEnv.NewTable();
        }

        public void Init()
        {
            CheckSingleton();
        }
        public object CallFunc(LuaTable luaTable, string funcName, params object[] args)
        {
            LuaFunction func = luaTable.Get<LuaFunction>(funcName);
            if (func == null)
            {
                return null;
            }
            return func.Call(args);
        }
        public object[] DoString(string str, string chunk = "chunk", LuaTable luaEnv = null)
        {
            return _luaEnv.DoString(str, chunk, luaEnv);
        }
        public object[] DoString(byte[] bytes, string chunk = "chunk", LuaTable luaEnv = null)
        {
            return _luaEnv.DoString(bytes, chunk, luaEnv);
        }
        /// <summary>
        /// 被动创建虚拟机
        /// </summary>
        public LuaManager()
        {
#if UNITY_EDITOR
            SGF.Debuger.Log("Consturct LuaModule...");
#endif

#if SLUA
            _luaSvr = new LuaSvr();
            _luaSvr.init(progress => { _initProgress = progress; }, () => { });
#else
            _luaEnv = new LuaEnv();
            _luaEnv.AddLoader(CustomLoaderHandler);

            LuaModuleCache = new Dictionary<string, LuaModule>();

            //var txtByte = Resource.ResourceFormalManager.Instance.LoadLuaAssetAsync(GlobalLuaPath);
            //_luaEnv.DoString(txtByte);
            //CallFunc(_luaEnv.Global, "Main");


            //StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTextAssetAsync(GlobalLuaPath, (UnityEngine.TextAsset textasset) =>
            //{
            //    if (textasset != null)
            //    {
            //        _luaEnv.DoString(textasset.bytes);
            //        CallFunc(_luaEnv.Global, "Main");
            //    }
            //});

            //var em = Init_Ienu();
            //while (em.MoveNext())
            //{
            //}
            //_luaEnv.AddLoader((ref string filename) =>
            //{
            //    var scriptPath = GetScriptPath(filename);
            //    if (scriptPath == null)
            //        return null;

            //    byte[] script = null;
            //    HotBytesLoader loader = null;
            //    try
            //    {
            //        loader = HotBytesLoader.Load(scriptPath, LoaderMode.Sync);
            //        Debuger.Assert(!loader.IsError, "Something wrong or Not exist Lua: " + scriptPath);
            //        script = loader.Bytes;
            //    }
            //    finally
            //    {
            //        if (loader != null)
            //            loader.Release();
            //    }

            //    return script;
            //});
            //LuaTimer.Init();

            PreloadWraps("*");
#endif
        }

        public async void OnPostInit()
        {
            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(GlobalLuaPath);
            if (textasset != null)
            {
                _luaEnv.DoString(textasset.bytes);
                await UniTask.DelayFrame(2, PlayerLoopTiming.LastTimeUpdate);
                CallFunc(_luaEnv.Global, "Main");
            }
            StarProject.Service.Resource.ResourceFormalManager.Instance.ReleaseAllLuaConfig();
            LuaTimer.Init();
        }

        public void PreloadWraps(string prefix, string suffix = null)   //modify by lijun08
        {
            if (_luaEnv != null && _luaEnv.translator != null && !string.IsNullOrEmpty(prefix))
            {
                _luaEnv.translator.PreloadWraps(prefix, suffix);
            }
        }

        private byte[] CustomLoaderHandler(ref string filepath)
        {
            //filepath = "LuaScripts/" + filepath.Replace('.', '/') + ".lua";
            //SGF.Debuger.Log($"lua require filepath={filepath}");
            //return Resource.ResourceFormalManager.Instance.LoadLuaAssetAsync(filepath);

            using(UString.Block())  //modify by lijun08
            {
                UString tmpPath = filepath;
                tmpPath = "LuaScripts/" + tmpPath.Replace('.', '/') + ".lua";
                filepath = tmpPath.Clone();
            }

            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync(filepath);
            if (textasset != null)
            {
                return textasset.bytes;
            }

            SGF.Debuger.LogWarning($"[LuaManager] CustomLoaderHandler() lua require filepath={filepath},err!!!");
            return null;
        }

        public LuaModule GetLuaModule(string name)
        {
            //luaModule维护就应该是去key，kv成对出现
            //k-KV
            if (!LuaModuleCache.ContainsKey(name))
            {
                LuaModuleCache.Add(name, new LuaModule(name));
            }
            else
            {
                //v-V
                if (LuaModuleCache[name] == null)
                {
                    LuaModuleCache[name] = new LuaModule(name);
                }
            }

            //初始化Table表

            //check inited，如不具体传参Luatable则默认包含于G表
            //如认为指定LuaTable进去则需要手动实现继承G
            //G具备全局查找文件能力，和Lua虚拟机的能力
            if (LuaModuleCache[name].luaTable == null)
            {
                LuaTable newLuaModuleTable = _luaEnv.NewTable();
                LuaModuleCache[name].luaTable = newLuaModuleTable;
                LuaTable meta = _luaEnv.NewTable();
                meta.Set("__index", _luaEnv.Global);

                newLuaModuleTable.SetMetaTable(meta);
                newLuaModuleTable.Set("self", LuaModuleCache[name]);
                // newLuaModuleTable.Set("OnModuleCreate", LuaModuleCache[name].OnModuleCreate);
                meta.Dispose();


                //不必实例，不必找Global，但需确保是同一个虚拟机的对象
            }

            return LuaModuleCache[name];
        }

        public LuaModule GetLuaModule(string name, LuaTable luaTable)
        {
            //luaModule维护就应该是去key，kv成对出现
            //k-KV
            if (!LuaModuleCache.ContainsKey(name))
            {
                LuaModuleCache.Add(name, new LuaModule(name));
            }
            else
            {
                //v-V
                if (LuaModuleCache[name] == null)
                {
                    LuaModuleCache[name] = new LuaModule(name);
                }
            }

            //check inited，如不具体传参Luatable则默认包含于G表
            //如认为指定LuaTable进去则需要手动实现继承G
            //G具备全局查找文件能力，和Lua虚拟机的能力
            if (LuaModuleCache[name].luaTable == null)
            {
                //LuaTable newLuaModuleTable = luaTable;

                LuaModuleCache[name].luaTable = luaTable;

                LuaTable meta = _luaEnv.NewTable();
                meta.Set("__index", _luaEnv.Global);

                LuaModuleCache[name].luaTable.SetMetaTable(meta);
                LuaModuleCache[name].luaTable.Set("self", LuaModuleCache[name]);
                // newLuaModuleTable.Set("OnModuleCreate", LuaModuleCache[name].OnModuleCreate);
                meta.Dispose();


                //不必实例，不必找Global，但需确保是同一个虚拟机的对象
            }
            else
            {
                LuaModuleCache[name].luaTable = luaTable;
            }
            return LuaModuleCache[name];
        }
        #region 4 加载引用Lua脚本

        /// <summary>
        /// Import script, with caching
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /*   public object Import(string fileName)
           {
               //			if (!HasScript (fileName))
               //                throw new FileNotFoundException(string.Format("Not found UI Lua Script: {0}", fileName));

               return DoImportScript(fileName);
           }*/

        /// <summary>
        /// Try import script, if 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        /*       public bool TryImport(string fileName, out object result)
               {
                   //            result = null;

                   //            if (!HasScript(fileName))
                   //                return false;

                   result = DoImportScript(fileName);
                   return true;
               }*/

        /*   object DoImportScript(string fileName)
           {
               object obj;
               if (!_importCache.TryGetValue(fileName, out obj))
               {
                   obj = this.CallScript(fileName);
                   if (CacheMode)
                       _importCache[fileName] = obj;
               }

               return obj;
           }*/

        #endregion

        /// <summary>
        /// 3调用脚本
        /// </summary>
        /// <param name="scriptRelativePath"></param>
        /// <returns></returns>
        //public object CallScript(string scriptRelativePath)
        //{
        //    //ProfilerTest.BeginSample("loadlua-" + scriptRelativePath);
        //    var luaPath = AppEngine.GetConfig("KSFramework.Lua", "LuaPath");
        //    var relativePath = string.Format("{0}/{1}.lua", luaPath, scriptRelativePath);
        //    byte[] script = GetScript(relativePath);
        //    var ret = ExecuteScript(script, relativePath);
        //    //ProfilerTest.EndSample();
        //    return ret;
        //}


        /// <summary>
        /// 3.1Execute lua script directly!
        /// 执行脚本
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <returns></returns>
        public object ExecuteScript(byte[] scriptCode, string file = "code")
        {
            object ret;
            ExecuteScript(scriptCode, out ret, file);
            return ret;
        }

        /// <summary>
        /// Execute lua script directly!
        /// 3.2执行脚本
        /// </summary>
        /// <param name="scriptCode"></param>
        /// <param name="ret">return result</param>
        /// <returns></returns>
        public bool ExecuteScript(byte[] scriptCode, out object ret, string file = "code")
        {
            //ProfilerTest.BeginSample("_luaEnv.DoString");
#if SLUA
            return _luaSvr.luaState.doBuffer(scriptCode, Encoding.UTF8.GetString(scriptCode), out ret);
#else
            var results = _luaEnv.DoString(scriptCode, file);

            if (results != null && results.Length == 1)
            {
                ret = results[0];
            }
            else
            {
                ret = results;
            }

            //ProfilerTest.EndSample();
            return true;
#endif
        }

        /// <summary>
        /// 3.3Lua驱动的机制有多种
        //1，DoString直接传递String进去
        //2,TextAsset.text传递进去
        //3,Require
        //4,byte[]
        //【执行顺序需遵守】
        ///1创建LModule
        ///2执行脚本
        ///虽然DoString通过GetLuaModule确保模块建立，但依然必须通过LuaModuleCreate进入这里
        /// </summary>
        /// <returns></returns>



        //        public bool CreateLuaScript(string scriptName, out object ret)
        //        {
        //            //ProfilerTest.BeginSample("_luaEnv.DoString");
        //#if SLUA
        //            return _luaSvr.luaState.doBuffer(scriptCode, Encoding.UTF8.GetString(scriptCode), out ret);
        //#else
        //            var bys = LuaBytes($"LuaScripts/{scriptName}.lua");

        //            var LuaModule = GetLuaModule(scriptName);
        //            var results = _luaEnv.DoString(bys,
        //                scriptName,
        //               LuaModule.luaTable
        //            );

        //            var tb = results.Get<LuaTable>(0);
        //            LuaModule.luaTable.Set("LuaFile", tb);
        //            LuaModule.BindLuaCall(tb);

        //            if (results != null && results.Length == 1)
        //            {
        //                ret = results[0];
        //            }
        //            else
        //            {
        //                ret = results;
        //            }

        //            /*ProfilerTest.EndSample();*/
        //            return true;
        //#endif
        //        }

        //private byte[] LuaBytes(string filepath)
        //{
        //    var textasset = Resource.ResourceFormalManager.Instance.LoadAssetSync<TextAsset>(filepath);
        //    if (textasset == null)
        //    {
        //        SGF.Debuger.LogError($"加载Lua文件失败{filepath}");
        //        return null;
        //    }
        //    return textasset.bytes;
        //}


        //public bool CreateLuaModuleScript(string scriptName, LuaModule luaModule)
        //{
        //    //"require '" + scriptName + "'"

        //    //"require LuaScripts.LuaModule.'" + scriptName + "'"
        //    //var results = _luaEnv.DoString("require '"+"LuaModule."+ scriptName + "'",
        //    //        scriptName,
        //    //        luaModule.luaTable
        //    //        );

        //    var textasset = Resource.ResourceFormalManager.Instance.LoadLuaAssetAsync($"LuaScripts/LuaModule/{scriptName}.lua");

        //    if (textasset == null)
        //    {
        //        return false;
        //    }

        //    var results = _luaEnv.DoString(textasset,
        //        scriptName,
        //       luaModule.luaTable
        //    );

        //    var tb = results.Get<LuaTable>(0);
        //    luaModule.BindLuaCall(tb);
        //    return true;
        //}

        public void CreateLuaModuleScriptAsync(string scriptName, LuaModule luaModule)
        {
            //Action<UnityEngine.TextAsset> cb = (UnityEngine.TextAsset textasset) =>
            //{
            //    if (textasset != null)
            //    {
            //        var results = _luaEnv.DoString(
            //            textasset.bytes,
            //            scriptName,
            //            luaModule.luaTable
            //        );
            //        var tb = results.Get<LuaTable>(0);
            //        luaModule.BindLuaCall(tb);
            //    }
            //};
            //Resource.ResourceFormalManager.Instance.LoadTextAssetAsync($"LuaScripts/LuaModule/{scriptName}.lua", cb);

            var textasset = StarProject.Service.Resource.ResourceFormalManager.Instance.LoadTxtAssetSync($"LuaScripts/LuaModule/{scriptName}.lua");
            if (textasset != null) 
            {
                var results = _luaEnv.DoString(
              textasset.bytes,
              scriptName,
              luaModule.luaTable
          );
                var tb = results.Get<LuaTable>(0);
                luaModule.BindLuaCall(tb);
            }
            else
            {
                UnityEngine.Debug.LogError("LuaModule不存在" + scriptName);
            }

        }


        /// <summary>
        /// 2获取脚本
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
        /*  public byte[] GetScript(string _path)
          {
              byte[] script = { };

              string _extPath = GetScriptRequirePath(_path);
              bool bExist = KEngine.KResourceModule.ContainsResourceUrl(_extPath);
              if (!bExist)
                  return script;


              HotBytesLoader loader = null;
              try
              {
                  loader = HotBytesLoader.Load(_extPath, LoaderMode.Sync);
                  script = loader.Bytes;
              }
              finally
              {
                  if (loader != null)
                      loader.Release();
              }

              return script;
          }

          /// <summary>
          /// 1这里对于Xlua的代码我们采用lua代码中的加载器自动匹配
          /// </summary>
          /// <param name="scriptRelativePath"></param>
          /// <returns></returns>
          static string GetScriptRequirePath(string scriptRelativePath)
          {
              var ext = AppEngine.GetConfig("KEngine", "AssetBundleExt");
              if (!AssetFileLoader.IsEditorLoadAsset)
              {
                  scriptRelativePath = KResourceModule.GetBuildPlatformName() + "/" + scriptRelativePath + ext;
                  //scriptRelativePath += ext;
              }

              return scriptRelativePath;
          }*/


        /*    public IEnumerator Init_Ienu()
            {
    #if SLUA
                int frameCount = 0;
                while (!_luaSvr.inited)
                {
                    if (frameCount % 30 == 0)
                        Log.LogWarning("SLua Initing...");
                    yield return null;
                    frameCount++;
                }
                var L = _luaSvr.luaState.L;
                LuaDLL.lua_pushcfunction(L, LuaImport);
                LuaDLL.lua_setglobal(L, "import");
                LuaDLL.lua_pushcfunction(L, LuaUsing);
                LuaDLL.lua_setglobal(L, "using"); // same as SLua's import, using namespace
                LuaDLL.lua_pushcfunction(L, ImportCSharpType);
                LuaDLL.lua_setglobal(L, "import_type"); // same as SLua's SLua.GetClass(), import C# type
    #else
                yield return null;
    #endif


    #if UNITY_EDITOR
                _luaEnv.Global.SetInPath<bool>("ISEDITOR", true);
    #endif
                CallScript("CsharpCallLuaFunction");
                LuaDelegate.Init(_luaEnv); //初始化csharp调用到lua中的函数,详见CsharpCallLuaFunction.lua-z>LuaDelegate.cs
                CallScript("Init");
                var luaUpdater = KSGame.Instance.gameObject.GetComponent<LuaUpdater>();
                if (luaUpdater == null)
                {
                    luaUpdater = KSGame.Instance.gameObject.AddComponent<LuaUpdater>();
                }

                luaUpdater.OnInit(_luaEnv);
                IsInited = true;
    #if UNITY_EDITOR
                //_luaEnv.Global.SetInPath<int>("DEBUG", VersionStyle.Instance.logLv);
                SGF.Debuger.Log("=============Test start=============");
                var editorLuaScriptPath = Path.Combine(KResourceModule.EditorProductFullPath, "LuaTests/TestInit.lua");
                var script = File.ReadAllBytes(editorLuaScriptPath);
                object ret;
                ExecuteScript(script, out ret, editorLuaScriptPath);
                SGF.Debuger.Log("=============Test end=============");
    #endif
                //LuaFunction newFuncObj = _luaEnv.Global.GetInPath<LuaFunction>("LogSwitch");
                //newFuncObj.Call();
            }*/


#if SLUA
		[LuaInterface.MonoPInvokeCallback(typeof(LuaCSFunction))]
		static public int ImportCSharpType(IntPtr l)
		{
			try
			{
				string cls;
				Helper.checkType(l, 1, out cls);
				Type t = LuaObject.FindType(cls);
				if (t == null)
				{
					return Helper.error(l, "Can't find {0} to create", cls);
				}

				LuaClassObject co = new LuaClassObject(t);
				LuaObject.pushObject(l,co);
				Helper.pushValue(l, true);
				return 2;
			}
			catch (Exception e)
			{
				return Helper.error(l, e);
			}
		}
        /// <summary>
        /// same as SLua default import
        /// </summary>
        /// <param name="luastate"></param>
        /// <returns></returns>
        [LuaInterface.MonoPInvokeCallback(typeof(LuaCSFunction))]
        private int LuaUsing(IntPtr l)
        {
            try
            {
                LuaDLL.luaL_checktype(l, 1, LuaTypes.LUA_TSTRING);
                string str = LuaDLL.lua_tostring(l, 1);

                string[] ns = str.Split('.');

                LuaDLL.lua_pushglobaltable(l);

                for (int n = 0; n < ns.Length; n++)
                {
                    LuaDLL.lua_getfield(l, -1, ns[n]);
                    if (!LuaDLL.lua_istable(l, -1))
                    {
                        return LuaObject.error(l, "expect {0} is type table", ns);
                    }
                    LuaDLL.lua_remove(l, -2);
                }

                LuaDLL.lua_pushnil(l);
                while (LuaDLL.lua_next(l, -2) != 0)
                {
                    string key = LuaDLL.lua_tostring(l, -2);
                    LuaDLL.lua_getglobal(l, key);
                    if (!LuaDLL.lua_isnil(l, -1))
                    {
                        LuaDLL.lua_pop(l, 1);
                        return LuaObject.error(l, "{0} had existed, import can't overload it.", key);
                    }
                    LuaDLL.lua_pop(l, 1);
                    LuaDLL.lua_setglobal(l, key);
                }

                LuaDLL.lua_pop(l, 1);

                LuaObject.pushValue(l, true);
                return 1;
            }
            catch (Exception e)
            {
                return LuaObject.error(l, e);
            }
        }
        
        /// <summary>
        /// This will override SLua default `import`
        /// 
        /// TODO: cache the result!
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        [LuaInterface.MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LuaImport(IntPtr L)
        {
            LuaModule luaModule = Instance;

            string fileName = LuaDLL.lua_tostring(L, 1);
            var obj = luaModule.Import(fileName);


            LuaObject.pushValue(L, obj);
            LuaObject.pushValue(L, true);
            return 2;

        }
#endif
        float lastGCTime = 0;
        float GCInterval = 1;
#if !SLUA
        [BlackList]
        public void Tick(float deltaTime)
        {
            if (UnityEngine.Time.time - lastGCTime > GCInterval)
            {
                _luaEnv.Tick();
                lastGCTime = UnityEngine.Time.time;
            }

            LuaTimer.Tick(deltaTime);
        }
#endif
    }
}