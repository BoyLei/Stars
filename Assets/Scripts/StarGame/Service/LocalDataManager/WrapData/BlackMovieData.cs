//来源表剧情配置表_Plot.xlsm.xlsx -> sheet:BlackMovie
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using MessagePack;
using StarProject.Service.Language;
namespace StarProjectDef
{
	[MessagePackObject]
	public class BlackMovieData
	{
		[Key(0)]
		public Dictionary<int, BlackMovieDataCell> StaticBlackMovieDatas = new Dictionary<int, BlackMovieDataCell>();
	}
	[MessagePackObject]
	public class BlackMovieDataCell
	{
		//编号
		[Key(0)]
		public int Id;
		public int GetId()
		{
			return Id;
		}
		//下一段
		[Key(1)]
		public int Next_id;
		public int GetNext_id()
		{
			return Next_id;
		}
		//出现方式
		[Key(2)]
		public int Show_type;
		public int GetShow_type()
		{
			return Show_type;
		}
		//持续时间
		[Key(3)]
		public int Time;
		public int GetTime()
		{
			return Time;
		}
		//黑幕文本内容
		[Key(4)]
		public string Desc
		{
			get { return LanguageManager.Instance.GetLanguageByKey(_Desc); }
			set { _Desc = value; }
        }
		[IgnoreMember]
		private string _Desc;
		//语音
		[Key(5)]
		public int Voice;
		public int GetVoice()
		{
			return Voice;
		}
		//对齐方式
		[Key(6)]
		public int Align;
		public int GetAlign()
		{
			return Align;
		}
	}
}
