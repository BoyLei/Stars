using System.Collections.Generic;
using SGF.UI.Framework;

public static class UIWindowStack
{
    static Stack<UIWindow> allWindows = new Stack<UIWindow>();

    public static void pushWindow(UIWindow win)
    {
        // if (win.needRecord)
        // {
        //     if (allWindows.Count == 0 || allWindows.Peek() != win)
        //     {
        //         allWindows.Push(win);
        //     }
        // }
    }

    public static UIWindow popWindow()
    {
        if (allWindows.Count > 0)
        {
            return allWindows.Pop();
        }
        return null;
    }

    public static void onClose(UIWindow win)
    {
        if (!string.IsNullOrEmpty(win.parentWin))
        {
            //UIManager.Instance.OpenWindow(win.parentWin, null, StarProjectDef.MainPageCommond.HideBoth);
            UIManager.Instance.OpenWindowAsync(win.parentWin, null, null, StarProjectDef.MainPageCommond.HideBoth);
            win.parentWin = "";
        }
    }
}