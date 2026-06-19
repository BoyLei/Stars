using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ProtoBuf;
using SGF.Network.SSPLite;
using StarProject.Game.Data;

namespace StarProject.Module.StarWordGame.Data
{
	

	   //[ProtoContract]
	public class StarWorldStartParam
	{
		//[ProtoMember(1)]
		public SSPParam sspParam = new SSPParam();
		//[ProtoMember(2)] 
		
		public GameParam gameParam = new GameParam();


		//[ProtoMember(3)] 
		public List<VitalSignData> players = new List<VitalSignData>();

		public ulong MainPlayerEnityId;

		public override string ToString()
		{
			XmlSerializer xs = new XmlSerializer(typeof(StarWorldStartParam));
			StringBuilder sb = new StringBuilder();
			TextWriter tw = new StringWriter(sb);
			xs.Serialize(tw, this);
			tw.Flush();
			return sb.ToString();
		}
	}
}
