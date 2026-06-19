//来源表CharacterData
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
namespace StarProjectDef
{
	public class CharacterData
	{
		public Dictionary<int, CharacterDataCell> StaticCharacterDatas = new Dictionary<int, CharacterDataCell>();
	}
	public class CharacterDataCell
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
		//化身ID
		public string AvatarID;
		private int _AvatarID = -1;
		public int GetAvatarID()
		{
			if (_AvatarID == -1 && int.TryParse(AvatarID, out _AvatarID))
			{
			}
			return _AvatarID;
		}
		//职业名
		public string JobName;
		//职业描述
		public string Desc;
		//性别选择
		public List<int> SexSelect = new List<int>();
		//职业标签
		public List<string> Label = new List<string>();
		//四维能力值
		public List<int> Power_Value = new List<int>();
		//转职方向
		public List<string> Switch = new List<string>();
		//转职名称
		public List<string> Name = new List<string>();
		//职业角标
		public string Icon_Image;
		//职业底图
		public string Back_Image;

	}
}
