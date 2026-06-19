using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using SkillEditor;
using UnityEditor;

#region 配置数据

public static class SkillConfigGenera
{
    public static string GeneraConfigPath = "Assets/DevTools/SkillEditor/Scripts/Define/";
    public static string GeneraSerializePath = "Assets/DevTools/SkillEditor/Scripts/Serialize/";
    public static string BaseDataConfigPath = "Assets/DevTools/SkillEditor/Config/BaseDataConfig.xml";
    public static string  MessagePackPattern = @"\[MessagePack\.Key\(\d+\)\]";
    public static string Node = @"
///--------------------------------------------------------------------
/// 文件名   :   #CLASSNAME#
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By BaseDataConfig.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
";

    public static string ClassTemplate = @"
    /// <summary>
    /// {0}
    /// </summary>
    [System.Serializable]
    [MessagePackObject(keyAsPropertyName:true)]
    public {1} class {2} {3}";

    public static string ClassTemplateItem = @"
        /// <summary>
        /// {0}
        /// </summary>
        [LabelText({1})]
        public {2} {3};";

    public static void CreateConfig()
    {
        if (!File.Exists(BaseDataConfigPath))
        {
            Debug.LogError("BaseDataConfig.xml 不存在");
            return;
        }

        Dictionary<string, ConfigTemplate> ConfigMap = new Dictionary<string, ConfigTemplate>();
        Dictionary<string, ConfigTemplate> SerializeMap = new Dictionary<string, ConfigTemplate>();
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(BaseDataConfigPath);
        if (textAsset != null && textAsset.text != null)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(textAsset.text);

            XmlNode xmlNode = document.SelectSingleNode("config");
            if (xmlNode != null && xmlNode.ChildNodes.Count > 0)
            {
                foreach (var item in xmlNode.ChildNodes)
                {
                    XmlElement node = item as XmlElement;
                    if (node == null || node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    string name = node.GetAttribute("name");
                    if (ConfigMap.ContainsKey(name) || SerializeMap.ContainsKey(name))
                    {
                        Debug.LogError($"存在重复值{name}");
                        continue;
                    }

                    string desc = node.GetAttribute("desc");
                    string super = node.GetAttribute("super");
                    bool ignore = node.GetAttribute("ignore").Equals("true") || node.GetAttribute("ignore").Equals("True");

                    string modifiers = node.GetAttribute("modifiers");
                    ConfigTemplate template = new ConfigTemplate(name, desc, super, ignore, modifiers);

                    foreach (var it in node.ChildNodes)
                    {
                        XmlElement child = it as XmlElement;
                        if (child == null || child.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        string child_name = child.GetAttribute("name");
                        string child_desc = child.GetAttribute("desc");
                        bool isEnum = child.GetAttribute("isEnum").Equals("true") || child.GetAttribute("isEnum").Equals("True");
                        string type = child.GetAttribute("type");
                        bool child_ignore = child.GetAttribute("ignore").Equals("true") || child.GetAttribute("ignore").Equals("True");
                        string child_SpecialbLabel = child.GetAttribute("SpecialbLabel");
                        bool child_prop = child.GetAttribute("IsProperty").Equals("true") || child.GetAttribute("IsProperty").Equals("True");
                        string child_propcontent = child.GetAttribute("PropertyContent");
                        string defaultvaule = child.GetAttribute("default");

                        string child_modifiers = child.GetAttribute("modifiers");
                        ConfigItemTemplate enumItem = new ConfigItemTemplate(child_name, child_desc, isEnum, type,
                            child_ignore, child_SpecialbLabel, child_prop, child_propcontent, defaultvaule,
                            child_modifiers);
                        template.AddItem(enumItem);
                    }

                    if (name.EndsWith("Serialize"))
                    {
                        SerializeMap.Add(name, template);
                    }
                    else
                    {
                        ConfigMap.Add(name, template);
                    }
                }
            }
        }

        GeneraConfig(ConfigMap);
        GeneraSerializeConfig(SerializeMap);
        UnityEditor.AssetDatabase.Refresh();
    }


    [MenuItem("Tools/SkillEditor/生成新的配置文件")]
    public static void ModifyConfig()
    {
        if (!File.Exists(BaseDataConfigPath))
        {
            Debug.LogError("BaseDataConfig.xml 不存在");
            return;
        }

        Dictionary<string, ConfigTemplate> ConfigMap = new Dictionary<string, ConfigTemplate>();
        Dictionary<string, ConfigTemplate> SerializeMap = new Dictionary<string, ConfigTemplate>();
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(BaseDataConfigPath);
        if (textAsset != null && textAsset.text != null)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(textAsset.text);

            XmlNode xmlNode = document.SelectSingleNode("config");
            if (xmlNode != null && xmlNode.ChildNodes.Count > 0)
            {
                foreach (var item in xmlNode.ChildNodes)
                {
                    XmlElement node = item as XmlElement;
                    if (node == null || node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    string name = node.GetAttribute("name");
                    if (ConfigMap.ContainsKey(name) || SerializeMap.ContainsKey(name))
                    {
                        Debug.LogError($"存在重复值{name}");
                        continue;
                    }

                    string desc = node.GetAttribute("desc");
                    string super = node.GetAttribute("super");
                    bool ignore = node.GetAttribute("ignore").Equals("true") || node.GetAttribute("ignore").Equals("True");

                    string modifiers = node.GetAttribute("modifiers");
                    ConfigTemplate template = new ConfigTemplate(name, desc, super, ignore, modifiers);

                    foreach (var it in node.ChildNodes)
                    {
                        XmlElement child = it as XmlElement;
                        if (child == null || child.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        string child_name = child.GetAttribute("name");
                        string child_desc = child.GetAttribute("desc");
                        bool isEnum = child.GetAttribute("isEnum").Equals("true") || child.GetAttribute("isEnum").Equals("True");
                        string type = child.GetAttribute("type");
                        bool child_ignore = child.GetAttribute("ignore").Equals("true") || child.GetAttribute("ignore").Equals("True");
                        string child_SpecialbLabel = child.GetAttribute("SpecialbLabel");
                        bool child_prop = child.GetAttribute("IsProperty").Equals("true") || child.GetAttribute("IsProperty").Equals("True");
                        string child_propcontent = child.GetAttribute("PropertyContent");
                        string defaultvaule = child.GetAttribute("default");

                        string child_modifiers = child.GetAttribute("modifiers");
                        ConfigItemTemplate enumItem = new ConfigItemTemplate(child_name, child_desc, isEnum, type,
                            child_ignore, child_SpecialbLabel, child_prop, child_propcontent, defaultvaule,
                            child_modifiers);
                        template.AddItem(enumItem);
                    }

                    if (name.EndsWith("Serialize"))
                    {
                        SerializeMap.Add(name, template);
                    }
                    else
                    {
                        ConfigMap.Add(name, template);
                    }
                }
            }
        }

        foreach (var item in ConfigMap)
        {
            item.Value.Modify();
        }
        foreach (var item in SerializeMap)
        {
            item.Value.Modify();
        }
        /*GeneraConfig(ConfigMap);
        GeneraSerializeConfig(SerializeMap);*/

        WriteModifyXml(ConfigMap,SerializeMap);
        
        UnityEditor.AssetDatabase.Refresh();
    }


    private static void WriteModifyXml(Dictionary<string, ConfigTemplate>  map1,Dictionary<string, ConfigTemplate> map2)
    {
        XmlDocument document = new XmlDocument();
        XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
        document.AppendChild(xmlDeclaration);

       /*
       var   className = document.CreateComment("Class name:类名");
       document.AppendChild(className);
       
       var   classDesc = document.CreateComment("Class desc:描述");
       document.AppendChild(classDesc);
       
       var   modifiers = document.CreateComment("Classs modifiers:修饰符 如 abstract ，partial");
       document.AppendChild(modifiers);
       
       var   super = document.CreateComment("Class super:父类名");
       document.AppendChild(super);
       */
       
       // 定义要添加的注释内容
       string[] comments = {
           "Class desc:类名",
           "Class desc:描述",
           "Class modifiers:修饰符 如 abstract ，partial",
           "Class super:父类名",
           "Member name:成员名",
           "Member desc:描述",
           "Member isEnum:是不是枚举",
           "Member type:成员类型",
           "Member ignore:是否忽略（忽略的字段不会生成代码，会通过partial 手动重写）",
           "Member SpecialLabel:特殊的标签 如 [ReadOnly][MessagePack.Key(0)] [Newtonsoft.Json.JsonIgnore][MessagePack.IgnoreMember]",
           "Member IsProperty:是不是属性",
           "Member PropertyContent:属性内容",
           "Member default:默认值",
           "Member [JsonConverter(typeof(StringEnumConverter))]:导出枚举的字符串"
       };

       // 遍历注释内容并创建注释节点
       foreach (string comment in comments)
       {
           // 按提供的格式创建注释
           XmlComment xmlComment = document.CreateComment(comment);

           // 将注释节点添加到根元素
           document.AppendChild(xmlComment);
       }
       
        var config= document.CreateElement("config");
        document.AppendChild(config);
        foreach (var item in map1)
        {
            XmlElement enumroot = document.CreateElement("enum");
            enumroot.SetAttribute("name", item.Value.ClassName);
            enumroot.SetAttribute("modifiers", item.Value.Modifiers);
            enumroot.SetAttribute("desc", item.Value.Desc);
            if (!string.IsNullOrEmpty(item.Value.Super))
            {
                enumroot.SetAttribute("super",  item.Value.Super);
            }
        

            foreach (var child in item.Value.Items)
            {
                XmlElement idItem = document.CreateElement("item");
                idItem.SetAttribute("name", child.Name);
                if (!string.IsNullOrEmpty(child.SpecialbLabel))
                {
                    idItem.SetAttribute("SpecialbLabel", child.SpecialbLabel);
                }
                idItem.SetAttribute("desc", child.Desc);
                if (child.IsEnum)
                {
                    idItem.SetAttribute("isEnum", child.IsEnum.ToString());
                }
 
                idItem.SetAttribute("type", child.Type);
                if (!string.IsNullOrEmpty(child.Defaultvaule))
                {
                    idItem.SetAttribute("default", child.Defaultvaule);
                }

                if (!string.IsNullOrEmpty(child.Modify))
                {
                    idItem.SetAttribute("modifiers", child.Modify);
                }

                if (child.Ignore)
                {
                    idItem.SetAttribute("ignore", child.Ignore.ToString());
                }

                if (child.IsProperty)
                {
                    idItem.SetAttribute("IsProperty", child.IsProperty.ToString());
                }
                
                if (!string.IsNullOrEmpty(child.PropertyContent))
                {
                    idItem.SetAttribute("PropertyContent", child.PropertyContent);
                }
           
                enumroot.AppendChild(idItem);
            }
            
            config.AppendChild(enumroot);
            
        }
        
        foreach (var item in map2)
        {
            XmlElement enumroot = document.CreateElement("enum");
            enumroot.SetAttribute("name", item.Value.ClassName);
            enumroot.SetAttribute("modifiers", item.Value.Modifiers);
            enumroot.SetAttribute("desc", item.Value.Desc);
            if (!string.IsNullOrEmpty(item.Value.Super))
            {
                enumroot.SetAttribute("super",  item.Value.Super);
            }
        

            foreach (var child in item.Value.Items)
            {
                XmlElement idItem = document.CreateElement("item");
                idItem.SetAttribute("name", child.Name);
                if (!string.IsNullOrEmpty(child.SpecialbLabel))
                {
                    idItem.SetAttribute("SpecialbLabel", child.SpecialbLabel);
                }
               
                idItem.SetAttribute("desc", child.Desc);
                if (child.IsEnum)
                {
                    idItem.SetAttribute("isEnum", child.IsEnum.ToString());
                }
 
                idItem.SetAttribute("type", child.Type);
                if (!string.IsNullOrEmpty(child.Defaultvaule))
                {
                    idItem.SetAttribute("default", child.Defaultvaule);
                }

                if (!string.IsNullOrEmpty(child.Modify))
                {
                    idItem.SetAttribute("modifiers", child.Modify);
                }

                if (child.Ignore)
                {
                    idItem.SetAttribute("ignore", child.Ignore.ToString());
                }

                if (child.IsProperty)
                {
                    idItem.SetAttribute("IsProperty", child.IsProperty.ToString());
                }
                
                if (!string.IsNullOrEmpty(child.PropertyContent))
                {
                    idItem.SetAttribute("PropertyContent", child.PropertyContent);
                }
           
                enumroot.AppendChild(idItem);
            }
            
            config.AppendChild(enumroot);
        }
        document.Save("Assets/DevTools/SkillEditor/Config/BaseDataConfig_New.xml");
        AssetDatabase.Refresh();
    }

    private static void GeneraConfig(Dictionary<string, ConfigTemplate> ConfigMap)
    {
        if (ConfigMap == null || ConfigMap.Count < 1)
        {
            return;
        }


        foreach (var item in ConfigMap)
        {
            if (item.Value.Ignore)
            {
                continue;
            }

            string path = GeneraConfigPath + item.Key + ".cs";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Node);
            stringBuilder.Append("using Sirenix.OdinInspector;\n");
            stringBuilder.Append("using System.Collections;\n");
            stringBuilder.Append("using System.Collections.Generic;\n");
            stringBuilder.Append("using UnityEngine;\n");
            stringBuilder.Append("using Newtonsoft.Json;\n");
            stringBuilder.Append("using MessagePack;\n");
            stringBuilder.Append("using Newtonsoft.Json.Converters;\n");
            stringBuilder.Append("namespace SkillEditor");
            stringBuilder.Append("\n");
            stringBuilder.Append("{");
            stringBuilder.Append(item.Value.ToString());
            stringBuilder.Append("\n}");
            FileStream fs = new FileStream(path, FileMode.CreateNew);
            string mContent = stringBuilder.ToString();
            mContent = mContent.Replace("#CLASSNAME#", item.Key);
            //mContent = mContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            byte[] bytes = Encoding.UTF8.GetBytes(mContent);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
    }

    private static void GeneraSerializeConfig(Dictionary<string, ConfigTemplate> SerializeMap)
    {
        if (SerializeMap == null || SerializeMap.Count < 1)
        {
            return;
        }


        foreach (var item in SerializeMap)
        {
            if (item.Value.Ignore)
            {
                continue;
            }

            string path = GeneraSerializePath + item.Key + ".cs";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Node);
            stringBuilder.Append("using Sirenix.OdinInspector;\n");
            stringBuilder.Append("using System.Collections;\n");
            stringBuilder.Append("using System.Collections.Generic;\n");
            stringBuilder.Append("using UnityEngine;\n");
            stringBuilder.Append("using Newtonsoft.Json;\n");
            stringBuilder.Append("using MessagePack;\n");
            stringBuilder.Append("using Newtonsoft.Json.Converters;\n");
            stringBuilder.Append("namespace SkillEditor");
            stringBuilder.Append("\n");
            stringBuilder.Append("{");
            stringBuilder.Append(item.Value.ToSerialize());
            stringBuilder.Append("\n}");
            FileStream fs = new FileStream(path, FileMode.CreateNew);
            string mContent = stringBuilder.ToString();
            mContent = mContent.Replace("#CLASSNAME#", item.Key);
            //mContent = mContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            byte[] bytes = Encoding.UTF8.GetBytes(mContent);
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
    }
}

public class ConfigTemplate
{
    public string ClassName { get; private set; }
    public string Desc { get; private set; }

    public string Super { get; private set; }
    public bool Ignore { get; private set; }
    public string Modifiers { get; private set; }
    public List<ConfigItemTemplate> Items { get; private set; }

    public void Modify()
    {
        int index = 0;
        foreach (var item in Items)
        {
            item.ModifySpecialbLabel(ref index);
        }
    }

    public ConfigTemplate(string name, string desc, string super, bool ignore, string modifiers)
    {
        this.ClassName = name;
        this.Desc = desc;
        this.Super = super;
        this.Ignore = ignore;
        this.Modifiers = modifiers;
        this.Items = new List<ConfigItemTemplate>();
    }

    public void AddItem(ConfigItemTemplate enumItem)
    {
        this.Items.Add(enumItem);
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("\n");
        builder.Append("    {\n");
        foreach (var item in Items)
        {
            if (item.Ignore)
            {
                continue;
            }

            if (!item.IsProperty)
            {
                builder.Append(item.ToString());
                builder.Append("\n");
            }
        }

        foreach (var item in Items)
        {
            if (item.IsEnum)
            {
                builder.Append(item.EnumFunction());
                builder.Append("\n");
            }
        }

        foreach (var item in Items)
        {
            if (item.Ignore)
            {
                continue;
            }

            if (item.IsProperty)
            {
                builder.Append(item.WriteProperty());
                builder.Append("\n");
            }
        }

        foreach (var item in Items)
        {
            if (item.Ignore)
            {
                continue;
            }

            if (item.Type == "List<CustomDictionary>")
            {
                if (item.Name == "BuffCopyData" || item.Name == "BulletCopyData" || item.Name == "PassiveCopyData")
                {
                    builder.Append(item.WriteCopy());
                    builder.Append("\n");
                }
            }
        }

        //BuffCopyData
        builder.Append("    }");
        builder.Append("\n");

        string className = ClassName;
        if (!string.IsNullOrEmpty(Super))
        {
            className += ":" + Super;
        }

        return string.Format(SkillConfigGenera.ClassTemplate, Desc, Modifiers, className, builder.ToString());
    }

    public string ToSerialize()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("\n");
        builder.Append("    {\n");
        foreach (var item in Items)
        {
            if (item.Ignore)
            {
                continue;
            }

            builder.Append(item.ToSerialize());
            builder.Append("\n");
        }

        string EnumName = string.Empty;
        string Enum = string.Empty;

        foreach (var item in Items)
        {
            if (item.IsEnum)
            {
                builder.Append(item.EnumFunction());
                builder.Append("\n");

                EnumName = item.Name;
                Enum = item.Type;
            }
        }

        if (!string.IsNullOrEmpty(EnumName) && !string.IsNullOrEmpty(Enum))
        {
            foreach (var item in Items)
            {
                if (!item.IsEnum)
                {
                    builder.Append(item.SerializeFunction(EnumName, Enum));
                    builder.Append("\n");
                }
            }
        }

        foreach (var item in Items)
        {
            if (item.Ignore)
            {
                continue;
            }

            if (item.Type == "List<CustomDictionary>")
            {
                if (item.Name == "BuffCopyData" || item.Name == "BulletCopyData" || item.Name == "PassiveCopyData")
                {
                    builder.Append(item.WriteCopy());
                    builder.Append("\n");
                }
            }
        }

        builder.Append("    }");
        builder.Append("\n");

        string className = ClassName;
        if (!string.IsNullOrEmpty(Super))
        {
            className += ":" + Super;
        }

        return string.Format(SkillConfigGenera.ClassTemplate, Desc, Modifiers, className, builder.ToString());
    }
}

public class ConfigItemTemplate
{
    public string Name { get; private set; }
    public string Desc { get; private set; }
    public bool IsEnum { get; private set; }
    public string Type { get; private set; }

