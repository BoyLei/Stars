using SGF.Module.Framework;
using SGF.Unity;
using SGF.Utlis;
using SkillEditor;
using StarProjectDef;
using System;
using System.Collections;
///--------------------------------------------------------------------
using System.Collections.Generic;
using Task;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using XLua;
using Yoka.UnityString.Core;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

/// <summary>
/// ==================================================================================================
/// 资源管理部分，内存+IO ，避免跷跷板问题
/// 包裹addressable外围控制封装屏蔽包裹addressable达成更好的资源控制卸载统计了解优化，封装引用计数，封装唯一返回资源（他应该会控制）
/// 【先都换成我的，删除原来的，不允许直接用addressable】，大家都调用release习惯，内存统计思路，配置调整，分配高低中优先级，维护阶段
/// 时间，空间，使用，版本号，cache，引用计数，弱引，优先级
/// 需求封装
/// |
/// v
/// update 时间缓解期
/// |
/// v
/// res Class 控制并发和负载
/// |
/// v
/// Addressable异步会处理，资源重复load的问题，和引用基数我完全包裹，我不必自己lock，和添加state
/// ==================================================================================================
/// 【使用规范】
/// 引用计数 加载AA几次就要  = release几次：这个我没考虑到，我根据自己的计数去释放就好；
///之前说的首先要做到下面要求：
///==================================================================================================
/// 1，这块首先就必须都调用 我这边规范的接口，单独去AALoad的都不合规。
///==================================================================================================
/// 2，ref类型的比如图片，不要反复调用AA.load，自己用字段持有
///结束的时候  字段 = null ，然后我这边释放一次。
///如果容器持有，容器遍历 = null ，然后我释放一次.
///==================================================================================================
///3，如果是gob需要实例的内容需要
///实例清空（分情况看有没有二次调用）
///容器 或 字段 清空
///然后我释放一次*/
///==================================================================================================
///要点记录：
///TODO:【内存大小控制：加载的，全局的】，依赖于全局内存控制，目前不做的原因是连资源拆分都要细化，并且控制数量好处大于内存大小；在不确定绝对情况他影响开发还是用数量更好
///TODO:可以拓展的东西： 缓存的字段合理利用如版本，用更多的参数来细化操作比如使用的引用先后
///Done:线程锁
///所有调用都由我管理，时间管理优先级 && 满了 == 依据时间强制清理 ；加载时（虽然弱引用可以自己释放+检测引用时机也会清理）； 外面自己减少引用;低级如果由弱引用给释放掉了看addressable句柄？
///【全部清理】================Release的内存控制功能==============================
///尊重的优先级【控制数量的阈值】->[引用数量]-稍大于>【最近引用】
///【第一优先】Done：重新登录的Release【重新启动到主界面】的释放：1：高级appMain一次永远不释放，其他进行加入和释放循环；清理+全局GC
///【第二优先】Done：数量为0的可主动释放接口：中级（高永不释放）（低永不处理）中级只能开放给别人只能通过要求无法控制左侧值| 中级是开放给外面资源释放的时候是可以操作句柄进行资源释放的 | 战斗entity不要控制因为是池只要继承IRecyclableObject都不要管理 | UI内的或者非实体都是可以删除的
///【第三优先】Done：Clear 触发时机：新增资源时 && 空间不够 | 移除空间外的“时间要求者”|处理办法：中级要 ClearReferenceCount - 单体GC - 不管析构-    中级不论交给程序，没空间，游戏重启都是交给CheckReferenceCount处理，本质都关心释放所以析构不需处理任何事情。所以高级也如此
///低级要： 空间不足一定释放1.1-1，（然后就是引用-没有），最近没用的，[处理空的]；ClearReferenceCount- 管析构- 最后调用Gc  
///没有高级 
///【第四优先】：类的析构，低级自动（主动被动情况）/| （中/高级不需要关心析构都主动都封装了）：
/// 需要处理的：系统释放的，主动释放的
///clear的Gc调用 = 切场景，check（加载），release重新登录游戏*/
///优先级定义 评判标准Done TODO存储提前写MainApp缓存，加载逻辑
///所有接口都换了
///角色壳子用池子---反复创建的壳子都是用一个减少实例化概念，公用销毁清理
///副本相机参数。----虚拟相机事件发送者的gameobject，也可以公用大量重复？，销毁清理参数，（可改可不改）
///其他都是实例
///==================================================================================================
/// </summary>

namespace StarProject.Service.Resource
{
    /// <summary>
    /// 目录：
    /// 文件分成几部分
    /// 1，外部实例缓存，外部字段，外部容器，外部实例
    /// 2，内部实例缓存，GameObjectPool
    /// ========================================================
    /// 我的封装，shared[object + count] = 对应AA  | 主做功能 ： 可主动通过计数保证释放【请查看RemoveMiddleRef()】 +主动强制释放AA【请查看ForceClearMiddleResourceCache(key)】+ 被动时间检测释放【Check关键词】
    /// ========================================================
    /// 内存部分：请查看 【内存主动清理】
    /// 
    /// Q&A：
    /// Q：引用类型图片的直接释放会不会给他带来问题测试，如果有问题就要 时间&&计数一起释放？
    /// A：【addressable没了，我的缓存没了】，【内存释放不掉，图片释放指针在AA层的也没了】；但图片左值等于空，下次GC时会清理掉；这样外层释放是外层释放不释放培养习惯，如再需要我再构建我和AA的缓存准备好，最终内存指向一定会覆盖，我确实可以释放底层而不影响表现，所以判断可依据时间直接释放而不依赖引用计数严谨释放保证表现
    /// Q:静态引用用不用全部 sprite == null; 
    /// A:不必，错了是因为uiMGr持有这个类了

    /// </summary>
    [LuaCallCSharp]
    public class GameObjectPool
    {
        //资源路径
        public string AssetPath;
        //原始资源，且唯一
        public GameObject Asset { get; private set; }

        private Transform Parent;
        //对象池
        private List<GameObject> Pools;

        //已经使用过的
        private Dictionary<int, GameObject> UsedList;
        public GameObjectPool(string path, GameObject asset, Transform parent)
        {

            this.AssetPath = path;
            this.Asset = asset;
            this.Parent = parent;
            this.Pools = new List<GameObject>();
            this.UsedList = new Dictionary<int, GameObject>();
        }
        //供lua调用
        public static GameObjectPool CreatePool(string path, GameObject asset, Transform parent)
        {
            GameObjectPool pool = new(path, asset, parent);
            return pool;
        }

        public GameObject Pop()
        {
            if (Asset == null)
            {
                SGF.Debuger.LogError($"GameObjectPool Pop GameObject Asset is null {AssetPath}");
                return null;
            }
            GameObject go = null;
            if (Pools.Count > 0)
            {
                go = Pools[0];
                Pools.RemoveAt(0);
            }
            else
            {
                go = GameObject.Instantiate(Asset);
                go.transform.SetParent(Parent, false);
            }

            if (go == null)
            {
                SGF.Debuger.LogError("GameObjectPool Pop GameObject is null");
                return null;
            }
            UsedList.Add(go.GetInstanceID(), go);
            return go;
        }

        /// <summary>
        /// 入栈
        /// 内部控制，调用者决定类型，调用者多种类型混合
        /// </summary>
        public void Push(GameObject go, GameObjectPoolType gameObjectPoolType = GameObjectPoolType.ActiveType)
        {
            if (go == null)
            {
                SGF.Debuger.LogError("GameObjectPool Push GameObject is null");
                return;
            }

            int instanceID = go.GetInstanceID();
            switch (gameObjectPoolType)
            {
                case GameObjectPoolType.ActiveType:
                    go.SetActive(false);
                    break;
                case GameObjectPoolType.CanvasGroupType:
                    go.GetComponent<CanvasGroup>().alpha = 0;
                    break;
                case GameObjectPoolType.PosType:
                    go.transform.position = Vector3.one * 999f;
                    break;
                case GameObjectPoolType.ScaleType:
                    go.transform.localScale = Vector3.zero;
                    break;
                default:
                    break;
            }
            if (UsedList.ContainsKey(instanceID))
            {
                go.transform.SetParent(Parent);
                Pools.Add(go);
                UsedList.Remove(instanceID);
            }
            else
            {
                GameObject.Destroy(go);
            }
        }

        public void Clear(bool clearall = true)
        {
            if (UsedList != null && UsedList.Count > 0)
            {
                foreach (var item in UsedList)
                {
                    GameObject.DestroyImmediate(item.Value);
                }
                UsedList.Clear();
            }

            // 统一clear, 避免每次 removeAt(0) 产生的 数组拷贝
            for (int i = 0; i < Pools.Count; i++)
            {
                GameObject.DestroyImmediate(Pools[i]);
            }
            Pools.Clear();

            if(clearall)
            {
                GameObject.DestroyImmediate(Asset);
            }
        }
    }

    public class SharedResource
    {
        public string pathKey;                          //空间换处理GC效率
        public /*T*/Object resource;                    //实际资源
        //public uint version;                            //资源版本号
        public float lastAccessTime;                    //天然的LRU算法，他可以做顺序池体缩容量，也可以做限制裁剪。
        public int referenceCount = 0;                    //通过被引用关系可以确定重要程度，就是我这里有池，你直接必须调用我接口，也不要自己缓存。
        public ResourcePriority resourcePriority;       //资源加载优先级
        public bool IsActiveRelease = false;            //DRC-CRC
                                                        //外面回收里面也回收，确保以下
        /// <summary>
        /// ============================================================================================================================
        /// 1，封装加强控制策略，2， m_InternalOp = null;内部自己不引用，3，我一定管理所以所有调用必须通过我，然后我通过他，别人不可以自己缓存资源，4，adr他也没办法确定谁缓存了他只能计数，5，原则一定是没有自己不缓存而是直接调用我，6游戏最高原则是不能直接删除那就废了只能计数，7我管理obj，adr管理句柄互相不重复且一一对应，8我的原则如果有不会重复给他计数的。9缺少直接调用我不可以自己缓存，10理论上我的计数和他的是一致的，11因为防止重复我和他都有所以计数一般就是个0~1之间，12我的引用是直接对obj的引用我的计数会超过他的
        ///
        ///“这是因为 Unity 的垃圾回收机制可能会在资源被释放之前调用析构函数。”你说的这一点逻辑有问题，我觉得正是调用了析构才有机会释放这个资源，如果资源先被释放了在调用release这个资源已经是空了还有什么释放必要么？第二点Release是资源引用减少并不是直接释放资源，所以我减少引用也没什么问题啊，第三点我这个资源是弱引用我根本没有明确的时机去检测释放，第四点我是弱引用情况判断资源非空的时候才去释放，这有问题么
        ///============================================================================================================================
        ///1【不必持有句柄】，因Releaseobj，就是句柄释放，句柄Addressable已经维护了我不用维护
        ///2【意图理解我本质不是Destroy】因为所有资源都通过Addressable管理的话，为了保证资源（引用类型）的“不释放导致游戏报错这一点”这个--，就意味着是0；如果有其他引用那我也不要直接释放
        ///3【那我的意图已经是首先】===通过我ResourcecManagerExtend===通过Addressable===管理内存资源和引用资源
        ///4【为啥由他我还要管理ResourceManager + Addressable】他做到我这些控制了？
        ///所以只要控制我【释放】即可
        ///============================================================================================================================
        ///1，首先你不能直接删除，你违反了游戏表现优先，不能错误，加载队列释放次之
        ///2,通过全部收集你也可以控制和删除这个物件，不过AsyncOperationHandle没开放给你Destroy，因为引用他是能持有
        ///但是不论是我还是他，都没法确定是不是所有资源你都通过我，这是口头约定你遵循我我控制一定准确，所以Addressable不开放给我，我也不开放给上层
        ///3，删除这个是事件回调*/
        /// ============================================================================================================================
        /// 程序客观上来说，系统自动析构，也应该处理addressable 和 obj的释放，这符合弱引用的设计需求
        ///但实际来说当Gc被调用析构时这时属于C#生命周期，这时候调用不到Unity主线程里面的任何信息，就容易报错
        ///虽然应该处理，但处理不了，会出现什么情况看能否接受
        ///1，会触发计数增加？，我的SR会被释放掉所以次数一定没了，缓存也没了，【addressable会持有这份缓存，在创建他的引用计数会增加】；【当我主动释放的时只能减少其1次】；【unity没有clear接口】；【所以unity会盲目记录一个低级的东西很多次数】【少用低级？不低级里面存的东西不一定那么有用】【主动清理将会直接减少这个引用低级】：所以大概率上会被主动释放一直减少次数
        ///2，只有addressable有这个资源的话，我这边weak引用类没了，资源也空引用了，就看addressable会不会对时间和弱引用进行释放了反正我调用不了
        ///3，整体看是平衡的（主动被动），但是单个看没法调用是有瑕疵的但是没办法
        ///=============================================================================================================================
        ///如果你想要监听弱引用被垃圾回收器自动释放的时机，你确实不能依赖于 Dispose() 方法，而可以依赖于对象的析构函数（Finalizer）。
        ///这个是用来主动调的，但是被动如果弱引用被调用了，你不确定Gc会不会调用你，也可能因为时机顺序的原因可能不调用你
        ///所以没用
        /// </summary>
        /// <param name="controlToDepose">默认是自动的，unity本身机制是切场景的时候会调用gc，这时候会触发弱引用的析构</param>
        protected virtual void OnDispose(bool controlToDepose = false)
        {
            if (controlToDepose)
            {
                if (resource != null)
                {
                    /*                 Addressables.Release(handler) 只释放了 AsyncOperationHandle 对象，使其不再占用内存，但加载完成后的资源会继续存在，可以继续使用。
                 Addressables.Release(result.Result) 除了释放 AsyncOperationHandle 对象外，还释放了加载完成后的资源，因此该资源将不再可用。*/

                    //首次  addressable + 1    二次 + 0
                    //          我的 + 1             + 1
                    //最后一次 我这里容器，本类，addressable一起释放
                    Addressables.Release(resource);
                    resource = null;
                    pathKey = string.Empty;
                }
            }
            else
            {

            }

        }



