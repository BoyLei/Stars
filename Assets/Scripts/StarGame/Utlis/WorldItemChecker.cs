using SGF.Module.Framework;
using SGF.UI.Framework;
using SGF.Unity;
using StarProject.Service.Cam;
using StarProjectDef;
using System.Collections.Generic;
using UnityEngine;
//绿轴朝上
//1m = 1m
//绑定点:配置，工具生成
//MeshRender/SkinRender

//一个unit的意识是标准和基础
//Unity的是米FBX所以要绑定玛雅 和 unity 基础标尺对应关系
//图片定义是什么意识sprite 米和像素关系 || canvas什么意识 也是 米和像素关系
//美术的ps像素，对应图片像素，对应uiRect像素和坐标===同时也做到了场景1米等于72像素位移
namespace StarProject.Service.WorldToUI
{//都是异步先后顺序不能确定还要解耦合
    public class TransRectMaps
    {
        public Transform role;          // 模型
        public RectTransform pendent;   // 头顶信息
        public float modelHeight;       // 模型高度  静态值不满足需求 >  [最好的策略就是动态骨骼]动静结合   > aabb包围盒动态算宽度 
        public void Clear()
        {
            role = null;
            pendent = null;
            modelHeight = 2.0f; // 默认值
        }
    }
    public class WorldItemChecker : ServiceModule<WorldItemChecker>
    {
        //池倒手不被gc而已
        public Stack<TransRectMaps> TransRectMapsPool = new();


        /*    private static WorldItemChecker instance;
            public static WorldItemChecker Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new WorldItemChecker();
                    }
                    return instance;
                }
            }
        */
        internal void Init()
        {
            CheckSingleton();
            StarUpdate();
        }
        internal void StarUpdate()
        {
            MonoHelper.AddLaterUpdateListener(OnLaterUpdate , MonoHelper.E_ModuleType.CommonService);
        }
        public override void Release()
        {
            MonoHelper.RemoveLaterUpdateListener(OnLaterUpdate, MonoHelper.E_ModuleType.CommonService);
            base.Release();
        }
        Canvas _Canvas;
        public Canvas Canvas
        {
            get
            {
                if (_Canvas == null)
                {
                    _Canvas = UIManager.Instance.M_Canvas;
                }
                return _Canvas;
            }
            set => _Canvas = value;
        }

        Camera _UiCam;
        public Camera UiCam
        {
            get
            {
                if (_UiCam == null)
                {
                    _UiCam = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.UICam).GetComponent<Camera>();//--
                }
                return _UiCam;
            }
        }

        Camera _BattleCamera;
        public Camera BattleCamera
        {
            get
            {
                if (_BattleCamera == null)
                {
                    _BattleCamera = CameraManager.Instance.GetCamera(StarProjectDef.E_CameraType.StarWorldCam).Camera;
                }
                return _BattleCamera;
            }
        }



        #region 3D转2D

        //public delegate Vector2 WorldPosToRectTransform(Vector3 worldPos);
        //角色id名字 实体id
        private Dictionary<ulong, TransRectMaps> pendGroups = new();
        private List<ulong> updateRoleTrans = new();
        //// TimelineState只有不开的时候才关闭为false，State如果为开启状态优先尊重TimeLineState
        //private bool iSTimeLineState = false;
        private bool ALLReflash = false;

        /// <summary>
        /// 1逻辑层坑位创建
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="transRectMaps"></param>
        public void RegToPendRoleMaps(ulong entityId)
        {
            //if (pendGroups.Count > GameConfig.SceneVisibleMax)
            //{
            //    return;
            //}
            if (!pendGroups.ContainsKey(entityId))
            {
                pendGroups.Add(entityId, GetNewTransRectMaps());
            }
        }

        /// <summary>
        /// 2坑位3D填充
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="transRectMaps"></param>
        public void RegToRoleTrans(ulong entityId, Transform role)
        {
            if (pendGroups.ContainsKey(entityId))
            {
                pendGroups[entityId].role = role;
                pendGroups[entityId].modelHeight = 2.0f;    // 这里先给个默认值
            }
        }

        /// <summary>
        /// 设置实体的模型高度
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="height"></param>
        public void RegToRoleHeight(ulong entityId, float height)
        {
            if (pendGroups.ContainsKey(entityId))
            {
                pendGroups[entityId].modelHeight = height;    // 这里先给个默认值

            }
        }

        /// <summary>
        /// 3坑位2D填充
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="transRectMaps"></param>
        public void RegToRectTransPend(ulong entityId, RectTransform transRectMaps)
        {
            if (pendGroups.ContainsKey(entityId))
            {
                pendGroups[entityId].pendent = transRectMaps;
            }
        }

