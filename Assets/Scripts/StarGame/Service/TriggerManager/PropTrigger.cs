///--------------------------------------------------------------------
/// 文件名   :   PropTrigger.cs
/// 内  容   :   
/// 说  明   :  属性触发器
/// 创建日期 :   2023/05/04 18:34:00
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using StarProject;
using StarProject.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using Task;
using UnityEngine;
namespace Trigger
{
    public class PropTrigger : BaseTrigger
    {
        public override TrrigerType M_TriggerType => TrrigerType.PropertyTrriger;

        /// <summary>
        /// 是否主角触发
        /// </summary>
        public bool IsMainRole { get; private set; }

        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropName { get; private set; }

        /// <summary>
        /// 比较值
        /// </summary>
        public int CompareValue { get; private set; }

        /// <summary>
        /// 比较符号
        /// </summary>
        public CompareEnum Compare { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propName">属性名</param>
        /// <param name="comparevalue">比较值</param>
        /// <param name="compare">比较符</param>
        /// <param name="isrole">是否主角触发</param>
        /// <param name="count">触发次数</param>
        /// <param name="action">回调</param>
        public PropTrigger(int index,long configid, string propName, string comparevalue, int compare, bool isrole, int count,bool isMainPlayer, System.Action<ulong> action) : base(count,false, action)
        {
            try
            {
                this.Index = index;
                this.ConfigID = configid;
                this.PropName = propName;
                this.CompareValue = System.Int32.Parse(comparevalue);
                this.Compare = (CompareEnum)compare;
                this.IsMainRole = isrole;
            }
            catch (Exception e)
            {
                SGF.Debuger.LogError($"地图NPC配置有问题 index={index},configid={configid},propName={propName},comparevalue={comparevalue},compare={compare},err={e}");
            }

            OnCreate();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            GlobalEvent.OnPropChange.AddListener(OnPropChangeHandler);
        }

        private void OnPropChangeHandler(ulong entityID, string propName, object value)
        {
            if (!IsValid())
            {
                return;
            }
            ulong uid = M_EntityID;
            if (IsMainRole)
            {
                uid = GameManager.Instance.mainPlayerId;
            }

            if (entityID != uid)
            {
                return;
            }

            if (PropName != propName)
            {
                return;
            }

            int newvalue = Convert.ToInt32(value);
            if (DoCompare(newvalue))
            {
                OnTrigger();
            }
        }

        private bool DoCompare(int newvalue)
        {
            switch (Compare)
            {
                case CompareEnum.Equal:
                    if (newvalue == CompareValue)
                    {
                        return true;
                    }
                    break;
                case CompareEnum.Greater:
                    if (newvalue > CompareValue)
                    {
                        return true;
                    }
                    break;
                case CompareEnum.Less:
                    if (newvalue < CompareValue)
                    {
                        return true;
                    }
                    break;
                case CompareEnum.GreaterEqual:
                    if (newvalue >= CompareValue)
                    {
                        return true;
                    }
                    break;
                case CompareEnum.LessEqual:
                    if (newvalue <= CompareValue)
                    {
                        return true;
                    }
                    break;
                case CompareEnum.UnEqual:
                    if (newvalue != CompareValue)
                    {
                        return true;
                    }
                    break;
            }
            return false;


        }

        protected override void OnRelease()
        {
            GlobalEvent.OnPropChange.RemoveListener(OnPropChangeHandler);
            base.OnRelease();
        }
    }
}