using Google.Protobuf;
using ProtoMsg;
using System.Collections.Generic;

[XLua.LuaCallCSharp]
public static class RecordManager
{
    //public class PBMsg 
    //{
    //    public EMsg type;
    //    public string data;
    //}

    //public class RecordItem
    //{
    //    public string txt;
    //    public List<PBMsg> extra;
    //}

    //static int maxNum = 10;

    //public static List<RecordItem> items = new List<RecordItem>();
    //public static void PushItem(string str, ChatExtraInfos info)
    //{
    //    RecordItem item = new RecordItem();
    //    item.txt = str;
    //    item.extra = new List<PBMsg>();

    //    int count = info.ChatExtraInfo.Count;
    //    if (count > 0)
    //    {
    //        for (int i = 0; i < count; i++)
    //        {
    //            var child = info.ChatExtraInfo[i];
    //            PBMsg pBMsg = new PBMsg();
    //            pBMsg.type = child.EType;
    //            pBMsg.data = child.ToString();

    //        }
    //    }


    //    //items.Add(new RecordItem { txt = str, extra = info.Clone() });
    //    if (items.Count > maxNum)
    //    {
    //        items.RemoveRange(0, items.Count - maxNum); 
    //    }



    public struct RecordItem
    {
        public string txt;
        public string extra;  // ChatExtraInfos 这里要转成string,用的时候在转，【pb的结构不能拷贝】
    }

    static int maxNum = 10;

    public static List<RecordItem> items = new List<RecordItem>();
    public static void PushItem(string str, ChatExtraInfos info)
    {
        string data = info.ToString();
        items.Add(new RecordItem { txt = str, extra = data });
        if (items.Count > maxNum)
        {
            items.RemoveRange(0, items.Count - maxNum);
        }
    }
}
