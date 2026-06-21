using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameDLL
{
    [Serializable]
    public class VisualizationMode
    {
        // 6FFF00
        // 0035FF
        // FF0083
        // 83FF97
        // 9881FF
        // FF88AB
        // 45937B
        // 984A66
        // FFC6EA
        // 1C3C3C
        // 523B82
        // 8E696F
        // D49957



        public Color m_Color0 = new Color(0x6F / 255f, 0xff / 255f, 0x00 / 255f);
        public Color m_Color1 = new Color(0x00 / 255f, 0x35 / 255f, 0xFF / 255f);
        public Color m_Color2 = new Color(0xFF / 255f, 0x00 / 255f, 0x83 / 255f);
        public Color m_Color3 = new Color(0x83 / 255f, 0xFF / 255f, 0x97 / 255f);
        public Color m_Color4 = new Color(0x98 / 255f, 0x81 / 255f, 0xFF / 255f);
        public Color m_Color5 = new Color(0xFF / 255f, 0x88 / 255f, 0xAB / 255f);
        public Color m_Color6 = new Color(0x45 / 255f, 0x93 / 255f, 0x7B / 255f);
        public Color m_Color7 = new Color(0x98 / 255f, 0x4A / 255f, 0x66 / 255f);
        public Color m_Color8 = new Color(0xFF / 255f, 0xC6 / 255f, 0xEA / 255f);
        public Color m_Color9 = new Color(0x1C / 255f, 0x3C / 255f, 0x3C / 255f);
        public Color m_Color10 = new Color(0x52 / 255f, 0x3B / 255f, 0x82 / 255f);
        public Color m_Color11 = new Color(0x8E / 255f, 0x69 / 255f, 0x6F / 255f);
        public Color m_Color12 = new Color(0xD4 / 255f, 0x99 / 255f, 0x57 / 255f);

        public Texture2D[] miptextures;

        private Texture2D m_MipmapVisTex;

        public Material m_MipmapMat;            //统计mipmap

        public Material m_LightCountMat;        //统计灯光

        private List<Renderer> m_AllRenderers = new List<Renderer>();
        private List<Material> m_BackupMaterials = new List<Material>();


        void FillTextureColor(int mipmap, Color c)
        {
            Color[] pixels = m_MipmapVisTex.GetPixels(mipmap);      //获取不同级别的贴图
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = c;
            m_MipmapVisTex.SetPixels(pixels, mipmap);               //设置级别的像素
        }

        void CreateTexture()
        {
            // if (m_MipmapVisTex == null)
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                m_MipmapVisTex = new Texture2D(2048, 2048, TextureFormat.RGB24, true);
#else
                m_MipmapVisTex = new Texture2D(4096, 4096, TextureFormat.RGB24, true);
                FillTextureColor(12, m_Color12);
#endif
                FillTextureColor(0, m_Color0);
                FillTextureColor(1, m_Color1);
                FillTextureColor(2, m_Color2);
                FillTextureColor(3, m_Color3);
                FillTextureColor(4, m_Color4);
                FillTextureColor(5, m_Color5);
                FillTextureColor(6, m_Color6);
                FillTextureColor(7, m_Color7);
                FillTextureColor(8, m_Color8);
                FillTextureColor(9, m_Color9);
                FillTextureColor(10, m_Color10);
                FillTextureColor(11, m_Color11);
                m_MipmapVisTex.Apply(false);

                m_MipmapMat.SetTexture("_MainTex", m_MipmapVisTex);
            }
        }

        void CreateMipMap()
        {
            var m_MipmapVisTex = new Texture2D (miptextures[miptextures.Length-1].width, miptextures[miptextures.Length-1].height);
            m_MipmapVisTex.name = miptextures[miptextures.Length-1].name;

            var cors = miptextures[miptextures.Length-1].GetPixels();
            m_MipmapVisTex.SetPixels(cors);
            m_MipmapVisTex.Apply();

            for (int i = miptextures.Length-2; i >=1; i--)
            {
                var pixels = miptextures[i].GetPixels();
                m_MipmapVisTex.SetPixels(pixels, miptextures.Length-1 - i);
            }
            m_MipmapVisTex.Apply(false, true);
            m_MipmapMat.SetTexture("_MainTex", m_MipmapVisTex);
        }

        internal void ReplaceMipmapRenderers()      //替换所有render为mipmapmat
        {
            //CreateMipMap();
            CollectAllRenderers();
            for (int i = 0; i < m_AllRenderers.Count; i++)
            {
                if (m_AllRenderers[i])
                    m_AllRenderers[i].material = m_MipmapMat;
            }
        }

        void InitLightCountMaterial()
        {
            m_LightCountMat.SetColor("_Color0", m_Color0);
            m_LightCountMat.SetColor("_Color1", m_Color1);
            m_LightCountMat.SetColor("_Color2", m_Color2);
            m_LightCountMat.SetColor("_Color3", m_Color3);
            m_LightCountMat.SetColor("_Color4", m_Color4);
            m_LightCountMat.SetColor("_Color5", m_Color5);
            m_LightCountMat.SetColor("_Color6", m_Color6);
            m_LightCountMat.SetColor("_Color7", m_Color7);
            m_LightCountMat.SetColor("_Color8", m_Color8);
            m_LightCountMat.SetColor("_Color9", m_Color9);
        }

        internal void ReplaceLightCountRenderers()      //替换所有render为灯光计数mat
        {
            InitLightCountMaterial();
            CollectAllRenderers();
            for (int i = 0; i < m_AllRenderers.Count; i++)
            {
                if (m_AllRenderers[i])
                    m_AllRenderers[i].material = m_LightCountMat;
            }
        }

        void CollectAllRenderers()      //搜集所有的render
        {
            RevertAllRenderers();
            m_AllRenderers.Clear();
            m_BackupMaterials.Clear();
            Renderer[] renderers = GameObject.FindObjectsOfType<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].material;
                if (material)
                {
                    m_AllRenderers.Add(renderers[i]);
                    m_BackupMaterials.Add(material);
                }
            }
        }

        internal void RevertAllRenderers()
        {
            for (int i = 0; i < m_AllRenderers.Count; i++)
            {
                if (m_AllRenderers[i])
                    m_AllRenderers[i].material = m_BackupMaterials[i];
            }
        }
    }
}
