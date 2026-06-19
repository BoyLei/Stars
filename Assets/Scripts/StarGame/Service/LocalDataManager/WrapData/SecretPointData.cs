//来源表SecretPointData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class SecretPointData
	{
		public Dictionary<int, SecretPointDataCell> StaticSecretPointDatas = new Dictionary<int, SecretPointDataCell>();
	}
	public class SecretPointDataCell
	{
		//刷怪点
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//中心点
		public string Center;
		//范围
		public string Range;
		private int _Range = -1;
		public int GetRange()
		{
			if (_Range == -1 && int.TryParse(Range, out _Range))
			{
			}
			return _Range;
		}
		//怪物群落
		public string GroupID;
		private int _GroupID = -1;
		public int GetGroupID()
		{
			if (_GroupID == -1 && int.TryParse(GroupID, out _GroupID))
			{
			}
			return _GroupID;
		}

	}
}