        public void Dispose()
        {
            /*throw new System.NotImplementedException();*/
        }
        //fINALIZE调用
        ~SharedResource()
        {
            switch (resourcePriority)
            {
                case ResourcePriority.Low:
                    //低优先级根本不记数
                    /*
                        --处理的 -- 全会处理，addressable，obj=null，析构，所以析构不用理会

                       --主动Clear释放的-- 全会处理，addressable，obj = null，析构，所以析构不用理会
                            |
                            V
                        新加载，SceneGC，游戏释放 = >都封装 DecrementReferenceCount 和 Crc
                    */
                    //系统释放的 - 你应该监听析构
                    if (IsActiveRelease == false)
                    {
                        referenceCount = 0;
                        OnDispose(false);
                    }
                    break;
                case ResourcePriority.Middle:
                    //中级不论交给程序，没空间，游戏重启都是交给CheckReferenceCount处理，本质都关心释放所以析构不需处理任何事情。
                    //【没有失控的释放，主动的都会走流程】
                    break;
                case ResourcePriority.High:
                    //所以高级也如此:高级压根不限制数量
                    //高级甚至都除了关闭app
                    //都不会释放高级资源
                    //【没有失控的释放，主动的都会走流程】
                    break;
                default:
                    break;
            }
        }



        //主动操作=======================
        public void ActiveAddReferenceCount()
        {
            //0 1 2
            referenceCount++;
        }



        /// <summary>
        /// 缓存的意义是 外面多地方引用  我这里一份  addressable多次 
        /// return isTrueRelease
        /// </summary>
        public bool ActiveDecrementReferenceCount()
        {
            bool isTrueRelease = false;

            if (referenceCount > 1)
            {
                referenceCount--;
                /*Addressables.Release(resource);*/
                isTrueRelease = false;
            }
            else// 1 - 0  的释放
            {
                referenceCount--;
                CheckFinalSuppressRelease();
                isTrueRelease = true;
            }
            return isTrueRelease;
        }



        //多处关联我自己删除【外面容器，字段，实例外层自己释放】（，我的集中缓存【我集中释放】，addressable是多个引用【我集中释放】；内存堆栈和释放）<-------------依据是时间发现没人用就给你清理了我和AA和内存的缓存不影响业务，业务自己删除即可，这样整体简单
        //都是我封装的所以Addressable 持有数量一定是0和1
        public void ActiveClearReferenceCount()
        {
            while (referenceCount > 1)
            {
                /*Addressables.Release(resource);*/
                //ref -- AA
                referenceCount--;
            }


            // 1 - 0  的释放
            referenceCount--;
            CheckFinalSuppressRelease();

        }
        //主动操作=======================


        /// <summary>
        /// 游戏进行中的常规释放
        /// Addressable释放资源，引用空，释放本类
        /// 主动释放都会走这里
        /// 1 - > 0 0有一次释放
        /// </summary>
        private void CheckFinalSuppressRelease()
        {
            if (referenceCount == 0)
            {
                IsActiveRelease = true;
                OnDispose(true);
                //最后允许被调用析构
                System.GC.SuppressFinalize(this);//释放自己，告诉垃圾回收机制，我可以提前释放了（原理跟我引用计数一样）
            }
        }


        //------------------------------------------------------------------------------------------------------------------------------
        //游戏结束，重新进入游戏重新开始，的强制释放。



    }

    public class ResourceReqLoadQueue/*<T> where T:Object */
    {
        private int m_LoadedCount = 0;
        private Queue<ResourceRequest/*<T>*/> m_RequestQueue = new();
        //这里同步就是协同，异步是多线程
        //同步异步都有用到：处理负载；并发由其他字段控制
        public int LoadedCount { get { return m_LoadedCount; } }
        //可用来控制并发，不过并发已经有控制了，负载
        public int RequestCount { get { return m_RequestQueue.Count; } }

        public void Enqueue(ResourceRequest/*<T>*/ request)
        {
            m_RequestQueue.Enqueue(request);
        }


        public ResourceRequest/*<T>*/ Dequeue()
        {
            return m_RequestQueue.Dequeue();
        }

        public void AddLoadedCount(int count = 1)
        {
            m_LoadedCount += count;
        }

        public void ResetLoadedCount()
        {
            m_LoadedCount = 0;
        }
        /// <summary>
        /// 下一个要处理数据的优先级
        /// </summary>
        public ResourcePriority PeekResPri()
        {
            return m_RequestQueue.Peek().Priority;
        }

        public int Count { get { return m_RequestQueue.Count; } }


    }

    public struct ResourceRequest/*<T> where T : Object*/
    {

        public string ResourcePath;
        public System.Action<Object/*T*//*Object*/, AsyncOperationStatus> Callback;
        public ResourcePriority Priority;
        public Type type;

        public ResourceRequest/*<T>*/(string resourcePath, System.Action<Object, AsyncOperationStatus> callback, ResourcePriority priority, Type type1)
        {
            ResourcePath = resourcePath;
            Callback = callback;
            Priority = priority;
            type = type1;
        }

        //public static ResourceRequest<T> Create(string resourcePath, System.Action<T> cb, ResourcePriority priority)
        //{
        //    return new ResourceRequest<T>
        //    {
        //        ResourcePath = resourcePath,
        //        Callback = cb,
        //        Priority = priority
        //    };
        //}
    }

    public enum ResourcePriority
    {
        //大量的物件加载都是低优先级的【也不重要也不频繁】
        //【需要异步获取】【更小的空间】【绝大多数默认使用】【检测比较更加频繁】【并且有特殊的释放点】
        Low,
        // 而一些不太重要且不太频繁使用的资源，例如图标、背景音乐等，会被标记为低优先级
        /*
                在实际项目中仍然具有一定的用途性，尤其在一些特殊的场景下，比如需要异步加载大量资源，或者需要对加载过程进行一些复杂的控制，或者需要精细地控制资源加载的优先级和顺序，这时使用协程的方式可能更为合适，效果更好。

        举个例子，假如我们需要为游戏制作一个资源下载器，在实现下载器时，我们并不能把异步加载的任务委托给 Addressable 或者 Unity 引擎下的协程调度器。相反，我们需要设法手动创建协程，并在协程内部实现异步操作，以便更好地控制异步操作的生命周期和执行顺序，定期向服务器请求资源并根据当前的网络状况来调整下载速度。

        除此之外，在某些资源加载速度较慢的场景下，比如加载 3D 模型和大型贴图时，使用协程的方式可以很好地控制加载的优先级和顺序，提高游戏性能和玩家体验。此外，面对一些我不方便获取回调的情况，比如涉及到 C++ 部分的代码，使用协程的方式也是比较好的选择。

        因此，在实际项目中，使用 StartCoroutine 的场景特别情况下仍然存在，但需要根据具体需求进行权衡和选择。一般而言，在游戏中涉及到资源加载时，建议首选 Addressable 的自带异步 API 方式进行实现。
        
        */

        //【协同拿的同步】【比较[频繁]的池】【小空间池】【一般重要频繁】

        Middle,
        //数据表，对抗并发的  而一些不太重要且不太频繁使用的资源，
        //而中优先级的资源则相对较少，所以采用LRU等算法对这部分资源进行缓存，可以提高多次重复使用同一资源时的性能和效率。
        //高级快速需要缓存机制
        //【直接拿的同步】【预加载策略】【不限制空间】【限制使用】【检测不会很频繁】[重要且频繁]
        //随时有，量级比较少，比较重要的

        High,//hud，例如公用的UI、音效等，会被标记为高优先级
        Config
    }

    /// <summary>
    /// 并发负载测试完毕
    /// 清理测试完毕
    /// 容量测试？
    /// </summary>
    [XLua.LuaCallCSharp]
    public class ResourceFormalManager : ServiceModule<ResourceFormalManager>
    {

        private float lastCheckTime;
        private float CHECK_CD = 1f;//所有层级公用，检查一次就可以了，同3秒内的不同检查
        //同步线程并发控制，和阻塞控制
        //取回的货物可以通过缓存可以Clone（这么理解货物）
        private static int s_CurConcurrencyRequests_MID = 0;//已经派遣出去的调货员
        private static int S_MAX_CON_CURRENTRE_QUESTS_MID = 200;//30;//一共雇佣调货员,通用头像问题导致30个就满了
        //=======================两个类型，上面控制并发，下面控制慵懒机制
        private static int S_BATCHSIZE_MID = 300;//假设订单一次下发需求Queue中有50种货物，我有100个取货员，限制区块大小为5
        //有新需求的时候清空休息机制清理当前懒散机制
        //如一直没有需求优先派遣IO需求（无限制）；
        //一旦遇到本地拿货机制（约到后期触发越多）【每帧只处理一个本地需求:进入懒散状态】  这里当没并发的情况获取在后期也进行阻塞（Update虽然上限是60但是他是动态帧，如果拿的资源小就拿的快，触发的就多，拿的也多】

        //异步线程并发控制，和阻塞控制
        private static int s_CurrentConcurrentRequests_L = 0;
        private static int s_MaxConcurrentRequests_L = 300;//并发上限
        private static int s_BatchSize_L = 500;//时间换负载，的上限

        //高级多少个都可以，都是必要的
        //中级是持久的空间要兼容释放点
        //低级一会儿就没了

        //当资源加载完成时，缓存中会缓存资源的版本，通过版本号可以检查缓存是否存在不同版本的同名资源。如果存在，则需要释放旧版本的资源并加载新版本。在缓存资源时，将此版本号添加到缓存项中。
        private const uint CURRENT_VERSION = 1;
        //抗并发middle可以大；但是利用率高的Lower的量级很大
        private const int MIDDLD_MAX_CACHE_SIZE = 400;//最大条目数量，暂未规划最大内存，抗冲突，对比完全不卸载规划的高级来说，这个是可以拓展的高级，但是他不提前加载（之后要动态从100拓展到200）
        private const int LOWER_MAX_CACHE_SIZE = 600;//弱引用清理是真的快，加载的时候保存一瞬间，随后就释放掉了保证给你的时候有随后他也不会持久很快就没了,作用就是倒手的作用，所以他其实是处理临时内存的,最大吞吐容器，和频繁程度
        //500 100就处理，并发太大在初级里面一起加载触发删除逻辑会导致卡死这个预料之中，预加载就是很多东西，就是要高级
        private const float MIDDLD_CHECKER_TIME_SEC = 60 * 20;//多长时间没人再用20Min;
        private const float LOWER_MAX_SAVE_TIME = 60 * 10;//多长时间没人再用10Min


        /// <summary>
        /// 缓存的 jsonAsset 资源, 预加载时 只需要 预加载 JsonAsset 资源. 
        /// 等到真正需要加载的时候, 再去 json 序列化
        /// </summary>
        private Dictionary<string, TextAsset> s_CfgResourceJsonAssetCache = new();
        private Dictionary<string, object> s_JsonAssetCacheDic = new();
        //资源缓存区
        private static Dictionary<string, SharedResource/*<Object>*/> s_HighResourceCache = new();
        private static Dictionary<string, SharedResource/*<Object>*/> s_MiddleResourceCache = new();

