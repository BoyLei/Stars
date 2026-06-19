//来源表GNActivBUFFData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class GNActivBUFFData
	{
		public Dictionary<int, GNActivBUFFDataCell> StaticGNActivBUFFDatas = new Dictionary<int, GNActivBUFFDataCell>();
	}
	public class GNActivBUFFDataCell
	{
		//ID
		public string ID;
		private int _ID = -1;
		public int GetID()
		{
			if (_ID == -1 && int.TryParse(ID, out _ID))
			{
			}
			return _ID;
		}
		//对应BUFFID
		public string BuffID;
		private int _BuffID = -1;
		public int GetBuffID()
		{
			if (_BuffID == -1 && int.TryParse(BuffID, out _BuffID))
			{
			}
			return _BuffID;
		}
		//所属BUFF库
		public string BuffType;
		private int _BuffType = -1;
		public int GetBuffType()
		{
			if (_BuffType == -1 && int.TryParse(BuffType, out _BuffType))
			{
			}
			return _BuffType;
		}
		//出现权重
		public string Weight;
		private int _Weight = -1;
		public int GetWeight()
		{
			if (_Weight == -1 && int.TryParse(Weight, out _Weight))
			{
			}
			return _Weight;
		}

	}
}
