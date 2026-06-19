using StarProject.Service.Cam;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XLua;

namespace Frame
{
    [LuaCallCSharp]//游戏业务逻辑
    public static class UIUtil
    {
        static Material _GrayMaterial;
        public static Material GrayMaterial
        {
            get
            {
                if (_GrayMaterial == null)
                {
                    Shader sd = Shader.Find("UI/Gray");
                    _GrayMaterial = new Material(sd);
                }
                return _GrayMaterial;
            }
        }
        public static void GrayUI(UIBehaviour ui, bool gray, bool changeMouseEnable = true)
        {
            //return;
            if (ui is Selectable)
            {
                if (changeMouseEnable) (ui as Selectable).interactable = !gray;
                Image img = (ui as Selectable).image;
                img.material = gray ? UIUtil.GrayMaterial : null;
                //ui.enabled = !gray;
            }
            else if (ui is Image)
            {
                Image img = ui as Image;
                img.material = gray ? UIUtil.GrayMaterial : null;
            }
            //Image img = ui.GetComponent<Image>();
        }
        public static GameObject Instantiate(string path, GameObject parent)
        {
            GameObject ui = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(path));
            ui.gameObject.transform.parent = parent.GetComponentInChildren<Canvas>().gameObject.transform;
            ui.gameObject.transform.localScale = new Vector2(1, 1);
            ui.gameObject.transform.localPosition = new Vector2(0, 0);
            return ui;
        }
         
      


