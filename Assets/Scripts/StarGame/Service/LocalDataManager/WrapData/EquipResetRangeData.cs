using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class EquipResetRangeData
	{
		public Dictionary<int, EquipResetRangeDataCell> StaticEquipResetRangeDatas = new Dictionary<int, EquipResetRangeDataCell>();
	}
	public class EquipResetRangeDataCell
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
		//职业
		public List<int> Job_id = new List<int>();
		//部位
		public string Slot_id;
		private int _Slot_id = -1;
		public int GetSlot_id()
		{
			if (_Slot_id == -1 && int.TryParse(Slot_id, out _Slot_id))
			{
			}
			return _Slot_id;
		}
		//等级
		public string Level;
		private int _Level = -1;
		public int GetLevel()
		{
			if (_Level == -1 && int.TryParse(Level, out _Level))
			{
			}
			return _Level;
		}
		//属性id
		public string NatureId;
		private int _NatureId = -1;
		public int GetNatureId()
		{
			if (_NatureId == -1 && int.TryParse(NatureId, out _NatureId))
			{
			}
			return _NatureId;
		}
		//提升下限（/次）
		public string NumMin;
		private int _NumMin = -1;
		public int GetNumMin()
		{
			if (_NumMin == -1 && int.TryParse(NumMin, out _NumMin))
			{
			}
			return _NumMin;
		}
		//提升上限（/次）
		public string NumMax;
		private int _NumMax = -1;
		public int GetNumMax()
		{
			if (_NumMax == -1 && int.TryParse(NumMax, out _NumMax))
			{
			}
			return _NumMax;
		}

	}
}
