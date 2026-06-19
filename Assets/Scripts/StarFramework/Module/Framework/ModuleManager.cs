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
* 工程 ：
* StarProject温馨提示：
通常来说之前我有讲过这里在重新提醒下
通常需要流程是：
  //：
1 开启模块(在需要的时候，现在我在AppMain里面分了三个组【工程启动时/主界面启动时/需要启动的时候】），
2,加载按钮（按钮，特殊时可能是一个动态界面，如小地图）
3,小界面awake（需要unity驱动时有这步）
4,UI缓存注册自己的Module，然后UI通过自己的Module和别人进行通讯
————————————————-  
标准流程是 "|_|"表现向下找自己逻辑，逻辑之间通讯，逻辑在找表现层次

逻辑层之间找Service来帮忙，表现需要取Service这是可以的
————————————————-
*/

using StarProject.Service.Lua;
using StarProjectDef;
using System;
using System.Collections.Generic;

namespace SGF.Module.Framework
{
    [XLua.LuaCallCSharp]
    public class ModuleManager : ServiceModule<ModuleManager>
    {

        class MessageObject
        {
            public string target;
            public string msg;
            public object[] args;
        }


        class StackMessageObject
        {
            public string target;//老的Module名
            public string msg;
            public object args;
        }

        /// <summary>
        /// 已创建的模块列表
        /// </summary>
        private Dictionary<string, BusinessModule> m_mapModules;

        /// <summary>
        /// 当目标模块未创建时，缓存的消息对象
        /// </summary>
        private Dictionary<string, List<MessageObject>> m_mapCacheMessage;

        /// <summary>
        /// 当目标模块未创建时，预监听的事件
        /// </summary>
        private Dictionary<string, EventTable> m_mapPreListenEvents;

        /// <summary>
        /// 用于模块反射的域
        /// </summary>
        private string m_domain;


        public ModuleManager()
        {
            m_mapModules = new Dictionary<string, BusinessModule>();
            m_mapCacheMessage = new Dictionary<string, List<MessageObject>>();
            m_mapPreListenEvents = new Dictionary<string, EventTable>();
            bModuleStack = new Stack<KeyValuePair<string, StackMessageObject>>();
        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="domain">业务模块所在的域</param>
        public void Init(string domain = "")
        {
            CheckSingleton();
            m_domain = domain;
        }

        /// <summary>
        /// 通过类型创建一个业务模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        private T CreateModule<T>(object args = null) where T : BusinessModule
        {
            return (T)CreateModule(
              ModuleDef.GetModuleName(typeof(T).Name)

                , args);
        }

        /// <summary>
        /// 通过名字创建一个业务模块
        /// 先通过名字反射出Class，如果不存在
        /// 则通过扫描Lua文件目录加载LuaModule
        /// [思路变更]：原来是不定义CS里面反射搜索，Lua原表-环境-Table模拟类搜索，但是明确不确定的为什么要搜索所以必须定义
        /// </summary>
        /// <param name="name">业务模块的名字</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public BusinessModule CreateModule(ModuleDef.Name moduleName, object args = null)
        {
            //未定义外面就报错了，明确的类型不能有特殊关键key，泛型入口会有转换限制
            if (moduleName == ModuleDef.Name.None || moduleName == ModuleDef.Name.LuaModuleType)
            {
                return null;
            }
            this.Log("CreateModule() name = " + moduleName + ", args = " + args);
            string name = ModuleDef.GetModuleName(moduleName);
            if (m_mapModules.ContainsKey(name))
            {
                this.LogError("CreateModule() The Module<{0}> Has Existed!");
                return null;
            }

            BusinessModule module = null;
            Type type = Type.GetType($"{m_domain}.{name}");
            if (type != null)
            {
                //CS通过反射，得到内存中New()实力空间，类的引用。
                module = Activator.CreateInstance(type) as BusinessModule;
            }
            else
            {
                module = LuaManager.Instance.GetLuaModule(name);
            }
            m_mapModules.Add(name, module);

            //处理预监听的事件
            //##在全部事件缓存列表中key为模块名称;
            //<string, EventTable,ModuleEvent（Action）>
            //##转存预监听到我本身事件
            if (m_mapPreListenEvents.ContainsKey(name))
            {
                EventTable mgrEvent = null;
                mgrEvent = m_mapPreListenEvents[name];
                m_mapPreListenEvents.Remove(name);

                module.SetEventTable(mgrEvent);
            }

            module.Create(args);

            //处理缓存的消息
            if (m_mapCacheMessage.ContainsKey(name))
            {
                List<MessageObject> list = m_mapCacheMessage[name];
                for (int i = 0; i < list.Count; i++)
                {
                    MessageObject msgobj = list[i];
                    module.HandleMessage(msgobj.msg, msgobj.args);
                }
                m_mapCacheMessage.Remove(name);
            }

            return module;
        }


        /// <summary>
        /// 释放一个由ModuleManager创建的模块
        /// 遵守谁创建谁释放的原则
        /// </summary>
        /// <param name="module"></param>
        public void ReleaseModule(BusinessModule module)
        {
            if (module != null)
            {
                if (m_mapModules.ContainsKey(module.Name))
                {
                    this.Log("ReleaseModule() name = " + module.Name);
                    m_mapModules.Remove(module.Name);
                    module.Release();
                    module = null;
                }
                else
                {
                    this.LogError("ReleaseModule() 模块不是由ModuleManager创建的！ name = " + module.Name);
                }
            }
            else
            {
                this.LogWarning("ReleaseModule() module = null!");
            }

        }

        /// <summary>
        /// 释放所有模块
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var @event in m_mapPreListenEvents)
            {
                @event.Value.Clear();
            }
            m_mapPreListenEvents.Clear();

            m_mapCacheMessage.Clear();

            foreach (var module in m_mapModules)
            {
                module.Value.Release();
            }
            m_mapModules.Clear();
        }


        /// <summary>
        /// Tips：含有LuaModule
        /// 通过名字获取一个Module
        /// 如果未创建过该Module，则返回null
        /// 可以Logic1对多么？ 除了Page可以，提前获取的数据存储，Module比较全生命周期的。比较大的Module，或者中转获取数据，window概念比较游离
        /// 如果明确双向事件需要写出来。如果单项写不写都行。提高抽象 ，自由  ，解耦合，可多样，
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public BusinessModule GetModule(ModuleDef.Name moduleName)
        {
            //每一个lua module部分 是 CSluamodule的一个对应关系的绑定也对应CS一个实例，并且最终走的是CS的Create，并且有自己的标准名
            if (moduleName == ModuleDef.Name.None || moduleName == ModuleDef.Name.LuaModuleType)
            {
                return null;
            }
            var name = ModuleDef.GetModuleName(moduleName);
            if (m_mapModules.ContainsKey(name))
            {
                return m_mapModules[name];
            }

            return null;
        }

        /// <summary>
        /// Show/Release是约束规范，一定有的走缓存，没有的走反射
        /// </summary>
        /// <param name="name"></param>
        /// <param name="arg"></param>
        public void ReleaseModule(ModuleDef.Name moduleName, object arg = null)
        {
            BusinessModule bm = GetModule(moduleName);
            ReleaseModule(bm);
            //SendMessage(name, "Release", arg);
        }
        /// <summary>
        /// 显示业务模块的默认UI
        /// </summary>
        /// <param name="name"></param>
        public void ShowModule(ModuleDef.Name name, object arg = null)
        {
            SendMessage(name, "Show", arg);
        }
        public void HideModule(ModuleDef.Name name, object arg = null)
        {
            SendMessage(name, "Hide", arg);
        }


        /// <summary>
        /// 向指定的模块发送消息
        /// 
        /// 哦 业务模块之间的消息无需关心
        /*   1另一个模块是否建立/启动：消息会有消息缓存，如模块并未即时启动，会缓存消息，保证消息发送

   ·····2业务之间底层解耦：底层是project包，业务是Game文件夹下

             3开发和运行时解耦：并不因为没发生关联和配置如模块define而找不到，甚至module可能是luaModule这个是找不到的*/
        /// </summary>
        /// <param name="target">这里定义什么无所谓，lua不能不同，各个方法不能不同都统一枚举；不然enum转换stringlua性能损耗；lua本质给值这边要值Cs性能吮毫；说到底lua是利用值传递也别弄什么反向表，xlua处理的就是lua到CS的值和枚举的强制转换
        /// 多说点，枚举是源，值是具体在cs和lua都能通用，string只有cs可以随便转换，所以最通用的就是源或值，对于lua没有源就用值就是xlua的唯一方式
        /// </param>？？？
        /// <param name="msg"></param>
        /// <param name="args"></param>
        public void SendMessage(ModuleDef.Name target, string function, params object[] args)
        {
            if (target == ModuleDef.Name.None || target == ModuleDef.Name.LuaModuleType)
            {
                return;
            }
            SendMessage_Internal(target, function, args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        private void SendMessage_Internal(ModuleDef.Name target, string msg, object[] args)
        {
            BusinessModule module = GetModule(target);
            if (module != null)
            {
                module.HandleMessage(msg, args);
            }
            else
            {
                //将消息缓存起来
                List<MessageObject> list = GetCacheMessageList(target.ToString());
                MessageObject obj = new MessageObject();
                obj.target = target.ToString();
                obj.msg = msg;
                obj.args = args;
                list.Add(obj);

                this.LogWarning("SendMessage() target不存在！将消息缓存起来! target:{0}, msg:{1}, args:{2}", target, msg, args);
            }
        }

        private List<MessageObject> GetCacheMessageList(string target)
        {
            List<MessageObject> list = null;
            if (!m_mapCacheMessage.ContainsKey(target))
            {
                list = new List<MessageObject>();
                m_mapCacheMessage.Add(target, list);
            }
            else
            {
                list = m_mapCacheMessage[target];
            }
            return list;
        }

        /// <summary>
        /// 监听指定模块的指定事件
        /// Tips:Todo：暂没处理<统计所有CS的BusinessModule>所以若一直瞎发一个Module，都会缓存消息，瞎发导致缓存过多
        /// 答案：反射记录CS—Business所有Module方式即可解决
        /// 也就以为目前：只能处理Creat
        /// 动态创建Module都应该交给CS：lua事件是否能缓存，答案是看情况：通常 module.SetEventTable(mgrEvent);lua是能拿到的
        ///首先由于网络，按钮点击，lua+LuaModule一起启动会有延迟
        ///那就需要OnMessage时发送的目标表创建完毕，他的Evt虽然封装了但是拿不到。
        ///所以交给Cs处理是万能的下一条（不明确的动态启动系统，用这个好），随着玩家等级，游戏体验事件，体验的场景，和动态点击，页面层级，活动开启有很多module是可以动态开启的。（这里跟服务器不同，服务器这个用户没60级，有人70了呢，都要提前准备，客户端是裁剪原则）
        ///self.evtOnFinishAvgTalkBox = CS.SGF.Module.Framework.LuaCallModuleManager.Event("AvgLuaModule", "OnFinishAvgTalkBox")
        ///（明确已经启动的module，节省性能，同时编写方便）APPMain提前启动的系统都可以这么调用
        ///--self.evtOnFinishAvgTalkBox = self.LuaEventTable:GetEvent("OnFinishAvgTalkBox")
        ///所以C#层次还是负责记录由ModuleManager，而luahelper甚负责中转
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ModuleEvent Event(ModuleDef.Name target, string type)
        {
            ModuleEvent evt = null;
            if (ModuleDef.GetModuleDefState(target) != ModuleDef.ModuleDefType.None)
            {

                BusinessModule module = GetModule(target);//Get运行时Module + Get缓存非运行时Module = 决定这个消息是否缓存，和这个Event是否缓存

                if (module != null)
                {
                    evt = module.Event(type);
                }
                else
                {
                    //预创建事件
                    EventTable table = GetPreEventTable(target);
                    evt = table.GetEvent(type);
                    this.LogWarning("Event() target不存在！将预监听事件! target:{0}, event:{1}", target, type);
                }
            }

            return evt;
        }

        private EventTable GetPreEventTable(ModuleDef.Name target)
        {
            EventTable table = null;
            if (!m_mapPreListenEvents.ContainsKey(target.ToString()))
            {
                table = new EventTable();
                m_mapPreListenEvents.Add(target.ToString(), table);
            }
            else
            {
                table = m_mapPreListenEvents[target.ToString()];
            }
            return table;
        }

        #region 模块堆栈
        private Stack<KeyValuePair< string, StackMessageObject>> bModuleStack;////key是新的Module名，MessageObject1中的target是老的
        //private bool mStartPopStackWndStatus = false;
        public void ClearStackWindows()
        {
            bModuleStack.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModuleName"></param>
        /// <param name="newChangedObjForPrivate">改变堆栈中上一个module onShow时候的参数</param>
        public void OnNextStackHide(ModuleDef.Name ModuleName,object newChangedObjForPrivate) 
        {
            if (bModuleStack.Count <= 0)
                return;
            var name = ModuleDef.GetModuleName(ModuleName);
            if (bModuleStack.Peek().Key == name)
            {
                //检测下一个弹出
                StackMessageObject mo = bModuleStack.Pop().Value;

                ModuleDef.Name result = ModuleDef.GetModuleName(mo.target);// (ModuleDef.Name)Enum.Parse(typeof(ModuleDef.Name), mo.target);
                //BusinessModule mudole = GetModule(ModuleName);
                BusinessModule mudole = GetModule(result);
                if (mudole != null)
                {
                    if(newChangedObjForPrivate!=null)
                    {
                        mudole.HandleMessage(mo.msg, new object[] { newChangedObjForPrivate });
                    }
                    else
                    {
                        mudole.HandleMessage(mo.msg, new object[] { mo.args });
                    }
                    
                }

            }
         

        }
        /// <summary>
        /// 缓存一个消息到对应模块
        /// 跳转缓存一定是已经打开的module没有的不管，不用帮他打开模块，不处理预监听
        /// 新的模块自己调用
        /// 如果没有module的widget本质来说要加，没有就是找到这个公用模块用args来区分,同系统理论不调用
        /// </summary>
        /// <param name="recordeModule"></param>
        /// <param name="function"></param>
        /// <param name="recordeArgs"></param>
        public void SendMessage_Stack_Internal(ModuleDef.Name recordeModule, ModuleDef.Name newModuleName, object recordeArgs, object newArgs,  string function = "Show")
        {
            BusinessModule module = GetModule(recordeModule);
            if (module != null)
            {
                //将消息缓存起来
                StackMessageObject obj = new StackMessageObject();
                obj.target = ModuleDef.GetModuleName(recordeModule); //recordeModule.ToString();
                obj.msg = function;
                obj.args = recordeArgs;
                bModuleStack.Push(new KeyValuePair<string, StackMessageObject>(ModuleDef.GetModuleName(newModuleName), obj));
                //this.LogWarning("SendMessage() target不存在！将消息缓存起来! target:{0}, msg:{1}, args:{2}", recordeModule, function, recordeArgs);
            }
            
            BusinessModule newModule = GetModule(newModuleName);
            if (newModule != null)
            {
                //将消息缓存起来
                newModule.CircleStackCount++;
                newModule.HandleMessage(function,new object[]{ newArgs});
            }
        }


     


      

       
     
        #endregion
    }
}