        /// <summary>
        /// 逻辑层坑位销毁
        /// 5销毁壳子
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="transRectMaps"></param>
        public void UnRegToRectTransformPend(ulong entityId)
        {
            if (pendGroups.ContainsKey(entityId))
            {
                pendGroups[entityId].Clear();
                BackNewTransRectMaps(pendGroups[entityId]);
                pendGroups.Remove(entityId);

                //SGF.Debuger.LogWarning($"名字测试 主角创建 5销毁壳子 entityId={entityId}");
            }
        }

        //public void SetTimeLineSate(bool state)
        //{
        //    //SGF.Debuger.LogError($"名字测试 主角创建 设置相机移动state={state}");

        //    iSTimeLineState = state;
        //    if (state)
        //    {
        //        NeedRefUICommand(0, false);
        //    }
        //    else
        //    {
        //        NeedRefUICommand(0, true);
        //    }
        //}

        /*       public int GetOverlayRenderGroupDistOrder(E_OverLayTypeOrderBase e_OverLayTypeOrderBase, Transform Model)//实体模型给我：逻辑层拿不到也行给我世界坐标
               {
               *//* float len = (BattleCamera.transform.position - Model.transform.position).magnitude;

                *//*int orderAdder = *//*return (int)(e_OverLayTypeOrderBase) + 1000 - Mathf.Min(1000, (int)(len * 10));*//*
               return (int)(e_OverLayTypeOrderBase) + 1000 - Mathf.Min(1000, (int)((BattleCamera.transform.position - Model.transform.position).magnitude * 10));
           }*/

        /// <summary>
        /// aoi，相机内（视锥形剔除，或者超过距离，UI位置超过分辨率位置了不必要渲染），fix，角色身上就一个一组
        /// 你需要order根节点有一个order就好，先别排序，特殊选择用10000
        /// 给你排序order
        /// </summary>
        /// <param name="e_OverLayTypeOrderBase"></param>
        /// <param name="moduleWorldPos"></param>
        /// <returns>-1就是太原不应该显示</returns>
        public int GetOverlayRenderGroupDistOrder(Vector3 moduleWorldPos, E_OverLayTypeOrderBase e_OverLayTypeOrderBase = E_OverLayTypeOrderBase.Pendant)//实体模型给我：逻辑层拿不到也行给我世界坐标，值变化的时候要告诉我
        {
            //float len = (BattleCamera.transform.position - moduleWorldPos).magnitude;
            //if (len > /*80*/GameConfig.SceneVisibleMax)
            //{
            //    return -1;
            //}
            // 将模型中心点从世界坐标转换到相机空间
            Vector3 viewSpaceCenter = BattleCamera.WorldToViewportPoint(moduleWorldPos);

            // 检查模型中心点是否在视锥之内
            if (viewSpaceCenter.x >= 0 && viewSpaceCenter.x <= 1 &&
                viewSpaceCenter.y >= 0 && viewSpaceCenter.y <= 1 &&
                viewSpaceCenter.z > 0)
            {
                /*Debug.Log("模型在相机视锥之内");*/
            }
            else
            {
                /*Debug.Log("模型在相机视锥之外");*/
                return -1;
            }
            //==========================================
            //如果有任意顶点都要显示这个信息的话，其实不可能都在头顶，那ui会适配，但ui如果真实超出范围了也不需要渲染TODO TODO TODO
            //如果这个值是黏值只要有任意顶点，这个头顶信息就自动找位置，那就要检测顶点
            //如果有任意值有顶点就生成（现在是中心点），这个头顶信息就表现，那可能显示补全，那就要排除ui不显示的


            //==========================================
            //ui位置超过分辨率位置了不必要渲染
            //如果固定在头顶，信息有一点都显示出来，全不显示就隐藏 TODO小曲做不到但是这个之后要做（节省内存，节省不了Dc本身就没DC


            //==========================================
            //fix你要做到 如果值发生变化变化再给我，不变不给我
            //或者每帧刷
            //dist相差0.1米都要给我，但是少于0.1千万别给我


            //==========================================
            //变化用5~10fix帧再给我，用lerp  TODO

            //==========================================
            /*int orderAdder = *//*return (int)(e_OverLayTypeOrderBase) + 1000 - Mathf.Min(1000, (int)(len * 10));*/
            //==========================================

            //写Manager，超过50个就不表现，其实是角色直接拦截，信息栏就拦截了，所以位置就拦截了，不用mgr，不用缓存信息获取index，也不好获取gameobject

            //==========================================
            //默认看40米，我预留400空间
            //最大看80米，我预留800order空间
            return (int)e_OverLayTypeOrderBase + 1000 - Mathf.Min(1000, (int)((BattleCamera.transform.position - moduleWorldPos).magnitude * 10));
        }