    public bool Ignore { get; private set; }

    public string SpecialbLabel { get; private set; }

    public bool IsProperty { get; private set; }

    public string PropertyContent { get; private set; }

    public string Defaultvaule { get; private set; }

    public string Modify { get; private set; }

    public ConfigItemTemplate(string name, string desc, bool isEnum, string type, bool ignore, string specialbLabel,
        bool isprop, string propContent, string defaultvaule, string modify)
    {
        this.Name = name;
        this.Desc = desc;
        this.IsEnum = isEnum;
        this.Type = type;
        this.Ignore = ignore;
        this.SpecialbLabel = specialbLabel;
        this.IsProperty = isprop;
        this.PropertyContent = propContent;
        this.Defaultvaule = defaultvaule;
        this.Modify = modify;
    }

    public void ModifySpecialbLabel(ref int index)
    {
        if (SpecialbLabel.Contains("JsonIgnore"))
        {
            if(!SpecialbLabel.Contains("[MessagePack.IgnoreMember]"))
            {
                SpecialbLabel += "[MessagePack.IgnoreMember]";
            }

        }
        else
        {
            if (SpecialbLabel.Contains("MessagePack"))
            {
                // 使用Regex.Replace替换掉所有匹配的标签
                SpecialbLabel = Regex.Replace(SpecialbLabel, SkillConfigGenera.MessagePackPattern, "");
            }
            /*if (SpecialbLabel.Contains("[MessagePack.Key("))
            {
                SpecialbLabel = SpecialbLabel.Replace("[MessagePack.Key(*)]", $"[MessagePack.Key({ index++})]");
            }
            else
            {
                SpecialbLabel += $"[MessagePack.Key({index++})]";
            }*/
        }
    }

