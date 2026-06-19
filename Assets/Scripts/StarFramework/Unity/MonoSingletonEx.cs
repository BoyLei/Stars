using UnityEngine;
namespace SGF.Unity
{

    /// <summary>
    /// 项目第二个管理器
    /// 继承我的Mono ，别写Awake，不然会覆盖我的
    /// 或者子类调用我的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingletonEx<T> : MonoBehaviour where T : MonoSingletonEx<T>
    {
        /// <summary>
        /// 主动式和被动式，那个快用哪个，习惯遵循程序员，反正是【单】例
        /// </summary>
        private static T m_instance = null;
        /// <summary>
        /// 被动式
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    //已经挂载的就也能找到，不是一切都基于创建
                    //初始化
                    m_instance = GameObject.FindObjectOfType(typeof(T)) as T;
                    //数量多
                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        return m_instance;
                    }

                    if (m_instance == null)
                    {
                        // new出脚本 和 comp
                        m_instance = new GameObject("Singleton of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();
                        //先挂由Awake驱动，后挂载由“.到=>加载完毕”驱动。
                        AfterLoad();

                    }
                }

                return m_instance;
            }
        }


        /// <summary>
        /// 不论主动（指向自己）还是被动（加载出来）所有公用模块都会设置层级
        /// 除了以下：
        /// 1，AppMain【第一步】
        /// 2，GlobalModule【第二部，自己】
        /// 2.5理论上后增加的，主动式Awake/被动式"."运算符时间戳都会在第二部之后，也会进行节点归纳，为了遵从本逻辑，在此之前的逻辑归纳放在AppMain中
        /// 3，其他都归属在GlobalModule下，但是本质类型相同，且常驻
        /// 每日贴士：一般绝不对有的逻辑漏洞是 A != 3 || A != 4,深思
        /// </summary>
        private static void AfterLoad()
        {
            if (typeof(T).ToString() != "AppMain" && typeof(T).ToString() != "GlobalModules" && typeof(T).ToString() != "EntityRoot")
            {
                //任意Mono模块会激活，G的生成，并再后续再G下
                //若GlobalModule为空，预先生成，或点之后Await生成。
                GlobalModules gM = GlobalModules.Instance;
                m_instance.transform.SetParent(gM.ModuleRoot);
            }

            m_instance.InitSingleton();
            //子集拓展
            m_instance.Init();
            //常驻Mono服务类
            DontDestroyOnLoad(m_instance.gameObject);
        }

        protected virtual void Init()
        {

        }
        protected virtual void InitSingleton()
        {
            //check
        }



        /// <summary>
        /// 主动式:EntityRoot,AvProRoot
        /// </summary>
        private void Awake()
        {
            if (m_instance == null)
            {
                m_instance = this as T;
            }

            //try
            //{
                AfterLoad();
            //}
            //catch (System.Exception e)
            //{
            //    Debug.LogError($"[MonoSingletonEx] 报错 {e.Message}");
            //    Debug.LogError($"[MonoSingletonEx] name: {transform.name}");
            //}
        }


        public void DestroySelf()
        {
            Dispose();
            MonoSingletonEx<T>.m_instance = null;
            UnityEngine.Object.Destroy(gameObject);
        }

        public virtual void Dispose()
        {

        }

        private void OnApplicationQuit()
        {
            m_instance = null;
        }


    }
}