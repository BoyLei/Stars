using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.TouchScreenKeyboard;

public class InputHeight : MonoBehaviour
{
    private AndroidJavaObject unityActivity;
    private AndroidJavaObject unityContext;
    private AndroidJavaClass unityPlayer;
    private AndroidJavaObject rootView;

    InputField input;
    public Canvas canvas;

    void Start()
    {
        input = GetComponent<InputField>();

        unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        unityContext = unityActivity.Call<AndroidJavaObject>("getApplicationContext");

        rootView = unityActivity.Call<AndroidJavaObject>("getWindow")
            .Call<AndroidJavaObject>("getDecorView")
            .Call<AndroidJavaObject>("getRootView");
    }

    public static int GetRelativeKeyboardHeight(RectTransform rectTransform, bool includeInput)
    {
        int keyboardHeight = GetKeyboardHeight(includeInput);
        float screenToRectRatio = Screen.height / rectTransform.rect.height;
        float keyboardHeightRelativeToRect = keyboardHeight / screenToRectRatio;

        return (int)keyboardHeightRelativeToRect;
    }

    private static int GetKeyboardHeight(bool includeInput)
    {
// #if UNITY_EDITOR
//         return 0;
// #elif UNITY_ANDROID
//         if (lastkeyboardHeight > 0)
//         {
//             return lastkeyboardHeight;
//         }
//         using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
//         {
//             AndroidJavaObject unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
//             AndroidJavaObject view = unityPlayer.Call<AndroidJavaObject>("getView");
//             AndroidJavaObject dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
//             if (view == null || dialog == null)
//                 return 0;
//             var decorHeight = 0;
//             if (includeInput)
//             {
//                 AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
//                 if (decorView != null)
//                     decorHeight = decorView.Call<int>("getHeight");
//             }
//             using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
//             {
//                 view.Call("getWindowVisibleDisplayFrame", rect);
//                 int h = Screen.height - rect.Call<int>("height") + decorHeight;
//                 if(h > 0)
//                 {
//                     lastkeyboardHeight = h;
//                 }
//                 return h;
//             }
//         }
// #elif UNITY_IOS
//         return (int)TouchScreenKeyboard.area.height;
// #endif
        return 0;
    }

    public void OnHideText()
    {
        input.DeactivateInputField();
    }

    public void OnShowText()
    {
        input.ActivateInputField();
    }

    void LateUpdate()
    {
        if (input.isFocused)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                int h = 0;
                h = GetRelativeKeyboardHeight(canvas.GetComponent<RectTransform>(), false);

                input.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, h, 0);
            }
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                input.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
            }
        }
    }
}
