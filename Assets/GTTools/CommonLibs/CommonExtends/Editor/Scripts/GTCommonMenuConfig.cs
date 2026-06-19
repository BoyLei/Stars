/*
 * @Description: 编辑器一些配置项，GameTech开发人员维护，各项目组谨慎修改
 */
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameTechTools.CommonLibs.CommonExtends
{
    [InitializeOnLoad]
    public class GTCommonMenuConfig
    {
        public static Texture menublue_tex;
        public static Texture menuwhite_tex;
        public static Texture menuarrow_tex;
        public static Texture menuhover_tex;
        public static string res_dir = "Assets/GTTools/CommonLibs/CommonExtends/Editor/Res/SketchImg/";
        public static string menublue_path = res_dir + "GTCommonMenu_hoverblue.png";
        public static string menuwhite_path = res_dir + "GTCommonMenu_normalwhite.png";
        public static string menuarrow_path = res_dir + "GTCommonMenu_arrow.png";
        public static string menuhover_path = res_dir + "GTCommonMenu_menuhover.png";

        static GTCommonMenuConfig()
        {
            InitRes();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptReload()
        {
            InitRes();
        }

        public static void InitRes()
        {
            menublue_tex = AssetDatabase.LoadAssetAtPath<Texture>(menublue_path);
            menuwhite_tex = AssetDatabase.LoadAssetAtPath<Texture>(menuwhite_path);
            menuarrow_tex = AssetDatabase.LoadAssetAtPath<Texture>(menuarrow_path);
            menuhover_tex = AssetDatabase.LoadAssetAtPath<Texture>(menuhover_path);
        }

        public static Texture GetCustomOdinMenuIcon(string iconName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture>(GTCommonMenuConfig.res_dir + iconName + ".png");
        }

    }
}
#endif