//来源表Common_Dialog.xlsx -> sheet:Common
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
namespace StarProjectDef
{
	[MessagePackObject]
	public class CommonData
	{
		[Key(0)]
		public Dictionary<int, CommonDataCell> StaticCommonDatas = new Dictionary<int, CommonDataCell>();
	}
	[MessagePackObject]
	public class CommonDataCell
	{
		//对话id
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//对话类型
		[Key(1)]
		public int Dialog_type;
		public int GetDialog_type()
		{
			return Dialog_type;
		}
		//下一句id
		[Key(2)]
		public List<int> Next_dialog_id = new List<int>();
		//对话者形象
		[Key(3)]
		public string Spine_id;
		//坐标偏移
		[Key(4)]
		public string Offset;
		//对话者名字
		[Key(5)]
		public string Name;
		//对话内容
		[Key(6)]
		public string Dialog_desc;
		//对话选项
		[Key(7)]
		public string Dialog_option;
		//语音id
		[Key(8)]
		public int Voice_id;
		public int GetVoice_id()
		{
			return Voice_id;
		}
		//是否跳过
		[Key(9)]
		public int Is_skip;
		public int GetIs_skip()
		{
			return Is_skip;
		}
		//显示时间
		[Key(10)]
		public int Show_time;
		public int GetShow_time()
		{
			return Show_time;
		}
	}
}
