using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class InteractConditionData
	{
		public Dictionary<int, InteractConditionDataCell> StaticInteractConditionDatas = new Dictionary<int, InteractConditionDataCell>();
	}
	public class InteractConditionDataCell
	{		//条件ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//条件类型
		public string ConditionType;
		private int _ConditionType = -1;
		public int GetConditionType()
		{
			if (_ConditionType == -1 && int.TryParse(ConditionType, out _ConditionType))
			{
			}
			return _ConditionType;
		}
		//提示描述
		public string Desc;
		//不满足时是否显示
		public string IsShow;
		public bool GetIsShow()
		{
			bool _IsShow;
			if (bool.TryParse(IsShow, out _IsShow))
			{
			}
			return _IsShow;
		}
		//是否为扣除
		public string IsCost;
		public bool GetIsCost()
		{
			bool _IsCost;
			if (bool.TryParse(IsCost, out _IsCost))
			{
			}
			return _IsCost;
		}
		//参数1
		public string Args1;
		//参数2
		public string Args2;
		//参数3
		public string Args3;
		//参数4
		public string Args4;
		//参数5
		public string Args5;
		//参数6
		public string Args6;
		//参数7
		public string Args7;
		//参数8
		public string Args8;
		//参数9
		public string Args9;
		//参数10
		public string Args10;

	}
}
