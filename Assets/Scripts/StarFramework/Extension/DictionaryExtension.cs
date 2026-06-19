using System;
using System.Collections.Generic;


public class DictionaryEx<TKey, TValue> : Dictionary<TKey, TValue>
{
    public new TValue this[TKey indexKey]
    {
        set { base[indexKey] = value; }
        get
        {
            if (base.ContainsKey(indexKey))
            {
                return base[indexKey];
            }
            else
            {
                return default(TValue);
            }
            /// 2024/1/26
            /// DL:
            /// 不用异常的方式, 这玩意在 win包调试的时候,每次 都会捕获异常上百次,严重影响调试体验 
            // try
            // {
            //     return base[indexKey];
            // }
            // catch (Exception)
            // {
            //     return default(TValue);
            // }
        }
    }
}