        private TransRectMaps GetNewTransRectMaps()
        {
            //if (pendGroups.Count > GameConfig.SceneVisibleMax)
            //{
            //    return null;
            //}

            if (TransRectMapsPool.Count == 0)
            {
                return new TransRectMaps();
            }
            else
            {
                return TransRectMapsPool.Pop();
            }
        }

        private void BackNewTransRectMaps(TransRectMaps items)
        {
            TransRectMapsPool.Push(items);
        }

        //4通知我由一起刷,相当于主动,也相当于主动中的一起统一刷,只要通知我就行
        //later一起刷有命令才刷新节省 + 特殊状态特殊刷 + later主动刷通知刷
        //多个都会进来处理update完事全都处理好了以后later
        //这里为啥我自己不检测而是小曲发给我,因为thd是服务器算的,一个客户端不知道,一个是节省算力
        public void NeedRefUICommand(ulong roleTrans, bool isMainPlayer = false)
        {
            ///他发生过移动的接口是这里,你不用先移动完成,你只要在laterUpdaet之前移动完成就行
            //fix也在later之前
            //只要你告诉我变化快乐,并且later之前做了值的改变就没问题

            //决定都要刷的时候，那就是都刷
            //SGF.Debuger.LogWarning($"名字测试 主角创建 设置状态 11111 roleTrans={roleTrans},isMainPlayer={isMainPlayer},ALLReflash={ALLReflash},iSTimeLineState{iSTimeLineState}");

            if (ALLReflash)//自己锁定和别人锁定都进不来
            {
                return;
            }
            //有一个变化都会重绘，没有遍历的时间损耗
            if (isMainPlayer/* || iSTimeLineState*/)
            {
                //直接刷
                ALLReflash = true;
                updateRoleTrans.Clear();
                //SGF.Debuger.Log($"名字测试 主角创建 设置状态 222222 roleTrans={roleTrans},isMainPlayer={isMainPlayer},ALLReflash={ALLReflash},iSTimeLineState{iSTimeLineState}");
                return;
            }

            if (!updateRoleTrans.Contains(roleTrans))
            {
                updateRoleTrans.Add(roleTrans);
            }
            //SGF.Debuger.LogWarning($"名字测试 主角创建 设置状态 3333333 roleTrans={roleTrans},isMainPlayer={isMainPlayer},ALLReflash={ALLReflash},iSTimeLineState{iSTimeLineState}");
        }

        public void UpdatePendGroupsPos()
        {
            if (!ALLReflash)
            {
                ALLReflash = true;
            }
        }

        //一一对应，且，update【都】自己关系和互相关系，完成后执行layter
        //有一个变化是要重绘，但是每个变化也消耗，虽然都是later准备数据没有dotween，然后马上交给render处理，绘制变化中间过程少，减少运算量
        //和何况还有其他人不动，自己不动，和特殊情况都要刷新的特殊状态和主角状态来区分节省。
        private void OnLaterUpdate()
        {
            //我动就刷新 = 全局刷新
            //其他人动 个人刷新
            if (ALLReflash)
            {
                foreach (var item in pendGroups)
                {
                    //你update检测是否有坐标变化，和你是谁
                    //我来调用你函数，让你传递vector3给我，我给你更新值
                    //你给我一个 setter 我给你一个getter
                    if (item.Value.pendent != null && item.Value.role != null)
                    {
                        Vector3 curPos = item.Value.role.position;
                        curPos.y += item.Value.modelHeight;//美术绑点有点低
                        //curPos.x = (float)Math.Round((double)curPos.x, 1);//45入精度一样//
                        Vector2 fromePOS = item.Value.pendent.anchoredPosition;
                        Vector2 toPOS = PositionConvert.ConvertWorldToCanvasPosition(curPos);
                        item.Value.pendent.anchoredPosition = toPOS;
                        //item.Value.pendent.anchoredPosition = Vector2.Lerp(fromePOS, toPOS, UnityEngine.Time.deltaTime * 200);

                        //SGF.Debuger.LogWarning($"名字测试 主角创建 设置位置 entityid={item.Key},curPos={curPos},fromePOS={fromePOS},toPOS={toPOS},BattleCamera={BattleCamera.transform.position},anchoredPosition={item.Value.pendent.anchoredPosition}");
                    }
                    //SGF.Debuger.Log($"名字测试 主角创建 设置位置 entityid={item.Key},pendent={item.Value.pendent},role={item.Value.role},index={index}");
                }
                ALLReflash = /*iSTimeLineState |*/ false; //TimelineState只有不开的时候才关闭为false，State如果为开启状态优先尊重TimeLineState,;,
                //SGF.Debuger.Log($"名字测试 主角创建 设置位置 状态ALLReflash={ALLReflash}");
            }
            else
            {
                for (int i = 0; i < updateRoleTrans.Count; i++)
                {
                    //不注册的东西你来访问，找不到报错就怪你
                    ulong entityID = updateRoleTrans[i];
                    if (pendGroups.TryGetValue(entityID, out var item))
                    {
                        if (item.pendent != null && item.role != null)
                        {
                            Vector3 curPos = item.role.position;
                            curPos.y += item.modelHeight;
                            //curPos.x = (float)Math.Round((double)curPos.x, 1);
                            //Vector2 fromePOS = item.pendent.anchoredPosition;
                            Vector2 toPOS = PositionConvert.ConvertWorldToCanvasPosition(curPos);
                            //item.pendent.anchoredPosition = Vector2.Lerp(fromePOS, toPOS, UnityEngine.Time.deltaTime * 200);
                            item.pendent.anchoredPosition = toPOS;
                        }
                    }
                }
                updateRoleTrans.Clear();//完事就要清理因为下次不添加确实省事儿，但是下次人不走了，你白更新
            }
        }

