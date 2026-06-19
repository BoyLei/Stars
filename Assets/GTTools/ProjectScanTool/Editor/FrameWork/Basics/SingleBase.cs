/*
 * @Description: 单例基类,继承这个类的都可以实现基本的单例操作
 */
namespace CasualEngine.ProjectScanTool
{
    public abstract class SingleBase<T> where T : new()
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

        protected SingleBase()
        {

        }
    }
}
