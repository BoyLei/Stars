using SGF;
using System;
using UnityEngine;

/// <summary>
/// 框架之后要打Dll的哦，提高效率，也不怎么改了
/// </summary>
namespace StarProject.Game.Entity.Factory
{
    //==========================================================
    //显示基于:显示和逻辑分离,先实现属性分离,然后进行组合
    //对所有显示物件进行,工厂式,池化,生命周期管理
    public static class ViewFactory
    {
        public static bool EnableLog = false;
        private const string LOG_TAG = "ViewFactory";
        private static bool m_isInit = false;
        //显示挂点:显示基于unity
        private static Transform m_viewRoot;
        private static Recycler m_recycler;

        /// <summary>
        /// 工厂所实例化的对象列表
        /// 这里与EB工厂不同,实际使用的是[池化]+[显示物件]
        /// 通过实体,和,view映射,成对出现且关联
        /// 因为,[实体](1)可以无显示,但是[现实]必然有[实体](1+1)
        /// </summary>
        private static DictionaryEx<EntityObject, ViewObject> m_mapObject;

        /// <summary>
        /// [工厂开业]
        /// </summary>
        /// <param name="viewRoot">显示实体池的跟节点</param>
        public static void Init(Transform viewRoot)
        {
            m_viewRoot = viewRoot;
            if (m_isInit)
            {
                return;
            }
            m_isInit = true;


            //使用器-使用名单
            m_mapObject = new DictionaryEx<EntityObject, ViewObject>();
            //备用器-组建后备公司
            m_recycler = new Recycler();

        }

        /// <summary>
        /// 释放工厂所创建的所有对象，包括空闲的对象
        /// [工厂倒闭]
        /// </summary>
        public static void Release()
        {

            //无需处理实体Module层,逻辑层次会被延迟处理,具备自己的管理
            foreach (var pair in m_mapObject)
            {
                //[解雇在职员工-显示层-推送到后备公司]
                pair.Value.ReleaseInFactory(); //实体子类的[直接][逻辑][释放],实体的待释放标记-待转存,
                //[在职-kill-等GC]
                pair.Value.Dispose();//[代GC][*删除][或兄弟类自定义Db层清理数据库]
            }
            //[清理在职名单]
            m_mapObject.Clear();
            //[解雇后备公司全员-清理后备公司名单]
            m_recycler.Release();

            m_viewRoot = null;
        }

