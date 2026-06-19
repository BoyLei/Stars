/*
 * @Description: 设置界面
 */
using System;
using Sirenix.OdinInspector;

namespace GameTechTools.UIHelpsTool
{
    [System.Serializable]
    internal class UIHelpsToolSetting : UIHelpsCustomObject
    {
        [BoxGroup("确认项目设置")]
        [InfoBox("请务必正确设置自己项目的分辨率，因为预览的效果图需要依据您设置的分辨率而进行不同尺寸的渲染显示")]
        [LabelText("Width"), PropertySpace(SpaceBefore = 5)]
        public int eScreenStdWidth = 1920;

        [BoxGroup("确认项目设置")]
        [LabelText("Height")]
        public int eScreenStdHeight = 1080;

        [BoxGroup("确认项目设置")]
        [InfoBox("请先查看项目中贴图(SpriteUI格式)的Pixels Per Unit值的设置")]
        public int pixelsPerUnit = 100;

        // [BoxGroup("预览工具操作提示")]
        [Title("操作提示", Bold = false)]
        [DisplayAsString(false), HideLabel, NonSerialized, ShowInInspector, PropertySpace(SpaceBefore = 2)]
        public string optionTips = ""
        + string.Format("1、<color=#FF8C00>【新增预览分类】\n</color>{0}", "点击菜单【操作】->【新增分类】即可增加通用件分类。添加后默认会创建一个【预览分类】目录，所有的分类都会放在此目录下\n\n")
        + string.Format("2、<color=#FF8C00>【修改和删除预览分类】\n</color>{0}", "右键分类菜单列表可以修改分类名称和删除对应的分类\n\n")
        + string.Format("3、<color=#FF8C00>【新增Prefab预览图】\n</color>{0}", "拖拽Prefab到面板空白区域即可添加预览，可以一次性拖拽多个哦\n\n")
        + string.Format("4、<color=#FF8C00>【Prefab预览进阶功能】\n</color>{0}", string.Format("<color=#1E90FF>{0}</color>", "鼠标移动到预览图") + "上即可查看对应Prefab资源路径，\n"
        + string.Format("<color=#1E90FF>{0}</color>", "单击预览图") + "会自动定位对应Prefab在Assets文件系统位置，\n"
        + string.Format("<color=#1E90FF>{0}</color>", "双击预览图") +"即可打开对应的Prefab，还可以直接" 
        + string.Format("<color=#1E90FF>{0}</color>", "拖拽预览图") + "添加到Hierarchy和Inspector面板中，和操作Prefab本体是一个效果\n\n")
        + "5、点击菜单【操作】->【详细说明文档】可以查看更多信息\n\n"
            + "6、遇到问题可以咨询GameTech相关工具开发人员";

        private void OnEnable()
        {
            menutype = EnumMenuItemType.eGeneralSetting;
        }
    }
}