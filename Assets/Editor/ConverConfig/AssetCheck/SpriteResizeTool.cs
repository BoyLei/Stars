using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteResizeTool : EditorWindow
{
    private Texture2D originalTexture;
    private Texture2D resizedTexture;
    [SerializeField]
    private Vector2Int newSize = new Vector2Int(128, 128);
    private Color fillColor = Color.white;

    [MenuItem("自动化工具/Sprite Resize Tool")]
    private static void Init()
    {
        SpriteResizeTool window = (SpriteResizeTool)EditorWindow.GetWindow(typeof(SpriteResizeTool));
        window.Show();
    }
    private void Awake()
    {
        newSize = new Vector2Int(128, 128);
    }
    Vector2Int oriText;
    private void OnGUI()
    {
        GUILayout.Label("Original Texture");
        originalTexture = (Texture2D)EditorGUILayout.ObjectField(originalTexture, typeof(Texture2D), false);
        GUILayout.Label("注意适用于散图，不适用于图集");
        GUILayout.Label("使用说明：新生成的图片需在[文件夹]目录下操作，1，自己改名替换原图;->,引用会帮你处理;->addressable也会帮你处理->设置也会帮你处理");

        GUILayout.BeginHorizontal();
        newSize = EditorGUILayout.Vector2IntField("New Size", newSize);

        if (GUILayout.Button("1,推荐我一个单图尺寸"))
        {
            oriText.x = originalTexture.width;
            oriText.y = originalTexture.height;
            currentPara = COMMON_SINGLE_PARA;
            newSize = GetRecommendedSize(oriText);
    
        }
        if (GUILayout.Button("2,推荐我一个图集尺寸"))
        {
            oriText.x = originalTexture.width;
            oriText.y = originalTexture.height;
            currentPara = COMMON_MUTI_PARA;
            newSize = GetRecommendedSize(oriText);

        }
       
        GUILayout.EndHorizontal();

        fillColor = EditorGUILayout.ColorField("多余的像素用什么颜色填充，支持透明当慎用", fillColor);
        if (GUILayout.Button("居中模式"))
        {
            _PixModeForTextureResize = SpriteResizeTool.PixModeForTextureResize.Center;

        }
        if (GUILayout.Button("拉伸模式"))
        {
            _PixModeForTextureResize = SpriteResizeTool.PixModeForTextureResize.Large;

        }
        if (GUILayout.Button("Resize Texture"))
        {
            ResizeTexture(/*newSize*/);
        }

        if (resizedTexture != null)
        {
            GUILayout.Label("帮我生成一个新的图片");
            EditorGUI.DrawPreviewTexture(EditorGUILayout.GetControlRect(), resizedTexture);
        }

        if (Event.current.type == EventType.DragPerform || Event.current.type == EventType.DragUpdated)
        {
            if (GUILayout.Button("Drop Image Here"))
            {
                DragAndDrop.AcceptDrag();
                foreach (Object obj in DragAndDrop.objectReferences)
                {
                    if (obj is Texture2D)
                    {
                        originalTexture = (Texture2D)obj;
                        newSize = GetRecommendedSize(new Vector2Int(originalTexture.width, originalTexture.height)/*, newSize*/);

                        // 保持其他图片设置与原图一致
                        TextureImporterSettings originalSettings = GetTextureSettings(originalTexture);
                        ApplyTextureSettings(originalTexture, originalSettings);

                        break;
                    }
                }
            }
        }
    }
    private TextureImporterSettings GetTextureSettings(Texture2D texture)
    {
        string filePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(filePath);
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        return settings;
    }
    private void ApplyTextureSettings(Texture2D texture, TextureImporterSettings settings)
    {
        string filePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(filePath);
        importer.SetTextureSettings(settings);
        AssetDatabase.ImportAsset(filePath);
    }
    PixModeForTextureResize _PixModeForTextureResize = SpriteResizeTool.PixModeForTextureResize.Center;
    public enum PixModeForTextureResize 
    {
    Center,//居中
    Large//拉伸
    }
    private void ResizeTexture(/*Vector2Int targetSize*/)
    {
        if (originalTexture == null)
        {
            Debug.LogError("Please assign the original texture first.");
            return;
        }

        // 获取原始贴图的路径
        string filePath = AssetDatabase.GetAssetPath(originalTexture);

        // 开启图片的读写功能
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(filePath);
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            AssetDatabase.ImportAsset(filePath);
        }
        //AI不知道程序怎么回事，也不需要，探索即可，你告诉他就减少探索而已，代码的代码，流程的流程嘛
        //表现和驱动逻辑一致了（newVector2了），也有默认值了
        // 使用之前提供的推荐尺寸方法获取最终的目标尺寸



        //前面他不填，就用默认128
        //填写了就按照填写的
        //推荐了就按照推荐的了
        Vector2Int finalSize = newSize;//因为人可能手动修改，还是基于人，毕竟是推荐GetRecommendedSize(new Vector2Int(originalTexture.width, originalTexture.height)/*, targetSize*/);

        // 计算需要填充的像素距离
        int paddingX = (finalSize.x - originalTexture.width) / 2;
        int paddingY = (finalSize.y - originalTexture.height) / 2;

        resizedTexture = new Texture2D(finalSize.x, finalSize.y);
        switch (_PixModeForTextureResize)
        {
            case PixModeForTextureResize.Center:
                for (int y = 0; y < finalSize.y; y++)
                {
                    for (int x = 0; x < finalSize.x; x++)
                    {
                        if (x >= paddingX && x < originalTexture.width + paddingX &&
                            y >= paddingY && y < originalTexture.height + paddingY)
                        {
                            // 在原始贴图的范围内，复制原始贴图的像素
                            Color pixelColor = originalTexture.GetPixel(x - paddingX, y - paddingY);
                            resizedTexture.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            // 填充像素居中分布
                            resizedTexture.SetPixel(x, y, fillColor);
                        }
                    }
                }
                break;
            case PixModeForTextureResize.Large:
                // 使用拉伸模式
                for (int y = 0; y < finalSize.y; y++)
                {
                    for (int x = 0; x < finalSize.x; x++)
                    {
                        float u = (float)x / finalSize.x; // 计算 u 坐标比例
                        float v = (float)y / finalSize.y; // 计算 v 坐标比例

                        // 根据 u, v 比例从原始贴图中获取像素颜色
                        Color pixelColor = originalTexture.GetPixelBilinear(u, v);

                        resizedTexture.SetPixel(x, y, pixelColor);
                    }
                }
                break;
            default:
                break;
        }


        resizedTexture.Apply();

        // 保存调整后的贴图为文件
        string directoryPath = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string savePath = Path.Combine(directoryPath, fileName + "_ReSize.png");
        File.WriteAllBytes(savePath, resizedTexture.EncodeToPNG());
        AssetDatabase.Refresh();

        // 使用 savePath 获取保存的贴图文件
        resizedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);

        // 还原图片的读写功能
        importer.isReadable = false;
        AssetDatabase.ImportAsset(filePath);

        // 重新应用原始贴图的纹理导入设置
        TextureImporterSettings originalSettings = GetTextureSettings(originalTexture);
        ApplyTextureSettings(resizedTexture, originalSettings);
    }

    private const float COMMON_SINGLE_PARA = 1.05f;
    private const float COMMON_MUTI_PARA = 1.2f;
    private float currentPara =1.01f;
    /// <summary>
    /// 拓展方案的话
    /// 2幂 4的倍数进行压缩 2的倍数
    /// </summary>
    /// <param name="originalSize"></param>
    /// <param name="targetSize"></param>
    /// <returns></returns>
    private Vector2Int GetRecommendedSize(Vector2Int originalSize/*, Vector2Int targetSize*/)
    {
        Vector2Int recommendedSize = Vector2Int.zero;// = targetSize;




        int closeP2_X = Mathf.ClosestPowerOfTwo(originalSize.x);

        if (closeP2_X <= originalSize.x && closeP2_X * currentPara <= originalSize.x)//缩小只能缩小5%
        {
            //缩小
            recommendedSize.x = closeP2_X;
        }
        else
        {
            //放大
            if (closeP2_X <= originalSize.x * currentPara)
            {
                //放大不草果5%，就幂
                recommendedSize.x = closeP2_X;
            }
            else
            {
                if (originalSize.x % 2 == 1)
                {
                    originalSize.x += 1;
                    recommendedSize.x = closeP2_X;
                }
                else
                {
                    recommendedSize.x = originalSize.x;
                }
            }
        }
        //================================================================
        int closeP2_Y = Mathf.ClosestPowerOfTwo(originalSize.y);

        if (closeP2_Y <= originalSize.y && closeP2_Y * currentPara <= originalSize.y)//缩小只能缩小5%
        {
            //缩小
            recommendedSize.y = closeP2_Y;
        }
        else
        {
            //放大
            if (closeP2_Y <= originalSize.y * currentPara)
            {
                //放大不草果5%，就幂
                recommendedSize.y = closeP2_Y;
            }
            else
            {
                if (originalSize.y % 2 == 1)
                {
                    originalSize.y += 1;
                    recommendedSize.y = closeP2_Y;
                }
                else
                {
                    recommendedSize.y = originalSize.y;
                }
            }
        }

        return recommendedSize;
    }
}