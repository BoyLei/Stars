//using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SGameBlurRF : ScriptableRendererFeature
{
    
    class CustomRenderPass : ScriptableRenderPass
    {
        
        [Range(0.0f, 5.0f)]
        public float _blurSize = 1;
        [Range(0, 10)]
        public int _blurIterations = 2;
        [Range(1.0f, 4.0f)] 
        public float _downsample = 1;

        public bool _blurOn = false;
        public Material _blurMaterial;

        private RenderTextureFormat texFormat;

        private RenderTexture destRT;
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            
            // int width = Mathf.RoundToInt(Screen.width / (_downsample * 2));
            // int height = Mathf.RoundToInt(Screen.height / (_downsample * 2));
            // RenderTextureFormat formatRT = renderingData.cameraData.camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            // destRT = new RenderTexture(width, height, 24, formatRT);
            // destRT.name = "PlanerReflectionTex";
            
            //两张混合RT 判断用哪个格式好点
            texFormat = RenderTextureFormat.ARGB32;
            // if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            // {
            //     texFormat = RenderTextureFormat.ARGBHalf;
            // }
            // if(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB565))
            // {
            //     texFormat = RenderTextureFormat.RGB565;
            // }
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_blurOn)
            {
                RenderTexture source = renderingData.cameraData.targetTexture;
                            RenderTargetIdentifier dest =  RenderTargetHandle.CameraTarget.Identifier();
                            
                            //renderingData.cameraData.targ
                
                            CommandBuffer buf = CommandBufferPool.Get("Blur Reflection Texture");//new CommandBuffer();
                            //buf.name = "Blur Reflection Texture";
                           // _cameras[cam] = buf;
                            if(source!=null)
                            {
                                float width = source.width;
                                float height = source.height;
                                int rtW = Mathf.RoundToInt(width / (_downsample * 2));
                                int rtH = Mathf.RoundToInt(height / (_downsample * 2));
                    
                                int blurredID = Shader.PropertyToID("_Temp1");
                                int blurredID2 = Shader.PropertyToID("_Temp2");
                                
                                buf.GetTemporaryRT(blurredID, rtW, rtH, 0, FilterMode.Bilinear, texFormat);
                                buf.GetTemporaryRT(blurredID2, rtW, rtH, 0, FilterMode.Bilinear, texFormat);
                    
                                buf.Blit(Shader.PropertyToID("_CameraColorAttachmentA"), blurredID);
                                for (int i = 0; i < _blurIterations; i++)
                                {
                                    float iterationOffs = (i * 1.0f);
                                    buf.SetGlobalFloat("_Offset", iterationOffs / (_downsample* 2) + _blurSize);
                                    buf.Blit(blurredID, blurredID2, _blurMaterial, 0);
                                    buf.Blit(blurredID2, blurredID, _blurMaterial, 0);
                                }
                                buf.Blit(blurredID, Shader.PropertyToID("_CameraColorAttachmentA"));
                    
                                buf.ReleaseTemporaryRT(blurredID);
                                buf.ReleaseTemporaryRT(blurredID2);
                            
                            
                                context.ExecuteCommandBuffer(buf);
                               // cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
                            
                                CommandBufferPool.Release(buf);
                            }
            }
            
          
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            //destRT.Release();
        }
    }

    CustomRenderPass m_ScriptablePass;

    
        [Range(0.0f, 5.0f)]
        public float _blurSize;
        [Range(0, 10)]
        public int _blurIterations;
        [Range(1.0f, 4.0f)] 
        public float _downsample;

        private Material _blurMaterial = null;
        public bool _blurOn;



        //public RenderPassData renderPassData;
    /// <inheritdoc/>
    public override void Create()
    {
        if (_blurMaterial == null)
        {
            _blurMaterial = new Material(StarProject.Service.Shader.ShaderManager.Instance.Find("Hidden/KawaseBlur"));
        }

        if (_blurMaterial != null)
        {
            m_ScriptablePass = new CustomRenderPass();
            
            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }
        
        
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass._blurSize = _blurSize;
        m_ScriptablePass._blurIterations = _blurIterations;
        m_ScriptablePass._downsample = _downsample;
        m_ScriptablePass._blurOn = _blurOn;
        m_ScriptablePass._blurMaterial = _blurMaterial;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


