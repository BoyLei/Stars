using SGF.UI.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SGF.Utlis
{
    
    [XLua.LuaCallCSharp]
    public  class CommonFunction
    {
        /// <summary>
        /// 视口坐标系是将Game视图的屏幕坐标系单位化，左下角（0，0），右上角（1，1）
        /// 摄像机视口坐标系下的坐标范围
        /// </summary>
        static Rect viewRect = new Rect(0, 0, 1, 1);

        /// <summary>
        /// 以Game视图为准
        /// 判断2D坐标系下的一个点是否在指定的摄像机视野范围内
        /// 忽略z轴
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="uiWorldPos"></param>
        /// <returns></returns>
        public static bool Is2DPointInsideCameraView(Camera camera, Vector3 uiWorldPos)
        {
            if (null == camera)
            {
                return false;
            }

            Vector3 viewPos = camera.WorldToViewportPoint(uiWorldPos);
            return viewRect.Contains(viewPos);
        }

        /// <summary>     /// 以Game视图为准
        /// 判断3D坐标系下的一个点是否在指定的摄像机视野范围内 
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="worldPos"></param>
        /// <returns></returns>
        public static bool Is3DPointInsideCameraView(Camera camera, Vector3 worldPos)
        {
            if (null == camera)
            {
                return false;
            }

            //获取摄像机的六个平面
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            for (int i = 0, iMax = planes.Length; i < iMax; ++i)
            {
                //判断一个点是否在平面的正方向
                if (!planes[i].GetSide(worldPos))
                {
                    return false;
                }
            }

            return true;
        }


        public static Vector3 ScreenPointToCanvas()
        {
            Vector2 canvasPos = Vector2.zero;
            var canvas = UIManager.Instance.M_Canvas;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, UnityEngine.Input.mousePosition, canvas.worldCamera, out canvasPos);
            return canvasPos;
        }

        public static Vector3 ScreenPointToCanvas2()
        {
            var canvas = UIManager.Instance.M_Canvas;
            RectTransform canvasRT = canvas.GetComponent<RectTransform>();
            float x = UnityEngine.Input.mousePosition.x / Screen.width * canvasRT.rect.width;
            float y = UnityEngine.Input.mousePosition.y / Screen.height * canvasRT.rect.height;

            float px = x - canvasRT.rect.width / 2;
            float py = y - canvasRT.rect.height / 2;

            return new Vector3(px, py, 0);
        }


        public static int PointerOverGameObject(List<GameObject> objs)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = UnityEngine.Input.mousePosition;
            GraphicRaycaster gr = UIRoot.UiWindowRoot.GetComponent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(pointerEventData, results);
            if (results.Count != 0)
            {
                foreach (var r in results)
                {
                    for (int i = 0; i < objs.Count; i++)
                    {
                        if (r.gameObject == objs[i])
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public static GameObject PointerOverGameObject2(List<GameObject> objs)
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = UnityEngine.Input.mousePosition;
            GraphicRaycaster gr = UIRoot.UiWindowRoot.GetComponent<GraphicRaycaster>();
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(pointerEventData, results);
            if (results.Count != 0)
            {
                foreach (var r in results)
                {
                    for (int i = 0; i < objs.Count; i++)
                    {
                        if (r.gameObject == objs[i])
                        {
                            return r.gameObject;
                        }
                    }
                }
            }
            return null;
        }
    }

}