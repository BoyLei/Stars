using SGF.Module.Framework;
using UnityEngine;
using XLua;
using System.Collections.Generic;

namespace SGF.UI.Framework
{
    [LuaCallCSharp]
    //提取常用代码到Xlua,被统一管理
    //所有模块都要通过Module来存储。除例外情况
    //本层隔离3方代码
    public class SaveManager : ServiceModule<SaveManager>
    {
        /// <summary>
        /// 非发布多语言时常变量，需在Module中管理
        /// </summary>
        private string DATA_PATH;

        internal void Init(string dataName)
        {
            CheckSingleton();
            DATA_PATH = dataName;
        }

        public override void Release()
        {
            base.Release();
        }

        public void Save<T>(string key, T value, string childPath, string fileName = "data")
        {
            string path = DATA_PATH + childPath + "/" + fileName;
            string cacheKey = $"{Fire.Utils.GetPlatformString()}_{key}";
            ES3.Save<T>(cacheKey, value, path);
            Debug.Log("@成功【写入】数据到本地：" + cacheKey + "---" + value + "---" + path);
        }

        public void Save(string key, List<ulong> pids, string childPath, string fileName = "data")
        {
            Save<List<ulong>>(key, pids, childPath, fileName);
        }

        public T Load<T>(string key, string childPath, string fileName = "data")
        {
            string cacheKey = $"{Fire.Utils.GetPlatformString()}_{key}";
            string path = DATA_PATH + childPath + "/" + fileName;
            T datas = ES3.Load<T>(cacheKey, path);
            Debug.Log("@成功【读取】数据到本地：" + cacheKey + "---" + datas + "---" + path);
            return datas;
        }

        public List<ulong> Load(string key, string childPath, string fileName = "data")
        {
            if(KeyExists(key, childPath, fileName))
            {
                return Load<List<ulong>>(key, childPath, fileName);
            }
            return new List<ulong>();
        }

        public bool KeyExists(string key, string childPath, string fileName = "data")
        {
            string cacheKey = $"{Fire.Utils.GetPlatformString()}_{key}";
            string path = DATA_PATH + childPath + "/" + fileName;
            Debug.Log("@成功【查看】数据到本地：" + cacheKey  + "---" + path);
            return ES3.KeyExists(cacheKey, path);
        }

        //TODO:https://blog.csdn.net/ChinarCSDN/article/details/88984861     给小曲得参考

        //ES3.LoadInto<T>() 把数据，直接赋值给指定对象
        //ES3.KeyExists() 判断该数据中，键是否存在

        //ES3.DeleteKey() 删除一条数据(指定键对应的数据)
        //ES3.DeleteFile() 删除一个数据文件
        //ES3.DeleteDirectory()

        //Encryption

        /// <summary>
        /// 初始化操作
        /// </summary>


    }
}
