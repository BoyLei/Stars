////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
* 描述：
* 工程 ：StarProject
*/

using SGF;
using SGF.UI.Framework;
using StarProject;
using StarProject.Service.Lua;
using StarProjectDef;
using System;
using UnityEngine;
using XLua;

namespace SGF.Module.Framework
{
    /// <summary>
    /// lua Event Function
    /// </summary>
    /// <param name="self"></param>
    /// <param name="eventName"></param>
    [LuaCallCSharp]
    public delegate ModuleEvent OnEvent(string eventName);

    /// <summary>
    /// 创建Module
    /// </summary>
    /// <param name="arg"></param>
    [LuaCallCSharp]
    public delegate void OnModuleCreate(object self, object arg);

    /// <summary>
    /// 显示Module
    /// </summary>
    /// <param name="arg"></param>
    [LuaCallCSharp]
    public delegate void OnModuleShow(object self, object arg);

    /// <summary>
    /// 释放
    /// </summary>
    [LuaCallCSharp]
    public delegate void OnModuleRelease(object self);

    /// <summary>
    /// 消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="args"></param>
    [LuaCallCSharp]
    public delegate void OnModuleMessage(object self, string msg, object[] args);

    [LuaCallCSharp]
    public static class LuaCallModuleManager
    {
        public static ModuleEvent Event(ModuleDef.Name target, string type)
        {
            ModuleEvent me = ModuleManager.Instance.Event(target, type);
            //me.Invoke("111", 111);
            return me;
        }
    }
    [LuaCallCSharp]
    public class LuaModule : BusinessModule
    {

        /// <summary>
        /// //每一个模块含有自己的LuaTable
        /// 被动式被查找的lua表，必然具有Table映射，{执行环境的空表}
        /// </summary>
        public LuaTable luaTable;

        /// <summary>
        /// Lua原始文件表
        /// </summary>
        private LuaTable mSourceLuaFile;


        private object m_args;
        //封装ModuleMgr有必要的函数
        //public LuaCallModuleManager luaCallMM = new LuaCallModuleManager();
        /// <summary>
        /// 构造函数传入Name
        /// 是因为Lua模块无法通过反射来获取Name
        /// 这里记录Lua脚本名字
        /// </summary>
        /// <param name="name"></param>
        internal LuaModule(string name) : base(name) { }

        public OnModuleCreate OnModuleCreateEvent;

        public OnModuleShow OnModuleShowEvent;

        public OnModuleShow OnModuleHideEvent;

        public OnModuleRelease OnModuleReleaseEvent;

        public OnModuleMessage OnModuleMessageEvent;

        public OnEvent onEvent { get; private set; }
        public LuaTable GetLuaTable()
        {
            return mSourceLuaFile;
        }
        
