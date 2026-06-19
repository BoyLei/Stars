using Google.Protobuf;
using ProtoMsg;
using StarProject.Service.Language;
using StarProjectDef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Frame
{
    public static class Util
    {

#if UNITY_EDITOR
        [XLua.DoNotGen]
        public static void AddUpdate(UnityEditor.EditorApplication.CallbackFunction ac)
        {
            UnityEditor.EditorApplication.update -= ac;
            UnityEditor.EditorApplication.update += ac;
        }
        [XLua.DoNotGen]
        public static void RemoveUpdate(UnityEditor.EditorApplication.CallbackFunction ac)
        {
            UnityEditor.EditorApplication.update -= ac;

        }
#endif
        public static int Int(object o)
        {
            return Convert.ToInt32(o);
        }

        public static float Float(object o)
        {
            return (float)Math.Round(Convert.ToSingle(o), 2);
        }

        public static long Long(object o)
        {
            return Convert.ToInt64(o);
        }

        public static int Random(int min, int max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        public static string Uid(string uid)
        {
            int position = uid.LastIndexOf('_');
            return uid.Remove(0, position + 1);
        }

        /// <summary>
        /// 格式化字符串
        /// </summary>
        /// <returns></returns>
        public static string f(string format, params object[] args)
        {
            StringBuilder sb = new();
            return sb.AppendFormat(format, args).ToString();
        }

        /// <summary>
        /// 字符串连接
        /// </summary>
        public static string c(params object[] args)
        {
            StringBuilder sb = new();
            for (int i = 0; i < args.Length; i++)
            {
                sb.Append(args[i].ToString());
            }
            return sb.ToString();
        }

        public static int GetTime()
        {
            TimeSpan ts = new(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (int)ts.TotalSeconds;
        }
        public static DateTime GetTimeEnd(float second)
        {
            return System.DateTime.Now + new System.TimeSpan((long)(second * 10000000));
        }
        /// <summary>
        /// 手机震动
        /// </summary>
        public static void Vibrate()
        {
            //int canVibrate = PlayerPrefs.GetInt(Const.AppPrefix + "Vibrate", 1);
            //if (canVibrate == 1) iPhoneUtils.Vibrate();
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        public static string Encode(string message)
        {
            byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        public static string Decode(string message)
        {
            byte[] bytes = Convert.FromBase64String(message);
            return Encoding.GetEncoding("utf-8").GetString(bytes);
        }

        /// <summary>
        /// 判断数字
        /// </summary>
        public static bool IsNumeric(string str)
        {
            if (str == null || str.Length == 0)
                return false;

            for (int i = 0; i < str.Length; i++)
            {
                if (!Char.IsNumber(str[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// HashToMD5Hex
        /// </summary>
        public static string HashToMD5Hex(string sourceStr)
        {
            byte[] Bytes = Encoding.UTF8.GetBytes(sourceStr);
            using (MD5CryptoServiceProvider md5 = new())
            {
                byte[] result = md5.ComputeHash(Bytes);
                StringBuilder builder = new();
                for (int i = 0; i < result.Length; i++)
                    builder.Append(result[i].ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            Resources.UnloadUnusedAssets();
            //GC.Collect();
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        public static bool IsNumber(string strNumber)
        {
            Regex regex = new("[^0-9]");
            return !regex.IsMatch(strNumber);
        }

        /// <summary>
        /// 取得App包里面的读取目录
        /// </summary>
        public static Uri AppContentDataUri
        {
            get
            {
                string dataPath = Application.dataPath;
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "file";
                    uriBuilder.Path = Path.Combine(dataPath, "Raw");
                    return uriBuilder.Uri;
                }
                else if (Application.platform == RuntimePlatform.Android)
                {
                    return new Uri("jar:file://" + dataPath + "!/assets");
                }
                else
                {
                    var uriBuilder = new UriBuilder();
                    uriBuilder.Scheme = "file";
                    uriBuilder.Path = Path.Combine(dataPath, "StreamingAssets");
                    return uriBuilder.Uri;
                }
            }
        }


        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        public static string LuaPath(string name)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                return Application.dataPath + "/lua/" + name;
            }
            string str = Application.persistentDataPath + "/" + Util.PlatformDir + "/";
            if (Directory.Exists(str))
            {
                return str + name;
            }
            return Application.streamingAssetsPath + "/" + Util.PlatformDir + "/" + name;

        }
        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        /// 
        //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
        public static string AppContentPath
        {
            get
            {
                //return Application.streamingAssetsPath;
                string path = Application.dataPath;
                if (Application.platform == RuntimePlatform.Android)
                {
                    path = "jar:file://" + Application.dataPath + "!/assets";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    path = "file://" + Application.dataPath + "/Raw";
                }
                //else if (Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.OSXWebPlayer)
                //{
                //    path = "Web";
                //}
                else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXEditor)
                {
                    path = "file://" + dataPathParent;//
                }
                return path;
            }
        }
        public static string dataPathParent
        {
            get
            {
                return Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets")) + "/StreamingAssets";
            }
        }
        public static string PersistentDataPath
        {
            get
            {
                string url = Application.persistentDataPath;
                if (Application.platform == RuntimePlatform.Android)
                {
                    url = "file://" + Application.persistentDataPath;
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
                {
                    url = "file://" + Application.persistentDataPath;
                }
                else if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    url = "file://" + Application.persistentDataPath;
                }
                return url;

            }
        }
        public static string PlatformDir
        {
            get
            {
#if UNITY_ANDROID
                return "Android";
#elif UNITY_IPHONE || UNITY_IOS
            return "iOS";
#elif UNITY_WEBPLAYER
            return "WebPlayer";
#elif UNITY_STANDALONE_WIN
                return "Windows";
#elif UNITY_STANDALONE_OSX
            return "iOS";
#else
            return iOS;
#endif
            }
        }
        /// <summary>
        /// 取得行文本
        /// </summary>
        public static string GetFileText(string path)
        {
            if (!File.Exists(path))
                return "";
            return File.ReadAllText(path);
        }

        public static string LoadText(string url, out string error)
        {
            WWW www = new(url);
            while (!www.isDone) { }
            error = www.error;
            if (www.error != null)
            {
                Debug.LogError("LoadText:" + url + ", error:" + www.error);
                return "";
            }
            return www.text.Trim();
        }

        /// <summary>
        /// HttpWebRequest 通过get
        /// </summary>
        /// <param name="url">URI</param>
        /// <returns></returns>
        public static string GetDataGetHtml(string url)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse webRespon = (HttpWebResponse)httpWebRequest.GetResponse();
                Stream webStream = webRespon.GetResponseStream();
                if (webStream == null)
                {
                    return "网络错误(Network error)：" + new ArgumentNullException("webStream");
                }
                StreamReader streamReader = new(webStream, Encoding.UTF8);
                string responseContent = streamReader.ReadToEnd();

                webRespon.Close();
                streamReader.Close();

                return responseContent;
            }
            catch (Exception ex)
            {
                return "网络错误(Network error)：" + ex.Message;
            }
        }


        public static bool IsTestClient = false;
        public static bool IsWeb()
        {
            //return true;
            if (IsTestClient)
                return false;
            if (Application.platform == RuntimePlatform.WindowsEditor)
                return false;
            if (Application.platform == RuntimePlatform.WindowsPlayer)
                return true;
            return false;
        }
        public static bool IsEditor()
        {
            //return false;
            //if(Wuxia.Config.IsHotFix)
            //    return false;
            if (Application.platform == RuntimePlatform.WindowsEditor) return true;
            return false;
        }
        public static bool IsMiniClient()
        {
            //return true;
            if (IsTestClient)
                return false;
            if (Application.platform == RuntimePlatform.WindowsPlayer) return true;
            return false;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        public static string MD5Encrypt(string strText)
        {
            if (null == strText) return "";
            MD5CryptoServiceProvider md5 = new();
            string result = BitConverter.ToString(md5.ComputeHash(UTF8Encoding.Default.GetBytes(strText)));
            result = result.Replace("-", "").ToLower();
            return result;
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static string parseTimeYMD(DateTime time)
        {
            return string.Format("{0}年{1}月{2}日", time.Year, time.Month, time.Day);
        }

        public static string parseTimeDHM(DateTime time)
        {
            return string.Format("{0}天{1}时{2}分", time.Day, time.Hour, time.Minute);
        }

        public static string timeSpan(DateTime time1, DateTime time2)
        {
            TimeSpan ts = time2.Subtract(time1);
            return string.Format("{0}天{1}时{2}分", ts.Days, ts.Hours, ts.Minutes);
        }
        public static string TimeToString(DateTime dt)
        {
            TimeSpan ts = System.DateTime.Now - dt;
            return ts.TimeSpanToString();
        }
        public static string parseTimeDHMBySecond(int second)
        {
            string timeStr = "";
            TimeSpan ts = new(0, 0, second);
            timeStr = string.Format("{0} : {1} : {2}", GetTwoInt((int)ts.TotalHours), GetTwoInt(ts.Minutes), GetTwoInt(ts.Seconds));
            return timeStr;
        }
        public static string TimeSpanToString(this TimeSpan ts)
        {
            if ((int)ts.TotalHours > 0)
                return string.Format("{0} : {1} : {2}", GetTwoInt((int)ts.TotalHours), GetTwoInt(ts.Minutes), GetTwoInt(ts.Seconds));
            return string.Format("    {0} : {1}", GetTwoInt(ts.Minutes), GetTwoInt(ts.Seconds));
        }

        public static bool TimeCompareMin(int second, int min)
        {
            int minSec = min * 60;
            return second >= minSec ? true : false;

        }
        public static string GetTwoInt(int num)
        {
            string str = "";
            if (num < 10)
            {
                str = "0" + num.ToString();
            }
            else
                str = num.ToString();

            return str;
        }
        public static bool RegexName(string str)
        {
            bool flag = Regex.IsMatch(str, @"^[A-Za-z0-9\u4e00-\u9fa5]+$");
            return flag;
        }

        public static string GteFirstChar(string str)
        {
            return str.Substring(0, 1);
        }

        public static void PlayAvgVoice(string eventName)
        {
            StarProject.Service.Sound.SoundManager.Instance.PlaySound(eventName);
        }

        /// <summary>
        /// 用于UnityEngine.Object及其子类对象的判空
        /// 在使用DestroyImmediate销毁一个UnityEngine.Object对象时，该对象会被Unity认为已经是null
        /// 但是C#并不认为它是null
        /// 因此在与Lua交互时，不能直接在Lua侧判断对象是否为nil（这样判断走的是C#的判空），应该调用此方法（走的是Unity的判空）
        /// </summary>
        public static bool IsNull(UnityEngine.Object target)
        {
            return target == null;
        }

        /// <summary>
        /// 客户端错误码
        /// </summary>
        /// <param name="code"></param>
        public static void ShowMessageByCode(StarProjectDef.CRetMsgEnum cRetMsgEnum)
        {
            StarProjectDef.CRetMsgDataCell retM = StarProject.Service.LocalData.LocalDataManager.Instance.GetCRetMsgDataCell(cRetMsgEnum);

            if (retM != null)
            {
                //全服消息
                if (retM.GetMsgType() == 1)
                {

                }

                //系统消息
                else if (retM.GetMsgType() == 2)
                {
                    StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(retM.Note);
                }

                //战斗消息
                else if (retM.GetMsgType() == 3)
                {
                    StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(retM.Note);
                }
            }
        }

        public static void ShowMessageByCode(StarProjectDef.CRetMsgEnum cRetMsgEnum, params object[] prames)
        {
            StarProjectDef.CRetMsgDataCell retM = StarProject.Service.LocalData.LocalDataManager.Instance.GetCRetMsgDataCell(cRetMsgEnum);

            if (retM != null)
            {
                //全服消息
                if (retM.GetMsgType() == 1)
                {

                }

                //系统消息
                else if (retM.GetMsgType() == 2)
                {
                    StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.Note, prames));
                }

                //战斗消息
                else if (retM.GetMsgType() == 3)
                {
                    StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(string.Format(retM.Note, prames));
                }

            }
        }
        public static int CountOnes(int number)
        {
            int count = 0;
            for (int i = 0; i < 32; i++)
            {
                if ((number & (1 << i)) != 0)
                {
                    count++;
                }
            }
            return count;
        }


        public static void ShowMessageByCode(int id)
        {
            StarProjectDef.RetMsgDataCell retM = StarProject.Service.LocalData.LocalDataManager.Instance.GetRetMsgDataCell(id);
            if (retM != null)
            {
                //全服消息
                if (retM.GetMsgType() == 1)
                {

                }

                //系统消息
                else if (retM.GetMsgType() == 2)
                {
                    if (retM.ClientStr != "")
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.ClientStr));
                    }
                    else
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.Id.ToString()));
                    }
                }
                //战斗消息
                else if (retM.GetMsgType() == 3)
                {
                    if (retM.ClientStr != "")
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(string.Format(retM.ClientStr));
                    }
                    else
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(string.Format(retM.Id.ToString()));
                    }
                }
                else if (retM.GetMsgType() == 4)
                {
                    if (retM.ClientStr != "")
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.ClientStr));
                    }
                    else
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.Id.ToString()));
                    }
                }
            }
        }

        public static void ShowMessageByCode(int id, params object[] prames)
        {
            StarProjectDef.RetMsgDataCell retM = StarProject.Service.LocalData.LocalDataManager.Instance.GetRetMsgDataCell(id);
            if (retM != null)
            {
                //全服消息
                if (retM.GetMsgType() == 1)
                {

                }

                //系统消息
                else if (retM.GetMsgType() == 2)
                {
                    if (retM.ClientStr != "")
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.ClientStr, prames));
                    }
                    else
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.Id.ToString()));
                    }
                }

                //战斗消息
                else if (retM.GetMsgType() == 3)
                {
                    if (retM.ClientStr != "")
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(string.Format(retM.ClientStr, prames));
                    }
                    else
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(string.Format(retM.Id.ToString()));
                    }
                }
                else if (retM.GetMsgType() == 4)
                {
                    if (retM.ClientStr != "")
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.ClientStr, prames));
                    }
                    else
                    {
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(string.Format(retM.Id.ToString()));
                    }

                }
            }
        }


        public static object RespParamValue(RespParam item)
        {
            switch (item.ParamValueCase)
            {
                case RespParam.ParamValueOneofCase.None:
                    break;
                case RespParam.ParamValueOneofCase.Int32Value:
                    return item.Int32Value.ToString();
                    break;
                case RespParam.ParamValueOneofCase.Uint32Value:
                    return item.Uint32Value.ToString();
                    break;
                case RespParam.ParamValueOneofCase.Int64Value:
                    return item.Int64Value.ToString();
                    break;
                case RespParam.ParamValueOneofCase.Uint64Value:
                    return item.Uint64Value.ToString();
                    break;
                case RespParam.ParamValueOneofCase.FloatValue:
                    return item.FloatValue.ToString();
                    break;
                case RespParam.ParamValueOneofCase.DoubleValue:
                    return item.DoubleValue.ToString();
                    break;
                case RespParam.ParamValueOneofCase.StringValue:
                    return item.StringValue;
                    break;
                default:
                    break;
            }

            return "";
        }

        /// <summary>
        /// 通用消息回复的提示处理
        /// </summary>
        /// <param name="msgRet"></param>
        public static void ShowMessageByMsgRet(ProtoMsg.MsgRet msgRet)
        {
            StarProjectDef.RetMsgDataCell retM = StarProject.Service.LocalData.LocalDataManager.Instance.GetRetMsgDataCell(msgRet.RetCode);
            string msgtext;
            int msgtype = 2;
            if (retM == null)
            {
                //msgtext = string.Format(GameConfig.LocalStr["MsgRetNullTips"], msgRet.RetMsgID, msgRet.RetCode);
                msgtext = string.Format(LanguageManager.Instance.GetLanguageByKey("MsgRetNullTips"), msgRet.RetMsgID, msgRet.RetCode);

            }
            else
            {
                msgtype = retM.GetMsgType();
                if (retM.ClientStr != "")
                {
                    List<string> li = new();
                    foreach (var item in msgRet.Params)
                    {
                        switch (item.ParamValueCase)
                        {
                            case RespParam.ParamValueOneofCase.None:
                                break;
                            case RespParam.ParamValueOneofCase.Int32Value:
                                li.Add(item.Int32Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.Uint32Value:
                                li.Add(item.Uint32Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.Int64Value:
                                li.Add(item.Int64Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.Uint64Value:
                                li.Add(item.Uint64Value.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.FloatValue:
                                li.Add(item.FloatValue.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.DoubleValue:
                                li.Add(item.DoubleValue.ToString());
                                break;
                            case RespParam.ParamValueOneofCase.StringValue:
                                li.Add(item.StringValue);
                                break;
                            default:
                                break;
                        }
                    }
                    //string[] param = string.Format(retM.ParamsKey, li.ToArray()).Split(',');
                    //msgtext = string.Format(retM.ClientStr, param);
                    //msgtext = RichTextUtils.ParseMailText(retM.ClientStr, string.Format(retM.ParamsKey, li.ToArray()));
                    msgtext = RichTextUtils.ParseRichText(retM.ClientStr, li);
                }
                else
                {
                    msgtext = retM.Id.ToString();
                }
            }
            switch (msgtype)
            {
                case 1:
                    {
                        //全服消息
                    }
                    break;
                case 2:
                    {
                        //系统消息
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(msgtext);
                    }
                    break;
                case 3:
                    {
                        //战斗消息
                        StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(msgtext);
                    }
                    break;
                case 4:
                    {
                        //弹窗消息
                        //StarProjectDef.UIAPI.ShowMsgBox(GameConfig.LocalStr["Tips"], msgtext, GameConfig.LocalStr["BtnSure"]);
                        StarProjectDef.UIAPI.ShowMsgBox(LanguageManager.Instance.GetLanguageByKey("Tips"), msgtext, LanguageManager.Instance.GetLanguageByKey("BtnSure"));
                    }
                    break;
                default:
                    break;
            }

        }

        public static void ShowMessage(string tex, int type = 2)
        {

            //全服消息
            if (type == 1)
            {

            }

            //系统消息
            else if (type == 2)
            {
                StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(tex);
            }

            //战斗消息
            else if (type == 3)
            {
                StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(tex);
            }

        }

        public static void ShowBattleMessage(string tex)
        {
            StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowBattleMessage(tex);
        }

        public static void ShowSystemMessage(string tex)
        {
            //StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.ShowSystemMessage(tex); 策划要求全走队列显示 @caojie
            StarProject.Service.DisplayProcess.DisplayProcessDispenser.Instance.AddSystemMessage(tex);
        }

        public static void UpPackData(IMessage md, ProtoMsg.MapModel MsgData)
        {
            var mdtp = md.GetType();
            foreach (var item in MsgData.Datas)
            {
                //SGF.Debuger.LogWarning($"差量日志找下报错  md={md},mdName={md.Descriptor.FullName},mdType={mdtp},item={item},key={item.Key},value={item.Value},");
                var field = mdtp.GetProperty(item.Key);
                switch (field.PropertyType.Name)
                {
                    //case :
                    //    // 字段是int类型
                    //    field.SetValue(md, BitConverter.ToInt32(item.Value.Buffer.ToByteArray(),0));
                    //    break;
                    case "Int8":
                        // 字段是long类型
                        field.SetValue(md, (byte)ReadVarint(item.Value.Buffer.ToByteArray()));
                        break;
                    case "Int16":
                        // 字段是long类型
                        field.SetValue(md, (Int16)ReadVarint(item.Value.Buffer.ToByteArray()));
                        break;
                    case "Int32":
                        // 字段是long类型
                        field.SetValue(md, (Int32)ReadVarint(item.Value.Buffer.ToByteArray()));
                        break;
                    case "Int64":
                        // 字段是long类型
                        field.SetValue(md, ReadVarint(item.Value.Buffer.ToByteArray()));
                        break;
                    //case "UInt8":
                    //    field.SetValue(md,(ubyte) ReadUvarint(item.Value.Buffer.ToByteArray()));
                    //    break;
                    case "UInt16":
                        field.SetValue(md, (UInt16)ReadUvarint(item.Value.Buffer.ToByteArray()));
                        break;
                    case "UInt32":
                        field.SetValue(md, (UInt32)ReadUvarint(item.Value.Buffer.ToByteArray()));
                        break;
                    case "UInt64":
                        field.SetValue(md, (UInt64)ReadUvarint(item.Value.Buffer.ToByteArray()));
                        break;
                    case "String":
                        // 字段是string类型
                        field.SetValue(md, item.Value.Buffer.ToStringUtf8());

                        break;
                    case "Boolean":
                        // 字段是bool类型
                        field.SetValue(md, item.Value.Buffer[0] == 1);
                        break;
                    case "[]byte":
                        field.SetValue(md, item.Value.Buffer.ToByteArray());
                        break;
                    default:
                        // 其他类型
                        if (field.PropertyType.GetInterface("Google.Protobuf.IMessage") != null)
                        {
                            IMessage pbMessage = SGF.Network.ProtoUtils.Deserialize(SGF.Network.ProtoDic.Instance.GetCMDByName(field.PropertyType.Name), item.Value.Buffer.ToByteArray());
                            field.SetValue(md, pbMessage);
                        }
                        else if (field.PropertyType.IsEnum)
                        {
                            field.SetValue(md, (Int32)ReadVarint(item.Value.Buffer.ToByteArray()));
                        }
                        break;
                }
            }
        }

        public const int MaxVarintLen16 = 3;

        public const int MaxVarintLen32 = 5;
        public const int MaxVarintLen64 = 10;

        //得到uint64
        public static ulong ReadUvarint(byte[] byteArray)
        {
            ulong result = 0;
            int shift = 0;

            for (int i = 0; i < byteArray.Length; i++)
            {
                var b = byteArray[i];
                if (i == MaxVarintLen64)
                {
                    return 0;
                }
                if (b < 0x80)
                {
                    if (i == MaxVarintLen64 - 1 && b > 1)
                    {
                        return 0;
                    }
                    return result | (((ulong)b) << shift);
                }
                result |= ((ulong)b & 0x7f) << shift;
                shift += 7;
            }
            return 0;
        }

        public static long ReadVarint(byte[] byteArray)
        {
            var ux = ReadUvarint(byteArray);
            long x = (long)(ux >> 1);

            if ((ux & 1) != 0)
            {
                x = ~x;
            }

            return x;
        }

        public static double ReadVarFloat64(byte[] byteArray)
        {
            ulong x = ReadUvarint(byteArray);
            byte[] b = BitConverter.GetBytes(x);
            return BitConverter.ToDouble(b, 0);
        }

        public static float ReadVarFloat32(byte[] x)
        {
            ulong b = ReadUvarint(x);
            byte[] byteArray = BitConverter.GetBytes(b);
            return BitConverter.ToSingle(byteArray, 0);
        }

        public static string StringFormatter(string str, params string[] args)
        {
            return string.Format(str, args);
        }



        public static DG.Tweening.Tweener TweenFloatTo(float start, float to, float duration, XLua.LuaFunction setter)
        {
            return DG.Tweening.DOTween.To(() => start, (value) => setter.Call(value), to, duration);
        }


    }
}