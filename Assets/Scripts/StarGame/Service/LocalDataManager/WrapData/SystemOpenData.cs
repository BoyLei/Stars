//来源表SystemOpenData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SystemOpenData
	{
		public Dictionary<int, SystemOpenDataCell> StaticSystemOpenDatas = new Dictionary<int, SystemOpenDataCell>();
	}
	public class SystemOpenDataCell
	{
		//编号
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//功能开启条件
		public string Value;
		private int _Value = -1;
		public int GetValue()
		{
			if (_Value == -1 && int.TryParse(Value, out _Value))
			{
			}
			return _Value;
		}
		//批注名字
		public string Note;
		//备注
		public string Comment;
		//枚举类型
		public string EnumName;

	}
}