    public bool IsFundamental(string type)
    {
        if (type == "int" || type == "long" || type == "float" || type == "double" ||
            type == "bool" || type == "string" || type == "DateTime" || type == "byte" || type == "uint" ||
            type == "ulong" || type == "sbyte" || type == "short" || type == "ushort" || type == "char" ||
            type == "decimal"
            || type == "byte[]" || type == "Guid" || type == "TimeSpan" || type== "BaseEffectType")
        {
            return true;
        }

        return false;
    }

    public bool IsCollection(System.Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type);
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("        /// <summary>\n");
        builder.Append($"        /// {Desc}\n");
        builder.Append("        /// <summary>\n");
        builder.Append("        [LabelText(\"");
        builder.Append($"{Desc}");
        builder.Append("\")]\n");
        if (!string.IsNullOrEmpty(SpecialbLabel))
        {
            builder.Append($"        {SpecialbLabel}\n");
        }

        if (IsEnum)
        {
            builder.Append("        [ValueDropdown(\"");
            builder.Append($"_{Name.ToLower()}");
            builder.Append("\")]\n");
        }


        string defaultv = $"= new {Type}()";
        if (IsFundamental(Type))
        {
            //基础类型  集合类型 
            defaultv = "";
        }

        if (Type.Contains("[]"))
        {
            //基础类型  集合类型 
            defaultv = "";
        }