        #endregion

        private Vector2 mDialogOffset = new(0, 0); //默认气泡UI 偏移量

        /// <summary>
        /// 查找所有子节点叫item的，并且确认开启和item的，就返回第一个
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        //protected Transform GetCurOnlyOpenedItem(Transform trans)
        //{
        //    List<Transform> deepList = trans.parent.DeepFirstTransList("Item");
        //    Transform itemObj = null;
        //    for (int i = 0; i < deepList.Count; i++)
        //    {
        //        if (deepList[i].gameObject.activeInHierarchy
        //        &&
        //        deepList[i].gameObject.name == "Item"
        //        )
        //        {
        //            itemObj = deepList[i].transform;
        //            break;
        //        }
        //    }

        //    return itemObj;
        //}

        private Dictionary<int, Vector3> posCache = new();
        private Dictionary<GameObject, Vector3> poolEnityDic = new();
        Vector2 mouseUGUIPos1 = new();
        /// <summary>
        /// 3D转3D或屏幕坐标
        /// 人就直接传Body进来，物件直接传递boxCollider
        /// body缓存起来
        /// </summary>
        /// <param name="sceneEnityItem">目标物体</param>
        /// <param name="showPosInPlayer">坐标位置</param>
        /// <param name="str">输出格式</param>
        /// <returns></returns>
        public Vector3 PlayerAndNpcPos(Transform sceneEnityItem, E_AnchorPresets showPosInPlayer, UiOr3D str, PivotPos pivotPos, bool isEnvItem = false)
        {
            if (!poolEnityDic.ContainsKey(sceneEnityItem.gameObject))
            {
                poolEnityDic.Add(sceneEnityItem.gameObject, new Vector3());
            }

            //TODO
            //！！！应该读取配置固定位置点及其优化！！！配置点是骨骼！！！并且是世界坐标的支撑点

            Vector3 vector = poolEnityDic[sceneEnityItem.gameObject];

            Transform roleEnity = null;
            SpriteRenderer roleEnitySR = null;

            GameRenderType gameRenderType = GameRenderType.ThreeD;//GameModeManager.gameRenderType;

            roleEnity = sceneEnityItem;
            float p_ToTopY = 0;
            float p_ToDownY = 0;
            float p_ToRightX = 0;
            float p_ToLeftX = 0;

            //【3D/2D】情况，主角和其他人
            //具备MeshFilter的节点，角色位置，ui/3d
            if (gameRenderType == GameRenderType.TwoD)
            {
                roleEnitySR = roleEnity.GetComponent<SpriteRenderer>();
                p_ToDownY = roleEnitySR.sprite.pivot.y;
                p_ToLeftX = roleEnitySR.sprite.pivot.x;
                p_ToTopY = (roleEnitySR.sprite.rect.height - p_ToDownY) * 2 * 0.22f / 128;
                p_ToRightX = (roleEnitySR.sprite.rect.width - p_ToLeftX) * 2 * 0.22f / 128;
            }
            else if (gameRenderType == GameRenderType.ThreeD)
            {
                //如果外面传错节点传了BaseCollider，就切换称item
                //Star项目不包含那么多特殊情况：[设计情况]
                //if (!isEnvItem && roleEnity.GetComponent<BaseCollider>() != null)
                //{
                //    roleEnity = GetCurOnlyOpenedItem(roleEnity);
                //}

                Vector3 _size;

                int insId = roleEnity.gameObject.GetInstanceID();
                if (!posCache.TryGetValue(insId, out _size))
                {
                    Bounds bds = GetRoleEnityBounds(roleEnity/*.transform.Find("Item")*/);
                    if (bds == null)
                    {
                        return Vector3.zero;
                    }

                    _size = bds.size;
                }

                Vector3 changeVector;

                if (_size.x * 100 <= 5)//0.05 * 100 = 5米 ; 0.05m //说明是1cm = 0.01米的情况-应该是1米 = 1米
                {
                    _size = _size * 100;
                }

                if (pivotPos == PivotPos.Center)
                {
                    changeVector.x = Mathf.Min(_size.x, _size.y, _size.z);
                    changeVector.y = Mathf.Max(_size.x, _size.y, _size.z);
                    changeVector.z = Mathf.Min(_size.x, _size.y, _size.z);
                    p_ToTopY = p_ToDownY = changeVector.y / 2f;
                    p_ToRightX = p_ToLeftX = changeVector.x / 2f;
                }
                else if (pivotPos == PivotPos.Down)
                {
                    ///是否需要变换（上一个项目ZYX有问题）
                    bool needChange = false;
                    if (needChange)
                    {
                        changeVector.x = _size.x;
                        changeVector.y = _size.z;
                        changeVector.z = _size.y;
                    }
                    else
                    {
                        changeVector = _size;
                    }

                    p_ToDownY = 0f;
                    p_ToTopY = changeVector.y;

                    p_ToRightX = p_ToLeftX = changeVector.x / 2f;
                }
            }

            switch (showPosInPlayer)
            {
                case E_AnchorPresets.UpLeft:
                    vector.x = roleEnity.position.x - p_ToLeftX;
                    vector.y = roleEnity.position.y + p_ToTopY;
                    vector.z = roleEnity.position.z;
                    break;
                case E_AnchorPresets.Up:
                    vector.x = roleEnity.position.x;
                    vector.y = roleEnity.position.y + p_ToTopY;
                    vector.z = roleEnity.position.z;
                    break;
                case E_AnchorPresets.UpRight:
                    vector.x = roleEnity.position.x + p_ToRightX;
                    vector.y = roleEnity.position.y + p_ToTopY;
                    vector.z = roleEnity.position.z;
                    break;
                //----------------------------------------------如果是中间过渡值单独研究吧，这个不通用，思路是统一在脚下点，然后统一加法
                case E_AnchorPresets.CenterLeft:
                    vector.x = roleEnity.position.x - p_ToLeftX;
                    vector.y = roleEnity.position.y + ((p_ToTopY - p_ToDownY) / 2f);//锚点在中间就是0，锚点在下面就是高的一半
                    vector.z = roleEnity.position.z;
                    break;
                case E_AnchorPresets.Center:
                    vector.x = roleEnity.position.x;
                    vector.y = roleEnity.position.y + ((p_ToTopY - p_ToDownY) / 2f);
                    vector.z = roleEnity.position.z;
                    break;
                case E_AnchorPresets.CenterRight:
                    vector.x = roleEnity.position.x + p_ToRightX;
                    vector.y = roleEnity.position.y + ((p_ToTopY - p_ToDownY) / 2f);
                    vector.z = roleEnity.position.z;
                    break;
                //----------------------------------------------
                case E_AnchorPresets.DownLeft:
                    vector.x = roleEnity.position.x - p_ToLeftX;
                    vector.y = roleEnity.position.y - p_ToDownY;
                    vector.z = roleEnity.position.z;
                    break;
                case E_AnchorPresets.Down:
                    vector.x = roleEnity.position.x;
                    vector.y = roleEnity.position.y - p_ToDownY;
                    vector.z = roleEnity.position.z;
                    break;
                case E_AnchorPresets.DownRight:
                    vector.x = roleEnity.position.x + p_ToRightX;
                    vector.y = roleEnity.position.y - p_ToDownY;
                    vector.z = roleEnity.position.z;
                    break;
            }
            switch (str)
            {
                case UiOr3D.D3:
                    return vector;  // 这里返回的是【相对模型支撑点】的位置，比如脚下到高度是1.7米
                                    //vector = Camera.main.ScreenToWorldPoint(vector);
                    break;
                case UiOr3D.Ui:
                    if (Canvas.renderMode == RenderMode.ScreenSpaceCamera)
                    {
                        //战斗相机渲染场景，ui相机渲染界面；
                        //物件坐标，在战斗相机，的屏幕位置
                        Vector2 mouseDown = /*Camera.main*/BattleCamera.WorldToScreenPoint(vector);
                        //相同只有一个不同，干掉ui，保留null
                        //三个相机全完事
                        //所有调用UICam枚举都完事
                        /*   bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, mouseDown, UiCam, out mouseUGUIPos1);*/
                        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, mouseDown, null, out mouseUGUIPos1);
                        if (isRect)
                        {
                            return mouseUGUIPos1 + mDialogOffset;
                        }
                    }
                    else if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        Vector2 bubblePos = BattleCamera.WorldToScreenPoint(vector);
                        return bubblePos;
                    }

