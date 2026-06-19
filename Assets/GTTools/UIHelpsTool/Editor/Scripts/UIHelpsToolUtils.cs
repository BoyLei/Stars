/*
 * @Description: 一些辅助接口
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GameTechTools.CommonLibs.CommonExtends;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameTechTools.UIHelpsTool
{
    internal class UIHelpsToolUtils
    {
        //根据guid删除对应的预览图和存储的配置
        static public void DeletePreviewDataByGoRemove(string previewGuid)
        {
            if (string.IsNullOrEmpty(previewGuid))
            {
                return;
            }
            //删除预览图
            string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + previewGuid + ".png";
            if (File.Exists(preview_path))
            {
                File.Delete(preview_path);
                AssetDatabase.Refresh();
            }
            //删除配置
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(UnityEngine.Object), new string[] { UIHelpsToolConfigure.ConfigDataPath.TrimEnd('/') });
            var bFind = false;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (go.GetType() != typeof(GeneralCompTypeConfig))
                {
                    continue;
                }
                var typeCfg = go as GeneralCompTypeConfig;
                foreach (var item in typeCfg.previewItems)
                {
                    if (item.prefabGo == null || GTHelper.ObjectToGUID(item.prefabGo) == previewGuid)
                    {
                        bFind = true;
                        typeCfg.previewItems.Remove(item);
                        EditorUtility.SetDirty(typeCfg);
                        break;
                    }
                }
            }

            if (bFind)
            {
                if (GeneralCompWindow.mainWindow)
                {
                    GeneralCompWindow.mainWindow.Repaint();
                }
            }
        }

        //检查某个预览对象，当删除某个预览对象时，如果所有分类里面都没有它了，则把预览图也删了，避免一直残留预览资源
        public static void DeletePreviewData(string previewGuid, int checkRefCnt = 1, bool autoRefresh = true)
        {
            if (string.IsNullOrEmpty(previewGuid))
            {
                return;
            }

            //查找配置
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(UnityEngine.Object), new string[] { UIHelpsToolConfigure.ConfigDataPath.TrimEnd('/') });
            var refCnt = 0;
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object go = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (go.GetType() != typeof(GeneralCompTypeConfig))
                {
                    continue;
                }
                var typeCfg = go as GeneralCompTypeConfig;
                foreach (var item in typeCfg.previewItems)
                {
                    if (item.prefabGo == null || GTHelper.ObjectToGUID(item.prefabGo) == previewGuid)
                    {
                        refCnt++;
                        break;
                    }
                }
            }

            //表示所有的分类都不需要这个预览了
            if (refCnt < checkRefCnt)
            {
                //删除预览图
                string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + previewGuid + ".png";
                if (File.Exists(preview_path))
                {
                    File.Delete(preview_path);
                    if (autoRefresh)
                    {
                        AssetDatabase.Refresh();
                    }
                }
            }
        }

        //导出通用件的预览图
        public static Texture ExportBasicCompImg(GameObject prefab)
        {
            string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + GTHelper.ObjectToGUID(prefab) + ".png";
            Texture Tex = UIHelpsToolUtils.GetAssetPreview3D(prefab, UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth / 5, UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight / 5, 0.5f);
            if (Tex != null)
            {
                UIHelpsToolUtils.SaveTextureToPNG(Tex as Texture2D, preview_path);
                {
                    AssetDatabase.ImportAsset(preview_path);
                    AssetDatabase.Refresh();

                    TextureImporter Importer = AssetImporter.GetAtPath(preview_path) as TextureImporter;
                    Importer.textureType = TextureImporterType.Default;
                    TextureImporterPlatformSettings setting = Importer.GetDefaultPlatformTextureSettings();
                    setting.format = TextureImporterFormat.RGBA32;
                    setting.textureCompression = TextureImporterCompression.Uncompressed;
                    Importer.SetPlatformTextureSettings(setting);
                    Importer.mipmapEnabled = false;

                    AssetDatabase.ImportAsset(preview_path);
                    AssetDatabase.Refresh();
                }
            }
            return Tex;
        }

        public static Texture ExportBasicCompImg(GameObject prefab, string guid)
        {
            string preview_path = UIHelpsToolConfigure.PreviewPath + "/" + guid + ".png";
            Texture Tex = UIHelpsToolUtils.GetAssetPreview3D(prefab, UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth / 5, UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight / 5, 0.5f);
            if (Tex != null)
            {
                UIHelpsToolUtils.SaveTextureToPNG(Tex as Texture2D, preview_path);
                {
                    AssetDatabase.ImportAsset(preview_path);
                    AssetDatabase.Refresh();

                    TextureImporter Importer = AssetImporter.GetAtPath(preview_path) as TextureImporter;
                    Importer.textureType = TextureImporterType.Default;
                    TextureImporterPlatformSettings setting = Importer.GetDefaultPlatformTextureSettings();
                    setting.format = TextureImporterFormat.RGBA32;
                    setting.textureCompression = TextureImporterCompression.Uncompressed;
                    Importer.SetPlatformTextureSettings(setting);
                    Importer.mipmapEnabled = false;

                    AssetDatabase.ImportAsset(preview_path);
                    AssetDatabase.Refresh();
                }
            }
            return Tex;
        }

        public static Texture2D LoadTextureInLocal(string file_path)
        {
            //创建文件读取流
            FileStream fileStream = new FileStream(file_path, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0, SeekOrigin.Begin);
            //创建文件长度缓冲区
            byte[] bytes = new byte[fileStream.Length];
            //读取文件
            fileStream.Read(bytes, 0, (int)fileStream.Length);
            //释放文件读取流
            fileStream.Close();
            fileStream.Dispose();
            fileStream = null;

            //创建Texture
            int width = 300;
            int height = 372;
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(bytes);
            return texture;
        }

        //要导出的prefab预览的资源，预览区域宽、高
        //参考的开源项目 https://github.com/liuhaopen/UGUI-Editor，但是这个开源项目逻辑更多是针对作者自己项目的设计，我们还是需要根据自己公司项目情况进行修改
        public static Texture GetAssetPreview3D(GameObject obj, int previewWidth = 512, int previewHeight = 512, float cutSize = 0.5f)
        {
            // Screen.SetResolution(UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth, UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight, true);
            GameObject canvas_obj = null;
            GameObject clone = GameObject.Instantiate(obj);
            Transform cloneTransform = clone.transform;

            GameObject cameraObj = new GameObject("render camera");
            cameraObj.hideFlags = HideFlags.DontSave;
            Camera renderCamera = cameraObj.AddComponent<Camera>();
            renderCamera.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            renderCamera.clearFlags = CameraClearFlags.Color;
            renderCamera.cameraType = CameraType.SceneView;
            renderCamera.cullingMask = 1 << 21;
            renderCamera.nearClipPlane = -100;
            renderCamera.farClipPlane = 100;

            Bounds bounds = GetBounds(clone);
            Vector3 Min = bounds.min;
            Vector3 Max = bounds.max;

            bool isUINode = false;
            if (cloneTransform is RectTransform)
            {
                //如果是UGUI节点的话就要把它们放在Canvas下了
                canvas_obj = new GameObject("render canvas", typeof(Canvas));
                canvas_obj.hideFlags = HideFlags.DontSave;
                Canvas canvas = canvas_obj.GetComponent<Canvas>();
                //一定要设置canvas的着色器通道数值，否则部分数据无法渲染出来
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Normal;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.Tangent;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord2;
                canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord3;

                cloneTransform.SetParent(canvas_obj.transform);
                cloneTransform.localPosition = Vector3.zero;
                //canvas_obj.transform.position = new Vector3(-1000, -1000, -1000);
                canvas_obj.layer = 21;//放在21层，摄像机也只渲染此层的，避免混入了奇怪的东西
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = renderCamera;

                isUINode = true;

                RectTransform rectTransform = cloneTransform as RectTransform;
                var canvasScaler = canvas_obj.AddComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
                canvasScaler.referencePixelsPerUnit = UIHelpsToolGlobalConfig.generalSetting.pixelsPerUnit;
                canvasScaler.scaleFactor = 1;
                // canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

                //居中限制大小的
                if (rectTransform.anchorMin.x == 0 && rectTransform.anchorMin.y == 0 &&
                rectTransform.anchorMax.x == 1 && rectTransform.anchorMax.y == 1)
                {
                    //全屏的，镜头拉到1/3的比率刚刚好
                    cutSize = 0.3f;
                    previewWidth = UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth;
                    previewHeight = UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight;
                    // canvasScaler.referenceResolution = new Vector2(UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth, UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight);
                }
                else
                {
                    // canvasScaler.referenceResolution = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                    var maxWidth = UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth;
                    var maxHeight = UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight;
                    float goWidth = Max.x - Min.x;
                    float goHeight = Max.y - Min.y;
                    //避免如果没有设置width，heigt的prefab，则计算边界来获取
                    goWidth = Math.Min(goWidth, UIHelpsToolGlobalConfig.generalSetting.eScreenStdWidth);
                    goHeight = Math.Min(goHeight, UIHelpsToolGlobalConfig.generalSetting.eScreenStdHeight);
                    goWidth = Math.Max(goWidth, rectTransform.sizeDelta.x);
                    goHeight = Math.Max(goHeight, rectTransform.sizeDelta.y);
                    var tmp = 5;
                    previewWidth = maxWidth / tmp;
                    previewHeight = maxHeight / tmp;
                    //取一个最合适的分辨率来渲染，避免太小的icon显示不清楚，同样渲染到的画布也取这样的大小，这样可以让预览效果图更加清晰，避免大图渲染到小画布中失真等
                    for (; tmp >= 1; tmp--)
                    {
                        previewWidth = maxWidth / tmp;
                        previewHeight = maxHeight / tmp;
                        if (previewWidth >= goWidth && previewHeight >= goHeight)
                        {
                            break;
                        }
                    }

                    //在canvas居中
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.position = Vector3.zero;
                }
                // canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            }
            else
                cloneTransform.position = new Vector3(-1000, -1000, -1000);

            Transform[] all = clone.GetComponentsInChildren<Transform>();
            foreach (Transform trans in all)
            {
                trans.gameObject.layer = 21;
            }

            bounds = GetBounds(clone);
            Min = bounds.min;
            Max = bounds.max;



            if (isUINode)
            {
                cameraObj.transform.position = new Vector3(0, 0, -10);
                //Vector3 center = new Vector3(cloneTransform.position.x, (Max.y + Min.y) / 2f, cloneTransform.position.z);
                cameraObj.transform.LookAt(Vector3.zero);

                renderCamera.orthographic = true;
                float width = Max.x - Min.x;
                float height = Max.y - Min.y;
                float max_camera_size = width > height ? width : height;
                renderCamera.orthographicSize = max_camera_size * 0.3f;//预览图要尽量少点空白
            }
            else
            {
                cameraObj.transform.position = new Vector3((Max.x + Min.x) / 2f, (Max.y + Min.y) / 2f, Max.z + (Max.z - Min.z));
                Vector3 center = new Vector3(cloneTransform.position.x, (Max.y + Min.y) / 2f, cloneTransform.position.z);
                cameraObj.transform.LookAt(center);

                int angle = (int)(Mathf.Atan2((Max.y - Min.y) / 2, (Max.z - Min.z)) * 180 / 3.1415f * 2);
                renderCamera.fieldOfView = angle;
            }
            RenderTexture texture = new RenderTexture(previewWidth, previewHeight, 0, RenderTextureFormat.Default);
            renderCamera.targetTexture = texture;

            if (!isUINode)
            {
                Undo.DestroyObjectImmediate(cameraObj);
                Undo.PerformUndo();//不知道为什么要删掉再Undo回来后才Render得出来模型的节点，测试了下fieldOfView设置后不会立即生效，需要修改一次
                renderCamera.RenderDontRestore();

            }

            var tex = RTImage(renderCamera);

            if (canvas_obj != null)
            {
                UnityEngine.Object.DestroyImmediate(canvas_obj);
            }
            if (cameraObj != null)
            {
                UnityEngine.Object.DestroyImmediate(cameraObj);
            }

            return tex;
        }

        static Texture2D RTImage(Camera camera)
        {
            // The Render Texture in RenderTexture.active is the one
            // that will be read by ReadPixels.
            var currentRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            // Render the camera's view.
            //camera.Render();

            camera.Render();
            // Make a new texture and read the active Render Texture into it.
            Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            image.Apply();

            // Replace the original active Render Texture.
            RenderTexture.active = currentRT;
            return image;
        }

        public static Bounds GetBounds(GameObject obj)
        {
            Vector3 Min = new Vector3(99999, 99999, 99999);
            Vector3 Max = new Vector3(-99999, -99999, -99999);
            MeshRenderer[] renders = obj.GetComponentsInChildren<MeshRenderer>();
            SpriteRenderer[] spriteRends = obj.GetComponentsInChildren<SpriteRenderer>();
            if (renders.Length > 0 || spriteRends.Length > 0)
            {
                for (int i = 0; i < renders.Length; i++)
                {
                    if (renders[i].bounds.min.x < Min.x)
                        Min.x = renders[i].bounds.min.x;
                    if (renders[i].bounds.min.y < Min.y)
                        Min.y = renders[i].bounds.min.y;
                    if (renders[i].bounds.min.z < Min.z)
                        Min.z = renders[i].bounds.min.z;

                    if (renders[i].bounds.max.x > Max.x)
                        Max.x = renders[i].bounds.max.x;
                    if (renders[i].bounds.max.y > Max.y)
                        Max.y = renders[i].bounds.max.y;
                    if (renders[i].bounds.max.z > Max.z)
                        Max.z = renders[i].bounds.max.z;
                }

                for (int i = 0; i < spriteRends.Length; i++)
                {
                    if (spriteRends[i].bounds.min.x < Min.x)
                        Min.x = spriteRends[i].bounds.min.x;
                    if (spriteRends[i].bounds.min.y < Min.y)
                        Min.y = spriteRends[i].bounds.min.y;
                    if (spriteRends[i].bounds.min.z < Min.z)
                        Min.z = spriteRends[i].bounds.min.z;

                    if (spriteRends[i].bounds.max.x > Max.x)
                        Max.x = spriteRends[i].bounds.max.x;
                    if (spriteRends[i].bounds.max.y > Max.y)
                        Max.y = spriteRends[i].bounds.max.y;
                    if (spriteRends[i].bounds.max.z > Max.z)
                        Max.z = spriteRends[i].bounds.max.z;
                }
            }
            else
            {
                RectTransform[] rectTrans = obj.GetComponentsInChildren<RectTransform>();
                Vector3[] corner = new Vector3[4];
                for (int i = 0; i < rectTrans.Length; i++)
                {
                    //获取节点的四个角的世界坐标，分别按顺序为左下左上，右上右下
                    rectTrans[i].GetWorldCorners(corner);
                    if (corner[0].x < Min.x)
                        Min.x = corner[0].x;
                    if (corner[0].y < Min.y)
                        Min.y = corner[0].y;
                    if (corner[0].z < Min.z)
                        Min.z = corner[0].z;

                    if (corner[2].x > Max.x)
                        Max.x = corner[2].x;
                    if (corner[2].y > Max.y)
                        Max.y = corner[2].y;
                    if (corner[2].z > Max.z)
                        Max.z = corner[2].z;
                }
            }

            Vector3 center = (Min + Max) / 2;
            Vector3 size = new Vector3(Max.x - Min.x, Max.y - Min.y, Max.z - Min.z);
            return new Bounds(center, size);
        }

        public static bool SaveTextureToPNG(Texture2D inputTex, string save_file_name)
        {
            byte[] bytes = inputTex.EncodeToPNG();
            string directory = Path.GetDirectoryName(save_file_name);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            FileStream file = File.Open(save_file_name, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(file);
            writer.Write(bytes);
            file.Close();

            return true;
        }

        static Texture2D mBackdropTex;
        static public Texture2D backdropTexture
        {
            get
            {
                if (mBackdropTex == null) mBackdropTex = CreateCheckerTex(
                    new Color(0.1f, 0.1f, 0.1f, 0.5f),
                    new Color(0.2f, 0.2f, 0.2f, 0.5f));
                return mBackdropTex;
            }
        }

        static Texture2D CreateCheckerTex(Color c0, Color c1)
        {
            Texture2D tex = new Texture2D(16, 16);
            tex.name = "[Generated] Checker Texture";
            tex.hideFlags = HideFlags.DontSave;

            for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
            for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
            for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
            for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        static public void DrawTiledTexture(Rect rect, Texture tex)
        {
            GUI.BeginGroup(rect);
            {
                int width = Mathf.RoundToInt(rect.width);
                int height = Mathf.RoundToInt(rect.height);

                for (int y = 0; y < height; y += tex.height)
                {
                    for (int x = 0; x < width; x += tex.width)
                    {
                        GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                    }
                }
            }
            GUI.EndGroup();
        }

        /// <summary>
        /// 修改分类名称
        /// </summary>
        /// <param name="path"></param>
        public static void ModifyPreviewTypeName(object _list)
        {
            List<object> list = (List<object>)_list;
            OdinMenuTree tree = (OdinMenuTree)list[0];
            System.Object selectItem = null;
            for (int i = 0; i < tree.Selection.Count; i++)
            {
                if (tree.Selection[i].Value != null && tree.Selection[i].Value.GetType() == typeof(GeneralCompTypeConfig))
                {
                    selectItem = tree.Selection[i].Value;
                    break;
                }
            }

            if (selectItem == null)
            {
                return;
            }
            ModifyGeneralCompTypeWindow.OpenWindow(selectItem as GeneralCompTypeConfig);
        }

        /// <summary>
        /// 修改目录名称
        /// </summary>
        /// <param name="path"></param>
        public static void ModifyPreviewFolderName(object _list)
        {
            List<object> list = (List<object>)_list;
            OdinMenuTree tree = (OdinMenuTree)list[0];
            System.Object selectItem = null;
            for (int i = 0; i < tree.Selection.Count; i++)
            {
                if (tree.Selection[i].Value == null && tree.Selection[i].Name != "预览分类")
                {
                    selectItem = tree.Selection[i];
                    break;
                }
            }

            if (selectItem == null)
            {
                return;
            }
            ModifyPreviewFolderWindow.OpenWindow((OdinMenuItem)selectItem);
        }
        
        /// <summary>
        /// 删除文件夹目录
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(object _list)
        {
            string content = "是否要删除分类 ";
            List<object> list = (List<object>)_list;
            OdinMenuTree tree = (OdinMenuTree)list[0];
            for (int i = 0; i < tree.Selection.Count; i++)
            {
                if (i < tree.Selection.Count - 1)
                {
                    content = content + "[" + tree.Selection[i].Name + "],";
                }
                else
                {
                    content = content + "[" + tree.Selection[i].Name + "] ？";
                }
            }

            var ret = EditorUtility.DisplayDialog("提示", content, "确定", "取消");
            if (ret)
            {
                DeleteAsset(_list);
            }
        }

        public static void DeleteAsset(object _list)
        {
            List<object> list = (List<object>)_list;
            OdinMenuTree tree = (OdinMenuTree)list[0];


            //选中被删除的上一个节点
            if (tree.EnumerateTree().Any() && tree.Selection[0].PrevVisualMenuItem != null)
            {
                var itemList = tree.EnumerateTree().ToList();
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (tree.Selection[0].PrevVisualMenuItem == itemList[i])
                    {
                        //改为选中上一个节点
                        GeneralCompWindow.GetWindow().mCurSelectItem = itemList[i];
                        break;
                    }
                }
            }
            else
            {
                GeneralCompWindow.GetWindow().mCurSelectItem = null;
            }

            for (int i = 0; i < tree.Selection.Count; i++)
            {
                var odinItem = tree.Selection[i] as OdinMenuItem;
                if (odinItem.Value == null)
                {
                    continue;
                }
                if (odinItem.Value is GeneralCompTypeConfig)
                {
                    var compCfg = odinItem.Value as GeneralCompTypeConfig;
                    foreach (var preview in compCfg.previewItems)
                    {
                        //这里引用次数传入2，因为要排除自身，如果出去自身，都没有其他引用次数了，则删除预览图
                        DeletePreviewData(GTHelper.ObjectToGUID(preview.prefabGo), 2, false);
                    }
                    if (compCfg.previewItems.Count > 0)
                    {
                        AssetDatabase.Refresh();
                    }
                }
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath((UnityEngine.Object)odinItem.Value));
            }
            GeneralCompWindow.GetWindow().UpdateMenuTree();
            if (GeneralCompWindow.GetWindow().mCurSelectItem != null)
            {
                GeneralCompWindow.GetWindow().mCurSelectItem.Select();
            }
            GeneralCompWindow.GetWindow().Repaint();
        }

    }
}