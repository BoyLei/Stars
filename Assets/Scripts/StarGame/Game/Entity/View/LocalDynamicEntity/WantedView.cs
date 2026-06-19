using SGF;
using StarProject.Game.Entity.View.VitalSign.State;
using UnityEngine;

namespace StarProject.Game.Entity.View.LocalDynamic
{
    public class WantedView : ViewInterctive, I_VVitalAnim
    {
        protected override void OnEventListener()
        {
            base.OnEventListener();

            GlobalEvent.OnSceneMapConfigLoad.AddListener(OnSceneLoaded);
        }

        protected override void OffEventListener()
        {
            base.OffEventListener();

            GlobalEvent.OnSceneMapConfigLoad.RemoveListener(OnSceneLoaded);
        }

        /////---------------- 宝箱有个自己的客户端模拟抛物线的掉落动画


        private RaycastHit[] birthHits = new RaycastHit[10];    // 定义RaycastHit数组，存储碰撞信息

        protected override Vector3 GetBirthPos()
        {
            Vector3 pos = m_entity.StartPosition();
            //RaycastHit hit;
            ////bool sd = Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 10.0f, GroundLayer);
            ////bool sd = Physics.Raycast(pos + (Vector3.up * 200), Vector3.down, out hit, 250f, LayerMask.GetMask("Ground"));
            //if (sd)
            //{
            //    pos.y = hit.point.y + 0.08f;
            //}


            Vector3 hitPos = PhysicsUtils.GetHitGroundPos(ref pos, ref birthHits, GroundLayer, out bool result);
            if (result)
            {
                // 0.08 是曲之前的 设置的神之数字, 先同样代码等效替换
                if (Mathf.Abs(pos.y - hitPos.y) >= 0.08f)
                {
                    pos.y = hitPos.y + 0.08f;
                }
            }
            birthHits = new RaycastHit[10];
            return pos;
        }

        private void OnSceneLoaded(int mapID)
        {
            // 本地实体场景加载完毕后，自己再重新找下Y轴位置
            SetBornPosition();
        }

    }
}
