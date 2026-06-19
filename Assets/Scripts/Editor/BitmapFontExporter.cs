using System;
using System.Xml;
using UnityEngine;
using UnityEditor;

namespace Stars.BitmapFont
{
    public class BitmapFontExporter : ScriptableWizard
    {
        [MenuItem("Tools/Create Font")]
        private static void CreateFont()
        {
            DisplayWizard<BitmapFontExporter>("Create Font");
        }

        public TextAsset fontFile;
        public Texture2D textureFile;

        [Obsolete("Obsolete")]
        private void OnWizardCreate()
        {
            if (fontFile == null || textureFile == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject("Save Font", fontFile.name, "", "");

            if (!string.IsNullOrEmpty(path))
            {
                ResolveFont(path);
            }
        }

        [Obsolete("Obsolete")]
        private void ResolveFont(string exportPath)
        {
            if (!fontFile) throw new UnityException(fontFile.name + "is not a valid font-xml file.");

            Font font = new Font();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(fontFile.text);

            XmlNode info = xml.GetElementsByTagName("info")[0];
            XmlNodeList chars = xml.GetElementsByTagName("chars")[0].ChildNodes;

            CharacterInfo[] charInfos = new CharacterInfo[chars.Count];

            for (int cnt = 0; cnt < chars.Count; cnt++)
            {
                XmlNode node = chars[cnt];
                CharacterInfo charInfo = new CharacterInfo();

                charInfo.index = ToInt(node, "id");
                charInfo.advance = ToInt(node, "xadvance");
                charInfo.uv = GetUV(node);
                charInfo.vert = GetVert(node);

                charInfos[cnt] = charInfo;
            }


            Shader shader = Shader.Find("Unlit/Transparent");
            Material material = new Material(shader) { mainTexture = textureFile };
            AssetDatabase.CreateAsset(material, exportPath + ".mat");

            font.material = material;
            if (info.Attributes != null)
                font.name = info.Attributes.GetNamedItem("face").InnerText;
            font.characterInfo = charInfos;
            AssetDatabase.CreateAsset(font, exportPath + ".fontsettings");
        }

        private Rect GetUV(XmlNode node)
        {
            Rect uv = new Rect
            {
                x = ToFloat(node, "x") / textureFile.width,
                y = ToFloat(node, "y") / textureFile.height,
                width = ToFloat(node, "width") / textureFile.width,
                height = ToFloat(node, "height") / textureFile.height
            };

            uv.y = 1f - uv.y - uv.height;

            return uv;
        }

        private Rect GetVert(XmlNode node)
        {
            Rect uv = new Rect
            {
                x = ToFloat(node, "xoffset"),
                y = ToFloat(node, "yoffset"),
                width = ToFloat(node, "width"),
                height = ToFloat(node, "height")
            };

            uv.y = -uv.y;
            uv.height = -uv.height;

            return uv;
        }

        private int ToInt(XmlNode node, string name)
        {
            return Convert.ToInt32(node.Attributes.GetNamedItem(name).InnerText);
        }

        private float ToFloat(XmlNode node, string name)
        {
            return ToInt(node, name);
        }
    }
}