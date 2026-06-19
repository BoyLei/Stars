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
using SGF.Module.Framework;
using StarProject.Service.Input;
using StarProject.Service.Sound;
using StarProjectDef;
using UnityEngine;

namespace SGF.Unity
{
    public class APPHelper : MonoSingletonEx<APPHelper>
    {
        private bool isPause = false;
        private bool isFocus = false;

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        private void OnEnable()
        {
            isPause = false;
            isFocus = false;
        }

        public void InitAPPHelper()
        {
            isPause = false;
            isFocus = false;
            Application.wantsToQuit += OnwantsToQuit;//退出拦截  
        }

        private void OnApplicationPause(bool pause)
        {
            SGF.Debuger.Log($"OnApplicationPause pause={pause},isPause={isPause},isFocus={isFocus}");
            SoundManager.Instance.SetApplicationPauseMusic(pause);

//#if UNITY_IPHONE || UNITY_ANDROID
//            UnityEngine.Debug.Log($"OnApplicationPause 1111  isPause={isPause},isFocus={isFocus}");
//            if (!isPause)
//            {
//                // 强制暂停时，事件
//                //pauseTime();
//            }
//            else
//            {
//                isFocus = true;
//            }
//            isPause = true;
//            UnityEngine.Debug.Log($"OnApplicationPause 2222  isPause={isPause},isFocus={isFocus}");
//#endif
        }

//        private void OnApplicationFocus(bool focus)
//        {
//            UnityEngine.Debug.Log($"OnApplicationFocus focus={focus},isPause={isPause},isFocus={isFocus}");
//#if UNITY_IPHONE || UNITY_ANDROID
//            UnityEngine.Debug.Log($"OnApplicationFocus 1111 isPause={isPause},isFocus={isFocus}");
//            if (isFocus)
//            {
//                // “启动”手机时，事件
//                isPause = false;
//                isFocus = false;
//            }
//            if (isPause)
//            {
//                isFocus = false;
//            }
//            UnityEngine.Debug.Log($"OnApplicationFocus 2222 isPause={isPause},isFocus={isFocus}");
//#endif
//        }

        private void OnApplicationQuit()
        {
            SGF.Debuger.Log($"OnApplicationQuit 退出");
        }

        private bool OnwantsToQuit()
        {
            SGF.Debuger.Log($"OnwantsToQuit 退出拦截");
            return true;
        }

        // //游戏失去焦点也就是进入后台时 focus为false 切换回前台时 focus为true
        private void OnApplicationFocus(bool focus)
        {
            SGF.Debuger.Log($"OnApplicationFocus 焦点={focus}");
            if (focus)
            {
                //切换到前台时执行，游戏启动时执行一次
            }
            else
            {
                //切换到后台时执行
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.ClearMoveCommand();
                }
                ModuleManager.Instance.SendMessage(ModuleDef.Name.PlayerPreviewModule, "ClosePlayerFuncTips", new object[] { });
                ModuleManager.Instance.SendMessage(ModuleDef.Name.ItemTipsModule, "OnCloseTips", new object[] { });
            }
        }
    }
}
