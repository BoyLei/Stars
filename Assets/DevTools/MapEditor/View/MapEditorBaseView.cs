#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{

    public abstract class MapEditorBaseView
    {
        /// <summary>
        /// 视图名称
        /// </summary>
        protected string ViewName;

        protected System.Action<string> OnShowMessage;
        
        /// <summary>
        /// 父节点
        /// </summary>
        protected Transform Parent {  get; private set; }

        
        /// <summary>
        /// 根节点
        /// </summary>
        protected Transform Root{  get; set; }

        /// <summary>
        /// 当前记录最大值
        /// </summary>
        protected int MaxIndex;
        
        public MapEditorBaseView(string viewName,System.Action<string> action)
        {
            ViewName = viewName;
            OnShowMessage = action;
        }

        /// <summary>
        /// 地图加载
        /// </summary>
        public virtual bool Load(SceneJsonData jsonData,Transform parent)
        {
            Parent = parent;
            Root = MapEditorUtils.CreatePort(ViewName,Parent);
            return true;
        }
        
        /// <summary>
        /// 地图卸载
        /// </summary>
        public virtual void UnLoad()
        {
            
        }

        /// <summary>
        /// GUI绘制
        /// </summary>
        public abstract void OnGUI();

        protected Area GetAreaById(int areaId)
        {
            if (areaId == 0)
            {
                return null;
            }

            Transform allRoot;
            if (Root != null)
            {
                allRoot = Root.parent;
                if (allRoot != null)
                {
                    Transform areasRoot = allRoot.Find("Areas");
                    if (areasRoot != null)
                    {
                        Area[] areas = areasRoot.GetComponentsInChildren<Area>();
                        foreach (var item in areas)
                        {
                            if (item != null && item.AreaID == areaId)
                            {
                                return item;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 创建子节点
        /// </summary>
        public abstract void CreateChild();

        /// <summary>
        /// 创建子节点
        /// </summary>
        /// <param name="userData"></param>
        public abstract void CreateChild(object userData);
        
        /// <summary>
        /// 导出Json
        /// </summary>
        public abstract void ExportJson(SceneJsonData sceneJson);
    }

}
#endif
