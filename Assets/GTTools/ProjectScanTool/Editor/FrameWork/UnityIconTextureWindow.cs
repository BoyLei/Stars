/*
 * @Description: 展示unity引擎系统默认的图标，方便编辑器随时查看
 */

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace CasualEngine.ProjectScanTool
{

    public class UnityIconsOverView : OdinSelector<object>
    {
        /// <summary>
        /// Opens a window which displays a list of all icons available from <see cref="EditorIcons"/>.
        /// </summary>
        public static void OpenEditorIconsOverview()
        {
            var window = OdinEditorWindow.InspectObject(new UnityIconsOverView());
            window.ShowUtility();
            window.WindowPadding = new Vector4();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(500, 700);
            window.minSize = new Vector2(500, 700);
        }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            this.DrawConfirmSelectionButton = false;
            tree.Config.DrawSearchToolbar = true;
            tree.DefaultMenuStyle.Height = 25;

            Texture2D[] t = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (Texture2D x in t)
            {
                var path = AssetDatabase.GetAssetPath(x);
                if (!string.IsNullOrEmpty(path) && (path.IndexOf("Assets/Res/") > -1 || path.IndexOf("Assets/Resources/") > -1 ))
                {
                    continue;
                }
                Debug.unityLogger.logEnabled = false;
                GUIContent gc = EditorGUIUtility.IconContent(x.name);
                Debug.unityLogger.logEnabled = true;
                tree.Add(x.name, x.name, x);
            }
        }

        [ShowInInspector, PropertyOrder(30)]
        [PropertyRange(10, 34), LabelWidth(50)]
        [InfoBox("使用EditorGUIUtility.TrIconContent(IconName).image获得")]
        private float Size
        {
            get { return this.SelectionTree.DefaultMenuStyle.IconSize; }
            set
            {
                this.SelectionTree.DefaultMenuStyle.IconSize = value;
                this.SelectionTree.DefaultMenuStyle.Height = (int)value + 9;
            }
        }
    }

    class UnityIconTextureWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Odin Inspector/OdinEditor Icons")]
        public static void OpenOdinBtnIconPreview()
        {
            EditorIconsOverview.OpenEditorIconsOverview();
        }

        [MenuItem("Tools/Odin Inspector/Unity Icons")]
        public static void OpenUintyIconsPreview()
        {
            UnityIconsOverView.OpenEditorIconsOverview();
        }
    }
}
