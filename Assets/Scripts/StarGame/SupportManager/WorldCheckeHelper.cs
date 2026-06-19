//using SGF.Utlis;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// 1，世界检测器含有世界检测功能，如摄像机检测
///// 2，3D世界坐标，到2D的转换
///// </summary>
//public class WorldCheckeHelper:SGF.Unity.MonoSingletonEx<WorldCheckeHelper> 1，不需要包.  2，是mono单例.  3，不是serviceModule，不是BusinessModule
//{

   

//    Canvas _Canvas;
//    public Canvas Canvas
//    {
//        get
//        {
//            if (_Canvas == null)
//            {
//                _Canvas = UIManager.Instance.CanvasGob.transform.GetComponent<Canvas>();
//            }
//            return _Canvas;
//        }
//        set => _Canvas = value;
//    }

//    Camera _UiCam;
//    public Camera UiCam
//    {
//        get
//        {
//            if (_UiCam == null)
//            {
//                _UiCam = UIManager.Instance.UICamera.GetComponent<Camera>();
//            }
//            return _UiCam;
//        }
//        set => _UiCam = value;
//    }


//    private Vector2 mDialogOffset = new Vector2(0, 0); //默认气泡UI 偏移量
//    private Vector2 uiPosition = new Vector2();
//    // ui组件坐标 -> 世界坐标转换成 -> 屏幕坐标 -> Canvas 下的 局部坐标
//    public Vector2 WorldPositionToAnchorPosition(Transform target)
//    {

//        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(UiCam, target.position);
//        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, screenPoint, UiCam, out uiPosition);
//        return uiPosition;
//    }

//    protected Transform GetCurOnlyOpenedItem(Transform trans)
//    {
//        List<Transform> deepList = trans.parent.DeepFirstTransList("Item");
//        Transform itemObj = null;
//        for (int i = 0; i < deepList.Count; i++)
//        {
//            if (deepList[i].gameObject.activeInHierarchy
//            &&
//            deepList[i].gameObject.name == "Item"
//            )
//            {
//                itemObj = deepList[i].transform;
//                break;
//            }
//        }

//        return itemObj;
//    }

//    private Dictionary<int, Vector3> posCache = new Dictionary<int, Vector3>();
//    private Dictionary<GameObject, Vector3> poolEnityDic = new Dictionary<GameObject, Vector3>();
//    Vector2 mouseUGUIPos1 = new Vector2();
//    /// <summary>
//    /// 3D转3D或屏幕坐标
//    /// </summary>
//    /// <param name="player">目标物体</param>
//    /// <param name="posPlayer">坐标位置</param>
//    /// <param name="str">输出格式</param>
//    /// <returns></returns>
//    public Vector3 PlayerAndNpcPos(Transform sceneEnityItem, Pos posPlayer, UiOr3D str, PivotPos pivotPos, bool isEnvItem = false)
//    {
//        if (!poolEnityDic.ContainsKey(sceneEnityItem.gameObject))
//        {
//            poolEnityDic.Add(sceneEnityItem.gameObject, new Vector3());
//        }

//        Vector3 vector = poolEnityDic[sceneEnityItem.gameObject];

//        Transform roleEnity = null;
//        SpriteRenderer roleEnitySR = null;

//        GameRenderType gameRenderType = GameModeManager.gameRenderType;

//        roleEnity = sceneEnityItem;
//        float p_ToTopY = 0;
//        float p_ToDownY = 0;
//        float p_ToRightX = 0;
//        float p_ToLeftX = 0;

//        //【3D/2D】情况，主角和其他人
//        //具备MeshFilter的节点，角色位置，ui/3d
//        if (gameRenderType == GameRenderType.TwoD)
//        {
//            roleEnitySR = roleEnity.GetComponent<SpriteRenderer>();
//            p_ToDownY = roleEnitySR.sprite.pivot.y;
//            p_ToLeftX = roleEnitySR.sprite.pivot.x;
//            p_ToTopY = (roleEnitySR.sprite.rect.height - p_ToDownY) * 2 * 0.22f / 128;
//            p_ToRightX = (roleEnitySR.sprite.rect.width - p_ToLeftX) * 2 * 0.22f / 128;
//        }
//        else if (gameRenderType == GameRenderType.ThreeD)
//        {
//            //如果外面传错节点传了BaseCollider，就切换称item
//            if (!isEnvItem && roleEnity.GetComponent<BaseCollider>() != null)
//            {
//                roleEnity = GetCurOnlyOpenedItem(roleEnity);
//            }

//            Vector3 _size;

//            int insId = roleEnity.gameObject.GetInstanceID();
//            if (!posCache.TryGetValue(insId, out _size))
//            {
//                Bounds bds = GetRoleEnityBounds(roleEnity/*.transform.Find("Item")*/);
//                if (bds == null)
//                {
//                    return Vector3.zero;
//                }