        [Obsolete("请使用 CreateViewAsync", false)]//标记该方法已弃用
        /// <summary>
        /// V面子层[工厂招聘]
        /// </summary>
        /// <param name="resPath">玩家id + 资源 唯一资源组string: 不同显示资源,不同玩家id ,导致分组不可以复用
        /// 1,对战游戏可能玩家颜色不同(可以相同模型,可复用)
        /// 2,球球大作战,贪食蛇可能大小不同(不可以复用)每个人大量重复资源 * 多个人不同 * Avatar
        /// 3,传奇装备(可以复用)
        /// 4,子弹(可以复用)
        /// 实用:如果想进入一个池就减少特异性条件令String可以相等,如只通过资源名,或者,玩家id
        /// 如不想复用,则增加特意条件A&B&C 合并为String
        /// 
        /// @他是可以具备特异性分组的 例如 a/b/wapon_None1,ResDefPath
        /// @也可以具备非特异性 respath = resdefpath 不影响性能
        /// @同时具备资源的,延迟性(美术未提供的代替),容错性资源加载异常的处理,下载出错,延迟,策划未设计,程序人为分割测试,策略等等等...不影响功能和线上运行
        /// </param>
        /// <param name="resDefaultPath"></param>
        /// <param name="entity">逻辑实体关联,由逻辑实体引发显示,module2View</param>
        /// <param name="parent">挂点,如没有则默认挂点</param>
        public static void CreateView(string resPath, string resDefaultPath, EntityObject entity, Transform parent = null, string layerMaskName = "")
        {
            ViewObject obj = null;
            //特异性资源string,用以分组到备选公司
            string recycleType = resPath;
            bool useRecycler = true;                        //默认池中回收

            ///逻辑实体，发起绑定View实体，形成一一关联
            ///View 为可选项，通常但都绑定同样逻辑实体
            ///但View实体根据不同类型，被同类型共享池，
            ///A战士，和 B战士 和 C战士 逻辑实体都是战士：但是A 和C得手环共享池，B和C得武器共享池，A和B得披风共享池
            ///表现依赖资源显示池复用
            ///逻辑依赖内存空间：（相似空间性）（及简性）分配池
            ///所以发起者必然是控制层：逻辑实体
            ///逻辑实体必然被逻辑实体管理器管理
            ///驱动管理器得：必然是管理集合
            ///
            ///隐患排查20220510，池的共用性：1用的是接口宽泛，2同一个文件资源可复生命周期控制【create-release】理论背书，3父级节点可以重新设定也就是说=
            ///==所有池资源只要类型名称相同-不论父级节点是谁都可以复用转换-重启生命周期
            obj = m_recycler.Pop(recycleType) as ViewObject;
            if (obj == null)
            {
                useRecycler = false;                        //Log中标识,是否使用池中物,如统计大量非池中物,则需要类内存标记,增大容量,减少释放
                obj = InstanceViewFromPrefab(recycleType, resDefaultPath);
            }

            if (obj != null)
            {
                if (!obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(true);
                }

                if (parent != null)
                {
                    obj.transform.SetParent(parent, false);
                }
                else
                {
                    obj.transform.SetParent(m_viewRoot, false);
                }
                //定义的RecyleType不但因加载异常的,去备选公司选,也会坚信的赋给工厂实体记录(这个不是反射名了,业务和V情况多变)
                obj.CreateInFactory(entity, recycleType);   //工厂招聘需要赋值给你[特定标记组牌key],和子类型(工厂)的新增加载方式[招聘方式]
                                                            //注意:这里是显示层[子类,新增处理],对逻辑层的额外处理,拓展接口入口                                            

                if (EnableLog && Debuger.EnableLog)         //宏debug,子定义
                {                                           //
                    Debuger.Log(LOG_TAG, "CreateView() logicLayerName{0}:HC{1} -> VLayerType{2}:InstId{3}, 是否在池里面取得的:{4}",
                        entity.GetType().Name, entity.GetHashCode(),
                        obj.GetRecycleType(), obj.GetInstanceID(),
                        useRecycler);
                }

                if (m_mapObject.ContainsKey(entity))
                {
                    Debuger.LogError(LOG_TAG, "CreateView() 不应该存在重复的映射！");
                }
                m_mapObject[entity] = obj;

                /////显示依赖Unity
                if (layerMaskName != "")
                {
                    obj.gameObject.layer = LayerMask.NameToLayer(layerMaskName);
                }
                //m_mapObject.Add(entity, obj);
            }
        }

