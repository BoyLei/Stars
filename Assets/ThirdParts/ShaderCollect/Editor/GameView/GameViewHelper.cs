using System;
using UnityEditor;

namespace GameExtensions
{
    public static class GameViewHelper
    {
        public static void RepaintAll()
        {
#if UNITY_EDITOR
            PlayModeView.RepaintAll();
            GameView.RepaintAll();
#endif
        }

        public static void OpenGameView()
        {
#if UNITY_EDITOR
            EditorWindow.GetWindow<GameView>();
#endif
        }
    }
}