        if (!string.IsNullOrEmpty(Defaultvaule))
        {
            defaultv = "=" + Defaultvaule;
        }

        builder.Append($"        public {Type} {Name}{defaultv};\n");
        return builder.ToString();
    }

    public string WriteProperty()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("        /// <summary>\n");
        builder.Append($"        /// {Desc}\n");
        builder.Append("        /// <summary>\n");
        builder.Append("        [LabelText(\"");
        builder.Append($"{Desc}");
        builder.Append("\")]\n");
        if (!string.IsNullOrEmpty(SpecialbLabel))
        {
            builder.Append($"        {SpecialbLabel}\n");
        }
        if (!string.IsNullOrEmpty(Modify))
        {
            builder.Append($"        public {Modify} {Type} {Name} \n");
        }
        else
        {
            builder.Append($"        public {Type} {Name} \n");
        }

        builder.Append("        {\n");
        builder.Append($"           {PropertyContent}\n");
        builder.Append("        }\n");
        return builder.ToString();
    }

    public string ToSerialize()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("        /// <summary>\n");
        builder.Append($"        /// {Desc}\n");
        builder.Append("        /// <summary>\n");

        builder.Append("        [LabelText(\"");
        builder.Append($"{Desc}");
        builder.Append("\")]\n");
        if (!string.IsNullOrEmpty(SpecialbLabel))
        {
            builder.Append($"        {SpecialbLabel}\n");
        }

        if (IsEnum)
        {
            builder.Append($"        [ValueDropdown(\"_{Name.ToLower()}\")]\n");
        }
        else
        {
            builder.Append("        [SerializeField]\n");
            builder.Append($"        [ShowIf(\"ShouldSerialize{Name}\")]\n");
        }

        string defaultv = $"= new {Type}()";
        if (Type == "string")
        {
            defaultv = "= \"\"";
        }

        if (Type.Contains("[]"))
        {
            defaultv = "";
        }

        if (!string.IsNullOrEmpty(Defaultvaule))
        {
            defaultv = "=" + Defaultvaule;
        }

        builder.Append($"        public {Type} {Name}{defaultv};\n");
        return builder.ToString();
    }

    public string EnumFunction()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append($"        public IEnumerable _{Name.ToLower()}()\n");
        builder.Append("        {\n");
        builder.Append(
            $"            return EnumDefineMap._{Type.Replace("[]", "").Replace("List<", "").Replace(">", "").ToLower()};\n"); //如果类型是数组，则把"[]"和List替换掉，去寻找对应枚举
        builder.Append("        }\n");
        return builder.ToString();
    }

    public string SerializeFunction(string enName, string Enum)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append($"        public bool ShouldSerialize{Name}()\n");
        builder.Append("        {\n");
        builder.Append($"            return this.{enName} == {Enum}.{Name};\n");
        builder.Append("        }\n");
        return builder.ToString();
    }

    public string WriteCopy()
    {
        string head = Name.Replace("CopyData", "");

        StringBuilder builder = new StringBuilder();
        builder.Append($"        [Button(\"Copy{head}\")]\n");
        builder.Append($"        public void DoCopy{head}()\n");
        builder.Append("        {\n");
        builder.Append("#if UNITY_EDITOR\n");
        builder.Append($"            if ({Name} == null)\n");
        builder.Append("            {\n");
        builder.Append($"                {Name} = new List<CustomDictionary>();\n");
        builder.Append("            }\n");
        builder.Append($"            {Name}.Clear();\n");
        builder.Append(
            $"            TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($\"Assets/DevTools/SkillEditor/Export/Json/{head}/{head}_");
        builder.Append("{");
        builder.Append($"{head}ID");
        builder.Append("}");
        builder.Append(".json\");\n");
        builder.Append("            if (textAsset != null && textAsset.text != null)\n");
        builder.Append("            {\n");
        builder.Append(
            $"                {head}Json json = Newtonsoft.Json.JsonConvert.DeserializeObject<{head}Json>(textAsset.text);\n");
        builder.Append("                if (json != null)\n");
        builder.Append("                {\n");
        builder.Append($"                    foreach (var item in json.config.{head}CopyData)\n");
        builder.Append("                    {\n");
        builder.Append($"                        {head}CopyData.Add(new CustomDictionary()");
        builder.Append("{");
        builder.Append("ToKey=item");
        builder.Append("});\n");
        builder.Append("                    }\n");
        builder.Append("                }\n");
        builder.Append("            }\n");
        builder.Append("#endif\n");
        builder.Append("        }\n");
        return builder.ToString();
    }
}

