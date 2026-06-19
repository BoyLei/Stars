//来源表宝箱_Boxes.xlsm.xlsx -> sheet:Boxes
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BoxesData
	{
		[Key(0)]
		public Dictionary<long, BoxesDataCell> StaticBoxesDatas = new Dictionary<long, BoxesDataCell>();
	}
	[MessagePackObject]
	public class BoxesDataCell
	{
		//道具编号
		[Key(0)]
		public long Id;
		public long GetId()
		{
			return Id;
		}
		//固定奖励包
		[Key(1)]
		public long FixAward;
		public long GetFixAward()
		{
			return FixAward;
		}
		//是否自选
		[Key(2)]
		public int FixChoice;
		public int GetFixChoice()
		{
			return FixChoice;
		}
		//选择数量
		[Key(3)]
		public int FixChoiceCount;
		public int GetFixChoiceCount()
		{
			return FixChoiceCount;
		}
		//随机奖励包
		[Key(4)]
		public long RandAward;
		public long GetRandAward()
		{
			return RandAward;
		}
	}
}
