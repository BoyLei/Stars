///--------------------------------------------------------------------
/// 文件名   :   ClientEntityMoveLogic.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2024/03/05 15:30:24
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using StarProject.Game.Player;
using StarProject.Service.FindPath;
using UnityEngine;

namespace ClientNpc
{
    public class ClientEntityMoveLogic
    {
        protected EntityCtrlBase m_Entity;
        
        

        public ClientEntityMoveLogic(EntityCtrlBase entity)
        {
            m_Entity = entity;
        }
        
        
        public void MoveByPosition(Vector3 position,System.Action cb)
        {
            var  Paths= new List<Vector3>();
            if (FindPathManager.Instance.FindPath(m_Entity.M_Curr.Position(), position, 1, out UnityEngine.Vector3[] potions) && potions != null && potions.Length > 0)
            {
                foreach (var item in potions)
                {
                    Paths.Add(item);
                }
            }

            MoveByPath(Paths.ToArray(),cb);
            // m_Entity.M_Curr.Speed = m_Entity.MoveSpeed;
        }

        public void MoveByPath(Vector3[] paths,System.Action cb)
        {
            m_Entity.M_Curr.SetWayPointData(paths, (result) =>
            {
                cb?.Invoke();
            });
        }
    }
}
