using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SGF { 
    /// <summary>
    /// 设计这个就是依赖Unity，驱动能力，动态注册的。
    /// 但是这个要限定一点：这个东西是可控的数量，已经明确的挂载脚本的。
    /// 如果不明确一大堆要注册进去，那就用池，工厂，因为factory他是可控的。
    /// </summary>
public interface IRegistrable
{
    /// <summary>
    /// List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="cell"></param>
    void RegToICollection(ICollection<IRegistrable> list, IRegistrable cell);

    /// <summary>
    /// Dic
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <param name="list"></param>
    /// <param name="cell"></param>
    void RegToICollection<K, IRegistrable>(ICollection<KeyValuePair<K, IRegistrable>> list,K kkk ,IRegistrable cell);


        //不过一定的了，拓展方法（跟lua一样重写”,“运算符的方式），肯定在类里面玩的
  //      /// <summary>
  //      /// List
		///// 还是点运算符的重写哦
		///// </summary>
		///// <param name="list"></param>
		///// <param name="cell"></param>
  //      public void RegToICollection(this ICollection<IRegistrable> list, IRegistrable cell);
  //      /// <summary>
  //      /// Dic
  //      /// </summary>
  //      /// <typeparam name="K"></typeparam>
  //      /// <typeparam name="IRegistrable"></typeparam>
  //      /// <param name="list"></param>
  //      /// <param name="cell"></param>
  //      public void RegToICollection<K, IRegistrable>(this ICollection<KeyValuePair<K, IRegistrable>> list, IRegistrable cell);
    }
}