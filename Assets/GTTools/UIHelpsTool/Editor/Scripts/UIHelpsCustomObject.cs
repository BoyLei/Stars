/*
 * @Description: 菜单栏的对象都继承这个基类，方便将来对类型的扩展
 */
using UnityEngine;

namespace GameTechTools.UIHelpsTool
{
    internal class UIHelpsCustomObject : ScriptableObject
    {
        [HideInInspector]
        public EnumMenuItemType menutype;
    }
}