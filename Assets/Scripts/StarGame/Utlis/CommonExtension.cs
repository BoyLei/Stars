using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace SGF.Utlis
{
    public static class CommonExtension
    {
        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }
        public static void SortList<T>(this List<T> list, Action<T, int> action)
        {
            if (list == null || action == null)
            {
                return;
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                action(list[i], i);//把I也暴露给你
            }
        }
        
        public static bool EachCharCompare(this string strOrgin, string str1)
        {
            // 如果原始字符串为空或null，直接返回false
            if (string.IsNullOrEmpty(strOrgin) || string.IsNullOrEmpty(str1))
            {
                return false;
            }

            // 如果Thelong比Theshort短，直接返回false
            if (strOrgin.Length < str1.Length)
            {
                return false;
            }

            // 从Theshort的末尾开始比较字符
            for (int i = str1.Length - 1; i >= 0; i--)
            {
                if (strOrgin[i] != str1[i])
                {
                    return false;
                }
            }

            // 如果Thelong和Theshort长度相等，说明它们完全相同
            if (strOrgin.Length == str1.Length)
            {
                return true;
            }

            // 如果Thelong比Theshort长，检查Theshort后面的字符是否为'/'
            return strOrgin[str1.Length] == '/';
        }
        
        /*//自己确认都是小写
        public static bool EachCharCompare(this string strOrgin, String str1)
        {
            bool iscontain = false;
            //还要看第二个
            if (string.IsNullOrEmpty(strOrgin))
            {
                return iscontain;
            }
            if (strOrgin == "")
            {
                return iscontain;
            }
            //长的是strOrgin
            char[] Thelong = strOrgin.ToCharArray();//res真实路径
            char[] Theshort = str1.ToCharArray();//截取路径，要求路径

            if (Thelong.Length < Theshort.Length)
            {
                //真实少于要求，还没有人家要求细致，那你一定是粗的，要求是细致的
                //都没有要求细致，一定不对
                return iscontain;
            }
            for (int i = Theshort.Length - 1; i >= 0; i--)
            {
                /*真实路径更具体  或者 完全一致 都可能是对的#1#
                if (Thelong[i] != Theshort[i])
                {
                    return iscontain;
                }
            }


            //到这里就是要求的，真实路径都有 = 可能完全一样也可能真实路径更细致 这正常 不重要

            if (Thelong.Length == Theshort.Length)
            {
                //实际路径一定是资源，说明要求也是资源，
                iscontain = true;
            }
            else
            {
                //long   >       Theshort 最少大于一个吧怎么会越界，并且约定一定是文件夹，通常会大于多个 
                //具体一定大于要求，要求一定更少是文件夹，并且要求一定是文件夹
                //"ui/abc"       "ui"2 0-1
                //如果具体比要求多的那个char字符正好是/ true 意味着也是文件夹对齐约束，完成 ，那就是符合约束要求true
                //不然就是那种情况//"uii/abc"       "ui"2 0-1
                iscontain = Thelong[Theshort.Length] == '/';
            }

            return iscontain;
        }*/
        public static T LinkedListFind<T>(this LinkedList<T> list, Predicate<T> action)
        {
            if (list == null || action == null)
            {
                return default(T);
            }
            LinkedListNode<T> node = list.First;
            while (node != null) 
            {
                if (action.Invoke(node.Value))
                {
                    return node.Value;
                }
                node = node.Next;
            }
            return default(T);
        }


        //=============
        //public static T GetOrAddComponent<T>(this GameObject gameobject)where T:Component
        //{
        //    if (gameobject == null)//只是要类型不需要实例，给的重来都是类型不需要也不能给实例
        //    {
        //        return null;
        //    }
        //    if (gameobject.GetComponent<T>() == null)
        //    {
        //        gameobject.AddComponent<T>();
        //    }
        //    return gameobject.GetComponent<T>();
        //}

        public static Transform DeepFindParentsGob(this Transform rootTransform, string containWord = null)
        {
            return FindParentNode(rootTransform, containWord);
        }


        /// <summary>
        /// 需循环的内存逻辑片段
        /// </summary>
        /// <param name="root"></param>
        /// <param name="containWord"></param>
        /// <returns></returns>
        static Transform FindParentNode(Transform root, string containWord)
        {
            //基于肯定找得到
            Transform findTransfrom = root;
            if (findTransfrom != null)
            {
                if (findTransfrom.parent != null)
                {
                    findTransfrom = findTransfrom.parent;
                    if (findTransfrom.name.Contains(containWord))
                    {
                        return findTransfrom;//1次
                    }
                    else
                    {
                        findTransfrom = FindParentNode(findTransfrom, containWord);//2到n（内部是1）
                    }
                }
            }
            return findTransfrom;//2~n
        }


        public static List<Transform> DeepFirstTransList(this Transform rootTransform, string findName = null)

        {
            if (rootTransform != null)
            {
                return DeepFirstFindChild(rootTransform, findName);
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 添加预制体
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddWidget<T>(Transform parent, GameObject item)
        {
            GameObject go = GameObject.Instantiate(item);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            T t = go.GetComponent<T>();
            return t;
        }



        static List<Transform> DepthFirstList = new List<Transform>();

        /// <summary>
        /// 深度遍历
        /// </summary>
        /// <param name="root"></param>
        /// <param name="deepFinalLayerName">查找的Nname，deepLayerName</param>
        /// <returns></returns>
        public static List<Transform> DeepFirstFindChild(Transform root, string findName = null)
        {
            DepthFirstList.Clear();
            DepthFirst_0(root, findName);
            return DepthFirstList;
        }

        static void DepthFirst_0(Transform tran, string findName)
        {
            //【但是会排除根节点】
            //tran当前节点
            foreach (Transform item in tran)//尽头是数量
            {
                //SGF.Debuger.Log(item.name);//不是所有，如果所有就没必要递归查找了，循环就行了；他是子节点一层的广度，递归的效果是广度每一个的深度，深度回归到上一次的广度
                //1所以没有深度就不继续深度，2跳过会回到上一次的广度，3【A先深度最后是广度，然后是广度，再循环】【A】
                //   1
                // 2      3
                //4 5    6  7 8 9
                //（1）2-4-5-3-6789
                //1
                //2 
                //4,5
                //3
                //6789 （是想要的）
                /////名字是空，意识就是全部查找；但是是深度优先的顺序排序
                if (string.IsNullOrEmpty(findName))
                {
                    DepthFirstList.Add(item);
                    if (item.childCount != 0)
                    {
                        DepthFirst_0(item, findName);
                    }

                    //DepthFirstList.Add(item);//深度的倒叙，广度层次的顺序
                }
                else//名字不是空，有条件的查找
                {
                    if (item.name == findName)//有名称查找，在这里，固定名称，深度顺序添加
                    {
                        DepthFirstList.Add(item);
                        if (item.childCount != 0)
                        {
                            DepthFirst_0(item, findName);
                        }
                    }
                    else
                    {
                        if (item.childCount != 0)
                        {
                            DepthFirst_0(item, findName);
                        }
                        //名字不相同，不代表子节点没有还要继续查找
                    }
                    /*Debug.Log(item.name);*/
                }


                //判断是否存在子物体,若本物体是Item已被发现，无需继续深度查找下一层



            }
        }


        public static void SortList<T>(this List<T> list, Action<T> action)
        {
            if (list == null || action == null)
            {
                return;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                action(list[i]);
            }
        }

        //public static void SortList<T>(this List<T> list, Action<T, int> action)
        //{
        //    if (list == null || action == null)
        //    {
        //        return;
        //    }

        //    for (int i = list.Count - 1; i >= 0; i--)
        //    {
        //        action(list[i], i); //把I也暴露给你
        //    }
        //}

        public static T RandomOneValue<T>(this List<T> list)
        {
            if (list == null)
            {
                return default(T);
            }
            else
            {
                return list[UnityEngine.Random.Range(0, list.Count)];
            }
        }

        /// <summary>
        /// 不包含特定物品的随机
        /// //目前只加引用类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="withOutOne"></param>
        /// <returns></returns>
        public static T RandomOneValue<T>(this List<T> list, T withOutOne) where T : class
        {
            if (list == null)
            {
                return default(T);
            }
            else
            {
                List<T> newListT = new List<T>();
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != withOutOne)
                    {
                        newListT.Add(list[i]);
                    }
                }

                if (newListT.Count == 0)
                {
                    return default(T);
                }
                else
                {
                    return newListT[UnityEngine.Random.Range(0, newListT.Count)];
                }
            }
        }


        //ActiveSkillDicTeam1.ExtendContains((KeyValuePair<AI, ISkillActiveByNode> iSkillKVP) => { return iSkillKVP.Key == aI;/*出现一次直接代表包含，调用者处理包含的详细条件*/}))
        public delegate bool ActionBack<T1>(T1 arg1);

        public static bool ExtendContains<T>(this List<T> list, ActionBack<T> actionBackContains) //返回值需是turn
        {
            bool IsContainers = false;
            if (list == null || actionBackContains == null)
            {
                //return false;
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                //你自己写条件，因为我不知道你T到底是什么结构
                if (actionBackContains(list[i]))
                {
                    return actionBackContains(list[i]);
                }
            }

            return IsContainers;
        }

        public static void SortDic<K, Y>(this Dictionary<K, Y> dic, Action<K, Y> action)
        {
            if (dic == null || action == null)
            {
                return;
            }

            Dictionary<K, Y>.Enumerator enumerator = dic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.Value != null)
                {
                    action(enumerator.Current.Key, enumerator.Current.Value);
                }
                else
                {
                }
            }
        }

        public static string ChangeTimeFormat(float fullSec)
        {
            string time = "";
            int hour;
            int minute;
            float second;

            hour = Mathf.FloorToInt(fullSec / 3600);
            minute = Mathf.FloorToInt((fullSec % 3600) / 60);
            second = (fullSec % 3600) % 60;

            time = hour.ToString() + ";" + minute.ToString() + ";" + second.ToString("F1");
            return time;
        }

        /// <summary>
        /// 获取字符串长度 字母数字为1 汉字为2
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetLength(string str)
        {
            if (str.Length == 0)
                return 0;
            ASCIIEncoding ascii = new ASCIIEncoding();
            int tempLen = 0;
            byte[] s = ascii.GetBytes(str);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
            }
            return tempLen;
        }


        /// <summary>
        /// 秒数转成时间格式
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="isShowSeconds"></param>
        /// <returns></returns>
        public static string ConvertTimeAdd(int seconds, bool isShowSeconds = true)
        {
            int day = seconds / 86400;

            int hour = (seconds / 3600) % 24;

            int minute = (seconds / 60) % 60;

            int sec = seconds % 60;


            string timeDesp = "";
            if (day > 0)
                timeDesp = day + ":"; //xx

            if (day > 0 || hour > 0)
            {
                if (hour >= 10)  //xx小时
                    timeDesp += hour + ":";
                else
                    timeDesp += "0" + hour + ":";
            }


            if (minute >= 10)   //xx分钟
                timeDesp += minute + ":";
            else
                timeDesp += "0" + minute + ":";

            if (isShowSeconds)  //秒
            {
                if (sec >= 10)
                    timeDesp += sec.ToString();
                else
                    timeDesp += "0" + sec;

            }

            return timeDesp;
        }
        /// <summary>
        /// 计算时间间隔 倒计时用
        /// 传入时间戳计算与当前时间的时间间隔
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static TimeSpan ConvertLongToDateTime(long d)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(d + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime dtResult = dtStart.Add(toNow);
            TimeSpan time = dtResult - DateTime.Now;
            return time;
        }

        //=============
    }
}

