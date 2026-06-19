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
using System.Reflection;
using SGF;
using SGF.UI.Framework;
using StarProjectDef;

namespace SGF.Module.Framework
{
    /// <summary>
    /// 全部都是业务，物品也有可能会释放，只不过生命周期长一点
    /// </summary>
    public abstract class BusinessModule : Module
    {
        private string m_name = null;

        /// <summary>
        /// 并非赋值，是通过运行时的反射机制
        /// Cs里反射本类名
        /// Lua则是调用文件的名称 
        /// </summary>
        public string Name
        {
            get
            {
                if (m_name == null)
                {
                    m_name = this.GetType().Name;
                }
                return m_name;
            }
        }

        public string Title;


        /// <summary>
        /// 本层封装，和快捷引用,Lua也应具备快捷存储
        protected void Save<T>(string key, T value, string fileName = "data")
        {
            //this.LogError("Name:" + Name);
            //Name equals Child Name 's folder
            SaveManager.Instance.Save<T>(key, value, Name, fileName);
        }

        /// <summary>
        /// 本层封装，和快捷引用,Lua也应具备快捷存储
        protected T Load<T>(string key, string fileName = "data")
        {
            return SaveManager.Instance.Load<T>(key, Name, fileName);
        }

        /// <summary>
        /// 本层封装，和快捷引用,Lua也应具备快捷存储
        protected bool KeyExists(string key, string fileName = "data")
        {
            return SaveManager.Instance.KeyExists(key, Name, fileName);
        }



        public BusinessModule()
        {

        }
        /// <summary>
        /// Lua时帮Lua缓存模块名称
        /// </summary>
        /// <param name="name"></param>
        internal BusinessModule(string name)
        {
            m_name = name;
        }

        #region 模块的事件系统
        /// <summary>
        /// 每个模块都会有，很多的事件可以：【通知界面】，【通知其他模块】
        /// 所以每个Module都要拥有自己的事件表
        /// </summary>
        private EventTable m_tblEvent;

        /// <summary>
        /// 维护业务模块BusinessModule的事件集合的封装
        /// 实现抽象事件功能
        /// 可以像这样使用：obj.Event("onLogin").AddListener(...)        
        /// 事件的发送方法：this.Event("onLogin").Invoke(args)
        /// 而不需要在编码时先定义好，以提高模块的抽象程度
        /// 但是在模块内部的类不应该过于抽象，比如数据发生更新了，
        /// 在UI类这样使用：obj.onUpdate.AddListener(...)
        /// 这两种方法在使用形式上，保持了一致性！
        /// 补充：可以通过定义事件ModuleEvent，或通过定义“KeyStringName”的方式实现消息互通<-就别再抽象了
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ModuleEvent Event(string type)
        {
            return GetEventTable().GetEvent(type);
        }

        internal void SetEventTable(EventTable mgrEvent)
        {
            m_tblEvent = mgrEvent;
        }
        /// <summary>
        /// Cs中调用则默认创建
        /// Lua则无（具体形参）创建，但这张表需要映射到lua中
        /// 每个BModule只拥有一个EvtTable事件表，要么来源于预监听事件，要么来源于自己创建
        /// </summary>
        /// <returns></returns>
        protected EventTable GetEventTable()
        {
            if (m_tblEvent == null)
            {
                m_tblEvent = new EventTable();
            }
            return m_tblEvent;
        }

        #endregion

        #region 模块的创建销毁
        /// <summary>
        /// 调用它以创建模块
        /// </summary>
        /// <param name="args"></param>
        public virtual void Create(object args = null)
        {
            this.Log("Create() args = " + args);

        }


        /// <summary>
        /// 调用它以释放模块
        /// </summary>
        public override void Release()
        {
            if (m_tblEvent != null)
            {
                m_tblEvent.Clear();
                m_tblEvent = null;
            }

            base.Release();
        }
        #endregion

        #region 消息处理

        /// <summary>
        /// 当模块收到消息后，对消息进行一些处理
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        internal void HandleMessage(string msg, object[] args)
        {
            this.Log("HandleMessage() msg:{0}, args:{1}", msg, args);

            MethodInfo mi = this.GetType().GetMethod(msg, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (mi != null)
            {
                ParameterInfo[] parameterInfo = mi.GetParameters();
                //参数定义和删除接受自己对照定义，框架不会帮你去掉参数，错了自己改。
                if (parameterInfo.Length == 0)
                {
                    mi.Invoke(this, BindingFlags.NonPublic, null, null, null);
                }
                else
                {
                    mi.Invoke(this, BindingFlags.NonPublic, null, args, null);
                }
            }
            else
            {
                OnModuleMessage(msg, args);
            }
        }



        /// <summary>
        /// 由派生类去实现，用于处理消息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="args"></param>
        protected virtual void OnModuleMessage(string msg, object[] args)
        {
            this.Log("OnModuleMessage() msg:{0}, args:{1}", msg, args);
        }

        #endregion

        #region 对V层的常用逻辑
        /// <summary>
        /// action
        /// 可以直接add一个事件，
        /// </summary>
        /// public ModuleEvent StackOnShowCache { get { return Event("onStackShowCache"); } }
        /// <summary>
        /// 显示业务模块的主UI
        /// 一般业务模块都有UI，这是游戏业务模块的特点
        /// </summary>
        protected virtual void Show(object arg)
        {
            
        }

        /// <summary>
        /// 按钮绑定这个，逻辑层实现通过他来关闭
        /// 推荐lua 和 C# 重写hide，因为hide时一定是show之后，【就是二次show之前，hide我需要处理隐藏的东西】；因为流程不通过uimanager直接协调而是到逻辑到表现
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void Hide(object newChangedObjForPrivate)
        {
            this.Log("Hide() arg:{0}", newChangedObjForPrivate);
            BakeToStackCmd(newChangedObjForPrivate);

       
            //自己实现通过uimanager里面关闭自己的界面，并且控制自己的关闭逻辑
        }

        public int CircleStackCount = 0;
        public void BakeToStackCmd(object newChangedObjForPrivate)
        {
            //帮忙注册一个返回，和使用次数
            if (CircleStackCount <= 0)
            {
                return;
            }
            else
            {
                ModuleDef.Name enumValue;
                
                if (System.Enum.TryParse(Name, out enumValue))
                {
                    ModuleManager.Instance.OnNextStackHide(enumValue, newChangedObjForPrivate);
                    CircleStackCount--;
                }
            }
        }
 
        #endregion
    }
}
