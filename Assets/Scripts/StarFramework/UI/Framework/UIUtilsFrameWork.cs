using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SGF.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SGF.UI.Framework
{
    /// <summary>
    /// 为UI操作提供基础封装，使UI操作更方便,框架逻辑
    /// </summary>
    public static class UIUtilsFrameWork
    {
        /// <summary>
        /// 设置一个UI元素是否可见
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="value"></param>
        public static void SetActive(UIBehaviour ui, bool value)
        {
            if (ui != null && ui.gameObject != null)
            {
                GameObjectUtils.SetActiveRecursively(ui.gameObject, value);
            }
        }


        public static void SetButtonText(Button btn, string text)
        {
            Text objText = btn.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }

		public static string GetButtonText(Button btn)
		{
			Text objText = btn.transform.GetComponentInChildren<Text>();
			if (objText != null)
			{
				return objText.text;
			}
            return "";
		}

        public static void SetChildText(UIBehaviour ui, string text)
        {
            Text objText = ui.transform.GetComponentInChildren<Text>();
            if (objText != null)
            {
                objText.text = text;
            }
        }

        public static void ChangeLayer(Transform trans, string targetLayer)
        {
            if (LayerMask.NameToLayer(targetLayer) == -1)
            {
                SGF.Debuger.LogWarning("Layer中不存在,请手动添加LayerName");
                return;
            }
            //遍历更改所有子物体layer
            trans.gameObject.layer = LayerMask.NameToLayer(targetLayer);
            foreach (Transform child in trans)
            {
                ChangeLayer(child, targetLayer);
            }
        }
    }
}
