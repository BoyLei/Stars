using System;
using UnityEngine;

namespace SickDev.DevConsole {
    [Serializable]
    public class OpenButton {
        public float width;
        public float height;
        private Rect rect = new Rect();
        private GUIContent guiContent = new GUIContent();
        public void Draw(float positionY)
        {
            rect.x = (Screen.width / 2) / DevConsole.settings.scale - width / 2;
            rect.y = positionY;
            rect.width = width;
            rect.height = height;
            guiContent.image = DevConsole.singleton.isOpen
                ? DevConsole.settings.closeConsoleIcon
                : DevConsole.settings.openConsoleIcon;
            if (GUIUtils.DrawCenteredButton(rect, guiContent, DevConsole.settings.mainColor))
                DevConsole.singleton.ToggleOpen();
        }
    }
}
