
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class ShadowMapNode
    {
        static ShadowMapNode noneNode = new ShadowMapNode(1, 0);
        public static int MINSIZE = 1024;
        public static int ROOTINDEX = -1;
        //
        private int BuildSize;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int DynamicSize { get; private set; }
        public int Area => Width * Height;
//      //索引对应位置,0为根节点
        //0,1
        //2,3
        private int _index = 0;
        private ShadowMapNode[] _children = null;
        ShadowMapNode _parent = null;
        private bool _occupied = false;
        private bool _bChildOccupied = false;
        static List<ShadowMapNode> _parentNodes = new List<ShadowMapNode>();

        public ShadowMapNode(int size, int index)
        {
            BuildSize = size;
            _index = index;
        }
        static Vector2[] _uv_maps =
        {
            new Vector2(0.0f,0.0f),
            new Vector2(1.0f,0.0f),
            new Vector2(0.0f,1.0f),
            new Vector2(1.0f,1.0f),
        };
        public bool Insert(CustomShadowMgr._ShadowParams quad)
        {
            if (_occupied ||
                quad.StaticShadowMapSize > BuildSize)
            {
                return false;
            }
            //
            if (quad.StaticShadowMapSize == BuildSize)
            {
                _occupied = true;
                _parentNodes.Clear();
                _parentNodes.Add(this);
                var pParent = _parent;
                while (pParent != null &&
                    pParent._index != ROOTINDEX)
                {
                    _parentNodes.Add(pParent);
                    //
                    pParent = pParent._parent;
                }
                float x_start = 0.0f;
                float y_start = 0.0f;
                //int n_counter = 1;
                for (int i = _parentNodes.Count - 1; i >= 0; i--)
                {
                    var node = _parentNodes[i];
                    if (node._index != ROOTINDEX)
                    {
                        Vector2 uv = _uv_maps[node._index];
                        x_start += uv.x * node.BuildSize;
                        y_start += uv.y * node.BuildSize;
                    }
                    //n_counter++;
                }
                quad.ShadowViewport_S = new Rect(x_start, y_start, BuildSize, BuildSize) ;
                return true;
            }
            //
            foreach (var one_child in _children)
            {
                if (one_child._occupied)
                {

                }
                else
                {
                    //
                    _bChildOccupied = one_child.Insert(quad);
                    break;
                }
            }
            bool bAllOcupied = _children[0]._occupied &
                _children[1]._occupied &
                _children[2]._occupied &
                _children[3]._occupied;
            _occupied = bAllOcupied;
            return true;
        }
        void _buildTreeImpl()
        {
            if (BuildSize == MINSIZE ||
                BuildSize == 1)
                return;
            //
            int childSize = BuildSize / 2;
            _children = new ShadowMapNode[4]
                { new ShadowMapNode(childSize,0), new ShadowMapNode(childSize,1), new ShadowMapNode(childSize,2), new ShadowMapNode(childSize,3) };
            _children[0]._parent = this;
            _children[1]._parent = this;
            _children[2]._parent = this;
            _children[3]._parent = this;
            //
            foreach (var one_child in _children)
            {
                one_child._buildTreeImpl();
            }
        }
        void _calculateDimens()
        {
            if(_children == null ||
                _children.Length == 0 ||
                _occupied)
            {
                Height = BuildSize;
                Width = BuildSize;
                return;
            }
            Width = _children[0].BuildSize;
            if (_children[1]._occupied || _children[1]._bChildOccupied)
            {
                Width *= 2;
            }
            Height = _children[0].BuildSize;
            if (_children[2]._occupied || _children[2]._bChildOccupied)
            {
                Height *= 2;
            }
        }
        public static ShadowMapNode Build(List<CustomShadowMgr._ShadowParams> quads)
        {
            quads.Sort((x, y) => y.StaticShadowMapSize.CompareTo(x.StaticShadowMapSize));
            //总面积
            int totalArea = 0;
            foreach (var s in quads)
            {
                totalArea += s.StaticShadowMapSize * s.StaticShadowMapSize;
            }
            if (totalArea == 0)
                return ShadowMapNode.noneNode;
            //得到最小四叉树size
            int minSize = quads[0].StaticShadowMapSize;// * 2;
            while (minSize * minSize < totalArea)
            {
                minSize = minSize * 2;
            }
            //建立四叉树并Build内部结构
            var quadTree = new ShadowMapNode(minSize, ROOTINDEX);
            quadTree._buildTreeImpl();
            //插入
            foreach (var q in quads)
            {
                quadTree.Insert(q);
            }
            quadTree._calculateDimens();

            quadTree.DynamicSize = quads[0].DynamicShadowMapSize;
            quads[0].ShadowViewport_D = new Rect(0, 0, quads[0].DynamicShadowMapSize, quads[0].DynamicShadowMapSize);
            return quadTree;
        }
    }

    public class CustomShadowMgrSingleton<T> where T : new()
    {
        protected static T instance;
        private static readonly object syslock = new object();

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syslock)
                    {
                        if (instance == null)
                        {
                            instance = new T();
                        }
                    }
                }
                return instance;
            }
        }

        public static bool HasInstance
        {
            get
            {
                return instance != null;
            }
        }

        public static void Destroy()
        {
            if (instance is IDisposable) (instance as IDisposable).Dispose();

            instance = default(T);
        }

    }

    public class CustomShadowMgr : CustomShadowMgrSingleton<CustomShadowMgr>
    {
        public const int MAX_SHADOW_COUNT = 9;
        public class _ShadowParams
        {
            public static _ShadowParams nullValue = new _ShadowParams();
            public ScriptableCullingParameters ShadowCullingParameters;
            public Rect ShadowViewport_S;
            public Rect ShadowViewport_D;
            public int StaticShadowMapSize;
            public int DynamicShadowMapSize;
            public Vector3 DynamicCameraPosition;
            public float DynamicOrthSize;
            public Vector3 StaticCameraPosition;
            public float StaticOrthSize;

            public Camera ShadowCamera;
            internal Matrix4x4 UVRectMat_S;
            internal Matrix4x4 UVRectMat_D;
            public Matrix4x4 ShadowMatrix;
            public Matrix4x4 ShadowMatrix_D;
            public Matrix4x4 CasterShadowMatrix;
            public Matrix4x4 CasterShadowMatrix_D;
            public Matrix4x4 ProjMatStatic;
            public Matrix4x4 ProjMatDynamic;
            public bool UseDynamicMatrix = false;
            public Vector4 ShadowBias;
            public Vector4 DynamicShadowBias;
            public float DepthBias_S;
            public float NormalBias_S;
            public float DepthBias_D;
            public float NormalBias_D;
            public Vector4 LightDir;
            //非Point阴影都为Unknown
            public CubemapFace FaceID = CubemapFace.Unknown;
            public Vector4 ExtraParam;
            public bool Inited = false;

            public int staticLayer;
            public int dynamicLayer;
        }
        public Dictionary<Light, List<_ShadowParams>> DictShadows = new Dictionary<Light, List<_ShadowParams>>();

        public ShadowMapNode ShadowMapNode;
        public RenderTexture CustomShadowMapDynamic;
        public RenderTexture CustomShadowMapStatic;
        public Matrix4x4[] CustomMatrixToShadow;
        public Matrix4x4[] CustomMatrixToShadowDynamic;
        public float[] CustomLightType;
        public Vector4[] StaticShadowUVRects;
        public Vector4[] DynamicShadowUVRects;
        public Dictionary<Light, int> LightIndex;

        public Vector4[] CustomLightExtraParam;
        //int[] sizes;
        public Vector4 ShadowDynamicPCFSize;
        public struct GPUInstanceData
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4[] matrices;
            public GPUInstanceData(Mesh mesh, Material material, Matrix4x4[] matrices)
            {
                this.mesh = mesh;
                this.material = material;
                this.matrices = matrices;
            }
        };

        [Serializable]
        public struct RendererData
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4 matrix;
            public RendererData(Mesh mesh, Material material, Matrix4x4 matrix)
            {
                this.mesh = mesh;
                this.material = material;
                this.matrix = matrix;
            }
        }
        public List<GPUInstanceData> gpuInstanceDataList
        {
            get { return m_GPUInstanceDataList; }
        }

        public List<RendererData> rendererDataList { get { return m_RendererDataList; } }

        public void AddGPUInstanceData(Mesh mesh, Material material, Matrix4x4[] matrices)
        {
            GPUInstanceData data = new GPUInstanceData(mesh, material, matrices);
            m_GPUInstanceDataList.Add(data);
        }

        public void AddRendererData(List<RendererData> datas)
        {
            m_RendererDataList.AddRange(datas);
        }
        public bool _bDirt { get; internal set; }
        public bool _bUpdateShadowNodeMatrix { get; internal set; } = false;
        public bool _bUpdateShadowNodeMatrixD { get; internal set; } = false;
        public bool renderStaticLayer { get; set; } = false;
        public bool IsNull()
        {
            return ShadowMapNode == null;
        }
        public bool IsEmpty()
        {
            return DictShadows.Count == 0;
        }
        List<_ShadowParams> _getShadowParams()
        {
            List<_ShadowParams> _params = new List<CustomShadowMgr._ShadowParams>();
            List<Light> invalidLights = null;
            foreach (var keyval in DictShadows)
            {
                var light = keyval.Key;
                if (light == null)
                {
                    if (invalidLights == null)
                        invalidLights = new List<Light>();
                    
                    invalidLights.Add(light);
                    continue;
                }
                
                var param = keyval.Value;
                if(light.isActiveAndEnabled && param[0].Inited)
                    _params.AddRange(param);
                else
                {
                    int a = 0;
                }
            }

            if (invalidLights != null && invalidLights.Count > 0)
            {
                Debug.LogWarning("[ZURP_ShadowMgr] DictShadows存在无效阴影，请检查当前或上一场景阴影初始化和销毁是否正确！");
                foreach (var toDel in invalidLights)
                {
                    DictShadows.Remove(toDel);
                }
            }
            
            return _params;
        }
        List<GPUInstanceData> m_GPUInstanceDataList;
        List<RendererData> m_RendererDataList = new List<RendererData>();
        public bool Build()
        {
            if (!_bDirt)
                return false;

            DestroyShadowMap();
            LightIndex = new Dictionary<Light, int>();
            CustomMatrixToShadow = new Matrix4x4[MAX_SHADOW_COUNT];
            CustomMatrixToShadowDynamic = new Matrix4x4[MAX_SHADOW_COUNT];
            CustomLightType = new float[MAX_SHADOW_COUNT] { -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f, -1.0f };
            StaticShadowUVRects = new Vector4[MAX_SHADOW_COUNT];
            DynamicShadowUVRects = new Vector4[MAX_SHADOW_COUNT];
            CustomLightExtraParam = new Vector4[MAX_SHADOW_COUNT];
            ShadowDynamicPCFSize = Vector4.one;
            if (DictShadows.Count > 0)
            {
                //
                ShadowMapNode = ShadowMapNode.Build(_getShadowParams());
                if (ShadowMapNode.Width == 0 ||
                    ShadowMapNode.Height == 0)
                {
                    _bDirt = false;

                    return true;
                }
                CreateShadowMap();
                //
                //var mat = Matrix4x4.TRS(
                //    new Vector3(0.5f, 0.5f, 0.0f),
                //    Quaternion.identity,
                //    new Vector3(0.5f, 0.5f, 1.0f));
                var one_over_width_D = 1.0f / ShadowMapNode.DynamicSize;
                var one_over_height_D = 1.0f / ShadowMapNode.DynamicSize;
                float dynamicShadowPCFWidth = 0.65f * one_over_width_D;
                float dynamicShadowPCFHeight = 0.65f * one_over_height_D;
                ShadowDynamicPCFSize = new Vector4(dynamicShadowPCFWidth, dynamicShadowPCFHeight, one_over_width_D, one_over_height_D);

                var one_over_width_S = 1.0f / ShadowMapNode.Width;
                var one_over_height_S = 1.0f / ShadowMapNode.Height;

                int paramCounter = 0;
                List<Light> invalidLights = null;
                for (int i = 0; i < DictShadows.Count; i++)
                {
                    var curPair = DictShadows.ElementAt(i);
                    var curParams = curPair.Value;
                    var curLight = curPair.Key;
                    if (curLight == null)
                    {
                        if (invalidLights == null)
                            invalidLights = new List<Light>();
                        
                        invalidLights.Add(curLight);
                        continue;
                    }
                    //
                    LightIndex[curLight] = paramCounter;
                    for (int j = 0;j < curParams.Count;j++)
                    {
                        var one_param = curParams[j];
                        //
                        //var uv = ShadowUVRects[paramCounter];
                        StaticShadowUVRects[paramCounter].x = one_param.ShadowViewport_S.x * one_over_width_S;
                        StaticShadowUVRects[paramCounter].y = one_param.ShadowViewport_S.y * one_over_height_S;
                        StaticShadowUVRects[paramCounter].z = one_param.ShadowViewport_S.width * one_over_width_S;
                        StaticShadowUVRects[paramCounter].w = one_param.ShadowViewport_S.height * one_over_height_S;
                        //
                        one_param.UVRectMat_S = Matrix4x4.identity;
                        one_param.UVRectMat_S.m00 = StaticShadowUVRects[paramCounter].z;
                        one_param.UVRectMat_S.m11 = StaticShadowUVRects[paramCounter].w;
                        one_param.UVRectMat_S.m22 = 1.0f;
                        one_param.UVRectMat_S.m03 = StaticShadowUVRects[paramCounter].x;
                        one_param.UVRectMat_S.m13 = StaticShadowUVRects[paramCounter].y;
                        one_param.UVRectMat_S.m23 = 0.0f;
                        one_param.ShadowBias =
                            GetShadowBias(curLight, one_param.DepthBias_S, one_param.NormalBias_S, one_param.ProjMatStatic, one_param.StaticShadowMapSize);

                        one_param.UVRectMat_D = Matrix4x4.identity;
                        one_param.UVRectMat_D.m00 = one_param.ShadowViewport_D.width * one_over_width_D;
                        one_param.UVRectMat_D.m11 = one_param.ShadowViewport_D.height * one_over_height_D;
                        one_param.UVRectMat_D.m22 = 1.0f;
                        one_param.UVRectMat_D.m03 = one_param.ShadowViewport_D.x * one_over_width_D;
                        one_param.UVRectMat_D.m13 = one_param.ShadowViewport_D.y * one_over_height_D;
                        one_param.UVRectMat_D.m23 = 0.0f;

                        CustomMatrixToShadow[paramCounter] = one_param.ShadowMatrix;// * matrix_vp_port;
                        CustomMatrixToShadowDynamic[paramCounter] = one_param.UseDynamicMatrix ? one_param.ShadowMatrix_D : Matrix4x4.identity;
                        if (curLight.type == LightType.Directional)
                        {
                            CustomMatrixToShadow[paramCounter] = one_param.UVRectMat_S * CustomMatrixToShadow[paramCounter];
                            CustomMatrixToShadowDynamic[paramCounter] = one_param.UVRectMat_D * CustomMatrixToShadowDynamic[paramCounter];
                        }

                        CustomLightType[paramCounter] = (int)curLight.type;
                        CustomLightExtraParam[paramCounter] = one_param.ExtraParam;
                        paramCounter++;
                        if (paramCounter >= MAX_SHADOW_COUNT)
                            break;
                    }
                    if (paramCounter >= MAX_SHADOW_COUNT)
                        break;
                }

                if (invalidLights != null && invalidLights.Count > 0)
                {
                    Debug.LogWarning("[ZURP_ShadowMgr] DictShadows存在无效阴影，请检查当前或上一场景阴影初始化和销毁是否正确！");
                    foreach (var toDel in invalidLights)
                    {
                        DictShadows.Remove(toDel);
                    }
                }
            }
            else
            {
                ShadowMapNode = null;
            }

            _bDirt = false;

            return true; 
        }
        internal void UpdateShadowNodeMatrix()
        {
            if (!_bUpdateShadowNodeMatrix)
                return;
            foreach(var kv in LightIndex)
            {
                var light = kv.Key;
                var index = kv.Value;
                var shadow_param = DictShadows[light];
                //
                switch (light.type)
                {
                    case LightType.Point:
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            CustomMatrixToShadow[index + i] = shadow_param[i].ShadowMatrix;// * matrix_vp_port;
                                shadow_param[i].ShadowBias =
                            shadow_param[i].ShadowBias = GetShadowBias(light, 
                                shadow_param[i].DepthBias_S, 
                                shadow_param[i].NormalBias_S, 
                                shadow_param[i].ProjMatStatic, 
                                shadow_param[i].StaticShadowMapSize);

                                shadow_param[i].DynamicShadowBias = shadow_param[i].ShadowBias;

                                //CustomMatrixToShadow[index + i] = shadow_param[i].UVRectMat * CustomMatrixToShadow[index + i];
                            }
                        } break;
                    case LightType.Directional:
                    {
                        CustomMatrixToShadow[index] = shadow_param[0].ShadowMatrix;// * matrix_vp_port;
                        CustomMatrixToShadow[index] = shadow_param[0].UVRectMat_S * CustomMatrixToShadow[index];
                        shadow_param[0].ShadowBias = GetShadowBias(light, 
                            shadow_param[0].DepthBias_S, 
                            shadow_param[0].NormalBias_S, 
                            shadow_param[0].ProjMatStatic, 
                            shadow_param[0].StaticShadowMapSize);

                        }
                        break;
                    default:
                    {
                        CustomMatrixToShadow[index] = shadow_param[0].ShadowMatrix;// * matrix_vp_port;
                        //CustomMatrixToShadow[index] = shadow_param[0].UVRectMat * CustomMatrixToShadow[index];
                        shadow_param[0].ShadowBias = GetShadowBias(light,
                            shadow_param[0].DepthBias_S,
                            shadow_param[0].NormalBias_S,
                            shadow_param[0].ProjMatStatic,
                            shadow_param[0].StaticShadowMapSize);

                        shadow_param[0].DynamicShadowBias = shadow_param[0].ShadowBias;
                        }
                        break;
                }
            }
            _bUpdateShadowNodeMatrix = false;
        }
        internal void UpdateShadowNodeMatrixD()
        {
            if (!_bUpdateShadowNodeMatrixD)
                return;
            foreach (var kv in LightIndex)
            {
                var light = kv.Key;
                var index = kv.Value;
                var shadow_param = DictShadows[light];
                //
                switch (light.type)
                {
                    case LightType.Point:
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                CustomMatrixToShadowDynamic[index + i] = shadow_param[i].UseDynamicMatrix ? shadow_param[0].ShadowMatrix_D : Matrix4x4.identity;
                                //CustomMatrixToShadowDynamic[index + i] = shadow_param[i].UVRectMat * CustomMatrixToShadowDynamic[index + i];
                            }
                        }
                        break;
                    case LightType.Directional:
                        {
                            CustomMatrixToShadowDynamic[index] = shadow_param[0].UseDynamicMatrix ? shadow_param[0].ShadowMatrix_D : Matrix4x4.identity;
                            CustomMatrixToShadowDynamic[index] = shadow_param[0].UVRectMat_D * CustomMatrixToShadowDynamic[index];
                            shadow_param[0].DynamicShadowBias = GetShadowBias(light,
                             shadow_param[0].DepthBias_D,
                             shadow_param[0].NormalBias_D,
                             shadow_param[0].ProjMatDynamic,
                             shadow_param[0].DynamicShadowMapSize);
                        }
                        break;
                    default:
                        {
                            CustomMatrixToShadowDynamic[index] = shadow_param[0].UseDynamicMatrix ? shadow_param[0].ShadowMatrix_D : Matrix4x4.identity;
                            //CustomMatrixToShadowDynamic[index] = shadow_param[0].UVRectMat * CustomMatrixToShadowDynamic[index];
                        }
                        break;
                }


            }
            _bUpdateShadowNodeMatrixD = false;
        }
        void CreateShadowMap() 
        {
            DestroyShadowMap();
            //_projectDepth1 = RenderTexture.GetTemporary(_projector.pixelWidth, _projector.pixelHeight, 24, RenderTextureFormat.ARGB32);

            int bits = 24;
            if (Application.platform == RuntimePlatform.Android) 
            {
                bits = 16;
            }

            CustomShadowMapDynamic = //RenderTexture.GetTemporary(ShadowMapNode.Width, ShadowMapNode.Height, 24, RenderTextureFormat.ARGB32); 
               ShadowUtils.GetTemporaryShadowTexture(ShadowMapNode.DynamicSize, ShadowMapNode.DynamicSize, bits); //
            //RenderTexture.GetTemporary(DepthMapDimens, DepthMapDimens, 24, RenderTextureFormat.RFloat);
            CustomShadowMapDynamic.name = "CustomShadowMapDynamic";


            CustomShadowMapStatic = //RenderTexture.GetTemporary(ShadowMapNode.Width, ShadowMapNode.Height, 24, RenderTextureFormat.ARGB32);
                ShadowUtils.GetTemporaryShadowTexture(ShadowMapNode.Width, ShadowMapNode.Height, bits); //
            //RenderTexture.GetTemporary(DepthMapDimens, DepthMapDimens, 24, RenderTextureFormat.RFloat);
            CustomShadowMapStatic.name = "CustomShadowMapStatic";
            //_projectStaticDepth.wrapModeU = TextureWrapMode.Clamp;
            //_projectStaticDepth.wrapModeV = TextureWrapMode.Clamp;
            //_projectStaticDepth.wrapMode = TextureWrapMode.Clamp;

            //_projectStaticDepth.dimension = TextureDimension.Cube;
            if (null == CustomShadowMapStatic ||
                null == CustomShadowMapDynamic)
            {
                Debug.LogError("ZURP_ShadowMgr:CreateShadowMap: create custom shadowmap failed!!!");
            }
        }
        internal void DestroyShadowMap()
        {
            if (null != CustomShadowMapDynamic)
                RenderTexture.ReleaseTemporary(CustomShadowMapDynamic);
            CustomShadowMapDynamic = null;
            //
            if (null != CustomShadowMapStatic)
                RenderTexture.ReleaseTemporary(CustomShadowMapStatic);
            CustomShadowMapStatic = null;
        }

        public void AddShadow(Light s, int faceID = -1,_ShadowParams p = null)
        {
            List<_ShadowParams> _params;
            if (!DictShadows.ContainsKey(s))
            {
                DictShadows[s] = new List<_ShadowParams>();
            }
            _params = DictShadows[s];
            if (_params.Contains(p))
                return;
            //
            if (p == null)
            {
                p  = new _ShadowParams();
            }
            _params.Add(p);
            p.FaceID = (CubemapFace)faceID;
            //
            _bDirt = true;
        }
        public void RmvShadow(Light s)
        {
            if (!DictShadows.ContainsKey(s))
                return;

            DictShadows.Remove(s);
            //
            _bDirt = true;
        }

        public void SetShadowCamera(Light light, int faceID, Camera cam)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.ShadowCamera = cam;
        }

        public void InitedShadow(Light light, int faceID)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.Inited = true;
            _bDirt = true;
        }

        public void SetStaticShadowMatrix(Light light, int faceID, Matrix4x4 matCaster, Matrix4x4 mat, Matrix4x4 proj,
                              float bias_depth, float bias_normal, float shadowStrength, float biasDepthDynamic, float biasNormalDynamic)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.ShadowMatrix = mat;
            p.CasterShadowMatrix = matCaster;
            //p.ShadowBias = new Vector4(bias_depth, bias_normal, 0.0f, 0.0f);
            p.DepthBias_S = bias_depth;
            p.NormalBias_S = bias_normal;
            p.LightDir = -light.transform.forward;//顺便一起初始化
            p.ExtraParam = new Vector4(shadowStrength, 0, 0, 0);
            p.ProjMatStatic = proj;
            _bUpdateShadowNodeMatrix = true;
            p.DepthBias_D = biasDepthDynamic;
            p.NormalBias_D = biasNormalDynamic;
        }

        public void SetDynamicShadowMatrix(Light light, int faceID, Matrix4x4 matDCaster, Matrix4x4 mat, Matrix4x4 projDynamic, float biasDepthDynamic, float biasNormalDynamic)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.ShadowMatrix_D = mat;
            p.CasterShadowMatrix_D = matDCaster;
            p.UseDynamicMatrix = true;
            p.ProjMatDynamic = projDynamic;
            p.DepthBias_D = biasDepthDynamic;
            p.NormalBias_D = biasNormalDynamic;
            _bUpdateShadowNodeMatrixD = true;
        }

        public void SyncStaticLayer(Light light, int faceID, LayerMask layerMask)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.staticLayer = layerMask;
        }

        public void SyncDynamicLayer(Light light, int faceID, LayerMask layerMask)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.dynamicLayer = layerMask;
        }

        public void SetStaticCameraInfo(Light light, int faceID, Vector3 cameraPosition, float staticShadowDistance)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.StaticCameraPosition = cameraPosition;
            p.StaticOrthSize = staticShadowDistance / 2;
        }
        public void SetDynamicCameraInfo(Light light, int faceID, Vector3 cameraPosition, float dynamicShadowDistance)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            p.DynamicCameraPosition = cameraPosition;
            p.DynamicOrthSize = dynamicShadowDistance / 2;
        }

        public void SetShadowSize(Light light, int faceID, int staticSize, int dynamicSize)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            if (p.StaticShadowMapSize == staticSize && p.DynamicShadowMapSize == dynamicSize)
            {
                return;
            }
            p.StaticShadowMapSize = staticSize;
            p.DynamicShadowMapSize = dynamicSize;
            _bDirt = true;
        }
        public void SetShadowStrength(Light light, int faceID, float shadowStrength)
        {
            var p = GetShadow(light, faceID);
            if (p == null)
                return;

            if (p.ExtraParam.x == shadowStrength)
            {
                return;
            }
            p.ExtraParam = new Vector4(shadowStrength, 0, 0, 0);
            _bDirt = true;
        }
        internal _ShadowParams GetShadow(Light s,int index = 0)
        {
            if (!DictShadows.ContainsKey(s))
                return _ShadowParams.nullValue;
            return DictShadows[s][index];
        }
        public static Vector4 GetShadowBias(Light shadowLight, float dBias, float nBias, Matrix4x4 lightProjectionMatrix, float shadowResolution)
        {
            float frustumSize;
            if (shadowLight.type == LightType.Directional)
            {
                // Frustum size is guaranteed to be a cube as we wrap shadow frustum around a sphere
                frustumSize = 2.0f / lightProjectionMatrix.m00;
            }
            else if (shadowLight.type == LightType.Spot)
            {
                // For perspective projections, shadow texel size varies with depth
                // It will only work well if done in receiver side in the pixel shader. Currently UniversalRP
                // do bias on caster side in vertex shader. When we add shader quality tiers we can properly
                // handle this. For now, as a poor approximation we do a constant bias and compute the size of
                // the frustum as if it was orthogonal considering the size at mid point between near and far planes.
                // Depending on how big the light range is, it will be good enough with some tweaks in bias
                frustumSize = Mathf.Tan(shadowLight.spotAngle * 0.5f * Mathf.Deg2Rad) * shadowLight.range; // half-width (in world-space units) of shadow frustum's "far plane"
            }
            else if (shadowLight.type == LightType.Point)
            {
                // [Copied from above case:]
                // "For perspective projections, shadow texel size varies with depth
                //  It will only work well if done in receiver side in the pixel shader. Currently UniversalRP
                //  do bias on caster side in vertex shader. When we add shader quality tiers we can properly
                //  handle this. For now, as a poor approximation we do a constant bias and compute the size of
                //  the frustum as if it was orthogonal considering the size at mid point between near and far planes.
                //  Depending on how big the light range is, it will be good enough with some tweaks in bias"
                // Note: HDRP uses normalBias both in HDShadowUtils.CalcGuardAnglePerspective and HDShadowAlgorithms/EvalShadow_NormalBias (receiver bias)
                float fovBias = GetPointLightShadowFrustumFovBiasInDegrees((int)shadowResolution, (shadowLight.shadows == LightShadows.Soft));
                // Note: the same fovBias was also used to compute ShadowUtils.ExtractPointLightMatrix
                float cubeFaceAngle = 90 + fovBias;
                frustumSize = Mathf.Tan(cubeFaceAngle * 0.5f * Mathf.Deg2Rad) * shadowLight.range; // half-width (in world-space units) of shadow frustum's "far plane"
            }
            else
            {
                Debug.LogWarning("Only point, spot and directional shadow casters are supported in universal pipeline");
                frustumSize = 0.0f;
            }

            // depth and normal bias scale is in shadowmap texel size in world space
            float texelSize = frustumSize / shadowResolution;
            float depthBias = -dBias * texelSize;
            float normalBias = -nBias * texelSize;

            // The current implementation of NormalBias in Universal RP is the same as in Unity Built-In RP (i.e moving shadow caster vertices along normals when projecting them to the shadow map).
            // This does not work well with Point Lights, which is why NormalBias value is hard-coded to 0.0 in Built-In RP (see value of unity_LightShadowBias.z in FrameDebugger, and native code that sets it: https://github.cds.internal.unity3d.com/unity/unity/blob/a9c916ba27984da43724ba18e70f51469e0c34f5/Runtime/Camera/Shadows.cpp#L1686 )
            // We follow the same convention in Universal RP:
            if (shadowLight.type == LightType.Point)
                normalBias = 0.0f;

            //if (shadowData.supportsSoftShadows && shadowLight.light.shadows == LightShadows.Soft)
            //{
            //    SoftShadowQuality softShadowQuality = SoftShadowQuality.Medium;
            //    if (shadowLight.light.TryGetComponent(out UniversalAdditionalLightData additionalLightData))
            //        softShadowQuality = additionalLightData.softShadowQuality;
            //
            //    // TODO: depth and normal bias assume sample is no more than 1 texel away from shadowmap
            //    // This is not true with PCF. Ideally we need to do either
            //    // cone base bias (based on distance to center sample)
            //    // or receiver place bias based on derivatives.
            //    // For now we scale it by the PCF kernel size of non-mobile platforms (5x5)
            //    float kernelRadius = 2.5f;
            //
            //    switch (softShadowQuality)
            //    {
            //        case SoftShadowQuality.High: kernelRadius = 3.5f; break; // 7x7
            //        case SoftShadowQuality.Medium: kernelRadius = 2.5f; break; // 5x5
            //        case SoftShadowQuality.Low: kernelRadius = 1.5f; break; // 3x3
            //        default: break;
            //    }
            //
            //    depthBias *= kernelRadius;
            //    normalBias *= kernelRadius;
            //}
            var a = new Vector4(depthBias, normalBias, 0.0f, 0.0f);
            return a;
        }
        private const int kMinimumPunctualLightHardShadowResolution = 8;
        private const int kMinimumPunctualLightSoftShadowResolution = 16;
        // Returns the guard angle that must be added to a point light shadow face frustum angle
        // in order to avoid shadows missing at the boundaries between cube faces.
        internal static float GetPointLightShadowFrustumFovBiasInDegrees(int shadowSliceResolution, bool shadowFiltering)
        {
            // Commented-out code below uses the theoretical formula to compute the required guard angle based on the number of additional
            // texels that the projection should cover. It is close to HDRP's HDShadowUtils.CalcGuardAnglePerspective method.
            // However, due to precision issues or other filterings performed at lighting for example, this formula also still requires a fudge factor.
            // Since we only handle a fixed number of resolutions, we use empirical values instead.
#if false
            float fudgeFactor = 1.5f;
            return fudgeFactor * CalcGuardAngle(90, shadowFiltering ? 5 : 1, shadowSliceResolution);
#endif
            float fovBias = 4.00f;

            // Empirical value found to remove gaps between point light shadow faces in test scenes.
            // We can see that the guard angle is roughly proportional to the inverse of resolution https://docs.google.com/spreadsheets/d/1QrIZJn18LxVKq2-K1XS4EFRZcZdZOJTTKKhDN8Z1b_s
            if (shadowSliceResolution <= kMinimumPunctualLightHardShadowResolution)
            {
#if DEVELOPMENT_BUILD
                //if (!m_IssuedMessageAboutPointLightHardShadowResolutionTooSmall)
                //{
                //    Debug.LogWarning("Too many additional punctual lights shadows, increase shadow atlas size or remove some shadowed lights");
                //    m_IssuedMessageAboutPointLightHardShadowResolutionTooSmall = true; // Only output this once per shadow requests configuration
                //}
#endif
            }
            else if (shadowSliceResolution <= 16)
                fovBias = 43.0f;
            else if (shadowSliceResolution <= 32)
                fovBias = 18.55f;
            else if (shadowSliceResolution <= 64)
                fovBias = 8.63f;
            else if (shadowSliceResolution <= 128)
                fovBias = 4.13f;
            else if (shadowSliceResolution <= 256)
                fovBias = 2.03f;
            else if (shadowSliceResolution <= 512)
                fovBias = 1.00f;
            else if (shadowSliceResolution <= 1024)
                fovBias = 0.50f;

            if (shadowFiltering)
            {
                if (shadowSliceResolution <= kMinimumPunctualLightSoftShadowResolution)
                {
#if DEVELOPMENT_BUILD
                    //if (!m_IssuedMessageAboutPointLightSoftShadowResolutionTooSmall)
                    //{
                    //    Debug.LogWarning("Too many additional punctual lights shadows to use Soft Shadows. Increase shadow atlas size, remove some shadowed lights or use Hard Shadows.");
                    //    // With such small resolutions no fovBias can give good visual results
                    //    m_IssuedMessageAboutPointLightSoftShadowResolutionTooSmall = true; // Only output this once per shadow requests configuration
                    //}
#endif
                }
                else if (shadowSliceResolution <= 32)
                    fovBias += 9.35f;
                else if (shadowSliceResolution <= 64)
                    fovBias += 4.07f;
                else if (shadowSliceResolution <= 128)
                    fovBias += 1.77f;
                else if (shadowSliceResolution <= 256)
                    fovBias += 0.85f;
                else if (shadowSliceResolution <= 512)
                    fovBias += 0.39f;
                else if (shadowSliceResolution <= 1024)
                    fovBias += 0.17f;

                // These values were verified to work on untethered devices for which m_SupportsBoxFilterForShadows is true.
                // TODO: Investigate finer-tuned values for those platforms. Soft shadows are implemented differently for them.
            }
            return fovBias;
        }

        public enum ShadowType
        {
            SSM,
        }

        public ShadowType staticShadowType = ShadowType.SSM;
        public int esmConst = 80;
        public float esmBlurDelta = 0.035f;

        public void RemoveRendererData(List<RendererData> datas)
        {
            foreach (var data in datas)
            {
                m_RendererDataList.Remove(data);
            }
        }
    }

    /// <summary>
    /// Renders a shadow map for the main Light.
    /// </summary>
    public class CustomShadowCasterPass : ScriptableRenderPass
    {
        private static class ShadowConstantBuffer
        {
            public static int id_ShadowMatrix;
            public static int id_ShadowBias;
            public static int id_DynamicSMRT;
            public static int id_StaticSMRT;
            public static int id_CustomShadowMatrices;
            public static int id_CustomShadowMatricesDynamic;
            public static int id_LightIndexMap;
            public static int id_CustomShadowUVRects;
            public static int id_CustomLightType;
            public static int id_LightDirection;
            public static int id_LightExtraParam;
            public static int id_ShadowDynamicPCFSize;
        }
        //static int _character_mask = LayerMask.GetMask("Character");
        //static int _scene_mask = LayerMask.GetMask("Scene");
        //public ZURP_ShadowMgr ZURP_ShadowMgr.Instance = new ZURP_ShadowMgr();
        //public ZURP_ShadowMgr ShadowMgr = ZURP_ShadowMgr.Instance;
        //public RenderTexture DynamicSMRT;
        //public RenderTexture StaticSMRT;

        //public Camera ShadowCamera;
        //ScriptableCullingParameters _shadowCullingParameters;
        //public Matrix4x4 ShadowMatrix;
        //public float DepthBias_Depth;
        //public float DepthBias_Normal;
        public bool ScreenSpaceShadow = false;
        bool NeedResetOnEmpty = true;
        bool HasMainLight = false;
        bool HasAddiLight = false;
        DrawingSettings DynamicDrawingSettings;
        FilteringSettings DynamicFilterSettings;

        DrawingSettings StaticDrawingSettings;
        FilteringSettings StaticFilterSettings;
        float[] LightIndicesToShadowMgr;

        //public bool CaptureStaticLayeEveryFrame = false;
        ShaderTagId _STag;
        const string m_StaticTag = "StaticShadow";
        const string m_DynamicTag = "DynamicShadow";
        ProfilingSampler m_StaticSampler = new ProfilingSampler(m_StaticTag);
        ProfilingSampler m_DynamicSampler = new ProfilingSampler(m_DynamicTag);
        public CustomShadowCasterPass(RenderPassEvent evt)
        {
            base.profilingSampler = new ProfilingSampler(nameof(CustomShadowCasterPass));
            renderPassEvent = evt;

            ShadowConstantBuffer.id_CustomShadowMatrices = Shader.PropertyToID("_CustomShadowMatrices");
            ShadowConstantBuffer.id_CustomShadowMatricesDynamic = Shader.PropertyToID("_CustomShadowMatricesDynamic");
            ShadowConstantBuffer.id_LightIndexMap = Shader.PropertyToID("_LightIndicesToShadowMgr");
            ShadowConstantBuffer.id_CustomShadowUVRects = Shader.PropertyToID("_CustomShadowUVRects");
            ShadowConstantBuffer.id_ShadowMatrix = Shader.PropertyToID("unity_MatrixVP");
            ShadowConstantBuffer.id_ShadowBias = Shader.PropertyToID("_ShadowBias");
            ShadowConstantBuffer.id_DynamicSMRT = Shader.PropertyToID("_CustomShadowMapDynamic");
            ShadowConstantBuffer.id_StaticSMRT = Shader.PropertyToID("_CustomShadowMapStatic");
            ShadowConstantBuffer.id_CustomLightType = Shader.PropertyToID("_CustomLightType");
            ShadowConstantBuffer.id_LightDirection = Shader.PropertyToID("_LightDirection");
            ShadowConstantBuffer.id_LightExtraParam = Shader.PropertyToID("_LightExtraParam");
            ShadowConstantBuffer.id_ShadowDynamicPCFSize = Shader.PropertyToID("_ShadowDynamicSize");

            //
            _STag = new ShaderTagId("ShadowCaster");// UniversalForward"); //
            DynamicFilterSettings = new FilteringSettings(RenderQueueRange.opaque);
            StaticFilterSettings = new FilteringSettings(RenderQueueRange.opaque);
        }

        public void OnPassDestroy()
        {
            CustomShadowMgr.Instance.DestroyShadowMap();
        }
        public bool Setup(ScriptableRenderContext context,ref RenderingData renderingData)
        {
            Clear(context);
            if (CustomShadowMgr.Instance.IsEmpty()) 
                return false;
            // 
            //if (DynamicDrawingSettings == null ||
            //    StaticDrawingSettings == null)
            {
                var sortFlags = (true)
                        ? renderingData.cameraData.defaultOpaqueSortFlags
                        : SortingCriteria.CommonTransparent;
                ///
                DynamicDrawingSettings = CreateDrawingSettings(_STag, ref renderingData, sortFlags);
                StaticDrawingSettings = CreateDrawingSettings(_STag, ref renderingData, sortFlags);

            }
            return true;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        { 
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            RenderShadowmap(ref context, ref renderingData.cullResults, ref renderingData.lightData, ref renderingData.shadowData);
            DrawMeshItem(ref context);
        }

        /// <inheritdoc/>
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd"); 

        }

        void DrawInstanceItem(ref ScriptableRenderContext context)
        {
            if (CustomShadowMgr.Instance.gpuInstanceDataList.Count > 0)
            {
                var cmd = CommandBufferPool.Get();
                cmd.SetRenderTarget(CustomShadowMgr.Instance.CustomShadowMapStatic);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                foreach (var shadow in CustomShadowMgr.Instance.DictShadows)
                {
                    var light = shadow.Key;
                    if (light == null) continue;

                    var shadowParams = shadow.Value;
                    foreach (var pam in shadowParams)
                    {
                        cmd.SetGlobalVector(ShadowConstantBuffer.id_ShadowBias, pam.ShadowBias);
                        cmd.SetGlobalVector(ShadowConstantBuffer.id_LightDirection, pam.LightDir);
                        cmd.SetViewport(pam.ShadowViewport_S);
                        cmd.SetGlobalMatrix(ShadowConstantBuffer.id_ShadowMatrix, pam.CasterShadowMatrix);
                        context.ExecuteCommandBuffer(cmd);

                        foreach (var group in CustomShadowMgr.Instance.gpuInstanceDataList)
                        {
                            if (group.mesh != null &&
                                group.material != null &&
                                group.matrices != null)
                            {
                                if (group.matrices.Length > 1023)
                                {
                                    var count = (int)Mathf.Ceil(group.matrices.Length / 1023.0f);
                                    for (int i = 0; i < count; ++i)
                                    {
                                        Matrix4x4[] matrices;
                                        if (i != count - 1)
                                            matrices = group.matrices.Skip(i * 1023).Take(1023).ToArray();
                                        else
                                            matrices = group.matrices.Skip((count - 1) * 1023).Take(group.matrices.Length - (count - 1) * 1023).ToArray();

                                        cmd.DrawMeshInstanced(group.mesh, 0, group.material, 1, matrices);
                                        context.ExecuteCommandBuffer(cmd);
                                        cmd.Clear();
                                    }
                                }
                                else
                                {
                                    cmd.DrawMeshInstanced(group.mesh, 0, group.material, 1, group.matrices);
                                    context.ExecuteCommandBuffer(cmd);
                                    cmd.Clear();
                                }
                            }
                        }
                        CustomShadowMgr.Instance.gpuInstanceDataList.Clear();
                    }
                }
            }
        }

        void DrawMeshItem(ref ScriptableRenderContext context)
        {
            if (CustomShadowMgr.Instance.rendererDataList.Count > 0)
            {
                var cmd = CommandBufferPool.Get();
                {
                    cmd.SetRenderTarget(CustomShadowMgr.Instance.CustomShadowMapStatic);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    foreach (var shadow in CustomShadowMgr.Instance.DictShadows)
                    {
                        var light = shadow.Key;
                        if (light == null) continue;

                        var shadowParams = shadow.Value;
                        foreach (var pam in shadowParams)
                        {
                            cmd.SetGlobalVector(ShadowConstantBuffer.id_ShadowBias, pam.ShadowBias);
                            cmd.SetGlobalVector(ShadowConstantBuffer.id_LightDirection, pam.LightDir);
                            cmd.SetViewport(pam.ShadowViewport_S);
                            cmd.SetGlobalMatrix(ShadowConstantBuffer.id_ShadowMatrix, pam.CasterShadowMatrix);
                            context.ExecuteCommandBuffer(cmd);

                            foreach (var group in CustomShadowMgr.Instance.rendererDataList)
                            {
                                if (group.mesh != null &&
                                    group.material != null &&
                                    group.matrix != null)
                                {
                                    cmd.DrawMesh(group.mesh, group.matrix, group.material, 0, 1);
                                    context.ExecuteCommandBuffer(cmd);
                                    cmd.Clear();
                                }
                            }
                            CustomShadowMgr.Instance.rendererDataList.Clear();
                        }
                    }
                }
                CommandBufferPool.Release(cmd);
            }
        }

        void Clear(ScriptableRenderContext context)
        {
            if(CustomShadowMgr.Instance.IsEmpty() && NeedResetOnEmpty)
            {
                CommandBuffer cmd = CommandBufferPool.Get(); 
                cmd.DisableShaderKeyword("_CUSTOM_SHADOW_ON");
                cmd.DisableShaderKeyword("_MAINLIGHT_CUSTOM_SHADOW_ON");
                cmd.DisableShaderKeyword("_ADDILIGHT_CUSTOM_SHADOW_ON");
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
                NeedResetOnEmpty = false;
            }
        }

        void RenderShadowmap(ref ScriptableRenderContext context, ref CullingResults cullResults, ref LightData lightData, ref ShadowData shadowData)
        {
            //
            var r = CustomShadowMgr.Instance.Build();
            // By Wuzhongjie 2023/10/31，
            // 阴影控件初始化设置Dirty后，LightData内容可能还是旧的，以前的做法是等几帧保证场景LightData刷新。
            // 测试发现这块逻辑耗时只有0.02~0.03ms（Mi8），可以尝试每帧更新灯光index
            //UnityEngine.Profiling.Profiler.BeginSample("RenderShadowmap.UpdateLightIndex");
            // TODO avoid new for every frame
            if (r)
                LightIndicesToShadowMgr = new float[CustomShadowMgr.MAX_SHADOW_COUNT] { 8.0f, 8.0f, 8.0f, 8.0f, 8.0f, 8.0f, 8.0f, 8.0f, 8.0f };

            if (LightIndicesToShadowMgr == null)
                return;

            HasMainLight = false;
            HasAddiLight = false;

            int Counter = 0;
            for (int i = 0; i < lightData.visibleLights.Length && i < CustomShadowMgr.MAX_SHADOW_COUNT; i++)
            {
                var _light = lightData.visibleLights[i].light;
                if (CustomShadowMgr.Instance.LightIndex.TryGetValue(_light, out int index))
                {
                    if (index != -1)
                    {
                        if (i == lightData.mainLightIndex)
                        {
                            HasMainLight = true;
                        }
                        else
                        {
                            HasAddiLight = true;
                        }
                        //var _param = ShadowMgr.DictShadows[_light];
                        //
                        LightIndicesToShadowMgr[Counter] = index;// + offset;
                        Counter++;
                        //LightIndicesToShadowMgr[i].z = _param.UV.z;
                        //LightIndicesToShadowMgr[i].w = _param.UV.w;
                    }
                    else
                    {
                        //Assertions.Assert.IsFalse(index == -1);
                        Debug.LogError("invalid light index");
                    }
                }
            }
            //UnityEngine.Profiling.Profiler.EndSample();
            // --By Wuzhongjie

            if (CustomShadowMgr.Instance._bUpdateShadowNodeMatrix)
            {
                CustomShadowMgr.Instance.UpdateShadowNodeMatrix();
                CustomShadowMgr.Instance.renderStaticLayer = true;
            }
            if (CustomShadowMgr.Instance._bUpdateShadowNodeMatrixD)
            {
                CustomShadowMgr.Instance.UpdateShadowNodeMatrixD();
            }
            //
            CommandBuffer cmd = CommandBufferPool.Get();   
            bool renderStatic = false;
            bool renderDynamic = false;
            //var a = new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.CustomShadowDynamic));
            if ( r || CustomShadowMgr.Instance.renderStaticLayer)
            {
                {
#if UNITY_EDITOR
                    Debug.Log("重新渲染静态ShadowMap");
#endif
                    
                    {
                        cmd.SetRenderTarget(CustomShadowMgr.Instance.CustomShadowMapStatic);
                        cmd.ClearRenderTarget(true, false, Color.black);
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                        foreach (var one_shadow in CustomShadowMgr.Instance.DictShadows)
                        {
                            var _light = one_shadow.Key;
                            if(_light==null)continue;

                            var _params = one_shadow.Value;
                            foreach (var one_param in _params)
                            {
                                if (null == one_param.ShadowCamera || 0 == (_light.cullingMask & one_param.staticLayer))
                                    continue;
                                if (one_param.FaceID != CubemapFace.Unknown)
                                {
                                    switch (one_param.FaceID)
                                    {
                                        case CubemapFace.PositiveX:
                                            one_param.ShadowCamera.transform.forward = Vector3.right;
                                            break;
                                        case CubemapFace.NegativeX:
                                            one_param.ShadowCamera.transform.forward = Vector3.left;
                                            break;
                                        case CubemapFace.PositiveY:
                                            one_param.ShadowCamera.transform.forward = Vector3.up;
                                            break;
                                        case CubemapFace.NegativeY:
                                            one_param.ShadowCamera.transform.forward = Vector3.down;
                                            break;
                                        case CubemapFace.PositiveZ:
                                            one_param.ShadowCamera.transform.forward = Vector3.forward;
                                            break;
                                        case CubemapFace.NegativeZ:
                                            one_param.ShadowCamera.transform.forward = Vector3.back;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                //if(one_param.CaptureStaticLayeEveryFrame || r)
                                {
                                    one_param.ShadowCamera.cullingMask = one_param.staticLayer;
                                    if (one_param.UseDynamicMatrix)
                                    {
                                        one_param.ShadowCamera.orthographicSize = one_param.StaticOrthSize;
                                        one_param.ShadowCamera.transform.position = one_param.StaticCameraPosition;
                                    }

                                    one_param.ShadowCullingParameters.cullingOptions = CullingOptions.ShadowCasters;
                                    one_param.ShadowCamera.TryGetCullingParameters(false,
                                        out one_param.ShadowCullingParameters);   

                                    var _rst = context.Cull(ref one_param.ShadowCullingParameters);
                                    //var _CurLight = one_shadow.Key;
                                    cmd.SetGlobalVector(ShadowConstantBuffer.id_ShadowBias, one_param.ShadowBias);
                                    cmd.SetGlobalVector(ShadowConstantBuffer.id_LightDirection, one_param.LightDir);
                                    //
                                    cmd.SetViewport(one_param.ShadowViewport_S);
                                    cmd.SetGlobalMatrix(ShadowConstantBuffer.id_ShadowMatrix,
                                        one_param.CasterShadowMatrix);
                                    context.ExecuteCommandBuffer(cmd);
                                    StaticFilterSettings.layerMask = one_param.staticLayer;
                                    context.DrawRenderers(_rst, ref StaticDrawingSettings, ref StaticFilterSettings);
                                    renderStatic = true;
                                    //one_param.CaptureStaticLayer = false;                                
                                }
                            }
                        }
                        cmd.Clear();
                    }
                    if (renderStatic == true)
                        cmd.DisableShaderKeyword("_DISABLE_STATIC_SHADOW");
                    else
                        cmd.EnableShaderKeyword("_DISABLE_STATIC_SHADOW");
                    context.ExecuteCommandBuffer(cmd);
                }
                CustomShadowMgr.Instance.renderStaticLayer = false;
                cmd.Clear();
                //context.ExecuteCommandBuffer(cmd);
            }

            //             cmd.SetRenderTarget(CustomShadowMgr.Instance.CustomShadowMapStatic);
            //             context.ExecuteCommandBuffer(cmd);
            //             cmd.Clear();

            // Currently there's an issue which results in mismatched markers.
            //using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.CustomShadowDynamic)))
            {
                cmd.SetRenderTarget(CustomShadowMgr.Instance.CustomShadowMapDynamic);
                cmd.ClearRenderTarget(true, false, Color.black);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                foreach (var one_shadow in CustomShadowMgr.Instance.DictShadows)
                {
                    //var _shadow = one_shadow.Key;
                    var _params = one_shadow.Value;
                    var _light = one_shadow.Key;
                    foreach (var one_param in _params)
                    {
                        if (null == one_param.ShadowCamera) 
                            continue;
                        if(one_param.FaceID != CubemapFace.Unknown)
                        {
                            switch(one_param.FaceID)
                            {
                            case CubemapFace.PositiveX:
                                    one_param.ShadowCamera.transform.forward = Vector3.right;
                                    break;
                            case CubemapFace.NegativeX:
                                    one_param.ShadowCamera.transform.forward = Vector3.left;
                                    break;
                            case CubemapFace.PositiveY:
                                    one_param.ShadowCamera.transform.forward = Vector3.up;
                                    break;
                            case CubemapFace.NegativeY:
                                    one_param.ShadowCamera.transform.forward = Vector3.down;
                                    break;
                            case CubemapFace.PositiveZ:
                                    one_param.ShadowCamera.transform.forward = Vector3.forward;
                                    break;
                            case CubemapFace.NegativeZ:
                                    one_param.ShadowCamera.transform.forward = Vector3.back;
                                    break;
                            default:
                                break;
                            }
                        }
                        CullingResults _shadowCullResults;
                        
                        one_param.ShadowCamera.cullingMask = one_param.dynamicLayer;
if (one_param.UseDynamicMatrix){
                        one_param.ShadowCamera.orthographicSize = one_param.DynamicOrthSize;
                        one_param.ShadowCamera.transform.position = one_param.DynamicCameraPosition;
}
                        one_param.ShadowCullingParameters.cullingOptions = CullingOptions.ShadowCasters;
                        one_param.ShadowCamera.TryGetCullingParameters(false, out one_param.ShadowCullingParameters);
                        _shadowCullResults = context.Cull(ref one_param.ShadowCullingParameters);
#if UNITY_EDITOR
                        //为了实时查看裁剪窗口
                       
                        one_param.ShadowCamera.cullingMask = one_param.staticLayer;
if(one_param.UseDynamicMatrix){
                        one_param.ShadowCamera.orthographicSize = one_param.StaticOrthSize;
                        one_param.ShadowCamera.transform.position = one_param.StaticCameraPosition;
}
                    
#endif
                        
                        //var _CurLight = one_shadow.Key;

                        cmd.SetGlobalVector(ShadowConstantBuffer.id_ShadowBias,one_param.DynamicShadowBias);
                        //
                        cmd.SetGlobalVector(ShadowConstantBuffer.id_LightDirection, one_param.LightDir);
                        cmd.SetViewport(one_param.ShadowViewport_D);
                        cmd.SetGlobalMatrix(ShadowConstantBuffer.id_ShadowMatrix, one_param.UseDynamicMatrix ? one_param.CasterShadowMatrix_D : one_param.CasterShadowMatrix);
                        context.ExecuteCommandBuffer(cmd);
                        DynamicFilterSettings.layerMask = one_param.dynamicLayer;
                        context.DrawRenderers(_shadowCullResults, ref DynamicDrawingSettings, ref DynamicFilterSettings);
                        renderDynamic = true;
                    }
                }

                if (!ScreenSpaceShadow)
                {
                    cmd.SetGlobalMatrixArray(ShadowConstantBuffer.id_CustomShadowMatrices, CustomShadowMgr.Instance.CustomMatrixToShadow);// new Matrix4x4[1] { ShadowMatrix });
                    cmd.SetGlobalMatrixArray(ShadowConstantBuffer.id_CustomShadowMatricesDynamic, CustomShadowMgr.Instance.CustomMatrixToShadowDynamic);// new Matrix4x4[1] { ShadowMatrix });
                    cmd.SetGlobalFloatArray(ShadowConstantBuffer.id_LightIndexMap, LightIndicesToShadowMgr);// new Matrix4x4[1] { ShadowMatrix });
                    cmd.SetGlobalVectorArray(ShadowConstantBuffer.id_CustomShadowUVRects, CustomShadowMgr.Instance.StaticShadowUVRects);// new Matrix4x4[1] { ShadowMatrix });
                    cmd.SetGlobalTexture(ShadowConstantBuffer.id_StaticSMRT, CustomShadowMgr.Instance.CustomShadowMapStatic);
                    cmd.SetGlobalTexture(ShadowConstantBuffer.id_DynamicSMRT, CustomShadowMgr.Instance.CustomShadowMapDynamic);
                    cmd.SetGlobalFloatArray(ShadowConstantBuffer.id_CustomLightType, CustomShadowMgr.Instance.CustomLightType);
                    cmd.SetGlobalVectorArray(ShadowConstantBuffer.id_LightExtraParam, CustomShadowMgr.Instance.CustomLightExtraParam);
                    cmd.SetGlobalVector(ShadowConstantBuffer.id_ShadowDynamicPCFSize, CustomShadowMgr.Instance.ShadowDynamicPCFSize);
                    //
                    if (false == HasMainLight) 
                        cmd.DisableShaderKeyword("_MAINLIGHT_CUSTOM_SHADOW_ON");
                    else
                        cmd.EnableShaderKeyword("_MAINLIGHT_CUSTOM_SHADOW_ON");
                    //
                    if(false == HasAddiLight)
                        cmd.DisableShaderKeyword("_ADDILIGHT_CUSTOM_SHADOW_ON");
                    else
                        cmd.EnableShaderKeyword("_ADDILIGHT_CUSTOM_SHADOW_ON");
                    //
                    cmd.EnableShaderKeyword("_CUSTOM_SHADOW_ON");

                    if (renderDynamic == true)
                        cmd.DisableShaderKeyword("_DISABLE_DYNAMIC_SHADOW");
                    else
                        cmd.EnableShaderKeyword("_DISABLE_DYNAMIC_SHADOW");
                }
                
                context.ExecuteCommandBuffer(cmd);
            }
            CommandBufferPool.Release(cmd); 

            NeedResetOnEmpty = true;
        }
    };
}
