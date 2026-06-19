////////////////////////////////////////////////////////////////////
//                            _ooOoo_                             //
//                           o8888888o                            //
//                           88" . "88                            //
//                           (| ^_^ |)                            //
//                           O\  =  /O                            //
//                        ____/`---'\____                         //
//                      .'  \\|     |//  `.                       //
//                     /  \\|||  :  |||//  \                      //
//                    /  _||||| -:- |||||-  \                     //
//                    |   | \\\  -  /// |   |                     //
//                    | \_|  ''\---/''  |   |                     //
//                    \  .-\__  `-`  ___/-. /                     //
//                  ___`. .'  /--.--\  `. . ___                   //
//                ."" '<  `.___\_<|>_/___.'  >'"".                //
//              | | :  `- \`.;`\ _ /`;.`/ - ` : | |               //
//              \  \ `-.   \_ __\ /__ _/   .-` /  /               //
//        ========`-.____`-.___\_____/___.-`____.-'========       //
//                             `=---='                            //
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      //
//            佛祖保佑       无BUG        不修改                   //
////////////////////////////////////////////////////////////////////
/*
* 描述：
* 工程 ：StarProject
*/
using System;
using XLua;

namespace SGF.Module.Framework
{
  //所有都是链接第三方的，Unity模块的
    public abstract class ServiceModule<T> : Module where T : ServiceModule<T>, new()
    {
        private static T ms_instance = default(T);

        /// <summary>
        /// 用于实现单例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (ms_instance == null)
                {
                    ms_instance = new T();
                }

                return ms_instance;
            }
        }

        /// <summary>
        /// 调用它以创建模块
        /// 并且检查它是否以单例形式创建
        /// </summary>
        /// <param name="args"></param>
        protected void CheckSingleton()
        {
            DateTime currentTime = DateTime.Now;
            UnityEngine.Debug.Log($"Current time is: {currentTime.ToString("HH:mm:ss")}" + GetType().Name + "模块启动");


            if (ms_instance == null)
            {
                var exp = new Exception("ServiceModule<" + typeof(T).Name + "> 无法直接实例化，因为它是一个单例!");
                throw exp;
            }
        }

        public override void Release()
        {
            base.Release();
        }
    }
}
