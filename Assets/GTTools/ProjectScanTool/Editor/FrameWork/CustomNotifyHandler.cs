/*
 * @Description: 自定义事件
 */
using System;
namespace CasualEngine.ProjectScanTool
{
    public class CustomNotifyHandler
    {
        //通知开启或关闭所有类型
        public static Action<EnumScanEnable> CCNotifyOpenOrCloseAllScan;
        //通知扫描所有类型
        public static Action CCNotifyScanAllTypes;
    }
}