#endregion

#region EnumDefine

public static class EnumDeineGenera
{
    public static string GeneraEnumPath = "Assets/DevTools/SkillEditor/Scripts/Define/SkillEditorDefine.cs";
    public static string EnumConfigPath = "Assets/DevTools/SkillEditor/Config/EnumDefine.xml";

    public static string Node = @"
///--------------------------------------------------------------------
/// 文件名   :   SkillEditorDefine
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   
/// 创建人   :   Create By EnumDefine.xml
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
";

    public static string EnumTemplate = @"
    /// <summary>
    /// {0}
    /// </summary>
    public enum {1} {2}";

    public static string EnumTemplateItem = @"
        {0}={1},                 //{2}";

    public static void CreateEnum()
    {
        if (!File.Exists(EnumConfigPath))
        {
            Debug.LogError("EnumDefine.xml 不存在");
            return;
        }

        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(EnumConfigPath);
        if (textAsset != null && textAsset.text != null)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(textAsset.text);

            XmlNode xmlNode = document.SelectSingleNode("config");
            if (xmlNode != null && xmlNode.ChildNodes.Count > 0)
            {
                Dictionary<string, EnumTemplate> EnumMap = new Dictionary<string, EnumTemplate>();

                foreach (var item in xmlNode.ChildNodes)
                {
                    XmlElement node = item as XmlElement;
                    if (node == null || node.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    string name = node.GetAttribute("name");
                    if (EnumMap.ContainsKey(name))
                    {
                        Debug.LogError($"存在重复枚举值{name}");
                        continue;
                    }

                    string desc = node.GetAttribute("desc");

                    string isTypeDictionary = node.GetAttribute("IsTypeDictionary");

                    EnumTemplate template = new EnumTemplate(name, desc, isTypeDictionary);

                    foreach (var it in node.ChildNodes)
                    {
                        XmlElement child = it as XmlElement;
                        if (child == null || child.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        string child_name = child.GetAttribute("name");
                        string key = child.GetAttribute("key");
                        int val = 0;
                        System.Int32.TryParse(child.GetAttribute("value"), out val);
                        EnumItemTemplate enumItem = new EnumItemTemplate(child_name, key, val);
                        template.AddItem(enumItem);
                    }

                    EnumMap.Add(name, template);
                }

                GeneraEnum(EnumMap);
            }
        }
    }

    private static void GeneraEnum(Dictionary<string, EnumTemplate> EnumMap)
    {
        if (EnumMap == null || EnumMap.Count < 1)
        {
            return;
        }

        if (File.Exists(GeneraEnumPath))
        {
            File.Delete(GeneraEnumPath);
        }

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(Node);
        stringBuilder.Append("using Sirenix.OdinInspector;\n");
        stringBuilder.Append("using System.Collections;\n");
        stringBuilder.Append("using System.Collections.Generic;\n");
        stringBuilder.Append("namespace SkillEditor");
        stringBuilder.Append("\n");
        stringBuilder.Append("{\n");

        foreach (var item in EnumMap)
        {
            stringBuilder.Append(item.Value.ToString());
            stringBuilder.Append("\n");
        }

        stringBuilder.Append("\n");
        stringBuilder.Append("\n");
        stringBuilder.Append("    public static class EnumDefineMap");
        stringBuilder.Append("    \n");
        stringBuilder.Append("    {\n");
        foreach (var item in EnumMap)
        {
            stringBuilder.Append(item.Value.ToMapString());
            if (item.Value.IsTypeDictionary == "true")
            {
                stringBuilder.Append(item.Value.ToDicString(item.Value.Name));
            }

            stringBuilder.Append("\n");
        }

        stringBuilder.Append("    }\n");
        stringBuilder.Append("\n}");
        string mContent = stringBuilder.ToString();
        //mContent = mContent.Replace("#CREATETIME#", System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
        FileStream fs = new FileStream(GeneraEnumPath, FileMode.CreateNew);
        byte[] bytes = Encoding.UTF8.GetBytes(mContent);
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
        fs.Dispose();
        UnityEditor.AssetDatabase.Refresh();
    }
}

public class EnumTemplate
{
    public string Name { get; private set; }
    public string Desc { get; private set; }
    public string IsTypeDictionary { get; private set; }
    public List<EnumItemTemplate> Items { get; private set; }