        //共享思考模型，Dic思考手抓一堆Key，下面的Value是乱的；他的顺序由LinkedList来排Value
        //其中的一个节点LinkedListNode;一个是快，一个是还原内存GC
        //索引资源：共享；获取的便捷性
        /*TODO：在单链表中，每个节点只能访问到后一个节点，如果需要访问前一个节点，需要依次遍历链表，直到找到目标节点。这种遍历速度较慢，因此查询和删除效率较低。

双向链表在单链表的基础上增加了一个指向前一个节点的指针，这样就可以在常数时间内访问每个节点的前驱和后继节点。因此，在实现 LRU 缓存淘汰算法时，如果使用双向链表，可以方便地在不影响遍历顺序的情况下，删除任意一个节点。

总的来说，使用双向链表实现 LRU 缓存淘汰算法会更加高效方便，但如果数据量较小，单链表也足够实现，具体的实现方式需要根据具体情况而定。*/
        private static Dictionary<string, LinkedListNode<System.WeakReference<SharedResource/*<Object>*/>>> s_LowResourceCache = new();
        //排序处理资源：共享；Sort和排除的便捷性
        private LinkedList<System.WeakReference<SharedResource/*<Object>*/>> m_LowPriorityResourcesLinked = new();

        public ResourceReqLoadQueue/*<Object>*/ m_NormalPriorityQueue = new();
        public ResourceReqLoadQueue/*<Object>*/ m_LowPriorityQueue = new();

        List<string> cfgPathes;
        List<string> highPathes;
        List<string> middlePathes;
        List<string> lowPathes;

        private Dictionary<string, GameObjectPool> GameobjectPools = null;
        private GameObject ObjectPools;

        private AssetsToDefine m_AssetsToDefine = null;

        public void Init()
        {



            CheckSingleton();
            float totalMemoryInGB = SystemInfo.systemMemorySize / 1024f;
            if (totalMemoryInGB > 6)
            {
                CHECK_CD = 1.5f;
            }


            MonoHelper.AddUpdateListener(OnMyUpdate, MonoHelper.E_ModuleType.Resource);
            //必须小写,必须完全（就是mapdata后面跟的一定是/，你不能 实际路径是mapdataXXX/ 这样不会帮你做模糊匹配的），必须最后不加 "/"
            cfgPathes = new List<string>
            {
                   "luascripts",
                   //  "fonts",
                   "config",
                   "mapdata",
                "config/skill"
            };

            //高优先级的永远缓存，真正用的时候可以认为就是同步。
            highPathes = new List<string>
            {
                /*   "mapdata",
                   "luascripts",
                   "config",
                   "fonts",
                   "pipelinesetting",
                   "properties/virtual",
                   "ui/starworldpage",
                   "ui/common/uimsgbox",
                   "ui/common/uitips",
                   "ui/common/formitem",
                   "properties/impulsesource",
                   "properties/cameraoffset",
                   "properties/virtualcamera",
                   "properties/settings",
                   "properties/artcommonhelperpfb",
                    "properties/rawimage",*/
                "animation/Roles/Character",
                "mapdata",
                "config",
                //技能配置放在高优先级加载和读取，但段磊自己有一套自己的【缓存你我二选一(dl是实例，我是文本我来释放不缓存)】，导致我白存了一份直接走他的了。@DL  【数量不足问题】
                //（不用改加数据压缩）配置用异步，但是提前加载，自己有缓存了@DL验证读取在这里【高优先，1异步加载，2提前加载，3类似同步，4逻辑层不用异步，5提前准备数据，6数据存储节省但是文本加载当作同步处理很简单【虽然异步很快，但是提前加载可以当作同步，不需要逻辑考虑异步问题而且数据很小不占内存】】
                //（挪到尔东地方）也要根据角色信息@DL改到尔东的地方【节省】
                "roles/template",
                "roles/world/character",
                "luascripts",

            };

            middlePathes = new List<string>{
                "animation",
                "timeline",
                "map",
                "rendertexture",
                "roles",
                "ui",
                "ui/Common/Prefab",
                "maps/commoncollider",
                "properties/timeline",
            };

            lowPathes = new List<string>{
                "effects",
                "interactobject",
                "model",
            };

            // 创建池节点（不可见，不删除）
            InitObjectPools();
        }

        private void InitObjectPools()
        {
            ObjectPools = GameObject.Find("ObjectPools");
            if (ObjectPools == null)
            {
                ObjectPools = new GameObject("ObjectPools");
                ObjectPools.transform.position = new Vector3(-9999, -9999, -9999);
                ObjectPools.transform.rotation = Quaternion.identity;
                ObjectPools.transform.localScale = Vector3.one;
                ObjectPools.hideFlags = HideFlags.HideInHierarchy;

                GameObject.DontDestroyOnLoad(ObjectPools);
            }

            if (GameobjectPools == null)
            {
                GameobjectPools = new Dictionary<string, GameObjectPool>();
            }
        }

        private void ReleaseGameobjectPool()
        {
            if (GameobjectPools != null && GameobjectPools.Count > 0)
            {
                foreach (var item in GameobjectPools)
                {
                    item.Value.Clear();
                }
                GameobjectPools.Clear();
            }
        }

        public override void Release()
        {
            MonoHelper.RemoveUpdateListener(OnMyUpdate, MonoHelper.E_ModuleType.Resource);
            OnModuleReleaseClearResourceCache();
            ReleaseGameobjectPool();
            s_CfgResourceJsonAssetCache.Clear();
            s_JsonAssetCacheDic.Clear();
            base.Release();
        }

        private RenderTexture _renderTexture;
        public RenderTexture RenderTexture
        {
            get
            {
                if (_renderTexture == null)
                {
                    _renderTexture = new RenderTexture(2048, 2048, 0, RenderTextureFormat.ARGB32);
                    int msaaSampleCount = Service.UniversalRenderPipeline.UniRenderPipline.Instance.GetMsaaSampleCount();
                    msaaSampleCount = 1;//msaaSampleCount < 2 ? 2 : msaaSampleCount;
                    _renderTexture.antiAliasing = msaaSampleCount;
                }
                return _renderTexture;
            }
            set
            {
                _renderTexture = value;
            }
        }

        [Obsolete("请使用 RecursionLoadAsset RecursionLoadGameObject", false)]//标记该方法已弃用
        public GameObject RecursionLoadGameObject(string path, E_AssetType type)
        {
            string str = GetRecursionLoadAssetPath(path, type);
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            {
                if (type == E_AssetType.Roles)
                {
                    SGF.Debuger.LogWarning($"角色资源没加载到 路径={path}");
                }
                return null;
            }
            //干, LoadGameObject 内部会 Addressables.InstantiateAsync 实例化prefab, 所以返回的不是 asset, 而是 prefab
            GameObject gob = LoadGameObject(str);
            if (gob == null && type == E_AssetType.Roles)
            {
                SGF.Debuger.LogWarning($"角色资源没加载到 路径={path},asset={gob}");
            }

            return gob;
        }

        /// <summary>
        /// 配置级：超高级
        /// </summary>
        /// <typeparam name="T"><UnityEngine.TextAsset></typeparam>
        /// <param name="resourcePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="subSpriteName"></param>
        public void LoadResourceUniRefAsync(string resourcePath, System.Action<string, UnityEngine.TextAsset> forCbCount)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                forCbCount?.Invoke(resourcePath, null);
                return;
            }

            bool pathExist = false;

            foreach (var item in cfgPathes)
            {
                if (resourcePath.EachCharCompare(item.ToLower()))
                {
                    pathExist = true;
                    break;
                }
            }
            if (pathExist == false)
            {
                forCbCount?.Invoke(resourcePath, null);
                return;
            }

