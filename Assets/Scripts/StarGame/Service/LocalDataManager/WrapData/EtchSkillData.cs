//来源表EtchSkillData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class EtchSkillData
	{
		public Dictionary<int, EtchSkillDataCell> StaticEtchSkillDatas = new Dictionary<int, EtchSkillDataCell>();
	}
	public class EtchSkillDataCell
	{
		//鸣器id
		public string Id;
		private int _Id = -1;
		public int GetId()
		{
			if (_Id == -1 && int.TryParse(Id, out _Id))
			{
			}
			return _Id;
		}
		//战技ID
		public string BattleSkillID;
		private int _BattleSkillID = -1;
		public int GetBattleSkillID()
		{
			if (_BattleSkillID == -1 && int.TryParse(BattleSkillID, out _BattleSkillID))
			{
			}
			return _BattleSkillID;
		}

	}
}
