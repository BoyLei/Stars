using System.Collections;
using System.Collections.Generic;
using StarProject.Module;
using UnityEngine;

namespace StarProject.Module
{
    /**
     * BaseScenePlay 是一个抽象基类，为所有场景的脚本类提供基础行为
     */
    public abstract class BaseScenePlay
    {
        /// <summary>
        /// 获取或设置一个值，该值指示当前场景是否正在播放。
        /// </summary>
        public bool IsPlaying { get; protected set; }

        /**
        * 获取或设置场景播放类型的属性
        * 属性类型：ScenePlayType，这是一个枚举类型，用于标识场景播放的类型
        * get 访问器：公开，允许外部代码读取类型属性
        * set 访问器：私有的，只允许类内部的代码修改变更属性
        * 总结：这个属性提供了访问和修改场景播放类型的接口，确保类型只能在类内部进行更改
        */
        public ScenePlayType ScenePlayType { get; private set; }

        // 受保护的构造函数，只允许继承类调用
        /**
         * 构造函数 BaseScenePlay，用于初始化基类实例
         * 参数：type（ScenePlayType 枚举类型），表示场景播放类型
         * 构造函数将传入的类型参数赋值给 ScenePlayType 属性，完成对象初始化
         * 总结：这个构造函数允许继承类在创建实例时，指定场景播放的类型
         */
        protected BaseScenePlay(ScenePlayType type)
        {
            ScenePlayType = type;
        }

        /**
         * 当播放开始时调用此方法
         */
        public abstract void OnPlayBegine(int playId);

        /**
         * 当播放结束时调用此方法
         */
        public abstract void OnPlayEnd();

        /**
         * OnUpdate 方法可以被重写以在每一帧更新场景逻辑
         */
        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 调用此方法以执行场景的刷新逻辑
        /// </summary>
        public virtual void OnFresh()
        {
        }
    }
}