        [Obsolete("请使用 CreateViewAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 壳子（容器+ViewAoi）---数据表---表现模型---绑定逻辑层次
        /// </summary>
        /// <param name="resPath">路径</param>
        /// <param name="resDefaultPath">默认路径</param>
        /// <param name="entity">实体逻辑层</param>
        /// <param name="parent">父节点</param>
        /// <param name="layerMaskName">层</param>
        /// <param name="replace">替换</param>
        /// <param name="e_AssetType">资源类型</param>
        /// <returns></returns>
        public static ViewObject CreateViewAddressables(string resPath, string resDefaultPath, EntityObject entity, Transform parent = null, string layerMaskName = "", bool replace = true, StarProjectDef.E_AssetType e_AssetType = StarProjectDef.E_AssetType.Roles)
        {
            resPath = resPath.Replace(".prefab", "");
            resPath = resPath.Replace("Assets/Res/", "");
            ViewObject obj = null;
            //特异性资源string,用以分组到备选公司
            string recycleType = resPath;
            bool useRecycler = true;                        //默认池中回收

            ///逻辑实体，发起绑定View实体，形成一一关联
            ///View 为可选项，通常但都绑定同样逻辑实体
            ///但View实体根据不同类型，被同类型共享池，
            ///A战士，和 B战士 和 C战士 逻辑实体都是战士：但是A 和C得手环共享池，B和C得武器共享池，A和B得披风共享池
            ///表现依赖资源显示池复用
            ///逻辑依赖内存空间：（相似空间性）（及简性）分配池
            ///所以发起者必然是控制层：逻辑实体
            ///逻辑实体必然被逻辑实体管理器管理
            ///驱动管理器得：必然是管理集合
            ///
            ///隐患排查20220510，池的共用性：1用的是接口宽泛，2同一个文件资源可复生命周期控制【create-release】理论背书，3父级节点可以重新设定也就是说=
            ///==所有池资源只要类型名称相同-不论父级节点是谁都可以复用转换-重启生命周期
            obj = m_recycler.Pop(recycleType) as ViewObject;
            if (obj == null)
            {
                useRecycler = false;                        //Log中标识,是否使用池中物,如统计大量非池中物,则需要类内存标记,增大容量,减少释放
                obj = InstanceViewFromAddressablesPrefab(recycleType, resDefaultPath, e_AssetType);
            }

            if (obj != null)
            {
                if (!obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(true);
                }

                if (parent != null)
                {
                    obj.transform.SetParent(parent, false);
                }
                else
                {
                    obj.transform.SetParent(m_viewRoot, false);
                }
                //定义的RecyleType不但因加载异常的,去备选公司选,也会坚信的赋给工厂实体记录(这个不是反射名了,业务和V情况多变)
                obj.CreateInFactory(entity, recycleType);   //工厂招聘需要赋值给你[特定标记组牌key],和子类型(工厂)的新增加载方式[招聘方式]
                                                            //注意:这里是显示层[子类,新增处理],对逻辑层的额外处理,拓展接口入口                                            

                if (EnableLog && Debuger.EnableLog)         //宏debug,子定义
                {                                           //
                    Debuger.Log(LOG_TAG, "CreateViewAddressables() logicLayerName{0}:HC{1} -> VLayerType{2}:InstId{3}, 是否在池里面取得的:{4}",
                        entity.GetType().Name, entity.GetHashCode(),
                        obj.GetRecycleType(), obj.GetInstanceID(),
                        useRecycler);
                }

                if (m_mapObject.ContainsKey(entity))
                {
                    Debuger.LogError(LOG_TAG, "CreateViewAddressables() 不应该存在重复的映射！");
                }

                if (replace)
                {
                    m_mapObject[entity] = obj;
                }

                /////显示依赖Unity
                if (layerMaskName != "")
                {
                    obj.gameObject.layer = LayerMask.NameToLayer(layerMaskName);
                }
                //m_mapObject.Add(entity, obj);
            }
            else
            {
                Debuger.LogError(LOG_TAG, $"CreateViewAddressables() resPath: {resPath}  资源加载失败，地址有问题！");
            }
            return obj;
        }

        [Obsolete("请使用 CreateViewAsync", false)]//标记该方法已弃用
        /// <summary>
        /// 壳子（容器+Follow）---数据表(影子模型）---表现模型---绑定逻辑层次
        /// 
        /// 创建 一个 壳子, 同时 传入 壳子内的模型 路径 modelPath
        /// </summary>
        /// <param name="resPath">壳子 路径</param>
        /// <param name="resDefaultPath">壳子 默认路径</param>
        /// <param name="modelPath">模型路径</param>
        /// <param name="entity"></param>
        /// <param name="parent"></param>
        /// <param name="layerMaskName"></param>
        /// <param name="replace"></param>
        /// <param name="e_AssetType"></param>
        /// <returns></returns>
        public static ViewModel CreateViewModelAddressables(string resPath, string resDefaultPath, string modelPath, EntityObject entity, Transform parent = null, string layerMaskName = "", bool replace = true, StarProjectDef.E_AssetType e_AssetType = StarProjectDef.E_AssetType.Roles)
        {
            resPath = resPath.Replace(".prefab", "");
            resPath = resPath.Replace("Assets/Res/", "");
            ViewModel obj = null;
            //特异性资源string,用以分组到备选公司
            string recycleType = resPath;
            bool useRecycler = true;                        //默认池中回收

            obj = m_recycler.Pop(recycleType) as ViewModel;
            if (obj == null)
            {
                useRecycler = false;                        //Log中标识,是否使用池中物,如统计大量非池中物,则需要类内存标记,增大容量,减少释放
                obj = InstanceTViewFromAddressablesPrefab<ViewModel>(recycleType, resDefaultPath, e_AssetType);
            }

            if (obj != null)
            {
                if (!obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(true);
                }

                if (parent != null)
                {
                    obj.transform.SetParent(parent, false);
                }
                else
                {
                    obj.transform.SetParent(m_viewRoot, false);
                }
                //定义的RecyleType不但因加载异常的,去备选公司选,也会坚信的赋给工厂实体记录(这个不是反射名了,业务和V情况多变)
                obj.CreateInFactory(entity, recycleType, modelPath);   //工厂招聘需要赋值给你[特定标记组牌key],和子类型(工厂)的新增加载方式[招聘方式]
                                                                       //注意:这里是显示层[子类,新增处理],对逻辑层的额外处理,拓展接口入口                                            

                if (EnableLog && Debuger.EnableLog)         //宏debug,子定义
                {                                           //
                    Debuger.Log(LOG_TAG, "CreateViewAddressables() logicLayerName{0}:HC{1} -> VLayerType{2}:InstId{3}, 是否在池里面取得的:{4}",
                        entity.GetType().Name, entity.GetHashCode(),
                        obj.GetRecycleType(), obj.GetInstanceID(),
                        useRecycler);
                }

                if (m_mapObject.ContainsKey(entity))
                {
                    Debuger.LogError(LOG_TAG, "CreateViewAddressables() 不应该存在重复的映射！");
                }

                if (replace)
                {
                    m_mapObject[entity] = obj;
                }

                /////显示依赖Unity
                if (layerMaskName != "")
                {
                    obj.gameObject.layer = LayerMask.NameToLayer(layerMaskName);
                }
                //m_mapObject.Add(entity, obj);
            }
            else
            {
                Debuger.LogError(LOG_TAG, $"CreateViewAddressables() resPath: {resPath}  资源加载失败，地址有问题！");
            }
            return obj;
        }

        public static void CreateViewAsync(string resPath, EntityObject entity, Transform parent, Action<GameObject> cb, string modelPath = "")
        {
            //特异性资源string,用以分组到备选公司
            string recycleType = resPath;
            ViewObject obj = m_recycler.Pop(recycleType) as ViewObject;
            if (obj == null)
            {
                StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<GameObject>(recycleType,
                (GameObject gob) =>
                {
                    if (gob != null)
                    {
                        GameObject item = GameObject.Instantiate<GameObject>(gob);
                        ViewObject instance = item.GetComponent<ViewObject>();  //GameObject 和 被Mono驱动的显示组件 ,实体 形成真正实例
                        if (instance == null)
                        {
                            SGF.Debuger.LogError(LOG_TAG, $"CreateViewAsync() recycleType={recycleType},item={item},instance={instance}, err!!!");
                            cb?.Invoke(null);
                        }
                        else
                        {
                            if (entity.IsReleased)
                            {
                                GameObject.Destroy(instance);
                            }
                            else
                            {
                                cb?.Invoke(item);
                                SetView(instance, recycleType, entity, parent, modelPath);
                            }
                        }
                    }
                });
            }
            else
            {
                cb?.Invoke(obj.gameObject);
                SetView(obj, recycleType, entity, parent, modelPath);
            }
        }

        private static void SetView(ViewObject obj, string recycleType, EntityObject entity, Transform parent, string modelPath = "")
        {
            if (obj != null)
            {
                if (!obj.gameObject.activeSelf)
                {
                    obj.gameObject.SetActive(true);
                }

                if (parent != null)
                {
                    obj.transform.SetParent(parent, false);
                }
                else
                {
                    obj.transform.SetParent(m_viewRoot, false);
                }
                //定义的RecyleType不但因加载异常的,去备选公司选,也会坚信的赋给工厂实体记录(这个不是反射名了,业务和V情况多变)
                //工厂招聘需要赋值给你[特定标记组牌key],和子类型(工厂)的新增加载方式[招聘方式]
                //注意:这里是显示层[子类,新增处理],对逻辑层的额外处理,拓展接口入口
                if (string.IsNullOrEmpty(modelPath) || string.IsNullOrWhiteSpace(modelPath))
                {
                    obj.CreateInFactory(entity, recycleType);   
                }
                else
                {
                    obj.CreateInFactory(entity, recycleType, modelPath);
                }

                if (m_mapObject.ContainsKey(entity))
                {
                    SGF.Debuger.LogWarning(LOG_TAG, "SetView() 不应该存在重复的映射！");
                }
                else
                {
                    m_mapObject.Add(entity, obj);
                }
            }
            else
            {
                SGF.Debuger.LogWarning(LOG_TAG, "SetView() obj=null err!!!");
            }
        }


        /// <summary>
        /// V面子层[工厂解雇]
        /// 卸载会依次检索，ntt，关联，view的判空
        /// </summary>
        /// <param name="entity">需提供逻辑层实体</param>
        public static void ReleaseView(EntityObject entity)
        {
            //逻辑,可以有显示 or not ;显示必须有逻辑支撑
            if (entity != null)
            {
                //[工厂,在职名单中]确保显示实体,若[显示层实体]为空,则无必要再显示工厂解雇,这个人都丢了好几天没上班警察查身份证都不存在这个人,你解雇谁?
                ViewObject obj = m_mapObject[entity];//unity池，也就是逻辑实体，表现层池，只不过被放在【回收】中效果是隐藏，但是关联还在//【表现依据逻辑而存在，逻辑是延迟帧释放，表现是关闭】//本质都是缓存起来减少io//逻辑层是脚本你名字显示层是物件路径加名字
                if (obj != null)
                {
                    if (EnableLog && Debuger.EnableLog)
                    {
                        Debuger.Log(LOG_TAG, "ReleaseView() 逻辑层名{0}:HC{1} -> 显示层类型{2}:InstID{3}",
                            entity.GetType().Name, entity.GetHashCode(),
                            obj.GetRecycleType(), obj.GetInstanceID());
                    }
                    //[工厂在职名单中,移除逻辑层 关联 显示层]的键值对
                    m_mapObject.Remove(entity);
                    //[显示层,对于工厂解雇(推送到备用厂子),通常是等待什么都不做]==子类可以针对拓展(左值制空)
                    obj.ReleaseInFactory();     //V,Remove不会对逻辑层处理
                                                //[v解雇,到,备用厂子,通常是显隐]            [关厂才是删除]
                    obj.gameObject.SetActive(false);

                    //将对象加入对象池:     对象池！！！
                    m_recycler.Push(obj);//1,这里是显示层直接移除,2备选层直接加入3,显示层不能延迟效果,4也不会对逻辑有影响
                    //5,通常来说显示有显示的集合,逻辑有逻辑的集合,逻辑和现实分离单独处理,分别调用,虽然这里有弱耦合是显示销毁要通知逻辑的(逻辑还在)
                    //6,顺序就是先处理显示后处理逻辑
                    //7,显示要求有及时性
                    //8,逻辑要求性能性
                    //9,显示的销毁具有短时间延迟,一般需等到EndofFrame,会调用Mono的Destroy,这时逻辑需要在,显示才能通知到
                    //10,同时逻辑具备聚合删除的性能特性



                }
            }
        }


        /// <summary>
        /// 资源加载的备选方案:这里具备资源容错性
        /// </summary>
        /// <param name="prefabName"></param>
        /// <param name="defaultPrefabName"></param>
        /// <returns></returns>
        private static ViewObject InstanceViewFromPrefab(string prefabName, string defaultPrefabName)
        {
            GameObject prefab = Resources.Load<GameObject>(prefabName);

            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>(defaultPrefabName);
            }
            //隐患2，脚本的共用性:daan 
            //1，可以兼容子集，2可以确保作用，3如此规范必须就别挂View
            //1，不挂View你不能确定这个具体是什么view子集（哪个孙子-例如vvanim孙子）--约束类型的必须手挂
            //2, 那如果你手挂了，并且用于非实体类型的资源---报错再说——【说明view层代码你写错了】所有的view
            //所有的view如果不是实体，不是工厂，那他不会走实体初始化111，222虽然是他是mono但是没用他生命周期所以不会驱动，333虽然耗说明美术资源管理有问题但是逻辑上不会驱动
            //从而实现了，资源的复用性【时刻关注检查】
            //3，如果其他非实体需要使用如LocalFxManager，必须EnsureComponent
            //4，形成了以挂载实体view脚本必要为基础，动态添加其他辅助脚本的设定和现象
            //5，虽然不会报错，但是本质节省上，资源分类必须要：【场景/UI】*【实体/独立】*【动/静】*【远/本】
            //6，根据用处单独划分，更加强资源地址划分的池化现象，和自然的池分类划分，和池化力度：本质是你能公用你就不会错乱
            //7，池的性能，最大Max,io,并行吞吐,策略，时间，量级，结构，数据对齐
            //8，池的组间，类型的复用层度，同类转化复用程度
            //9，池的多组拓展和生命周期管理
            GameObject go = GameObject.Instantiate(prefab);
            //ViewObject instance = GameObjectUtils.EnsureComponent<ViewObject>(go);
            ViewObject instance = go.GetComponent<ViewObject>();  //GameObject 和 被Mono驱动的显示组件 ,实体 形成真正实例

            if (instance == null)
            {
                Debuger.LogError(LOG_TAG, "InstanceViewFromPrefab() prefab = " + prefabName);
            }

            return instance;
        }

        private static ViewObject InstanceViewFromAddressablesPrefab(string prefabName, string defaultPrefabName, StarProjectDef.E_AssetType e_AssetType)
        {
            return InstanceTViewFromAddressablesPrefab<ViewObject>(prefabName, defaultPrefabName, e_AssetType);
        }

        [Obsolete("请使用 CreateViewAsync", false)]//标记该方法已弃用
        private static T InstanceTViewFromAddressablesPrefab<T>(string prefabName, string defaultPrefabName, StarProjectDef.E_AssetType e_AssetType) where T : class
        {
            GameObject prefab = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(prefabName, e_AssetType);

            if (prefab == null)
            {
                prefab = Service.Resource.ResourceFormalManager.Instance.RecursionLoadGameObject(defaultPrefabName, e_AssetType);
            }
            //隐患2，脚本的共用性:daan 
            //1，可以兼容子集，2可以确保作用，3如此规范必须就别挂View
            //1，不挂View你不能确定这个具体是什么view子集（哪个孙子-例如vvanim孙子）--约束类型的必须手挂
            //2, 那如果你手挂了，并且用于非实体类型的资源---报错再说——【说明view层代码你写错了】所有的view
            //所有的view如果不是实体，不是工厂，那他不会走实体初始化111，222虽然是他是mono但是没用他生命周期所以不会驱动，333虽然耗说明美术资源管理有问题但是逻辑上不会驱动
            //从而实现了，资源的复用性【时刻关注检查】
            //3，如果其他非实体需要使用如LocalFxManager，必须EnsureComponent
            //4，形成了以挂载实体view脚本必要为基础，动态添加其他辅助脚本的设定和现象
            //5，虽然不会报错，但是本质节省上，资源分类必须要：【场景/UI】*【实体/独立】*【动/静】*【远/本】
            //6，根据用处单独划分，更加强资源地址划分的池化现象，和自然的池分类划分，和池化力度：本质是你能公用你就不会错乱
            //7，池的性能，最大Max,io,并行吞吐,策略，时间，量级，结构，数据对齐
            //8，池的组间，类型的复用层度，同类转化复用程度
            //9，池的多组拓展和生命周期管理
            //ViewObject instance = GameObjectUtils.EnsureComponent<ViewObject>(go);
            if (prefab != null)
            {
                T instance = prefab.GetComponent<T>();  //GameObject 和 被Mono驱动的显示组件 ,实体 形成真正实例
                if (instance == null)
                {
                    Debuger.LogError(LOG_TAG, "InstanceViewFromPrefab() prefab = " + prefabName);
                }
                return instance;
            }
            else
            {
                return null;
            }
        }





    }
}
