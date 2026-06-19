using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO:物件，什么的慢慢加吧
/// 物件和怪物，其实也可以不加，随着场景切换自己删除即可
/// 
/// 
/// 
/// 只是在这个节点下，服务器要删就删除（我们存储nnt字典，并不是通过root）
/// 但是如果有变化的话，我们也要变更对应的root节点
/// 这里只管初始化，部管删除
/// </summary>
/// 
[XLua.LuaCallCSharp]
public class EntityRoot : SGF.Unity.MonoSingletonEx<EntityRoot>
{
    public GameObject DotRemoveRoot; //组队不会清除，自己/（**伙伴/自己的召唤物）-服务器发不发都行
    public GameObject RemoveRoot; //换服务，换场景，换线AOI都会清除其他人
}