                    break;
            }
            return vector;
        }

        static Bounds bds1 = new();
        private static Bounds GetRoleEnityBounds(Transform roleEnityItem)
        {


            MeshFilter mf = null;
            SkinnedMeshRenderer smr = null;
            if (roleEnityItem == null)
            {
                return bds1;
            }
            if (roleEnityItem.GetComponent<MeshFilter>() != null)
            {
                mf = roleEnityItem.GetComponent<MeshFilter>();
            }
            else
            {
                //找一层,不用原生方法:耗,还可能配置错误
                //Item下面会配置一堆meshfilter，到底哪个对呢
                //for (int i = 0; i < roleEnityItem.childCount; i++)
                //{
                //    if (roleEnityItem.GetChild(i).GetComponent<MeshFilter>() != null)
                //    {
                //        mf = roleEnityItem.GetChild(i).GetComponent<MeshFilter>();
                //        break;
                //    }
                //}

            }

            if (mf != null)
            {
                bds1 = mf.mesh.bounds;
            }
            //如果找不到MeshFilter,就找SkinM
            else
            {
                FindSkinMeshRender(roleEnityItem, ref bds1, ref smr);
            }

            //那就是动画还没到呢，也要配置
            //那就配置box，我帮他找到位置虽然消耗内存但是保底
            //如果前面找到了就拉倒了不会走本步，没找到的保底机制
            //if (smr == null)
            //{
            //    bds = roleEnityItem.GetComponent<BoxCollider>().bounds;
            //}


            //center是中心点世界坐标，拓展是外围长度拓展，不能用因为外面人物xy要反转
            return bds1;
        }

        private static void FindSkinMeshRender(Transform roleEnity, ref Bounds bds, ref SkinnedMeshRenderer smr)
        {
            if (roleEnity.GetComponent<SkinnedMeshRenderer>() != null)
            {
                smr = roleEnity.GetComponent<SkinnedMeshRenderer>();
            }
            else
            {
                //找一层  
                // 目前的模型给的是：第一个恰巧是body。所以没问题
                for (int i = 0; i < roleEnity.childCount; i++)
                {
                    if (roleEnity.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        smr = roleEnity.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                        break;
                    }
                }

            }

            if (smr != null)
            {
                bds = smr.sharedMesh.bounds;
            }
        }


        private Dictionary<int, Vector3> IDpoolEnityDic = new();
        //    private Transform _SkinTrans;
        //    private Transform SkinTrans
        //    {
        //        get
        //        {
        //            if (_SkinTrans == null)
        //            {
        //                _SkinTrans = PlayerManager.Instance.M_ShinChanRenderModel.transform;
        //            }
        //            return _SkinTrans;
        //        }

        //    }
        //    private Transform _ShinChanRenderModel
        //; private Transform ShinChanRenderModel
        //    {
        //        get
        //        {
        //            if (_ShinChanRenderModel == null)
        //            {
        //                _ShinChanRenderModel = PlayerManager.Instance.M_ShinChanRenderModel.transform;
        //            }
        //            return _ShinChanRenderModel;
        //        }

        //    }


        /// <summary>
        /// 获取坐标
        /// </summary>
        /// <param name="npcId">npcId,小新是0</param>
        /// <param name="posPlayer">位置</param>
        /// <param name="str">UI还是3D</param>
        /// <returns></returns>
        //    public Vector3 PlayerAndNpcPos(int npcId, Pos posPlayer, UiOr3D str)
        //    {
        //        if (!IDpoolEnityDic.ContainsKey(npcId))
        //        {
        //            IDpoolEnityDic.Add(npcId, new Vector3());
        //        }

        //        Vector3 vector = IDpoolEnityDic[npcId];

        //        Vector3 _SkinChan_BoundCache = Vector3.zero;

        //        Transform roleEnity = null;
        //        SpriteRenderer roleEnitySR = null;
        //        float p_ToTopY = 0;
        //        float p_ToDownY = 0;

        //        float p_ToRightX = 0;
        //        float p_ToLeftX = 0;
        //        GameRenderType gameRenderType = GameModeManager.gameRenderType;
        //        if (npcId == 0)//主角的：2d/3d
        //        {
        //            if (gameRenderType == GameRenderType.TwoD)
        //            {
        //                if (PlayerManager.Instance == null)
        //                {
        //                    return Vector3.zero;
        //                }
        //                roleEnity = SkinTrans;
        //                roleEnitySR = roleEnity.GetComponent<SpriteRenderer>();
        //                p_ToDownY = roleEnitySR.sprite.pivot.y;
        //                p_ToLeftX = roleEnitySR.sprite.pivot.x;
        //                p_ToTopY = (roleEnitySR.sprite.rect.height - p_ToDownY) * 2 * 0.22f / 128;
        //                p_ToRightX = (roleEnitySR.sprite.rect.width - p_ToLeftX) * 2 * 0.22f / 128;
        //            }
        //            else if (gameRenderType == GameRenderType.ThreeD)
        //            {
        //                SGF.Debuger.Log(npcId);
        //                Bounds bds;
        //                if (npcId == 0)
        //                {
        //                    Vector3 changeVector;
        //                    if (PlayerManager.Instance == null)
        //                    {
        //                        return Vector3.zero;
        //                    }
        //                    roleEnity = ShinChanRenderModel;
        //                    //bounds.size 0.5.0.4.0.6 ,小新高0.6，前后（后脑到嘴）0.4，肩膀0.5
        //                    //设定一个vector3，x是肩膀==x，y是高矮=z，z是前后=y
        //                    //设定中心就是模型的脚下点
        //                    //小新的模型在中心点,小新bone因动画影响会变动，为了不影响摄像机缓存一帧数值即可
        //                    if (_SkinChan_BoundCache == Vector3.zero)//0就是给新的
        //                    {
        //                        _SkinChan_BoundCache = roleEnity.GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds.size;
        //                    }
        //                    changeVector.x = Mathf.Min(_SkinChan_BoundCache.x, _SkinChan_BoundCache.y, _SkinChan_BoundCache.z);
        //                    changeVector.y = Mathf.Max(_SkinChan_BoundCache.x, _SkinChan_BoundCache.y, _SkinChan_BoundCache.z);
        //                    changeVector.z = Mathf.Min(_SkinChan_BoundCache.x, _SkinChan_BoundCache.y, _SkinChan_BoundCache.z);
        //                    //center0,0,0.3  ;    extend0,2;0,2;0.3
        //                    //*2是显示适配unity压缩，*0.22是数据对其---unity压缩显示---美术比例尺的缩放
        //                    PivotPos pivotPos = PivotPos.Down;
        //                    if (pivotPos == PivotPos.Center)
        //                    {
        //                        p_ToTopY = p_ToDownY = changeVector.y / 2f;
        //                        p_ToRightX = p_ToLeftX = changeVector.x / 2f;
        //                    }
        //                    else if (pivotPos == PivotPos.Down)
        //                    {
        //                        p_ToDownY = 0f;
        //                        p_ToTopY = changeVector.y;

        //                        p_ToRightX = p_ToLeftX = changeVector.x / 2f;
        //                    }
        //                }
        //            }


        //        }
        //        else//其他人的2d/3d
        //        {
        //            var data = GameEventsManager.Instance.NpcId_BuildCol_Maps.GetEnumerator();
        //            while (data.MoveNext())
        //            {
        //                if (data.Current.Key == npcId)
        //                {
        //                    //buildmask=》root=》item//item
        //                    roleEnity = data.Current.Value.transform; //data.Current.Value.transform.parent;
        //                    if (gameRenderType == GameRenderType.TwoD)
        //                    {
        //                        roleEnitySR = roleEnity.GetComponent<SpriteRenderer>();
        //                        p_ToDownY = roleEnitySR.sprite.pivot.y;
        //                        p_ToLeftX = roleEnitySR.sprite.pivot.x;
        //                        p_ToTopY = (roleEnitySR.sprite.rect.height - p_ToDownY) / 128;
        //                        p_ToRightX = (roleEnitySR.sprite.rect.width - p_ToLeftX) / 128;
        //                        break;
        //                    }
        //                    else if (gameRenderType == GameRenderType.ThreeD)
        //                    {
        //                        //如果外面传错节点传了BaseCollider，就切换称item
        //                        if (roleEnity.GetComponent<BaseCollider>() != null)
        //                        {
        //                            //找到FirstItem
        //                            roleEnity = GetCurOnlyOpenedItem(roleEnity);
        //                        }

        //                        //Bounds bds = GetRoleEnityBounds(roleEnity.transform.Find("Item"));
        //                        //自己或者子节点的MeshFilter
        //                        Bounds bds = GetRoleEnityBounds(roleEnity);


        //                        Vector3 changeVector;
        //                        Vector3 _size = bds.size;
        //                        if (_size.x * 100 <= 5)//0.05 * 100 = 5米 ; 0.05m //说明是1cm = 0.01米的情况-应该是1米 = 1米
        //                        {
        //                            _size = _size * 100;
        //                        }

        //                        //美术模型问题多多
        //                        changeVector.y = Mathf.Max(_size.x, _size.y, _size.z);
        //                        changeVector.z = Mathf.Min(_size.x, _size.y, _size.z);
        //                        changeVector.x = Mathf.Min(_size.x, _size.y, _size.z);//x，z不管，随便
        //                        //changeVector.y = _size.z;
        //                        //changeVector.z = _size.y;

        //                        PivotPos pivotPos = PivotPos.Down;
        //                        if (pivotPos == PivotPos.Center)
        //                        {
        //                            p_ToTopY = p_ToDownY = changeVector.y / 2f;
        //                            p_ToRightX = p_ToLeftX = changeVector.x / 2f;
        //                        }
        //                        else if (pivotPos == PivotPos.Down)
        //                        {
        //                            p_ToDownY = 0f;
        //                            p_ToTopY = changeVector.y;
        //                            //*2是显示适配unity压缩，*0.22是数据对其---unity压缩显示---美术比例尺的缩放
        //                            p_ToRightX = p_ToLeftX = changeVector.x / 2f;
        //                        }
        //                    }


        //                }
        //            }
        //        }


        //        switch (posPlayer)
        //        {
        //            case Pos.UpLeft:
        //                vector.x = roleEnity.position.x - p_ToLeftX;
        //                vector.y = roleEnity.position.y + p_ToTopY;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.Up:
        //                vector.x = roleEnity.position.x;
        //                vector.y = roleEnity.position.y + p_ToTopY;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.UpRight:
        //                vector.x = roleEnity.position.x + p_ToRightX;
        //                vector.y = roleEnity.position.y + p_ToTopY;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.CenterLeft:
        //                vector.x = roleEnity.position.x - p_ToLeftX;
        //                vector.y = roleEnity.position.y;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.Center:
        //                vector.x = roleEnity.position.x;
        //                vector.y = roleEnity.position.y;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.CenterRight:
        //                vector.x = roleEnity.position.x + p_ToRightX;
        //                vector.y = roleEnity.position.y;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.DownLeft:
        //                vector.x = roleEnity.position.x - p_ToLeftX;
        //                vector.y = roleEnity.position.y - p_ToDownY;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.Down:
        //                vector.x = roleEnity.position.x;
        //                vector.y = roleEnity.position.y - p_ToDownY;
        //                vector.z = roleEnity.position.z;
        //                break;
        //            case Pos.DownRight:
        //                vector.x = roleEnity.position.x + p_ToRightX;
        //                vector.y = roleEnity.position.y - p_ToDownY;
        //                vector.z = roleEnity.position.z;
        //                break;
        //        }
        //        switch (str)
        //        {
        //            case UiOr3D.D3:
        //                return vector;
        //                //vector = GamePlayMainManager.Instance.UICamera.ScreenToWorldPoint(vector);
        //                break;
        //            case UiOr3D.Ui:
        //                if (Canvas.renderMode == RenderMode.ScreenSpaceCamera)
        //                {
        //                    Vector2 mouseDown = Camera.main.WorldToScreenPoint(vector);
        //                    Vector2 mouseUGUIPos = Vector2.zero;
        //                    bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, mouseDown, UiCam, out mouseUGUIPos);
        //                    if (isRect)
        //                    {
        //                        return mouseUGUIPos + mDialogOffset;
        //                    }
        //                }
        //                else if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        //                {
        //                    Vector2 bubblePos = Camera.main.WorldToScreenPoint(vector);
        //                    return bubblePos;
        //                }
        //                break;
        //        }
        //        return vector;
        //    }

        //    //public Vector3 CenterPos(SpriteRenderer sr)
        //    //{ 
        //    //  sr.sprite.pivot.GetType()
        //    //}
        //}

        // 9个方向枚举
        public enum E_AnchorPresets
        {
            UpLeft,
            Up,
            UpRight,
            CenterLeft,
            Center,
            CenterRight,
            DownLeft,
            Down,
            DownRight,
        }
        public enum UiOr3D
        {
            Ui,
            D3,

        }

        public enum PivotPos
        {
            Center,
            Down,

        }
    }
}