    public EnumTemplate(string name, string desc, string isTypeDictionary)
    {
        this.Name = name;
        this.Desc = desc;
        this.IsTypeDictionary = isTypeDictionary;
        this.Items = new List<EnumItemTemplate>();
    }

    public void AddItem(EnumItemTemplate enumItem)
    {
        this.Items.Add(enumItem);
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("\n");
        builder.Append("    {\n");
        foreach (var item in Items)
        {
            builder.Append(item.ToString());
        }

        builder.Append("\n");
        builder.Append("    }");
        return string.Format(EnumDeineGenera.EnumTemplate, Desc, Name, builder.ToString());
    }

    public string ToMapString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("\n");
        builder.Append("        {\n");
        foreach (var item in Items)
        {
            builder.Append(item.ToMapString(Name));
            builder.Append("\n");
        }

        builder.Append("        };");
        builder.Append("\n");
        return string.Format("        static public IEnumerable _{0} = new ValueDropdownList<{1}>(){2}", Name.ToLower(),
            Name, builder.ToString());
    }

    public string ToDicString(string enumName)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(
            $"        public static Dictionary<{enumName}, System.Type> {enumName}Dic = new Dictionary<{enumName}, System.Type>()\r\n        {{");
        foreach (var item in Items)
        {
            builder.Append($"\n            {{{enumName}.{item.ItemName},typeof({enumName}{item.ItemName})}},");
        }

        builder.Remove(builder.Length - 1, 1);
        builder.Append("\n        };");
        return builder.ToString();
    }
}

public class EnumItemTemplate
{
    public string ItemName { get; private set; }
    public string Key { get; private set; }
    public int Value { get; private set; }

    public EnumItemTemplate(string name, string key, int value)
    {
        this.ItemName = name;
        this.Key = key;
        this.Value = value;
    }

    public override string ToString()
    {
        return string.Format(EnumDeineGenera.EnumTemplateItem, ItemName, Value, Key);
    }

    public string ToMapString(string EnumType)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("            {");
        builder.Append("\"");
        builder.Append(Key);
        builder.Append("\"");
        builder.Append(",");
        builder.Append(EnumType);
        builder.Append(".");
        builder.Append(ItemName);
        builder.Append("},");

        return builder.ToString();
    }
}

#endregion