            // 释放这个lua 对应的 textAsset 资源的引用计数
            //Addressables.Release(textasset);
            // 1,属于高优先级的特殊情况，全加全卸，懒加懒卸,不包裹一层直接调用addressable
            // 2,技能，这里，两个结构持有json结构化类
            // 3,Lua部分缓存byte[]执行入口；计数-1被unuse和清理在切换场景
            if (s_CfgResourceJsonAssetCache.ContainsKey(resourcePath))
            {
                forCbCount?.Invoke(resourcePath, null);
            }
            else
            { /*首先不要计数，也不用封装，也不用分帧，
                    但是他确实要返回值
*/
                var asyncOperation = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(resourcePath);
                asyncOperation.Completed += handle =>
                   {

                       if (handle.Status == AsyncOperationStatus.Succeeded)
                       {
                           if (handle.Result == null)
                           {
                               SGF.Debuger.LogError($"LoadResourceUniRefAsync() load path: {resourcePath} error!!!");
                               forCbCount?.Invoke(resourcePath, null);
                           }
                           else
                           {
                               //if (resourcePath.Contains("config/skill") && !resourcePath.EndsWith(".lua"))//这里筛选是战斗配置
                               //{
                               //    CacheBattleConfig(resourcePath, handle.Result);
                               //}
                               //else
                               //{
                               s_CfgResourceJsonAssetCache[resourcePath] = handle.Result;
                               //}
                               forCbCount?.Invoke(resourcePath, handle.Result);
                           }
                       }
                       else
                       {
                           forCbCount?.Invoke(resourcePath, null);
                       }
                   };
                /*通常情况下Unity Addressable会自动处理取消订阅和资源的清理。这意味着，当异步操作完成后，Unity Addressable会自动取消对该回调函数的引用，以及释放相关资源，从而防止内存泄漏。局部方法会被释放的同时，他的订阅list也会空，所以左值自然会被释放，并且addressable也会自己处理*/
            }
        }
        //上来讲所有战斗数据反序列化 缓存 防止卡顿
        private void CacheBattleConfig(string path, TextAsset textAsset)
        {
            try
            {
                object data = null;
                if (!s_JsonAssetCacheDic.ContainsKey(path))
                {
                    if (path.Contains("buff"))
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<BuffJson>(textAsset.text);
                    }
                    else if (path.Contains("bullet"))
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<BulletJson>(textAsset.text);
                    }
                    else if (path.Contains("passive"))
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<PassiveJson>(textAsset.text);
                    }
                    else if (path.Contains("fxdetail"))
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<FXDetailJson>(textAsset.text);
                    }
                    else if (path.Contains("skill"))
                    {
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<SkillJson>(textAsset.text);
                    }
                    if (data != null)
                    {
                        s_JsonAssetCacheDic.Add(path, data);
                        Addressables.Release(textAsset);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }

        }

        /// <summary>
        /// 1,TimeLine必须准备好资源
        /// 2,Timeline不必缓存
        /// 3,Timeline用Addressable层次的资源跟预加载无关
        /// 4,仅限于动画，因为其他角色mesh一定要预先加载
        /// 5,注意其他类型的要通过预加载噢！
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadAssetSyncForTimeline<T>(string path) where T : Motion
        {
            if (string.IsNullOrEmpty(path))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }
            path = path.ToLower();
            var asyncOperation = Addressables.LoadAssetAsync<T>(path);
            return asyncOperation.WaitForCompletion() as T;
        }

        /// <summary>
        /// 统一入口只有一个，并且唯一只调用我
        /// 所有unityaddressable加载的东西都是unityengine.object,资源管理器，资源都是unity封装的unityengine.object
        /// 我给的应该全是引用，我不应在帮你new，因为我不知道你的生命周期，你自己释放
        /// [我持有的是唯一引用比如sprite，TextAsset这一类资源有一定特点是一个修改全部修改]如果重复跟我要你一定不关心顺序
        /// [另一类我持有的是perfab，你需要自己new，这一类多个，单独修改互相不影响]限制了你要自己new所以回调你自己处理顺序，自己释放
        /// [我这里全是引用，你需要new的东西比如gob，你都当成同步处理，我内部首次无法确认顺序，不想加lock，所以你实例化后当成同步处理]
        /// 我相当于不提供addressable里面的inst的那个方法，就要基于外部外部自己释放，我有池我也不知道你啥时候释放啊
        /// [使用细则：需要new的目前只有gameobject 遍历之类的放在回调之后，比如先申请资源异步回调处理gob的实例化然后自己处理：sprite引用类型不用关心]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourcePath"></param>
        /// <param name="onComplete"></param>
        public void LoadResourceUniRefAsync<T>(string resourcePath, System.Action<T/*Object*/> onComplete, string subSpriteName = "") where T : Object
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return;
            }

            resourcePath = resourcePath.Replace("Assets/Res/", string.Empty);

            resourcePath = resourcePath.ToLower();

            if (!string.IsNullOrEmpty(subSpriteName))
            {
                resourcePath += $"[{subSpriteName}]";
            }
            //都没有则落在low
            ResourcePriority priority = ResourcePriority.Low;


            bool pathExist = false;


            //SGF.Debuger.Log($"AssetDatabase 资源加载 resourcePath={resourcePath}");

            foreach (var item in highPathes)
            {
                if (resourcePath.EachCharCompare(item.ToLower()))
                {
                    priority = ResourcePriority.High;
                    pathExist = true;
                    break;
                }
            }

            if (!pathExist)
            {
                foreach (var item in middlePathes)
                {
                    if (resourcePath.EachCharCompare(item.ToLower()))
                    {
                        priority = ResourcePriority.Middle;
                        pathExist = true;
                        break;
                    }
                }
            }

            if (!pathExist)
            {
                foreach (var item in lowPathes)
                {
                    if (resourcePath.EachCharCompare(item.ToLower()))
                    {
                        priority = ResourcePriority.Low;
                        pathExist = true;
                        break;
                    }
                }
            }

            if (!pathExist)
            {

                Debug.Log("高中低都没检测到，默认低优先级，这符合逻辑:" + resourcePath);
            }

            //【这里的东西，可以是唯一，也可能是创建并且发生很多实例，依据具体需求而拓展另一个选择维度；然后用池，缓存，最后释放时候清理】【依据具体情况选择shareResource里面存储的是，引用还是值】
            //调用Addressable是不可能因为字典改不了
            //泛型完毕了，处理点额外的texture2D 到 sprite
            _LoadResourceAsync/*<T>*/(resourcePath, priority, /*onComplete 我先处理下在弹出去*/(Object @object, AsyncOperationStatus aos) =>
            {
                T v = @object as T;
                onComplete?.Invoke(v);
                {
                    /*  //cache存储的不是转好的
                      if (typeof(T).Equals(typeof(UnityEngine.Sprite)))
                      {
                          //意味着Gob实际记录是1对多的，同时也意味着删除business层跟我没关系，我删除也不影响business
                          //内部缓存应该也是sprite了，因为是调用机器方法他new我不new
                          *//*Texture2D img = (Texture2D)@object;
                          Sprite sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), Vector2.one * 0.5f);*//*

                          onComplete?.Invoke(@object as T);
                      }
                      else if (typeof(T).Equals(typeof(UnityEngine.GameObject)))
                      {
                          *//*//Transform resides in a Prefab asset and cannot be set to prevent data corruption
                          //不允许你直接调用  Gameobject
                          //Gob也是perfab这种链接内存的方式被用作了perfab上Gob需要new出来
                          //可以用池来处理确实
                          GameObject gob = (GameObject)@object;
                          GameObject isntGob = MonoHelper.Instantiate<GameObject>(gob);
                          onComplete?.Invoke(isntGob as T);*//*
                          onComplete?.Invoke((T)@object);
                      }
                      else if (typeof(T).Equals(typeof(UnityEngine.TextAsset)))
                      {
                          onComplete?.Invoke((T)@object);
                      }
                      else
                      {
                          //gameobject textasset
                          //资源引用是1对1的
                          //Debug.LogError(@object.GetType().Name + "_" + typeof(T).Name);
                          onComplete?.Invoke((T)@object);
                      }*/
                }
            } /*T.GetType()*/
            , typeof(T));
        }

        /// <summary>
        /// 每帧多个请求是可能的，因为代码时间颗粒度更细化.
        /// 多帧解决一个请求也可能.
        /// 本质上是分帧加载所以就是时间分摊压力，所以时间解本函数调用频度虽不定忽高忽低，但高并发时可以理解成颗粒度最细致的.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="priority"></param>
        /// <param name="onComplete"></param>
        private void _LoadResourceAsync/*<T>*/(string resourcePath, ResourcePriority priority, System.Action</*T*/Object, AsyncOperationStatus> onComplete, Type tp)/* where T : Object*/
        {
            //缓存了一堆需求
            //高优先级已经有了两个containKey，低级优先多了两个（等于0个）本质也是两个，中优先也应该两个
            ResourceRequest/*<Object>*/ request;
            switch (priority)
            {
                //需求包分发
                //这里理解成，多次呗调用构成需求组
                case ResourcePriority.Low://异步加载

                    LinkedListNode<System.WeakReference<SharedResource>> node;
                    SharedResource sR;
                    if (s_LowResourceCache.ContainsKey(resourcePath) && s_LowResourceCache.TryGetValue(resourcePath, out node) && node.Value.TryGetTarget(out sR) && sR != null)
                    {
                        // 如果缓存中已经存在了该资源，更新访问时间并增加引用计数
                        sR.lastAccessTime = UnityEngine.Time.time;
                        //高级引用计数
                        sR.ActiveAddReferenceCount();
                        onComplete?.Invoke(sR.resource, AsyncOperationStatus.Succeeded);
                        break;
                    }
                    else
                    {
                        request = new ResourceRequest(resourcePath, onComplete, priority, tp);/**///ResourceRequest<Object>.Create(resourcePath, onComplete, priority);

                        //这里结构只能Obj
                        m_LowPriorityQueue.Enqueue(request);

                        //清理对应类的计数，而并非Queue引用，针对异步记数而设计
                        m_LowPriorityQueue.ResetLoadedCount();
                        break;
                    }



                    break;
                case ResourcePriority.Middle://同步加载
                                             //问题3级问题是，如果重复的加入到Update中会负载均衡和并发限制。我应该拦截
                                             //问题2级是思路问题：这里高优先级是一定缓存所以加载会很快；执行上首先：111能同步返回就返回，【也就意味着先申请的甚至高优先的不一定优先返回】一切基于先有缓存就优先返回（想一想饭店如何上菜）真实不占队列也 不增加无效类型缓存，负载，均衡压力--然后按照优先级；222如果都没有高级从【现象上肯定】第一个返回  ；333中级和低级用不同的速度协同多线程，不同的机制加载，但是有自己独立的负载均衡并发控制，中级比低级优先检测进入队列，中级比低级优先update执行，中级比低级控制更细致



                    SharedResource sR1;
                    if (s_MiddleResourceCache.ContainsKey(resourcePath) && s_MiddleResourceCache.TryGetValue(resourcePath, out sR1) && sR1 != null)
                    {
                        // 如果缓存中已经存在了该资源，更新访问时间并增加引用计数
                        sR1.lastAccessTime = UnityEngine.Time.time;
                        //高级引用计数
                        sR1.ActiveAddReferenceCount();
                        onComplete?.Invoke(sR1.resource, AsyncOperationStatus.Succeeded);
                        break;
                    }
                    else
                    {
                        request = new ResourceRequest(resourcePath, onComplete, priority, tp);
                        m_NormalPriorityQueue.Enqueue(request);
                        m_NormalPriorityQueue.ResetLoadedCount();//低级有，中级协同，不要有不然压力会很大？

                        break;
                    }



                    break;
                case ResourcePriority.High:
                    SharedResource sR2;
                    //根据预加载几乎都可以直接拿到，直接处理
                    if (s_HighResourceCache.ContainsKey(resourcePath) && s_HighResourceCache.TryGetValue(resourcePath, out sR2) && sR2 != null)
                    {
                        // 如果缓存中已经存在了该资源，更新访问时间并增加引用计数
                        sR2.lastAccessTime = UnityEngine.Time.time;
                        //高级引用计数
                        sR2.ActiveAddReferenceCount();
                        onComplete?.Invoke(sR2.resource, AsyncOperationStatus.Succeeded);
                        break;
                    }
                    else
                    {



                        //Debug.LogWarning("高级资源必须，提前预先加载好，如是预加载逻辑可忽略本条");//添加一个bool来控制预先加载逻辑End的阶段。
                        //Debug.Log("AssetDatabase path：>" + resourcePath);
                        TryLoadAsync(resourcePath, priority, onComplete, tp);
                        break;
                    }
                    break;
                default:
                    break;
            }
        }


        #region 配置类型 ：属于高优先级:里的特殊 1，提前全部加载全部卸载；2，懒加载懒卸载

        //[Obsolete]//标记该方法已弃用
        /// <summary>
        /// 弃用弃用弃用弃用弃用弃用
        /// 获得 配置的 接口， 对于配置而言, 所有的 配置应该用的是同一份内存拷贝.
        /// 所以 对于加载的 json 的 TextAsset, 加载完成后 既可以释放掉.
        /// 同时, 在本地 存储一份 对应 配置的 T类型的 实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        //public T LoadConfig<T>(string path) where T : class
        //{
        //    var textAsset = LoadTxtAssetSync(path);
        //    if (textAsset != null)
        //    {
        //        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(textAsset.text);
        //    }

        //    // 不应该有多线程调用的才对, 但是 安卓机上 有 add key 重复的报错
        //    // 先用同步, 后续 都改成异步接口. 跟 博哥的 异步加载 流程 放在一起
        //    var jsonAsset = LoadAssetSync<TextAsset>(path);
        //    // 说明加载不到资源, 此时 直接存储null 进入 jsonCache
        //    s_CfgResourceJsonAssetCache.Add(path, jsonAsset);

        //    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(textAsset.text);
        //}

        //[Obsolete]//标记该方法已弃用
        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        ///    /// TODO：先全加载，之后根据不要的，根据角色动态需求再拆分
        /// 根据职业和等级
        /// 算了按需加载
        /// 因为【配置表】 为了统一，为了同步，也不必统计，容易嵌套异步所以保留同步，被动式
        /// 所以配置表用同步接口
        /// 【LuaUICell】先不改了不统计lua，一个是lua继承c#的框架结构将会被，lua统计这件事情所改变，因为异步不遵循mono生命周期了要自己实现。
        /// <returns></returns>
        //public T LoadAssetSync<T>(string path) where T : Object
        //{
        //    if (string.IsNullOrEmpty(path))
        //    {
        //        StarDebug.LogError("path is IsNullOrEmpty");
        //        return null;
        //    }
        //    path = path.ToLower();
        //    var asyncOperation = Addressables.LoadAssetAsync<T>(path);
        //    return asyncOperation.WaitForCompletion() as T;
        //}

        public UnityEngine.TextAsset LoadTxtAssetSync(string path)
        {
            //path = path.ToLower();    //modify by lijun08

            UnityEngine.TextAsset textAsset = null;
            using (UString.Block())
            {
                UString ustrPath = path;
                path = ustrPath.ToLower();
            }
            if (s_CfgResourceJsonAssetCache.TryGetValue(path, out textAsset))
            {
            }
            return textAsset;
        }
        public void ReleaseTextAssetCache(string path)
        {
            path = path.ToLower();
            UnityEngine.TextAsset textAsset = null;
            if (s_CfgResourceJsonAssetCache.TryGetValue(path, out textAsset))
            {
                Addressables.Release(textAsset);
                s_CfgResourceJsonAssetCache.Remove(path);
            }
        }
        public void ReleaseAllLuaConfig()
        {
            UnityEngine.TextAsset textAsset = null;
            List<string> removeList = new List<string>();
            string luaPath = "LuaScripts/Common/Config";
            if (s_CfgResourceJsonAssetCache != null)
            {
                foreach (var item in s_CfgResourceJsonAssetCache)
                {
                    if (item.Key.Contains(luaPath.ToLower()))
                    {
                        removeList.Add(item.Key);
                    }
                }
            }
            for (int i = 0; i < removeList.Count; i++)
            {

                if (s_CfgResourceJsonAssetCache.TryGetValue(removeList[i], out textAsset))
                {
                    Addressables.Release(textAsset);
                    s_CfgResourceJsonAssetCache.Remove(removeList[i]);
                }
            }
            removeList.Clear();
        }
        /// <summary>
        /// 异步 加载 配置表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="ac"></param> <summary>
        public void LoadConfigAsync<T>(string path, Action<T> ac) where T : class
        {
            path = path.ToLower();
            object jsonData;
            if (s_JsonAssetCacheDic.TryGetValue(path, out jsonData))
            {
                ac?.Invoke(jsonData as T);
                return;
            }
            if (s_CfgResourceJsonAssetCache.TryGetValue(path, out var text))
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(text.text);
                ac?.Invoke(data);
                s_JsonAssetCacheDic.Add(path, data);
                ReleaseTextAssetCache(path);
                return;
            }

            // 异步加载配置, 配置表 json 的释放 交给 资源管理 统一释放
            LoadResourceUniRefAsync(path, (TextAsset jsonAsset) =>
            {

                if (jsonAsset != null)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonAsset.text);
                    ac?.Invoke(data);
                }
                else
                {
                    ac?.Invoke(null);
                }
            });
        }

        #endregion

        #region  递归加载资源
        private readonly string AssetsDefineCfgPath = "Config/AssetsDefine/AssetsDefine";
        private void LoadAssetsDefine()
        {
            var textasset = LoadTxtAssetSync(AssetsDefineCfgPath);
            if (textasset != null)
            {
                m_AssetsToDefine = MessagePack.MessagePackSerializer.Deserialize<AssetsToDefine>(textasset.bytes);
                ReleaseTextAssetCache(AssetsDefineCfgPath);
            }
        }

        public bool IsPathContains(string path, E_AssetType type)
        {
            if (m_AssetsToDefine == null)
            {
                LoadAssetsDefine();
            }
            bool isContains = true;
            if (m_AssetsToDefine != null)
            {
                path = path.ToLower();
                switch (type)
                {
                    case E_AssetType.Animation:
                        //isContains = AssetsToDefine.AnimationPathList.Contains(path);
                        isContains = m_AssetsToDefine.AnimationPathList.Contains(path);
                        break;
                    case E_AssetType.Effects:
                        //isContains = AssetsToDefine.EffectPathList.Contains(path);
                        isContains = m_AssetsToDefine.EffectPathList.Contains(path);
                        break;
                    case E_AssetType.Roles:
                        //isContains = AssetsToDefine.RolePathList.Contains(path);
                        isContains = m_AssetsToDefine.RolePathList.Contains(path);
                        break;
                    default:
                        break;
                }
            }
            return isContains;
        }

        /// <summary>
        /// 递归获取加载资源的路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetRecursionLoadAssetPath(string path, E_AssetType type)
        {
            return ResourcesUtli.GetReadPath(path, type, IsPathContains);
        }

        /// <summary>
        /// 递归异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">路径</param>
        /// <param name="type">资源类型</param>
        /// <param name="callBack">回调</param>
        public void RecursionLoadAsset<T>(string path, E_AssetType type, Action<T> callBack) where T : Object
        {
            string str = GetRecursionLoadAssetPath(path, type);
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            {
                callBack?.Invoke(null);
            }
            LoadResourceUniRefAsync<T>(str, callBack);
        }

        /// <summary>
        /// 递归异步加载预制体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public void RecursionLoadGameObject(string path, E_AssetType type, bool isPools, Action<GameObject> callBack)
        {
            string str = GetRecursionLoadAssetPath(path, type);
            if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
            {
                callBack?.Invoke(null);
            }
            if (isPools)
            {
                PopGameObject(str, callBack);
            }
            else
            {
                LoadResourceUniRefAsync<GameObject>(str, callBack);
            }
        }

        private Dictionary<int, GameObject> guidKey2Obj = new();



        private GameObject RecordGuidKey(int guidKey)
        {
            // 如果之前已经有 guidKey 对应的加载, 直接干掉
            RemoveTagGuidLoad(guidKey);

            // 生成一个 新的 guidKey 对应的 obj
            var obj = new GameObject();
            guidKey2Obj.Add(guidKey, obj);

            return obj;
        }

        /// <summary>
        /// 删除 guidKey 对应的 资源加载
        /// </summary>
        /// <param name="guidKey"></param>
        public void RemoveTagGuidLoad(int guidKey)
        {
            if (!guidKey2Obj.ContainsKey(guidKey))
            {
                return;
            }
            GameObject.DestroyImmediate(guidKey2Obj[guidKey]);
            guidKey2Obj.Remove(guidKey);
        }

        /// <summary>
        /// 加载 唯一key 为 tagGuidKey， 路径为 path 的资源, 会根据 tagGuidKey 保证加载的顺序
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <param name="isPools"></param>
        /// <param name="callBack"></param>
        /// <param name="tagGuidKey"></param>
        public void LoadTagGuidGameObject(string path, E_AssetType type, bool isPools, Action<GameObject> callBack, int tagGuidKey)
        {

            var obj = RecordGuidKey(tagGuidKey);

            RecursionLoadGameObject(path, type, isPools, (GameObject go) =>
            {
                // 异步加载被杀掉
                if (obj == null)
                {
                    callBack?.Invoke(null);
                    Service.Resource.ResourceFormalManager.Instance.PushGameObject(path, go);
                    return;
                }
                RemoveTagGuidLoad(tagGuidKey);
                callBack?.Invoke(go);
            });

        }

        #endregion

        #region 对象池异步加载预制体

        /// <summary>
        /// 从对象池加载GameObject
        /// 必须先取
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        public void PopGameObject(string path, System.Action<GameObject> callBack)
        {
            if (string.IsNullOrEmpty(path))
            {
                callBack?.Invoke(null);
                SGF.Debuger.LogWarning("path is IsNullOrEmpty");
                return;
            }
            if (GameobjectPools == null)
            {
                GameobjectPools = new Dictionary<string, GameObjectPool>();
            }

            path = path.ToLower();
            if (GameobjectPools.TryGetValue(path, out var pool))
            {
                callBack?.Invoke(pool.Pop());
            }
            else
            {
                LoadResourceUniRefAsync<GameObject>(path,
                (GameObject go) =>
                {
                    if (go != null)
                    {
                        if (!GameobjectPools.ContainsKey(path))
                        {
                            var gob = GameObject.Instantiate<GameObject>(go);
                            gob.transform.SetParent(ObjectPools.transform);
                            gob.transform.position = new Vector3(99999, 99999, 99999);
                            GameObjectPool objectPool = new(path, gob, ObjectPools.transform);
                            GameobjectPools.Add(path, objectPool);
                        }
                        else
                        {
                        }
                        callBack?.Invoke(GameobjectPools[path].Pop());
                    }
                });
            }
        }

        /// <summary>
        /// 回收GameObejct
        /// </summary>
        /// <param name="path"></param>
        /// <param name="go"></param>
        public void PushGameObject(string path, GameObject go, GameObjectPoolType gameObjectPoolType = GameObjectPoolType.ActiveType)
        {
            if (go == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(path))
            {
                path = path.ToLower();
            }
            if (GameobjectPools.TryGetValue(path, out var pool) && pool != null)
            {
                pool.Push(go, gameObjectPoolType);
            }
            else
            {
                GameObject.Destroy(go);
            }
        }

        #endregion

        // 异步加载图片资源【LUA用】
        public void LoadSpriteAtlasAsync(string path, string spriteName, System.Action<UnityEngine.Sprite> callBack, bool isPng = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                callBack?.Invoke(null);
                SGF.Debuger.LogWarning("ResourceFormalManager LoadSpriteAsync() path is IsNullOrEmpty");
                return;
            }
            path = path.ToLower();
            Action<Sprite> onComplete = (Sprite sprite) =>
            {
                callBack?.Invoke(sprite);

            };
            if (isPng)
            {
                spriteName = spriteName + ".png";
            }
            LoadResourceUniRefAsync(path, onComplete, spriteName);
        }

        // 异步加载图片资源【LUA用】
        public void LoadSpriteAsync(string path, System.Action<UnityEngine.Sprite> callBack, string iconExt = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                callBack?.Invoke(null);
                SGF.Debuger.LogWarning("ResourceFormalManager LoadSpriteAsync() path is IsNullOrEmpty");
                return;
            }
            path = path.ToLower();
            Action<UnityEngine.Sprite> onComplete = (UnityEngine.Sprite sprite) =>
            {
                callBack?.Invoke(sprite);
            };
            LoadResourceUniRefAsync(path, onComplete, iconExt);
        }

        // 异步加载图片资源【LUA用】
        public void LoadTextureAsync(string path, System.Action<UnityEngine.Texture2D> callBack)
        {
            if (string.IsNullOrEmpty(path))
            {
                callBack?.Invoke(null);
                SGF.Debuger.LogWarning("ResourceFormalManager LoadSpriteAsync() path is IsNullOrEmpty");
                return;
            }
            path = path.ToLower();
            Action<UnityEngine.Texture2D> onComplete = (UnityEngine.Texture2D texture) =>
            {
                callBack?.Invoke(texture);
            };

            LoadResourceUniRefAsync(path, onComplete);
        }


        //[Obsolete("请使用 LoadResourceUniRefAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 同步加载预制体
        /// 本质是Key，全部路径都要给
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public GameObject LoadGameObject(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                StarDebug.LogError("path is IsNullOrEmpty");
                return null;
            }

            path = path.ToLower();
            AsyncOperationHandle<GameObject> asyncOperation = Addressables.InstantiateAsync(path);
            return asyncOperation.WaitForCompletion();
        }

        /// <summary>
        /// 时间缓解器
        /// </summary>
        public void OnMyUpdate()
        {
            // 依次处理高、中、低优先级请求队列
            OnFrameExcuteData(m_NormalPriorityQueue);
            OnFrameExcuteData(m_LowPriorityQueue);
        }

        private void OnFrameExcuteData(ResourceReqLoadQueue queue)
        {
            if (queue.Count > 0)
            {
                //获取下一个请求，不从队列中取出
                ResourcePriority nextReq = queue.PeekResPri();
                switch (nextReq)
                {
                    case Resource.ResourcePriority.High:
                        //这里不存在高优先级的，高优先级不存在队列就是直接
                        //...
                        break;
                    case Resource.ResourcePriority.Middle:
                        //当前请求，小于最大允许请求
                        if (s_CurConcurrencyRequests_MID < S_MAX_CON_CURRENTRE_QUESTS_MID)
                        {
                            //当前请求，小于最大允许请求   并且  队列有请求
                            while (s_CurConcurrencyRequests_MID < S_MAX_CON_CURRENTRE_QUESTS_MID && queue.Count > 0)
                            {
                                // 取出一个请求
                                var req = queue.Dequeue();

                                //Debug.LogError("Request Middle:>" + req.ResourcePath);

                                SharedResource sR;
                                // 货物缓存直接给
                                if (s_MiddleResourceCache.TryGetValue(req.ResourcePath, out sR))
                                {
                                    // 如果缓存中已经存在了该资源，更新访问时间并增加引用计数
                                    sR.lastAccessTime = UnityEngine.Time.time;

                                    s_MiddleResourceCache[req.ResourcePath].ActiveAddReferenceCount();

                                    req.Callback?.Invoke(sR.resource, AsyncOperationStatus.Succeeded);

                                    //没有请求柜台排队人数为0，或者本帧【异步其他线程】负载了就算了
                                    if (queue.Count == 0 || queue.LoadedCount >= S_BATCHSIZE_MID)
                                    {
                                        break;
                                    }
                                }
                                else//派遣员工去取货
                                {
                                    // 开始异步加载，同时加载，客人阀门
                                    s_CurConcurrencyRequests_MID++;

                                    // 注意：使用 lambda 表达式来保存回调函数中间的参数，否则会出现参数不一致的情况，记得弱引应该也可以
                                    System.Action<Object, AsyncOperationStatus> assetCallback = (asset, aps) =>
                                    {
                                        if (aps == AsyncOperationStatus.Failed || aps == AsyncOperationStatus.None)
                                        {
                                            //有结果但失败了，员工回来可以干别的
                                            //不是一定要成功，员工不是就废了
                                            //是要修复资源但是，这个是美术策划没配置没下载下来的情况不用阻塞警告全部而是失败就报错失败，然后正常进行
                                            //Debug.Log("资源未加载成功或者没有此资源：>" + req.ResourcePath);
                                            req.Callback.Invoke(null, aps);
                                            s_CurConcurrencyRequests_MID--;
                                        }
                                        else if (aps == AsyncOperationStatus.Succeeded)
                                        {
                                            // 调用回调函数返回资源
                                            req.Callback.Invoke(asset, AsyncOperationStatus.Succeeded);

                                            // 异步加载完成，计数器减1，这里是协同单线程的异步，可以理解闭包
                                            s_CurConcurrencyRequests_MID--;
                                        }
                                    };
                                    //同步阻塞式加载
                                    //MonoHelper.StartCoroutine(CoroutineLoadResource(req.ResourcePath, assetCallback, req.Priority, req.type));
                                    //Debug.Log("AssetDatabase path：>" + req.ResourcePath);

                                    MiddleLoadResource(req.ResourcePath, assetCallback, req.Priority, req.type);
                                    break;
                                }
                            }
                        }
                        break;
                    case Resource.ResourcePriority.Low:
                        //当前请求，小于最大允许请求
                        if (s_CurrentConcurrentRequests_L < s_MaxConcurrentRequests_L)
                        {
                            //当前请求，小于最大允许请求   并且  队列有请求
                            while (s_CurrentConcurrentRequests_L < s_MaxConcurrentRequests_L && queue.Count > 0)
                            {
                                // 取出一个请求
                                var req = queue.Dequeue();

                                //Debug.LogError("Request Low:>" + req.ResourcePath);

                                SharedResource sR;
                                LinkedListNode<System.WeakReference<SharedResource>> node;
                                // 货物缓存直接给，这里是有键未必有值：key曾经建立过 && 但是值可能GC丢掉
                                // 都加载完毕后基本都是在这里进行资源拿取
                                // 是否建立过，linklist存储是否存在
                                if (s_LowResourceCache.ContainsKey(req.ResourcePath) && s_LowResourceCache.TryGetValue(req.ResourcePath, out node))//这里凭空多两个其实可以没有
                                {
                                    #region [弱引用]更新
                                    //弱引用是否失效GC
                                    if (node.Value.TryGetTarget(out sR))
                                    {

                                        //==============更新时间，到手资源，删除链表是0(1)=================
                                        // 如果缓存中已经存在了该资源，更新访问时间并增加引用计数
                                        sR.lastAccessTime = UnityEngine.Time.time;
                                        m_LowPriorityResourcesLinked.Remove(node);//O(n) 本质是循环+equal 我用while 也是O（n）所以也用不到Key
                                        m_LowPriorityResourcesLinked.AddLast(node);//O(1)调换顺序存在的新呗使用的放在尾巴。
                                                                                   //=================================================================
                                                                                   //低级弱引用不会处理引用计数的,只有时间，全凭时间
                                                                                   //s_LowResourceCache[resourcePath].referenceCount++;
                                        queue.AddLoadedCount();


                                        req.Callback?.Invoke(sR.resource, AsyncOperationStatus.Succeeded);

                                        //没有请求柜台排队人数为0，或者本帧【异步其他线程】负载了就算了
                                        if (queue.Count == 0 || queue.LoadedCount >= s_BatchSize_L)
                                        {
                                            break;//负载这块
                                                  //          1，如果我想要优化成：排序发送，优先发送需要{ 去取货的}需求：不可能，取得看 有没有缓存才知道
                                                  //          2，当没需求后，同时也没触发并发峰值的时候，他负载量级取决于1帧到是否遇到重复
                                                  //          一帧的处理交给unity动态帧率取得处理，如果想一帧最大加载两个不如调整帧率和负载率和处理负载刷新频率


                                        }
                                    }
                                    else // 弱引用已经失效，从缓存池中移除 ： 没加载到资源（因为被GC）流程上属于空跑一次不影响，更新本次信息，重新循环
                                    {
                                        //此处不必break切断循环影响流程，并且不会引发并处理Count，和负载等复杂问题，如需更健壮可以考虑
                                        m_LowPriorityResourcesLinked.Remove(node);
                                        s_LowResourceCache.Remove(req.ResourcePath);
                                        //处理完成存储结构之后立刻派遣去拿。
                                        DispatchEmployeesToPickGoods(req);
                                    }


                                    #endregion

                                }
                                else//派遣员工去取货
                                {
                                    DispatchEmployeesToPickGoods(req);
                                }
                            }
                        }
                        break;
                }
            }

        }
        /// <summary>
        /// 2个低优先级
        /// </summary>
        /// <param name="req"></param>
        private void DispatchEmployeesToPickGoods(ResourceRequest req)
        {
            // 开始异步加载，同时加载，客人阀门
            s_CurrentConcurrentRequests_L++;

            // 注意：使用 lambda 表达式来保存回调函数中间的参数，否则会出现参数不一致的情况，记得弱引应该也可以
            System.Action<Object, AsyncOperationStatus> assetCallback = (asset, aos) =>
            {
                if (aos == AsyncOperationStatus.None || aos == AsyncOperationStatus.Failed

                )
                {
                    // 调用回调函数返回资源
                    req.Callback.Invoke(null, aos);
                }
                else
                {
                    // 调用回调函数返回资源
                    req.Callback.Invoke(asset, aos);

                }
                // 异步加载完成，计数器减1，这里是协同单线程的异步，可以理解闭包
                s_CurrentConcurrentRequests_L--;
            };
            //2这里是同步吧？await 导致等待 comp所以同步了
            TryLoadAsync(req.ResourcePath, req.Priority, assetCallback, req.type);

        }


        /// <summary>
        /// 加载完成的回调
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="onComplete"></param>
        /// <param name="resourcePath"></param>
        /// <param name="priority"></param>
        private void LoadResource<T>(AsyncOperationHandle<T> handle, System.Action<T, AsyncOperationStatus> onComplete, string resourcePath, ResourcePriority priority) where T : Object
        {
            if (handle.Status == AsyncOperationStatus.Failed || handle.Status == AsyncOperationStatus.None)
            {
                Debug.LogError($"[ResourceManagerExtend] load path {resourcePath} error");
                onComplete?.Invoke(null, handle.Status);
                return;
            }
            //我资源的缓存，向addressable请求状态，用我的资源缓存
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                uint version = CURRENT_VERSION;
                switch (priority)
                {
                    case ResourcePriority.Low:
                        #region [弱引用]新增
                        //Addressable异步提交很多需求，addressable最后他只能给你一份引用，这里可以拦截相同的值
                        if (!s_LowResourceCache.ContainsKey(resourcePath))
                        {
                            //没有加过，避免重复增加
                            //addressable有，我没有，我缓存
                            var sharedReference = new System.WeakReference<SharedResource/*<T>*/>(new SharedResource() { resource = handle.Result, pathKey = resourcePath/*, version = version*/, lastAccessTime = UnityEngine.Time.time, referenceCount = 1, resourcePriority = priority });

                            var node = m_LowPriorityResourcesLinked.AddLast(sharedReference);
                            s_LowResourceCache.Add(resourcePath, node);

                            onComplete?.Invoke(handle.Result, handle.Status);

                            CheckLowerResourceCache(UnityEngine.Time.time);

                            //Addressables.Release(handle);
                        }
                        else if (s_LowResourceCache[resourcePath] == null)
                        {
                            //丢掉
                            //addressable有，我没有，我缓存
                            var sharedReference = new System.WeakReference<SharedResource/*<T>*/>(new SharedResource() { resource = handle.Result, pathKey = resourcePath/*, version = version*/, lastAccessTime = UnityEngine.Time.time, referenceCount = 1, resourcePriority = priority });
                            //var node = m_LowPriorityResourcesLinked.First;//.AddLast(sharedReference);//保持上一个是空
                            //while (node != null)
                            //{
                            //    //遍历为空的多了
                            //    //失去了内容不知道key
                            //    //没存key
                            //    node = node.Next;
                            //}
                            //空的那个节点只能下次排查remove，也没用了，也寻找不到痕迹，新建一个触发关联
                            var node = m_LowPriorityResourcesLinked.AddLast(sharedReference);
                            s_LowResourceCache[resourcePath] = node;

                            onComplete?.Invoke(handle.Result, handle.Status);

                            CheckLowerResourceCache(UnityEngine.Time.time);

                            //Addressables.Release(handle);
                        }
                        else
                        {
                            //Debug.Log("发现加载重复资源" + resourcePath + "你要我一定返回给你");
                            //外围反复调用不会进入，这里一定是同帧多个需求了，不会调用资源检测
                            //addressable有，我有，我不缓存，addressable不会重复，有唯一资源引用，还是会给我同一个他不会生成多个的，我用这个你调用我有我也给你
                            onComplete?.Invoke(handle.Result, handle.Status);

                            //Addressables.Release(handle);

                        }

                        #endregion

                        break;
                    case ResourcePriority.Middle:
                        //s_MiddleResourceCache.Add(resourcePath, new SharedResource() { resource = handle.Result, version = version, lastAccessTime = UnityEngine.Time.time, referenceCount = 1 });
                        //CheckMiddleResourceCache();
                        break;
                    case ResourcePriority.High:


                        //Addressable异步提交很多需求，addressable最后他只能给你一份引用，这里可以拦截相同的值
                        if (!s_HighResourceCache.ContainsKey(resourcePath))
                        {
                            //没有加过，避免重复增加
                            s_HighResourceCache.Add(resourcePath, new SharedResource() { resource = handle.Result/*, version = version*/, /*pathKey = resourcePath,*/ lastAccessTime = UnityEngine.Time.time, referenceCount = 1, resourcePriority = priority });
                            onComplete?.Invoke(handle.Result, handle.Status);//有时间不会在之前清理掉的

                            //Addressables.Release(handle);

                        }
                        else if (s_HighResourceCache[resourcePath] == null)
                        {
                            //丢掉
                            s_HighResourceCache[resourcePath] = new SharedResource() { resource = handle.Result/*, version = version*/, /*pathKey = resourcePath,*/ lastAccessTime = UnityEngine.Time.time, referenceCount = 1, resourcePriority = priority };
                            onComplete?.Invoke(handle.Result, handle.Status);//有时间不会在之前清理掉的
                            //Addressables.Release(handle);
                        }
                        else
                        {
                            //Debug.Log("发现加载重复资源" + resourcePath + "你要我一定返回给你");
                            //外围反复调用不会进入，这里一定是同帧多个需求了，不会调用资源检测
                            //addressable有，我有，我不缓存，addressable不会重复，有唯一资源引用，还是会给我同一个他不会生成多个的，我用这个你调用我有我也给你
                            onComplete?.Invoke(handle.Result, handle.Status);
                            //Addressables.Release(handle);
                        }


                        break;
                    default:
                        break;
                }

            }
        }

        #region 屏蔽代码
        /// <summary>
        /// 中级用协同
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="onComplete"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        private IEnumerator CoroutineLoadResource(string resourcePath, System.Action<Object, AsyncOperationStatus> onComplete, ResourcePriority priority, Type type)
        {
            AsyncOperationHandle handle;
            if (type.Equals(typeof(UnityEngine.Sprite)))
            {
                handle = Addressables.LoadAssetAsync<Sprite>(resourcePath);
            }
            else if (type.Equals(typeof(SpriteAtlas)))
            {
                handle = Addressables.LoadAssetAsync<SpriteAtlas>(resourcePath);
            }
            else if (type.Equals(typeof(UnityEngine.GameObject)))
            {
                handle = Addressables.LoadAssetAsync<GameObject>(resourcePath);
            }
            else
            {
                handle = Addressables.LoadAssetAsync<Object>(resourcePath);
            }


            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                uint version = CURRENT_VERSION;
                if (priority == ResourcePriority.Middle)
                {
                    if (!s_MiddleResourceCache.ContainsKey(resourcePath) || s_MiddleResourceCache[resourcePath] == null)
                    {
                        s_MiddleResourceCache[resourcePath] = new SharedResource() { resource = (Object)handle.Result, lastAccessTime = UnityEngine.Time.time, referenceCount = 1, resourcePriority = priority };
                    }
                    onComplete?.Invoke((Object)handle.Result, handle.Status);
                    CheckMiddleResourceCache(UnityEngine.Time.time);
                }
                else
                {
                    onComplete?.Invoke(null, handle.Status);
                    Debug.Log("此处不允许其他级别来调用携程,多携程并发难以控制");
                }
            }
            else
            {
                onComplete?.Invoke(null, handle.Status);
            }
        }
        #endregion

        private void MiddleLoadResource(string resourcePath, System.Action<Object, AsyncOperationStatus> onComplete, ResourcePriority priority, Type type)
        {
            AsyncOperationHandle handle;
            if (type.Equals(typeof(UnityEngine.Sprite)))
            {
                handle = Addressables.LoadAssetAsync<Sprite>(resourcePath);
            }
            else if (type.Equals(typeof(SpriteAtlas)))
            {
                handle = Addressables.LoadAssetAsync<SpriteAtlas>(resourcePath);
            }
            else if (type.Equals(typeof(UnityEngine.GameObject)))
            {
                handle = Addressables.LoadAssetAsync<GameObject>(resourcePath);
            }
            else
            {
                handle = Addressables.LoadAssetAsync<Object>(resourcePath);
            }

            handle.Completed += (opHandle) =>
            {
                if (opHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    uint version = CURRENT_VERSION;
                    if (priority == ResourcePriority.Middle)
                    {
                        //Dictionary 的索引器 []
                        if (!s_MiddleResourceCache.ContainsKey(resourcePath) || s_MiddleResourceCache[resourcePath] == null)
                        {
                            s_MiddleResourceCache[resourcePath] = new SharedResource() { resource = (Object)opHandle.Result, lastAccessTime = UnityEngine.Time.time, referenceCount = 1, resourcePriority = priority };
                            onComplete?.Invoke((Object)opHandle.Result, opHandle.Status);
                            CheckMiddleResourceCache(UnityEngine.Time.time);
                        }
                        else
                        {
                            //Debug.Log("发现加载重复资源" + resourcePath + "你要我一定返回给你");
                            onComplete?.Invoke((Object)opHandle.Result, opHandle.Status);
                        }
                    }
                    else
                    {
                        onComplete?.Invoke(null, opHandle.Status);
                        Debug.Log("此处不允许其他级别来调用，多并发难以控制");
                    }
                }
                else
                {
                    onComplete?.Invoke(null, opHandle.Status);
                }
            };
        }

        public void TryLoadAsync(string resourcePath, ResourcePriority priority, System.Action</*T*/Object, AsyncOperationStatus> onComplete, Type tp)
        {

            if (tp.Equals(typeof(UnityEngine.Sprite)))
            {
                //Debug.LogWarning("高级资源必须，提前预先加载好，如是预加载逻辑可忽略本条");//添加一个bool来控制预先加载逻辑End的阶段。
                Addressables.LoadAssetAsync</*T addressable会帮我处理*/Sprite>(resourcePath).Completed += handle => LoadResource(handle, onComplete, resourcePath, priority);
            }
            else if (tp.Equals(typeof(UnityEngine.GameObject)))
            {
                //SGF.Debuger.LogError($"AssetDatabase 资源加载 resourcePath={resourcePath}");

                Addressables.LoadAssetAsync</*T addressable会帮我处理*/GameObject>(resourcePath).Completed += handle => LoadResource(handle, onComplete, resourcePath, priority);
            }
            else
            {
                Addressables.LoadAssetAsync</*T addressable会帮我处理*/Object>(resourcePath).Completed += handle => LoadResource(handle, onComplete, resourcePath, priority);
            }


        }


        #region 检测清理:兜底AA检测逻辑----频度:加载时 ----额度：时间 ---意义：封装减少大家不释放带来的问题。

        //UnityEngine.Profiling.Profiler.GetRuntimeMemorySize
        /// <summary>
        /// 【新场景】里面来调用，【游戏结束】里面调用
        /// 首先根据类型
        /// 【是否超过最大数量】，【是否超过时间没人用（引用频率低不代表当前没人用）】，
        /// 【是否相对最低引用（是大量引用程度）】（首先依赖一点自己不能缓存）
        /// </summary>
        public void GCResourceCache(ResourcePriority resourcePriority)
        {
            switch (resourcePriority)
            {
                case ResourcePriority.Low:
                    //lower本身一层作为弱引用就随时可能被排除
                    //我这里更会然通过策略进行排除
                    //【是否超过Max时间没人用确实是废物】&&【没有引用】 => 【系统层次就不存在最大容量的概念他随时可能GC我控制的是超过就干掉时间】 && 【使用时间（低级保采用时间）】
                    //他不扛并发，最大内存利用率，引用不计数就交给数值的所以他自身清除不具备计数，只看时间
                    CheckLowerResourceCache(0);
                    break;
                case ResourcePriority.Middle:
                    //中级加载比较即时，相对很少去清理，加载有队列来处理
                    //中级思路强引用：他不会被卸载，他是强引用，只有我去卸载
                    //首先要求这里全部由我来获取资源
                    //【是否超过时间没人用确实是废物】&&【没有引用】 => 【中级是最大容量缓存策略（排除额外）】&&【超出的排除最低引用（中级保重复率）】  池具备扛冲突性，支持低延迟提高可控制力度，有卡了之后就不允许在卡了的拓展峰值的作用，因为他本身是协同所以本身也扛不住并发之后有可能拓展这个池的大小
                    //例如：并发20个，我就会创建20个强引用不会被删除，下次并发30个我知识拓展池这次卡10个的量
                    CheckMiddleResourceCache(0);
                    break;
                case ResourcePriority.High:
                    //高级不必处理,因为高级的设定就是一定会用到的。
                    //【低级是弱引用方式】queue要处理不同东西path就是不同的优先级外面不需要关心选择
                    //本质是unity
                    break;
                default:
                    break;
            }




            //GC卡顿，并且恢复不了
            //GC2();
        }



        //新增加检查，可能反复检查还好，反复GC时间长容易并发问题，容易吧新增的东西给干掉有时间不可能
        //所有应该公用一个锁。检查啥时候都行，不可能都一起触发遍历和GC
        public bool isInAllChecker = false;

        /// <summary>
        /// 空间不足一定释放，然后就是引用，最近有用的
        /// 新增资源时调用(调用约束Done） && 空间不够（系数）|不够空间时才进行时间释放，防止Gc，频繁|（暂时没其他需求）
        /*ClearReferenceCount-单体GC-*/ /*不管析构- */
        //一定要清理到目标值
        /// </summary>
        private /*static*/ void CheckMiddleResourceCache(float _checkTime)
        {
            //！=0 就是正常清理
            if (_checkTime != 0f)
            {
                if (_checkTime - lastCheckTime < CHECK_CD)
                {
                    return;
                }
                else
                {
                    lastCheckTime = _checkTime;
                }

            }


            if (isInAllChecker == true)
            {
                return;
            }
            else
            {
                isInAllChecker = true;
            }


            bool isMemoryToMuchtTriGC = false;
            //调用时机确定，&& 空间不够释放到空间要求 &&----------------------------------------------
            if (s_MiddleResourceCache.Count > (MIDDLD_MAX_CACHE_SIZE * 1.5))//1.5倍拓容
            {
                //大于1.5倍立刻清理到1
                //---流程继续走下去
                //大于3
                if (s_MiddleResourceCache.Count > (MIDDLD_MAX_CACHE_SIZE * 3))//并发和资源太大了
                {
                    isMemoryToMuchtTriGC = true;
                }
                //无法删除怎么办，会越来越多，本质addressable不释放也没问题，但是有越多的途径释放就利于管理，但绝对不能一直检测释放，甚至陷入释放循环
            }
            else
            {
                //少于要求部管。大于要求小于1.5冗余倍数部管
                //回归到1good，太被动不好，适合大量并发不好，运算排序浪费不好
                isInAllChecker = false;
                return;
            }
            //排除空数据
            List<string> removeNullKey = new();
            if (s_MiddleResourceCache.Count > 0)
            {
                foreach (var item in s_MiddleResourceCache)
                {
                    if (item.Value == null)
                    {
                        removeNullKey.Add(item.Key);
                    }
                }
                for (int i = 0; i < removeNullKey.Count; i++)
                {
                    s_MiddleResourceCache.Remove(removeNullKey[i]);
                }
            }
            removeNullKey.Clear();
            removeNullKey = null;


            var refSort = new List<KeyValuePair<string, SharedResource>>(s_MiddleResourceCache);
            refSort.Sort((a, b) => a.Value.referenceCount.CompareTo(b.Value.referenceCount));//小到大.排序是为了删除，添加时候自然有时间顺序，但引用是第一优先级

            List<string> refUpAndTimeNoUse = new();

            //引用数量排序--->依次加入时间不满足的
            foreach (var resource in refSort)
            {
                //引用是0一定删除 resource.Value.referenceCount <= 0 调用就会触发删除，不调用也不会减少
                //引用重要，还是时间重要不是并行逻辑  其实一样重要；
                //就算数据不会删除，本质还是要缓存避免io还是不要轻易删除 “&& 的话就相当于要求别人必须释放”“ || 引用最少得，按照时间删除”
                //虽然标记引用 || 时间过时 都应该释放   ； 外面可能标记，标记会直接就会走释放； 过时的文件直接清理
                if (MIDDLD_CHECKER_TIME_SEC < UnityEngine.Time.time - resource.Value.lastAccessTime/* || resource.Value.referenceCount <= 0*/) // 检查引用计数，如果为0则清除资源缓存)
                {
                    refUpAndTimeNoUse.Add(resource.Key);
                }
            }

            int removeCount = s_MiddleResourceCache.Count - MIDDLD_MAX_CACHE_SIZE;
            if (removeCount > refUpAndTimeNoUse.Count)
            {

                /*先干掉所有不符合时间
                    middle再干掉引用少的*/
                for (int i = 0; i < refUpAndTimeNoUse.Count; i++)
                {
                    s_MiddleResourceCache[refUpAndTimeNoUse[i]].ActiveClearReferenceCount();
                    s_MiddleResourceCache.Remove(refUpAndTimeNoUse[i]);
                }
                removeCount = s_MiddleResourceCache.Count - MIDDLD_MAX_CACHE_SIZE;
                //s_MiddleResourceCache 删除了 refSort 没删，维护（没有数据contunue ）不如依次oN快排呢
                refSort = new List<KeyValuePair<string, SharedResource>>(s_MiddleResourceCache);
                refSort.Sort((a, b) => a.Value.referenceCount.CompareTo(b.Value.referenceCount));
                for (int i = 0; i < removeCount; i++)
                {
                    refSort[i].Value.ActiveClearReferenceCount();
                    //一定要清理到目标值，里面内容都是不符合时间的（近期没调用的），优先引用数量少的
                    s_MiddleResourceCache.Remove(refSort[i].Key);
                }
            }
            else
            {
                //干掉middle其中   removeKeys一部分（removeCount）优先引用少的即可
                for (int i = 0; i < removeCount; i++)
                {
                    //立刻清理到Addressable，就是不容忍的删除，并且和Address同步
                    s_MiddleResourceCache[refUpAndTimeNoUse[i]].ActiveClearReferenceCount();
                    //一定要清理到目标值，里面内容都是不符合时间的（近期没调用的），优先引用数量少的
                    s_MiddleResourceCache.Remove(refUpAndTimeNoUse[i]);
                }
            }

            if (isMemoryToMuchtTriGC)
            {
                GC2();
                isMemoryToMuchtTriGC = false;
            }
            isInAllChecker = false;
            //硬性处理-------------------------------------------------------------------
        }


        /// <summary>
        /// 空间不足一定释放1.1-1，（然后就是引用-没有），最近没用的，[处理空的]
        /// 弱引用系统会处理，但遇到限制瓶颈也要直接处理
        /// 新增资源成功时调用(调用约束Done） && 空间不够（系数）|不够空间时才进行时间释放，防止Gc，频繁|（暂时没其他需求）
        /*ClearReferenceCount-单体GC-*/ /*不管析构- */
        /// </summary>
        /// 
        private void CheckLowerResourceCache(float _checkTime)
        {
            //！=0 就是正常清理
            if (_checkTime != 0f)
            {
                if (_checkTime - lastCheckTime < CHECK_CD)
                {
                    return;
                }
                else
                {
                    lastCheckTime = _checkTime;
                }

            }

            if (isInAllChecker == true)
            {
                return;
            }
            else
            {
                isInAllChecker = true;
            }
            int delCount = 0;
            //坚持1.2倍
            #region [弱引用]删除，没有查询
            //硬性处理----这里我也会帮忙处理空的问题--------------------------------------------------------------- /*因为都是遍历所以迭代器，和链表的时间复杂度都是O（n） 又设计到删除，链表更好 而且有顺序的你必须先处理链表*/ //那么循环出现错误严谨的说 应该是迭代器引发的，而不是“遍历”这个概念引发的对吧，，所以用迭代器做的东西都不应该在过程中进行删除，，所以链表他不基MaxCount或者说天然没这个问题，可以节省一次记录循环
            if (s_LowResourceCache.Count > (LOWER_MAX_CACHE_SIZE * 1.1))//1,低级底子量大，2，弱引用系统会自然释放
            {
                //大于1.5倍立刻清理到1
                //---流程继续走下去

            }
            else
            {
                isInAllChecker = false;
                return;
            }

            LinkedListNode<System.WeakReference<SharedResource>> node = m_LowPriorityResourcesLinked.First;
            LinkedListNode<System.WeakReference<SharedResource>> nodeCache;
            SharedResource sR;//共享左值

            //数量大于1倍，坚持处理掉到1倍
            //好处，永远获取头节点，while不访问关键，只对数据count处理
            while (node != null && s_LowResourceCache.Count > LOWER_MAX_CACHE_SIZE)
            {
                sR = null;
                if (node.Value.TryGetTarget(out sR))//找到
                {
                    //时间不满住就删除
                    //node 默认带时序的，操作前数据 = 老数据；
                    //1，不会记录自增不用计数，2，初始化是1也不会减少导致删除
                    //任意满足都要干掉，比如一致时间都是废弃时间就会删除到底；比如Count达到限定Size哪怕一个都满足时间也要删除
                    //虽然标记引用 || 时间过时 都应该释放   ； 外面可能标记，标记会直接就会走释放； 过时的文件直接清理
                    if (LOWER_MAX_SAVE_TIME < UnityEngine.Time.time - sR.lastAccessTime/* ||sR.referenceCount <= 0*/)//处理过时
                    {
                        sR.ActiveClearReferenceCount();
                        //本质Remove(Node)看起来更O(1)一点，本质都一样
                        //不符合时间的要删除，现在不会处理超过数量的删除，而是超过缓存时间的删除，超过缓存时间以上的都删除

                        nodeCache = node;
                        node = node.Next;

                        m_LowPriorityResourcesLinked.Remove(nodeCache);
                        s_LowResourceCache.Remove(sR.pathKey);//这个缓存确实有点意义，起码在这里能用，也只有低级会缓存占用内存，
                        delCount++;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
                else
                {
                    nodeCache = node;
                    node = node.Next;
                    //这里如果遇到【GC回收情况了】，数量减少自动处理，也会处理，丢了就删除
                    m_LowPriorityResourcesLinked.Remove(nodeCache);
                    //s_LowResourceCache.Remove(sR.pathKey);
                }
            }

            if (delCount >= LOWER_MAX_CACHE_SIZE * 0.2)
            {
                GC2();
            }
            #endregion
            isInAllChecker = false;
            //硬性处理-------------------------------------------------------------------
        }
        #endregion


        #region AA部分【间接标记】

        /// <summary>
        /// 平时是高级不会清，低级不依赖计数也完全不控制计数，中级低级都有时间控制
        /// 通常用于特殊时刻的资源引用计数增加
        /// </summary>
        /// <param name="resourcePath"></param>
        public void AddMiddleRef(string resourcePath)
        {
            if (s_MiddleResourceCache.TryGetValue(resourcePath, out SharedResource sharedResource))
            {
                // 增加引用计数
                sharedResource.ActiveAddReferenceCount();
            }
        }

        /// <summary>
        /// 如果程序员想间接调用我也可以减少引用计数
        /// </summary>
        /// <param name="resourcePath"></param>
        public void RemoveMiddleRef(string resourcePath)
        {
            if (s_MiddleResourceCache.TryGetValue(resourcePath, out SharedResource sharedResource))
            {
                // 减少引用计数
                if (sharedResource.ActiveDecrementReferenceCount())
                {
                    s_MiddleResourceCache.Remove(resourcePath);
                }

            }
        }

        #endregion


        #region AA部分 【强力清理AA】
        /// <summary>
        /// 勤快卸载接口
        /// </summary>
        /// <param name="containStr"></param>
        /// 
        public void ForceClearMiddleResourceCache(string containStr)
        {
            Debug.Log("----------------清理中级开始----------------");

            //梳理空数据
            List<string> removeNullKey = new();
            if (s_MiddleResourceCache.Count > 0)
            {
                foreach (var item in s_MiddleResourceCache)
                {
                    if (item.Value == null)
                    {
                        removeNullKey.Add(item.Key);
                    }
                }
                for (int i = 0; i < removeNullKey.Count; i++)
                {
                    s_MiddleResourceCache.Remove(removeNullKey[i]);
                }
            }
            removeNullKey.Clear();


            if (s_MiddleResourceCache.Count > 0)
            {
                foreach (var item in s_MiddleResourceCache)
                {
                    if (item.Key.Contains(containStr, StringComparison.OrdinalIgnoreCase))
                    {
                        removeNullKey.Add(item.Key);
                    }
                }
                for (int i = 0; i < removeNullKey.Count; i++)
                {

                    s_MiddleResourceCache[removeNullKey[i]].ActiveClearReferenceCount();
                    s_MiddleResourceCache.Remove(removeNullKey[i]);

                }
            }

            removeNullKey = null;
            GC4();
        }

        /// <summary>
        /// 高级中正常不删除是为了被动式
        /// 但是有一些资源需要，资源勤快全部准备，好有节点直接全部ab全部释放
        /// 勤快卸载
        /// </summary>
        public void ForceClearHighResourceCache(string containStr)
        {
            Debug.Log("----------------清理中级开始----------------");


            //梳理空数据
            List<string> removeNullKey = new();
            if (s_HighResourceCache.Count > 0)
            {
                foreach (var item in s_HighResourceCache)
                {
                    if (item.Value == null)
                    {
                        removeNullKey.Add(item.Key);
                    }
                }
                for (int i = 0; i < removeNullKey.Count; i++)
                {
                    s_HighResourceCache.Remove(removeNullKey[i]);
                }
            }
            removeNullKey.Clear();


            if (s_HighResourceCache.Count > 0)
            {
                foreach (var item in s_HighResourceCache)
                {
                    if (item.Key.Contains(containStr, StringComparison.Ordinal))
                    {
                        removeNullKey.Add(item.Key);
                    }
                }
                for (int i = 0; i < removeNullKey.Count; i++)
                {
                    s_HighResourceCache[removeNullKey[i]].ActiveClearReferenceCount();
                    s_HighResourceCache.Remove(removeNullKey[i]);

                }
            }

            removeNullKey = null;
            GC4();
        }


        public void ForceClearMiddleResourceCache()
        {
            Debug.Log("----------------清理中级开始----------------");

            bool isMemoryToMuchtTriGC = false;
            Debug.Log(s_MiddleResourceCache.Count + "中级数量");

            //梳理空数据
            List<string> removeNullKey = new();
            if (s_MiddleResourceCache.Count > 0)
            {
                foreach (var item in s_MiddleResourceCache)
                {
                    if (item.Value == null)
                    {
                        removeNullKey.Add(item.Key);

                    }
                }
                for (int i = 0; i < removeNullKey.Count; i++)
                {
                    Debug.Log(removeNullKey[i] + "即将移除");
                    s_MiddleResourceCache[removeNullKey[i]].ActiveClearReferenceCount();
                    s_MiddleResourceCache.Remove(removeNullKey[i]);
                }
            }
            removeNullKey.Clear();
            removeNullKey = null;


            //排序
            var refSort = new List<KeyValuePair<string, SharedResource>>(s_MiddleResourceCache);
            refSort.Sort((a, b) => a.Value.referenceCount.CompareTo(b.Value.referenceCount));
            List<string> allNoUse = new();
            foreach (var resource in refSort)
            {
                allNoUse.Add(resource.Key);
            }



            int removeCount = s_MiddleResourceCache.Count;
            if (removeCount > allNoUse.Count)
            {

                /*先干掉所有不符合时间
                    middle再干掉引用少的*/
                for (int i = 0; i < allNoUse.Count; i++)
                {
                    s_MiddleResourceCache[allNoUse[i]].ActiveClearReferenceCount();
                    Debug.Log(s_MiddleResourceCache[allNoUse[i]].pathKey + "删除");
                    s_MiddleResourceCache.Remove(allNoUse[i]);
                }
                removeCount = s_MiddleResourceCache.Count - MIDDLD_MAX_CACHE_SIZE;
                //s_MiddleResourceCache 删除了 refSort 没删，维护（没有数据contunue ）不如依次oN快排呢
                refSort = new List<KeyValuePair<string, SharedResource>>(s_MiddleResourceCache);
                refSort.Sort((a, b) => a.Value.referenceCount.CompareTo(b.Value.referenceCount));
                for (int i = 0; i < removeCount; i++)
                {
                    refSort[i].Value.ActiveClearReferenceCount();
                    //一定要清理到目标值，里面内容都是不符合时间的（近期没调用的），优先引用数量少的
                    s_MiddleResourceCache.Remove(refSort[i].Key);
                }
            }
            else
            {
                //干掉middle其中   removeKeys一部分（removeCount）优先引用少的即可
                for (int i = 0; i < removeCount; i++)
                {
                    //立刻清理到Addressable，就是不容忍的删除，并且和Address同步
                    s_MiddleResourceCache[allNoUse[i]].ActiveClearReferenceCount();
                    //一定要清理到目标值，里面内容都是不符合时间的（近期没调用的），优先引用数量少的
                    s_MiddleResourceCache.Remove(allNoUse[i]);
                }
            }

            if (isMemoryToMuchtTriGC)
            {
                Resources.UnloadUnusedAssets();
                GC2();
                isMemoryToMuchtTriGC = false;
            }
            Debug.Log("----------------清理中级结束----------------");
        }

        public void ForceClearLowerResourceCache()
        {
            int delCount = 0;

            Debug.Log("----------------清理低级开始----------------");
            LinkedListNode<System.WeakReference<SharedResource>> node = m_LowPriorityResourcesLinked.First;
            Debug.Log(m_LowPriorityResourcesLinked.Count + "低级数量");
            SharedResource sR;
            LinkedListNode<System.WeakReference<SharedResource>> nodeCache;
            while (node != null)
            {
                sR = null;
                if (node.Value.TryGetTarget(out sR))
                {
                    sR.ActiveClearReferenceCount();
                    nodeCache = node;
                    node = node.Next;
                    m_LowPriorityResourcesLinked.RemoveFirst();
                    Debug.Log(sR.pathKey + "删除");
                    s_LowResourceCache.Remove(sR.pathKey);

                    delCount++;
                }
                else //弱引用被释放掉了，没有这个值,删除容器就好
                {
                    nodeCache = node;
                    node = node.Next;
                    m_LowPriorityResourcesLinked.Remove(nodeCache);
                    //s_LowResourceCache.Remove(sR.pathKey);
                }
            }
            //处理字典容器
            List<string> keysToRemove = new();

            SharedResource sR1;
            foreach (KeyValuePair<string, LinkedListNode<System.WeakReference<SharedResource/*<Object>*/>>> kvp in s_LowResourceCache)
            {
                sR1 = null;
                Console.WriteLine($"Key: {kvp.Key}, Value: {kvp.Value}");

                if (kvp.Value == null || !kvp.Value.Value.TryGetTarget(out sR1) || sR1 == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                s_LowResourceCache.Remove(key);
            }
            //改上面去


            if (false)
            {
                GC2();
            }
            Resources.UnloadUnusedAssets();
            //硬性处理-------------------------------------------------------------------

            Debug.Log("----------------清理低级结束----------------");
        }
        #endregion

        #region 内存部分

        public void GC1()
        {
            System.GC.Collect();

        }
        public void GC2()
        {
            System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();


        }
        public void GC3()
        {
            //Resources.UnloadUnusedAssets();
            System.GC.Collect();
            // System.GC.Collect();
            // System.GC.WaitForPendingFinalizers();
        }

        public void GC4()
        {

            //Addressables.ClearDependencyCacheAsync
            //Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        #endregion

        #region 自身模块释放

        //高级永远不会清理，因为永远有用（高级需要自己添加预处理增加进来所以不会删除）
        //中级通常不会清理（这里清理）
        //下级随时都会清理（这里清理）
        //清理删除，真正的卸载，真正的删除
        public void OnModuleReleaseClearResourceCache()
        {
            #region [中级]清理
            //强力会清理Middle。middle正常不会清理
            //清理每一个数据
            foreach (var item in s_MiddleResourceCache)
            {
                item.Value.ActiveClearReferenceCount();
            }
            //清理全部数据引用
            s_MiddleResourceCache.Clear();

            #endregion
            #region [低级]清理
            //双引用选择问题，一把抓Key，一堆鸡蛋排序
            //弱引用如果丢失的话，1link引领遍历的话Weak无法定位到Dic key就不干净就要两次2*O（N）能定位就是O(N+1)，2Dic可以根据空来定位Link但是如果迭代器+Remove就是O(n)平方,都有问题怎么办
            //删除所有保证内容删除即可，不必定位同步元素容器删除，【遍历资源两个容器是对应资源一定是一样的】
            //Link一定最小，遍历稳妥，删除方便没问题
            LinkedListNode<System.WeakReference<SharedResource>> node = m_LowPriorityResourcesLinked.First;

            SharedResource sR;//共享左值


            while (node != null)
            {
                sR = null;
                if (node.Value.TryGetTarget(out sR))
                {
                    sR.ActiveClearReferenceCount();
                    node = node.Next;
                    m_LowPriorityResourcesLinked.RemoveFirst();//要处理LinkRemove
                    /*s_LowResourceCache.Remove(sR.pathKey);*/

                }
                else
                {
                    node = node.Next;
                    //没有就算了，也不找Dic，但是容易找不到所以逻辑在  O（n+1)~2 O（n）之间浮动
                    m_LowPriorityResourcesLinked.RemoveFirst();
                }

            }
            s_LowResourceCache.Clear();

            #endregion
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        #endregion
    }

}