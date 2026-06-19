///--------------------------------------------------------------------
/// 文件名   :   AtlasGenera.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/12/06 19:11:14
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D.Sprites;

public static class AtlasGenera
{
    static void GeneraAtlas()
    {
        ProcessToSprite();
    }

    [MenuItem("Assets/图集工具/生成图集", false, 2)]
    public static string ProcessToSprite()
    {
        TextAsset txt = (TextAsset)Selection.activeObject;
        if (txt == null)
        {
            return null;
        }
        return ProcessToSprite(txt);
    }

    public static string ProcessToSprite(TextAsset textAsset)
    {
        string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(textAsset));
        TexturePacker.MetaData meta = TexturePacker.GetMetaData(textAsset.text);
        string Name = System.IO.Path.GetFileNameWithoutExtension(meta.image);
        List<SpriteMetaData> sprites = TexturePacker.ProcessToSprites(textAsset.text);

        string path = rootPath + "/" + meta.image;
        Dictionary<string, SpriteMetaData> oldMaps = new Dictionary<string, SpriteMetaData>();

        TextureImporter texImp = AssetImporter.GetAtPath(path) as TextureImporter;

        if (texImp != null)
        {
            texImp.isReadable = true;
            if (texImp.spritesheet != null && texImp.spritesheet.Length > 0)
            {
                ///检查旧元素是否在新列表中存在，如果存在，覆盖其Border，alignment,和pivot
                for (int i = 0; i < texImp.spritesheet.Length; i++)
                {
                    SpriteMetaData oldSprite = texImp.spritesheet[i];
                    oldMaps.Add(oldSprite.name.ToLower(), oldSprite);
                }
            }

        }






        for (int i = 0; i < sprites.Count; i++)
        {
            SpriteMetaData newSprite = sprites[i];
            if (oldMaps.ContainsKey(newSprite.name.ToLower()))
            {
                SpriteMetaData oldSprite = oldMaps[newSprite.name.ToLower()];
                newSprite.name = oldSprite.name;
                newSprite.alignment = oldSprite.alignment;
                newSprite.border = oldSprite.border;
                sprites[i] = newSprite;
            }
        }

        


        texImp.spritesheet = sprites.ToArray();
        texImp.textureType = TextureImporterType.Sprite;
        texImp.spriteImportMode = SpriteImportMode.Multiple;
        texImp.isReadable = false;

        

        updateSprites(texImp, sprites.ToArray());

        /*
        var m_SpriteDataProvider =
             spriteDataProviderFactories.GetSpriteEditorDataProviderFromObject(
                 AssetDatabase.LoadMainAssetAtPath(path));
        m_SpriteDataProvider.InitSpriteEditorDataProvider();
        m_SpriteDataProvider.GetSpriteRects();
        m_SpriteDataProvider.Apply();
        */

        UnityEditor.EditorUtility.SetDirty(texImp);
        texImp.SaveAndReimport();
        GeneraPrefab(Name, rootPath, path);
        return path;
    }

    static SpriteDataProviderFactories m_SpriteDataProviderFactories = null;

    static SpriteDataProviderFactories spriteDataProviderFactories
    {
        get
        {
            if (m_SpriteDataProviderFactories == null)
            {
                m_SpriteDataProviderFactories = new SpriteDataProviderFactories();
                m_SpriteDataProviderFactories.Init();
            }
            return m_SpriteDataProviderFactories;
        }
    }


    private static void GeneraPrefab(string Name, string rootPath, string path)
    {
        string PrefabPath = rootPath + $"/{Name}.asset";
        bool isNew = false;
        Atlas atlas = AssetDatabase.LoadAssetAtPath<Atlas>(PrefabPath);
        if (atlas == null)
        {
            isNew = true;
            atlas = ScriptableObject.CreateInstance<Atlas>();
        }
        atlas.Clear();
        Object[] oAssets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < oAssets.Length; i++)
        {
            Object obj = oAssets[i];
            if (obj == null)
                continue;

            if (obj is Sprite)
            {
                atlas.AddSprite(obj as Sprite);
            }
        }
        if (isNew)
        {
            AssetDatabase.CreateAsset(atlas, PrefabPath);
        }
        else
        {
            UnityEditor.EditorUtility.SetDirty(atlas);
        }
        AssetDatabase.SaveAssets();
    }



#if UNITY_2021_2_OR_NEWER
    private static void updateSprites(TextureImporter importer, SpriteMetaData[] metadata)
    {
        var dataProvider = GetSpriteEditorDataProvider(importer);
        var spriteNameFileIdDataProvider = dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

        var oldIds = spriteNameFileIdDataProvider.GetNameFileIdPairs();
        SpriteRect[] rects = sheetInfoToSpriteRects(metadata);
        SpriteNameFileIdPair[] ids = generateSpriteIds(oldIds, rects);

        dataProvider.SetSpriteRects(rects);
        spriteNameFileIdDataProvider.SetNameFileIdPairs(ids);
        dataProvider.Apply();
        EditorUtility.SetDirty(importer);
    }


    private static ISpriteEditorDataProvider GetSpriteEditorDataProvider(TextureImporter importer)
    {
        var dataProviderFactories = new SpriteDataProviderFactories();
        dataProviderFactories.Init();
        var dataProvider = dataProviderFactories.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        return dataProvider;
    }


    private static SpriteRect[] sheetInfoToSpriteRects(SpriteMetaData[] metadata)
    {
        int spriteCount = metadata.Length;
        SpriteRect[] rects = new SpriteRect[spriteCount];

        for (int i = 0; i < spriteCount; i++)
        {
            SpriteRect sr = rects[i] = new SpriteRect();
            SpriteMetaData smd = metadata[i];

            sr.name = smd.name;
            sr.rect = smd.rect;
            sr.pivot = smd.pivot;
            sr.border = smd.border;
            sr.alignment = (SpriteAlignment)smd.alignment;

            // sr.spriteID not yet initialized, this is done in generateSpriteIds()
        }

        return rects;
    }


    private static SpriteNameFileIdPair[] generateSpriteIds(IEnumerable<SpriteNameFileIdPair> oldIds,
                                                            SpriteRect[] sprites)
    {
        SpriteNameFileIdPair[] newIds = new SpriteNameFileIdPair[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].spriteID = idForName(oldIds, sprites[i].name);
            newIds[i] = new SpriteNameFileIdPair(sprites[i].name, sprites[i].spriteID);
        }

        return newIds;
    }


    private static GUID idForName(IEnumerable<SpriteNameFileIdPair> oldIds, string name)
    {
        foreach (SpriteNameFileIdPair old in oldIds)
        {
            if (old.name == name)
            {
                return old.GetFileGUID();
            }
        }
        return GUID.Generate();
    }
#endif
}
