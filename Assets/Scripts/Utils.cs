
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Fire
{


    public class Utils
    {

        public static float GetPinch()
        {
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.touches[0];
                Touch touchOne = Input.touches[1];

                var zeroPrevPos = touchZero.position - touchZero.deltaPosition;
                var onePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevTouchDeltaMag = (zeroPrevPos - onePrevPos).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                return touchDeltaMag - prevTouchDeltaMag;
            }
            return 0;
        }

        public static float GetAxisRawScrollUniversal()
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll < 0) return -1;
            if (scroll > 0) return 1;
            return 0;
        }


        public static bool IsCursorOverUserInterface()
        {
            if (EventSystem.current == null || EventSystem.current.IsPointerOverGameObject())
                return true;
            for (int i = 0; i < Input.touchCount; ++i)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                    return true;
            }

            return false;
        }

        public static float GetZoomUniversal()
        {
            if (Input.mousePresent)
                return GetAxisRawScrollUniversal();
            else if (Input.touchCount > 0)
                return GetPinch();
            return 0;
        }

        public static bool Vector3Equal(Vector3 left, Vector3 right)
        {
            return Mathf.Approximately(left.x, right.x) && Mathf.Approximately(left.y, right.y) && Mathf.Approximately(left.z, right.z);
        }
        public static Texture2D CaptureCamera(Camera ca, Rect rect, string url = "")
        {
#if !UNITY_WEBPLAYER
            bool noRT = ca.targetTexture == null;
            RenderTexture rt = noRT ? new RenderTexture((int)rect.width, (int)rect.height, 24, RenderTextureFormat.ARGB32) : ca.targetTexture;
            if (noRT)
                ca.targetTexture = rt;
            ca.Render();
            //ps:可以增加相机
            //
            RenderTexture.active = rt;

            Texture2D tex = new(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            if (noRT)
                ca.targetTexture = null;
            RenderTexture.active = null;
            //GameObject.Destroy(rt);
            if (string.IsNullOrEmpty(url))
                url = Application.streamingAssetsPath + "/renderTexture" + System.DateTime.UtcNow.ToShortDateString() + ".png";
            string dir = Path.GetDirectoryName(url);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(url);
            System.IO.File.WriteAllBytes(url, tex.EncodeToPNG());
            Debug.Log("截图：" + url);
            System.GC.Collect();
            return tex;
#endif
            return null;
        }


        public static void LogError(string error)
        {
            Debug.LogError(error);
        }


        public static Vector3 ServerRota2Vector(float serverRota)
        {
            float rota_radius = serverRota / 180 * Mathf.PI;
            int SP = 0;
            int CP = 1;
            float SY = Mathf.Sin(rota_radius);
            float CY = Mathf.Cos(rota_radius);
            Vector3 v3 = Vector3.zero;
            v3.x = CP * CY;
            v3.y = SP;
            v3.z = CP * SY;
            return v3;
        }

        public static Vector3 GetClientRota2Vec3(float clientYRota)
        {
            return (Quaternion.Euler(0, clientYRota, 0) * Vector3.forward).normalized;
        }

        /// <summary>
        /// 得到客户端 角度 rota 对应的 服务器朝向
        /// </summary>
        /// <param name="rota"></param>
        public static int GetRoata2ServerAngle(float clientRota)
        {
            // 首先 获得角度rote 对应的客户端的 方向向量
            Vector3 dir = GetClientRota2Vec3(clientRota);

            // 然后计算出 客户端朝向的方向向量 对应的 服务器的角度
            int serverRot = (int)(Math.Atan2(dir.z, dir.x) * Mathf.Rad2Deg % 360);

            return serverRot;
        }



        // public static Vector3 Rota2Vector(float x, float y, float z)
        // {

        // }

        /// <summary>
        /// 坐标移动根据角度
        /// </summary>
        /// <param name="pos">坐标</param>
        /// <param name="serverRota">服务器角度</param>
        /// <param name="radius">半径</param>
        /// <returns></returns>
        public static Vector3 PosMoveBySeverRota(Vector3 pos, float serverRota, float radius)
        {
            Vector3 forward = ServerRota2Vector(serverRota);
            forward = forward * radius;
            return pos + forward;
        }

        /// <summary>
        /// 坐标 根据 客户端的朝向 位移多少
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="clientRota"></param>
        /// <param name="radius"></param>
        public static Vector3 PosMoveByClientRota(Vector3 pos, float clientRota, float radius)
        {
            Vector3 forward = GetClientRota2Vec3(clientRota);

            forward = forward * radius;
            return pos + forward;
        }

        /// <summary>
        /// 根据起点、终点和等分距离。返回根据距离等分成多个点，
        /// </summary>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="distance">距离</param>
        /// <param name="includeStartPoint">是否包含起点</param>
        /// <param name="includeEndPoint">是否包含终点</param>
        public static List<Vector3> DividePoints(Vector3 startPoint, Vector3 endPoint, float distance, bool includeStartPoint, bool includeEndPoint, ref Vector3 lastPoint)
        {
            // 计算起点到终点的向量
            Vector3 direction = endPoint - startPoint;
            // 计算向量的长度
            float magnitude = direction.magnitude;
            // 计算向量的单位向量
            Vector3 unitDirection = direction.normalized;
            // 计算距离上等分的数量（包括起点和终点）
            int pointCount = Mathf.FloorToInt(magnitude / distance);
            // 如果需要包括起点和终点，则等分数量加1
            if (includeStartPoint)
            {
                pointCount++;
            }
            if (includeEndPoint)
            {
                pointCount++;
            }
            if (pointCount <= 0)
            {
                pointCount = 1;
            }
            // 计算每个点之间的距离（包括起点和终点）
            float segmentDistance = magnitude / pointCount;
            // 创建存储点的数组
            //Vector3[] points = new Vector3[pointCount];
            List<Vector3> dfPonint = new();

            // 计算并存储每个点的位置
            for (int i = 0; i < pointCount; i++)
            {
                // 计算当前点的位置
                Vector3 pointPosition = startPoint + (unitDirection * segmentDistance * i);
                // 存储该点的位置
                //points[i] = pointPosition;
                dfPonint.Add(pointPosition);
                if (i + 1 == pointCount)
                {
                    lastPoint = pointPosition;
                }
            }
            return dfPonint;
        }

        /// <summary>
        /// 两点间等分成几个点
        /// </summary>
        /// <param name="hasStart">最后的结果包括起点</param>
        /// <param name="hasEnd">最后的结果包括终点</param>
        /// <param name="count">等分数量</param>
        /// <param name="endPoint">终点</param>
        /// <param name="startPoint">起点</param>
        /// <returns></returns>
        public static List<Vector3> GetIntervalPoint(bool hasStart, bool hasEnd, int count, Vector3 endPoint, Vector3 startPoint)
        {
            List<Vector3> dfPonint = new();
            float dfXLenght = System.Math.Abs(endPoint.x - startPoint.x) / (count + 1);
            float dfYLenght = System.Math.Abs(endPoint.y - startPoint.y) / (count + 1);
            float x;
            float y;
            if (hasStart)
            {
                dfPonint.Add(startPoint);
            }
            for (int i = 0; i < count; i++)
            {
                dfPonint.Add(new Vector3());
                if (endPoint.x >= startPoint.x)
                {
                    x = endPoint.x - ((i + 1) * dfXLenght);
                }
                else
                {
                    x = endPoint.x + ((i + 1) * dfXLenght);
                };

                if (endPoint.y >= startPoint.y)
                {
                    y = endPoint.y - ((i + 1) * dfYLenght);
                }
                else
                {
                    y = endPoint.y + ((i + 1) * dfYLenght);
                };
                dfPonint[i] = new Vector3(x, y);
            }

            if (hasEnd)
            {
                dfPonint.Add(endPoint);
            }

            return dfPonint;
        }

        public static void TLog(params object[] args)
        {
            Debug.Log($"T: {SGF.Time.TimeUtils.ClientUtcNow} {args.ListToString<object>()}");
        }

        public static void SafeRunAction(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($" [SafeRunAction] catch error:  {e.Message},{e.StackTrace}");
            }
        }


        // public static Vector3 GetOrientationBuilderRotate(ulong builderID, Vector3 ownPos, Vector3 ownEulerAngles)
        // {
        //     Vector3 rot = new Vector3(ownEulerAngles.x, ownEulerAngles.y, ownEulerAngles.z);
        //     Vector3 builderPos = GameManager.Instance.GetEntityPosById(builderID);
        //     Vector3 dirInterpolation = builderPos - ownPos;
        //     rot.y = (float)(Math.Atan2(dirInterpolation.x, dirInterpolation.z) * Mathf.Rad2Deg) % 360;
        //     return rot;
        // }

        //------//搜索字符串(参数1：完整的内容，参数2：左边的内容，参数3：右边的内容)----(获取两个字符串中间的字符串)       
        public static string Search_string(string s, string s1, string s2)  //获取搜索到的数目  
        {
            int n1, n2;
            n1 = s.IndexOf(s1, 0) + s1.Length;   //开始位置  
            n2 = s.IndexOf(s2, n1);               //结束位置    
            return s.Substring(n1, n2 - n1);   //取搜索的条数，用结束的位置-开始的位置,并返回    
        }

        /// <summary>
        /// 根据 服务器 下发 的 cd 开始/ 结束 时间，计算出 当前cd实际 的时间
        /// </summary>
        /// <param name="cdStartTime"></param>
        /// <param name="cdEndTime"></param>
        /// <returns></returns>
        public static long CalculateCD(long cdStartTime, long cdEndTime)
        {
            long cdTime = cdEndTime - cdStartTime;

            long serverNow = SGF.Time.TimeUtils.ServerNowStampMilli;

            DateTime dateTime = SGF.Time.TimeUtils.GetDateTime((int)cdStartTime);
            string starStr = dateTime.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);

            DateTime dateTime2 = SGF.Time.TimeUtils.GetDateTime((int)cdStartTime);
            string endStr = dateTime2.ToString("yyyy/MM/dd HH:mm:ss", DateTimeFormatInfo.InvariantInfo);


            long leastCD = 0;
            if (cdStartTime <= serverNow && serverNow < cdEndTime)
            {
                leastCD = cdEndTime - serverNow;
            }
            else if (cdStartTime > serverNow)
            {
                leastCD = cdTime;
            }
            else
            {
                leastCD = 0;
            }

            SGF.Debuger.Log($"公共CD 开始时间={starStr}-----结束时间={endStr}----cha={cdEndTime - cdStartTime}----leastCD={leastCD}");


            return leastCD;
        }


        /// <summary>
        /// 获取两个向量的夹角  Vector3.Angle 只能返回 [0, 180] 的值
        //  如真实情况下向量 a 到 b 的夹角（80 度）则 b 到 a 的夹角是（-80）
        //  通过 Dot、Cross 结合获取到 a 到 b， b 到 a 的不同夹角
        /// </summary>
        /// <param name="fromVector"></param>
        /// <param name="toVector"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public static float GetAngle(Vector3 fromVector, Vector3 toVector, Vector3 upVector)
        {
            float angle = Vector3.Angle(fromVector, toVector); //求出两向量之间的夹角
            Vector3 normal = Vector3.Cross(fromVector, toVector);//叉乘求出法线向量
            angle *= Mathf.Sign(Vector3.Dot(normal, upVector));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向
            return angle;
        }

        /// <summary>
        /// 取 int64 的 高八位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetInt64High8(long value)
        {
            int v = (int)((value >> 24) & 0xff);

            return v;
        }

        /// <summary>
        /// 取 int64 的 低八位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetInt64Low8(long value)
        {
            int v = (int)(value & 0xff);
            return v;
        }


        public static int GetInt64High2Low8(long value)
        {
            // 取 最高的 8位
            int v = (int)((value >> 40) & 0xff);
            if (v > 0)
            {
                return v;
            }
            // 取 不到 最高 8位, 那就取高 8-16 位
            v = (int)((value >> 32) & 0xff);
            if (v > 0)
            {
                return v;
            }
            // 取 不到 最高 8位, 那就取高 8-16 位
            v = (int)((value >> 24) & 0xff);
            if (v > 0)
            {
                return v;
            }
            // 取 不到 最高 8位, 那就取高 8-16 位
            v = (int)((value >> 16) & 0xff);
            if (v > 0)
            {
                return v;
            }
            // 取 低8-16 位
            v = (int)((value >> 8) & 0xff);
            if (v > 0)
            {
                return v;
            }
            // 取低 8 位
            v = (int)(value & 0xff);
            return v;
        }

        /// <summary>
        /// 将16进制的字符串转为byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }


        private static int rep = 1;
        // 随机生成字符串（数字和字母混和）
        public static string GenerateCheckCode(int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + rep;
            rep++;
            if (rep > int.MaxValue)
            {
                rep = 1;
            }
            System.Random random = new(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }

        /// <summary>
        /// 传入域名返回对应的IP 
        /// </summary>
        /// <param name="domainName">域名</param>
        /// <returns></returns>
        public static string GetIp(string domainName)
        {
            domainName = domainName.Replace("http://", "").Replace("https://", "");
            IPHostEntry hostEntry = Dns.GetHostEntry(domainName);
            IPEndPoint ipEndPoint = new(hostEntry.AddressList[0], 0);
            return ipEndPoint.Address.ToString();
        }

        /// <summary>
        /// 校验字符串是否含有字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isDomin(string str)
        {
            return Regex.Matches(str, "[a-zA-Z]").Count > 0;
        }

        /// <summary>
        /// 保留N位小数点（不四舍五入）
        /// </summary>
        /// <param name="number"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static float TruncateFloat(float number, int decimalPlaces)
        {
            float factor = (float)Math.Pow(10, decimalPlaces);
            return (float)(Math.Floor(number * factor) / factor);
        }

        public static string GetPlatformString()
        {
            string platformIdentifier = "";

#if UNITY_EDITOR
            platformIdentifier = "Editor";
#elif UNITY_STANDALONE_WIN
    platformIdentifier = "Windows";
#elif UNITY_STANDALONE_OSX
    platformIdentifier = "MacOS";
#elif UNITY_STANDALONE_LINUX
    platformIdentifier = "Linux";
#elif UNITY_ANDROID
    platformIdentifier = "Android";
#elif UNITY_IOS
    platformIdentifier = "iOS";
#elif UNITY_WEBGL
    platformIdentifier = "WebGL";
#else
    platformIdentifier = "Unknown";
#endif
            return platformIdentifier;
        }

        public static string GetVideoPath(string localPath)
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE_WIN
            return "file://" + Application.streamingAssetsPath + "/" + localPath;
#elif UNITY_IOS
            return Application.dataPath + "/Raw/"+localPath;
#elif UNITY_ANDROID
            return "jar:file://" + Application.dataPath + "!/assets/" + localPath;
#endif
        }

    }




}