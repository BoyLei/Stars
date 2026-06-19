//来源表FuncOpenConditionsData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class FuncOpenConditionsData
	{
		public Dictionary<int, FuncOpenConditionsDataCell> StaticFuncOpenConditionsDatas = new Dictionary<int, FuncOpenConditionsDataCell>();
	}
	public class FuncOpenConditionsDataCell
	{
		//功能ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//功能名称
		public string FuncName;
		//备注
		public string Comment;
		//开启条件组
		public string OpenCondition;
		private int _OpenCondition = -1;
		public int GetOpenCondition()
		{
			if (_OpenCondition == -1 && int.TryParse(OpenCondition, out _OpenCondition))
			{
			}
			return _OpenCondition;
		}

	}
}
