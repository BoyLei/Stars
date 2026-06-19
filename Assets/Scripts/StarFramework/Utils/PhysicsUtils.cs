using UnityEngine;

namespace SGF
{
    public static class PhysicsUtils
    {
        /// <summary>
        /// 从 startPos 向下 射线检测的 hitLayer 层的 坐标
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="raycastHits"></param>
        /// <param name="hitLayer"></param>
        /// <param name="result">是否命中</param>
        /// <returns></returns>
        public static Vector3 GetHitGroundPos(ref Vector3 startPos, ref RaycastHit[] raycastHits, int hitLayer, out bool result)
        {
            result = false;
            var physicsPos = startPos + Vector3.up;
            float maxDistance = 5f;
            int hitCount = Physics.RaycastNonAlloc(physicsPos, Vector3.down, raycastHits, maxDistance);    // 发射射线并检测碰撞
            if (hitCount == 0)
            {
                return Vector3.zero;
            }

            // 取离脚最小的地面
            var rayHit = raycastHits.MinBy((item) =>
               {
                   if (item.collider != null && item.collider.gameObject != null)
                   {
                       if (item.collider.gameObject.layer == hitLayer)
                       {
                           return physicsPos.y - item.point.y;
                       }
                   }

                   // 如果射线点 不是地面, 那就过滤这个点.直接将这个点 的距离 超过最大居里点就可以过滤
                   return maxDistance + 1;
               });

            return rayHit.point;
        }
        static Ray ray = new Ray();
        static int groudLayer = LayerMask.GetMask("Ground");
        public static Vector3 GetGroundPoint(Vector3 point,
    QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            Vector3 groundPoint = point;
            groundPoint.y = 0;
            float height = Mathf.Min(point.y, 990f) + 10;

            ray.origin = groundPoint + Vector3.up * height;
            ray.direction = Vector3.down;

            if (Physics.Raycast(ray, out var hit, 20f, groudLayer, queryTriggerInteraction))
            {
                // Debug.Log(hitInfo.transform.gameObject.name);
                groundPoint.y = hit.point.y;
            }
            else
            {
                groundPoint = point;
                //Debug.LogWarningFormat("无法在{0}处获取场景地面的高度信息。", point);
            }

            return groundPoint;
        }
    }
}
