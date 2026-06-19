using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using StarProjectDef;
using Sirenix.OdinInspector.Editor;

public class ProtoWindow : OdinEditorWindow
{
    private const string pattern = @"\/\*([\s\S]*?)\bmessage (.*?)\s";

    [DisplayAsString] public string Tips = @"
    帮助客户端根据协议生成对应的Lua 文件
    ";

    public ModuleDef.Name ModuleName;

    [Sirenix.OdinInspector.FilePath] public string ProtoFilePath;

    [MenuItem("Tools/ProtoWindow")]
    public static void OpenWidow()
    {
        var window = GetWindow<ProtoWindow>();
        window.Show();
    }

    public class Info
    {
        public string Note;
        public string MessageName;
    }

    private List<Info> C2SList = new List<Info>();

    private List<Info> S2CList = new List<Info>();

    [Button("执行")]
    public void Execute()
    {
        C2SList.Clear();
        S2CList.Clear();
        string content = System.IO.File.ReadAllText(ProtoFilePath);
        //Debug.LogError(content);

        MatchCollection matches = Regex.Matches(content, pattern);
        foreach (Match match in matches)
        {
            int s = match.Value.IndexOf('/');
            int e = match.Value.LastIndexOf('/');
            string note = match.Value.Substring(s, e);

            string msg = match.Value.Replace(note, string.Empty);
            msg = msg.Replace("message", string.Empty);
            msg = msg.Replace("{", string.Empty);
            msg = msg.Replace("/", string.Empty);
            msg = msg.Trim();

            string sNote = note.Replace("/", string.Empty).Replace("*", string.Empty).Trim();
            //Debug.LogError($"{sNote}");
            //Debug.LogError($"{msg}");
            //Debug.LogError(note);
            if (match.Value.Contains("Req"))
            {
                C2SList.Add(new Info() { Note = sNote, MessageName = msg });
                continue;
            }

            if (match.Value.Contains("Ret") || match.Value.Contains("Ntf"))
            {
                S2CList.Add(new Info() { Note = sNote, MessageName = msg });
                continue;
            }
            // Debug.LogError(match.Value);
        }

        StringBuilder sb = new StringBuilder();
        foreach (var info in C2SList)
        {
            sb.AppendLine($"--[[{info.Note}]]");
            sb.AppendLine($"[\"On{info.MessageName}\"]=function(...)");
            sb.AppendLine($"		{ModuleName}:{info.MessageName}(...)");
            sb.AppendLine($"end,");
            sb.AppendLine($"");
        }

        C2SMsgEvent = sb.ToString();
        sb.Clear();


        foreach (var info in C2SList)
        {
            sb.AppendLine($"--[[{info.Note}]]");
            sb.AppendLine($"function {ModuleName}:{info.MessageName}(...)");
            sb.AppendLine($"");
            sb.AppendLine($"end");
        }
        C2sMsgFunction = sb.ToString();
        sb.Clear();


        foreach (var info in S2CList)
        {
            sb.AppendLine($"--[[{info.Note}]]");
            sb.AppendLine($"	self.{info.MessageName}=function(data)");
            sb.AppendLine($"		self:On{info.MessageName}(data)");
            sb.AppendLine($"	end");
            sb.AppendLine($"	NetworkManager.Instance:OnMessage(CS.ProtoMsg.{info.MessageName}.Descriptor.FullName, self.{info.MessageName}, self)");
            sb.AppendLine($"");
        }
        S2CMsgEvent = sb.ToString();
        sb.Clear();
        foreach (var info in S2CList)
        {
            sb.AppendLine($"--[[{info.Note}]]");
            sb.AppendLine($"	NetworkManager.Instance:OffMessage(CS.ProtoMsg.{info.MessageName}.Descriptor.FullName, self.{info.MessageName}, self);");
            sb.AppendLine($"");
        }
        S2CMsgUnEvent = sb.ToString();
        sb.Clear();
        
        foreach (var info in S2CList)
        {
            sb.AppendLine($"--[[{info.Note}]]");
            sb.AppendLine($"function {ModuleName}:On{info.MessageName}(data)");
            sb.AppendLine($"");
            sb.AppendLine($"end");
        }
        S2CMsgFunction = sb.ToString();
        sb.Clear();
    }

    [FoldoutGroup("客户端发给服务器")]
    [LabelText("消息注册")]
    [TextArea(1,1000)]
    public string C2SMsgEvent;
    
    [FoldoutGroup("客户端发给服务器")]
    [LabelText("方法")]
    [TextArea(1,1000)]
    public string C2sMsgFunction;
    
    [FoldoutGroup("服务器发给客户端")]
    [LabelText("消息注册")]
    [TextArea(1,1000)]
    public string S2CMsgEvent;
    
    [FoldoutGroup("服务器发给客户端")]
    [LabelText("消息解注册")]
    [TextArea(1,1000)]
    public string S2CMsgUnEvent;
    
    [FoldoutGroup("服务器发给客户端")]
    [LabelText("方法")]
    [TextArea(1,1000)]
    public string S2CMsgFunction;
    
}