        static public T AddChild<T>(GameObject parent) where T : UIBehaviour
        {
            GameObject go = AddChild(parent);
            go.name = typeof(T).ToString();
            return go.AddComponent<T>();
        }
        static public GameObject AddChild(GameObject parent)
        {
            GameObject go = new GameObject();
            go.AddComponent<RectTransform>();
            //go.AddComponent<CanvasRenderer>();
            if (parent != null)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = parent.layer;
            }
            return go;
        }
        static public T[] FindActive<T>() where T : Component
        {
            return GameObject.FindObjectsOfType(typeof(T)) as T[];
        }
        static public string GetRichText(string str, string color)
        {
            return GetRichText(str, color, false);
        }
        static public string GetRichText(string str, string color, bool b, int size = -1)
        {
            string text = str;
            if (!string.IsNullOrEmpty(color))
                text = string.Format("<color=#{0}>{1}</color>", color, text);
            if (b)
            {
                text = string.Format("<b>{0}</b>", text);
            }
            if (size > 0)
            {
                text = string.Format("<size={0}>{1}</size>", size, text);

            }
            return text;
        }
        public static bool IsInputField()
        {
            if (EventSystem.current.currentSelectedGameObject == null)
                return false;
            InputField tf = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            return tf != null;
        }
        public static void SlowMove(RectTransform target, RectTransform slow, float speed = 2f)
        {
            slow.anchorMax = Vector2.Lerp(slow.anchorMax, target.anchorMax, Time.deltaTime * speed);
        }
        //public static void SetImg(this Image img, string spriteName, Action<Image> ac = null, bool enable = true)
        //{
        //    ResourceManager.SetImg(img, spriteName, ac, enable);
        //}

        //public static int GetDepth(this UIBehaviour ui,bool up = true)
        //{
        //    Canvas cvs = ui.GetComponentInParent<Canvas>();
        //    int depth = cvs.sortingOrder;
        //    if(up)
        //        depth += cvs.transform.childCount + 1;
        //    return depth;
        //}

        public static Vector3 Parabola(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            return (1 - t) * (1 - t) * p0 + 2 * t * (1 - t) * p1 + t * t * p2;
        }



        public static Texture2D CaptureCamera(Camera ca, Rect rect)
        {
            bool noRT = ca.targetTexture == null;
            RenderTexture rt = noRT ? new RenderTexture((int)rect.width, (int)rect.height, 0) : ca.targetTexture;
            rt.antiAliasing = 2;
            if (noRT)
                ca.targetTexture = rt;
            ca.Render();
            //ps:可以增加相机
            //
            RenderTexture.active = rt;

            Texture2D tex = new Texture2D(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            if (noRT)
                ca.targetTexture = null;
            RenderTexture.active = null;
            GameObject.Destroy(rt);

            string name = Application.streamingAssetsPath + "/renderTexture" + System.DateTime.UtcNow.ToShortDateString() + ".png";
            System.IO.File.WriteAllBytes(name, tex.EncodeToPNG());
            Debug.Log("截图：" + name);
            System.GC.Collect();
            return tex;
        }

        /// <summary>
        /// 向量旋转后的向量
        /// </summary>
        /// <param name="angle">角度0-360</param>
        /// <param name="center">中心点</param>
        /// <returns></returns>
        public static Vector2 RotatePos(Vector2 pos, float angle, Vector2 center)
        {

            float dis = Vector2.Distance(center, pos);
            float sAngle = Vector2.Angle(pos, center);
            if (pos.y < center.y)
                sAngle = 360f - sAngle;
            angle += sAngle;
            //while (angle > 2 * Mathf.PI)
            //{
            //    angle = angle - 2 * Mathf.PI;
            //}

            //Debug.Log(angle / Mathf.Rad2Deg);
            return new Vector2(Mathf.Cos(angle / Mathf.Rad2Deg) * dis, Mathf.Sin(angle / Mathf.Rad2Deg) * dis);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="souce">原点</param>
        /// <param name="angle">角度 0-360</param>
        /// <param name="length">长度距离</param>
        /// <returns></returns>
        public static Vector2 GetRoteMovePos(Vector2 souce, float angle, float length)
        {
            Vector2 target = souce;
            Vector2 vec2 = Vector2.zero;
            //float x = centerPos.x + radius * Mathf.Cos (angle * 3.14f / 180f);  
            //float y = centerPos.y + radius * Mathf.Sin (angle * 3.14f / 180f);  
            //target.x += Mathf.Cos(angle / Mathf.Rad2Deg) * length;
            //target.y += Mathf.Sin(angle / Mathf.Rad2Deg) * length;        
            vec2.x = target.x + length * Mathf.Cos(angle * 3.14f / 180f);
            vec2.y = target.y + length * Mathf.Sin(angle * 3.14f / 180f);
            return vec2;
        }

        public static float GetVec2Angle(Vector2 a, Vector2 b)
        {
            float sAngle = Vector2.Angle(a, b);
            if (a.y < b.y)
                sAngle = 360f - sAngle;

            return sAngle;// Vector2.Angle(a, b);
        }
        public static Quaternion GetAngle(Vector3 a, Vector3 b)
        {
            Vector3 pos = b - a;
            // float angle = Vector3.Angle(Vector2.up, pos);
            // if(pos.y < 0)
            //   angle = -angle;
            //float radians = Mathf.Deg2Rad * angle;
            Quaternion q = Quaternion.FromToRotation(Vector3.up, pos);
            return q;
        }

        /// <summary>
        /// 计算一个点周围的距离
        /// </summary>
        /// <param name="souce"></param>
        /// <param name="angle"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static Vector3 GetRoteMovePosVec3(Vector3 souce, float angle, float length)
        {
            Vector3 target = souce;
            Vector3 vec3 = Vector2.zero;
            //float x = centerPos.x + radius * Mathf.Cos (angle * 3.14f / 180f);  
            //float y = centerPos.y + radius * Mathf.Sin (angle * 3.14f / 180f);  
            //target.x += Mathf.Cos(angle / Mathf.Rad2Deg) * length;
            //target.y += Mathf.Sin(angle / Mathf.Rad2Deg) * length;        
            vec3.x = target.x + Mathf.Cos(angle / Mathf.Rad2Deg) * length; // length * Mathf.Cos(angle * 3.14f / 180f);
            vec3.z = target.z + Mathf.Sin(angle / Mathf.Rad2Deg) * length; // length * Mathf.Sin(angle * 3.14f / 180f);
            vec3.y = target.y;
            return vec3;
        }


        public static void LookAtScreenPos(Transform trans, Vector3 vPosTarget)
        {
            trans.LookAt(vPosTarget);
        }

        /// <summary>
        /// 计算两个物体之间的角度
        /// </summary>
        /// <param name="aPos"></param>
        /// <param name="bPos"></param>
        /// <returns></returns>
        public static float GetAngleBetweenPostion(Vector3 VPosFrom, Vector3 vPosTo)
        {
            //float sAngle = Vector2.Angle(pos, center);
            //if (pos.y < center.y)
            //  sAngle = 360f - sAngle;
            //Transform ts = null;            
            Vector3 v3 = Vector3.Cross(VPosFrom, vPosTo);
            if (v3.y < 0)
            {
                float fAngle = Vector3.Angle(VPosFrom, vPosTo);
                return fAngle;
            }
            else
                return 360f - Vector3.Angle(VPosFrom, vPosTo);
            //float dot = Vector3.Dot(VPosFrom, vPosTo);
            //float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            //return angle;
        }

        /// <summary>
        /// 获取两点之间的一个点,在方法中进行了向量的减法，以及乘法运算
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">结束点</param>
        /// <param name="distance">距离</param>
        /// <returns></returns>
        public static Vector3 GetBetweenPoint(Vector3 start, Vector3 end, float distance)
        {
            Vector3 normal = (end - start).normalized;
            return normal * distance + start;
        }
        public static int GetChildNum(Transform tranform)
        {
            int num = 1;
            for (int i = 0; i < tranform.childCount; i++)
            {
                num += GetChildNum(tranform.GetChild(i));
            }

            return num;
        }

        public static int GetChildsName(Transform tranform, StringBuilder sb)
        {
            int num = 1;
            sb.Append(tranform.name);
            sb.Append("\n");
            for (int i = 0; i < tranform.childCount; i++)
            {
                num += GetChildsName(tranform.GetChild(i), sb);
            }
            return num;
        }
        static int WriteGameObjectsInfoNum = 0;
        public static void WriteGameObjectsInfo(GameObject go)
        {
            StringBuilder sb = new StringBuilder();
            int num = GetChildsName(go.transform, sb);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.persistentDataPath + "/" + go.name + WriteGameObjectsInfoNum + ".txt", false);
            sw.Write(num + "\n" + sb);
            sw.Close();
            WriteGameObjectsInfoNum++;
        }

        public static void WriteLog(string name, string log)
        {
            string timeStr = System.DateTime.Now.ToString("yyyy-MM-dd HH：mm：ss");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.persistentDataPath + "/" + name + timeStr + ".txt", false);
            sw.Write(log);
            sw.Close();
        }





        static RenderTexture uiRootBlurRt = new RenderTexture(Screen.width, Screen.height, 16);
        static Texture2D uiRootBlurTx = new Texture2D(Screen.width, Screen.height);
        static Rect uiRootBlurRect = new Rect(0, 0, Screen.width, Screen.height);

        /// <summary>
        /// UIditu beijing mohu
        /// </summary>
        /// <param name="m_Camera"></param>
        /// <param name="mImage"></param>
        public static void ScreenshotToBlurImage(Camera m_Camera, Image mImage)
        {

            m_Camera.targetTexture = uiRootBlurRt;
            m_Camera.Render();
            RenderTexture.active = uiRootBlurRt;
            //uiRootBlurRt.antiAliasing = 2;
            uiRootBlurTx.ReadPixels(uiRootBlurRect, 0, 0);
            uiRootBlurTx.Apply();

            Sprite sprite = Sprite.Create(uiRootBlurTx, uiRootBlurRect, Vector2.zero);
            mImage.sprite = sprite;

            //还原各项截图前得初始设定
            m_Camera.targetTexture = null;
            RenderTexture.active = null;
            //GameObject.Destroy(rt);
        }


    }
}