        /// <summary>
        /// 这里应该去加载Lua脚本
        /// 并且将EventManager映射到Lua脚本中
        /// </summary>
        /// <param name="args"></param>
        public override void Create(object args = null)
        {
           
            base.Create(args);//Args要传递进去
            onEvent = base.Event;
            this.Log("Create() Lua = " + Name);


            EventTable mgrEvent = GetEventTable();//来源于预监听事件 或 调用创建，BModule唯一事件表          
            mgrEvent.GetEvent("OnShow");
            //Set名字和方法的绑定关系
            //赋能lua每一个Module的“基类”G,通过“MyLuaModuleEvtTable”可调用所有事件实现通讯
            // LuaManager.Global.Set(Name + "EvtTable", mgrEvent);
            //LuaManager.Global.Set("loginEvt", StarProject.GlobalEvent.onLogin);
            //LuaManager.Global.Set("L_luaCallMM", luaCallMM);

            //启动系统逻辑层Module，启动参数传递--------------------------------------------
            //G是luaTable，Set是lua表是lua的键值对的方式，模拟lua里面调用键得到值，与unity对应，所以key是约定熟成的一致
            LuaManager.Global.Set(Name + "CreateArgs", args);

            //Module之间通讯测试-----------------------------------------------------------------------
            //StarProject.Module.LoginModule lm = ModuleManager.Instance.GetModule(StarProjectDef.ModuleDef.Name.LoginModule) as StarProject.Module.LoginModule;
            //lm.testLoginModuleEvt.AddListener((object obj) =>
            //{
            //    this.LogWarning("Cs等待lua响应,的值是：" + obj);//CS堆栈指向的log，cs响应，值lua-obj-StringValue
            //});

            //执行文件片段
            //LuaManager.Instance.CreateLuaModuleScript(Name,this);
            LuaManager.Instance.CreateLuaModuleScriptAsync(Name, this);

            //luaTable.Set("LuaEventTable", mgrEvent);

            //LuaManager.Global.Get<EventTable>("")
            //TODO 需要映射到Lua脚本中
            //映射需求是：Cs和Lua目前唯一的消息纽带；1业务层次管理层可以发送事件，2lua内可以由响应v层事件，unity也可以响应，3网路数据层可以发送消息

            //全局事件测试
            //StarProject.GlobalEvent.onLogin.Invoke(true);

            //ServiceModule测试--不依赖ServiceModule标记--同时也不需要Set值到lua虚拟机
            //SGF.UI.Framework.UIManager.Instance.onServiceModuleEventTest.Invoke("lalala");


            // LuaTable lua = outPara as LuaTable;
            //  LuaTable tb=null;
            //luaTable.Get("LuaFile",out tb);

            OnModuleCreateEvent?.Invoke(mSourceLuaFile, args);
            //   luaTable.CallFunc("OnModuleCreate",this);

            //this.LogWarning("Lua创建模块时的实际返回值是：");

            //测试代码 ModuleMessage
            //ModuleManager.Instance.SendMessage(ModuleDef.Name.LuaModuleTest, "OnTestModuleMessage", new object[] { 1,2.0f,"abc"});

        }
        //public ModuleEvent OnShow { get { return Event("OnShow"); } } //Base定义的，存在Business中的表
        /// <summary>
        /// lua开启界面不强制要求 调用C#，然后回到lua
        /// Cs可以到CS，并且可以推送到lua，lua之间没问题，lua到CS直接通过BusinessWrap有，或者通过ModuleManager，都可以通讯
        /// </summary>
        /// <param name="arg"></param>
        protected override void Show(object arg)
        {
            //自己事件表中，Str==Show，事件；消息转事件，
            //每一个lua文本OnShow写自己的，这里不是统一交给G处理，LuaModule会new出一份
            //OnShow.Invoke(arg);
            GlobalEvent.onLuaUIShow.Invoke(arg);
            //UIWindow wnd = UIManager.Instance.OpenWindow(UIDef.UICreateRoleWindow, GameLoginInfo.M_PickSrvAck);
            base.Show(arg);

            //推送到OnModuleShow
            OnModuleShowEvent?.Invoke(mSourceLuaFile,arg);
            

            //测试OnShowEvent
            //GetEventTable().GetEvent("OnShow").Invoke(arg);
        }
        protected override void Hide(object arg)
        {
            //自己事件表中，Str==Show，事件；消息转事件，
            //每一个lua文本OnShow写自己的，这里不是统一交给G处理，LuaModule会new出一份
            //OnShow.Invoke(arg);
            GlobalEvent.onLuaUIHide.Invoke(arg);
            //UIWindow wnd = UIManager.Instance.OpenWindow(UIDef.UICreateRoleWindow, GameLoginInfo.M_PickSrvAck);
            base.Hide(arg);

            //推送到OnModuleHide
            OnModuleHideEvent?.Invoke(mSourceLuaFile, arg);

            //测试OnShowEvent
            //GetEventTable().GetEvent("OnHide").Invoke(arg);
        }
        protected override void OnModuleMessage(string msg, object[] args)
        {
            //用luaTable类型过去都行？？
            //用iPare forEach == 但是我要固定数据 这个不行
            /*原来C#【结构】是什么就是什么，数组结构在lua那边还是c#0开始的，在lua那边是封装成UserData类型，数组是UseData类型  所有类都是userData类型
             * Int String 会保持类型不会变化
             */
            //
            OnModuleMessageEvent?.Invoke(mSourceLuaFile, msg,args);
        }


        /// <summary>
        /// 调用它以卸载Lua脚本
        /// </summary>
        public override void Release()
        {
            OnModuleReleaseEvent?.Invoke(mSourceLuaFile);
            base.Release();
            this.Log("Release() Lua = " + Name);

        }

        public void BindLuaCall()
        {
            luaTable.Get("OnModuleCreate", out OnModuleCreateEvent);
            luaTable.Get("OnModuleShow", out OnModuleShowEvent);
            luaTable.Get("OnModuleHide", out OnModuleHideEvent);
            luaTable.Get("OnModuleRelease", out OnModuleReleaseEvent);
            luaTable.Get("OnModuleMessage", out OnModuleMessageEvent);            
            luaTable.Set("LuaEventTable", GetEventTable());

        }

        public void BindLuaCall(LuaTable tb)
        {
            mSourceLuaFile = tb;
            mSourceLuaFile.Get("OnModuleCreate", out OnModuleCreateEvent);
            mSourceLuaFile.Get("OnModuleShow", out OnModuleShowEvent);
            mSourceLuaFile.Get("OnModuleHide", out OnModuleHideEvent);
            mSourceLuaFile.Get("OnModuleRelease", out OnModuleReleaseEvent);
            mSourceLuaFile.Get("OnModuleMessage", out OnModuleMessageEvent);
            mSourceLuaFile.Set("self", mSourceLuaFile);
            mSourceLuaFile.Set("LuaEventTable", GetEventTable());
            mSourceLuaFile.Set("Event", onEvent);
        }
    }
}
