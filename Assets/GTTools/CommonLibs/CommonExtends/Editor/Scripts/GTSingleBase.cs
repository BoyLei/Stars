/*
 * @Description: 单例基类，GameTech开发人员维护，各项目组谨慎修改
 */
/// <summary>
/// 单例基类,继承这个类的都可以实现基本的单例操作
/// </summary>
namespace GameTechTools.CommonLibs.CommonExtends
{
    public abstract class GTSingleBase<T> where T : new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (null == instance)
                    instance = new T();

                return instance;
            }
        }

        protected GTSingleBase()
        {

        }
    }
}
