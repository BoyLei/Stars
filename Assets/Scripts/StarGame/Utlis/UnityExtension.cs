using UnityEngine;
/// <summary>
/// UnityExtension拓展transform的    
/// 用this  类似lua  self.  
/// 以后要拓展深度和广度遍历递归
/// </summary>
namespace SGF.Utlis
{
    public static class UnityExtension
    {
        /// <summary>
        /// transform类型递归查找子物体
        /// </summary>
        /// <returns>返回需要查找的子物体.</returns>
        /// <param name="parent">查找起点.</param>
        /// <param name="targetName">需要查找的子物体名字.</param>
        public static Transform FindFunc(Transform parent, string targetName)
        {
            Transform target = parent.Find(targetName);
            //如果找到了直接返回
            if (target != null)
                return target;
            //如果没有没有找到，说明没有在该子层级，则先遍历该层级所有transform，然后通过递归继续查找----再次调用该方法
            for (int i = 0; i < parent.childCount; i++)
            {
                //通过再次调用该方法递归下一层级子物体
                target = FindFunc(parent.GetChild(i), targetName);

                if (target!=null)
                    return target;
            }
            return target;
        }

        /// <summary>
        /// 泛型查找:深度优先
        /// </summary>
        /// <returns>返回需要查找的子物体的类型.</returns>
        /// <param name="parent">查找起点.</param>
        /// <param name="targetName">需要查找的子物体名字.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T FindFunc<T>(Transform parent, string targetName) where T : Component
        {
            Transform target = parent.Find(targetName);
            if (target!=null)
            {
                return target.GetComponent<T>();
            }
            for (int i = 0; i < parent.childCount; i++)
            {
                target = FindFunc(parent.GetChild(i), targetName);
                if (target!=null)
                {
                    return target.GetComponent<T>();
                }
            }
            return target.GetComponent<T>();
        }
    }
}