//                _size = bds.size;
//            }

//            Vector3 changeVector;

//            if (_size.x * 100 <= 5)//0.05 * 100 = 5米 ; 0.05m //说明是1cm = 0.01米的情况-应该是1米 = 1米
//            {
//                _size = _size * 100;
//            }

//            if (pivotPos == PivotPos.Center)
//            {
//                changeVector.x = Mathf.Min(_size.x, _size.y, _size.z);
//                changeVector.y = Mathf.Max(_size.x, _size.y, _size.z);
//                changeVector.z = Mathf.Min(_size.x, _size.y, _size.z);
//                p_ToTopY = p_ToDownY = changeVector.y / 2f;
//                p_ToRightX = p_ToLeftX = changeVector.x / 2f;
//            }
//            else if (pivotPos == PivotPos.Down)
//            {
//                changeVector.x = _size.x;
//                changeVector.y = _size.z;
//                changeVector.z = _size.y;

//                p_ToDownY = 0f;
//                p_ToTopY = changeVector.y;

//                p_ToRightX = p_ToLeftX = changeVector.x / 2f;
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
//                //vector = Camera.main.ScreenToWorldPoint(vector);
//                break;
//            case UiOr3D.Ui:
//                if (Canvas.renderMode == RenderMode.ScreenSpaceCamera)
//                {
//                    Vector2 mouseDown = Camera.main.WorldToScreenPoint(vector);

//                    bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, mouseDown, UiCam, out mouseUGUIPos1);
//                    if (isRect)
//                    {
//                        return mouseUGUIPos1 + mDialogOffset;
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

//    static Bounds bds1 = new Bounds();
//    private static Bounds GetRoleEnityBounds(Transform roleEnityItem)
//    {


//        MeshFilter mf = null;
//        SkinnedMeshRenderer smr = null;
//        if (roleEnityItem == null)
//        {
//            return bds1;
//        }
//        if (roleEnityItem.GetComponent<MeshFilter>() != null)
//        {
//            mf = roleEnityItem.GetComponent<MeshFilter>();
//        }
//        else
//        {
//            //找一层,不用原生方法:耗,还可能配置错误
//            //Item下面会配置一堆meshfilter，到底哪个对呢
//            //for (int i = 0; i < roleEnityItem.childCount; i++)
//            //{
//            //    if (roleEnityItem.GetChild(i).GetComponent<MeshFilter>() != null)
//            //    {
//            //        mf = roleEnityItem.GetChild(i).GetComponent<MeshFilter>();
//            //        break;
//            //    }
//            //}

//        }

//        if (mf != null)
//        {
//            bds1 = mf.mesh.bounds;
//        }
//        //如果找不到MeshFilter,就找SkinM
//        else
//        {
//            FindSkinMeshRender(roleEnityItem, ref bds1, ref smr);
//        }

//        //那就是动画还没到呢，也要配置
//        //那就配置box，我帮他找到位置虽然消耗内存但是保底
//        //如果前面找到了就拉倒了不会走本步，没找到的保底机制
//        //if (smr == null)
//        //{
//        //    bds = roleEnityItem.GetComponent<BoxCollider>().bounds;
//        //}


//        //center是中心点世界坐标，拓展是外围长度拓展，不能用因为外面人物xy要反转
//        return bds1;
//    }

//    private static void FindSkinMeshRender(Transform roleEnity, ref Bounds bds, ref SkinnedMeshRenderer smr)
//    {
//        if (roleEnity.GetComponent<SkinnedMeshRenderer>() != null)
//        {
//            smr = roleEnity.GetComponent<SkinnedMeshRenderer>();
//        }
//        else
//        {
//            //找一层
//            for (int i = 0; i < roleEnity.childCount; i++)
//            {
//                if (roleEnity.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
//                {
//                    smr = roleEnity.GetChild(i).GetComponent<SkinnedMeshRenderer>();
//                    break;
//                }
//            }

//        }

//        if (smr != null)
//        {
//            bds = smr.sharedMesh.bounds;
//        }
//    }


//    private Dictionary<int, Vector3> IDpoolEnityDic = new Dictionary<int, Vector3>();
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


//    /// <summary>
//    /// 获取坐标
//    /// </summary>
//    /// <param name="npcId">npcId,小新是0</param>
//    /// <param name="posPlayer">位置</param>
//    /// <param name="str">UI还是3D</param>
//    /// <returns></returns>
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

//public enum Pos
//{

//    UpLeft,
//    Up,
//    UpRight,
//    CenterLeft,
//    Center,
//    CenterRight,
//    DownLeft,
//    Down,
//    DownRight,
//}
//public enum UiOr3D
//{
//    Ui,
//    D3,

//}

//public enum PivotPos
//{
//    Center,
//    Down,

//}