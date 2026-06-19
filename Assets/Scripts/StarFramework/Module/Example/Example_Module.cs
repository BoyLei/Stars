using SGF.Module.Framework;//框架空间，模块，框架
using StarProject;//项目工程
using UnityEngine;

namespace SGF.Module.Example
{
    public class Example_Module : MonoBehaviour
    {
        void Awake()
        {
            Debuger.EnableLog = true;

            ModuleC.Instance.Init();
            ModuleManager.Instance.Init("SGF.Module.Example");

            //ModuleManager.Instance.CreateModule(StarProjectDef.ModuleDef.Name.ModuleA/*"ModuleA"*/);
            //ModuleB B = ModuleManager.Instance.CreateModule(StarProjectDef.ModuleDef.Name.ModuleB/*"ModuleB"*/) as ModuleB;

            //ModuleA a = ModuleManager.Instance.GetModule(StarProjectDef.ModuleDef.Name.ModuleA/*"ModuleA"*/) as ModuleA;
            
            //a.Call();

            //B.ModuleB_Event();
        }


    }


    /// <summary>
    /// 测试例子
    /// </summary>
    public class ModuleA : BusinessModule
    {
        public override void Create(object args = null)
        {
            base.Create(args);
            //缓存消息组
            //业务层模块之间，通过Message进行通讯：模块名，方法名，参数组
            //ModuleManager.Instance.SendMessage("ModuleB", "MessageFromA_1", 1, 2, 3);
            //ModuleManager.Instance.SendMessage("ModuleB", "MessageFromA_2", "abc", 123);

            ////业务层模块之间，通过Event进行通讯 ：模块名，ModuleEvent字段名，相应函数（调用CallBack）
            //ModuleManager.Instance.Event("ModuleB", "onModuleEventB").AddListener(OnModuleEventB);

            ////业务层调用服务层，通过事件监听回调
            //ModuleC.Instance.onEvent.AddListener(OnModuleEventC);
            //ModuleC.Instance.DoSomething();

            ////全局事件
            //GlobalEvent.onLogin.AddListener(OnLogin);

        }
        public void Call()
        {
            //业务层模块之间，通过Message进行通讯：模块名，方法名，参数组
            //  ModuleManager.Instance.SendMessage("ModuleB", "MessageFromA_1", 1, 2, 3); //> 16:18:18.847 ModuleB::OnModuleMessage() msg:MessageFromA_1, args:1,2,3
            // ModuleManager.Instance.SendMessage("ModuleB", "MessageFromA_2", "abc", 123);//> 16:18:18.848 ModuleB::MessageFromA_2() args:abc,123
            //业务层模块之间，通过Event进行通讯 ：模块名，[ModuleEvent事件类，字段名]，响应函数（调用CallBack）？？？？？？？？？？？？？？> 16:18:18.850 ModuleA::OnModuleEventB() args:aaaa
            //【说明1】流程是在Bmodule中，的事件表中，创建一个key为onModuleEventB,的ModuleEvent
            //ModuleManager.Instance.Event(StarProjectDef.ModuleDef.Name.ModuleB/*"ModuleB"*/, "onModuleEventB").AddListener(OnModuleEventB);

            //业务层A调用服务层C，通过事件监听回调||Log ModuleA::得到ModuleC的通知
            //ModuleC.Instance.onEvent.AddListener(OnModuleEventC);//服层的的事件响应，我来注册进去//> 16:18:18.849 ModuleA::OnModuleEventC() args:
            //ModuleC.Instance.DoSomething();//服务层实际处理逻辑时候，广播给所有注册的业务模块

            //全局事件:LOGModuleA得到Login的通知
            GlobalEvent.onLogin.AddListener(OnLogin);//> 16:18:18.850 ModuleA::OnLogin() args:True
           // GlobalEvent.onLogin.Invoke(true);
        }

        private void OnModuleEventC(object args)
        {
            //Log 和 Xlua 冲突了；改用LogWarmming
            this.LogWarning("OnModuleEventC() args:{0}", args);
        }

        private void OnModuleEventB(object args)
        {
            this.LogWarning("OnModuleEventB() args:{0}", args);
        }

        private void OnLogin(bool args)
        {
            this.LogWarning("OnLogin() args:{0}", args);
        }
    }

    public class ModuleB : BusinessModule
    {
        /// <summary>
        /// 定义一个属性，返回固定类型的事件
        /// 
        /// 【说明1】这里ModuleB中有一个onModuleEventB事件被别人注册
        /// 通过声明属性，直接取得对应事件。进行处理
        /// 也可以通过Get方式取得别人注册的这个Key =“onModuleEventB”的事件
        /// </summary>
        public ModuleEvent onModuleEventB { get { return Event("onModuleEventB"); } } //Base定义的，存在Business中的表

        public override void Create(object args = null)
        {
            base.Create(args);

        }


        public void ModuleB_Event()
        {

            onModuleEventB.Invoke("aaaa");
        }

        protected void MessageFromA_2(string args1, int args2)
        {
            this.LogWarning("MessageFromA_2() args:{0},{1}", args1, args2);
        }

        protected override void OnModuleMessage(string msg, object[] args)
        {
            base.OnModuleMessage(msg, args);
            this.LogWarning("OnModuleMessage() msg:{0}, args:{1},{2},{3}", msg, args[0], args[1], args[2]);
        }
    }

    public class ModuleC : ServiceModule<ModuleC>
    {
        public ModuleEvent onEvent = new ModuleEvent();
        public void Init()
        {

        }

        public void DoSomething()
        {
            onEvent.Invoke(null);
        }
    }


}
