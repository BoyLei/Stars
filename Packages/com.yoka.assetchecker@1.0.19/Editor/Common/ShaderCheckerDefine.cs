using AssetChecker.Define;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderCheckerDefine : ScriptableObject
{
    public string SVNUser = "";
    public string SVNPass = "";
    public string maliGPUCompiler = "";
    public bool enableShaderStrip = true;
    public MaliGpuInfo maliGpuInfo = new MaliGpuInfo() { _type = MaliGPUType.MaliG51, _complexity = 35 };
    public List<string> shaderPaths = new List<string>() { "Assets/Arts/Shaders" };
    public List<string> shaderFuncs = new List<string>() {
            "clamp",
            "pow",
            "tex2D",

            "SAMPLE_TEXTURE2D",
            "SAMPLE_TEXTURE2D_LOD",
            //"TEXTURE2D",
            "TEXTURE2D_PARAM",
            "TEXTURE2D_ARGS",
            "TEXTURE2D_HALF",
            "TEXTURE2D_FLOAT",
            "LOAD_TEXTURE2D",
            "LOAD_TEXTURE2D_LOD",
            "GATHER_TEXTURE2D",
            "GATHER_RED_TEXTURE2D",
            "GATHER_GREEN_TEXTURE2D",
            "GATHER_BLUE_TEXTURE2D",

            "SAMPLE_TEXTURE3D",

            "SAMPLE_TEXTURE2D_ARRAY",
            "SAMPLE_TEXTURE2D_ARRAY_LOD",
            "TEXTURE2D_ARRAY",
            "TEXTURE2D_ARRAY_PARAM",
            "TEXTURE2D_ARRAY_ARGS",
            "TEXTURE2D_ARRAY_HALF",
            "TEXTURE2D_ARRAY_FLOAT",
            "LOAD_TEXTURE2D_ARRAY",
            "LOAD_TEXTURE2D_ARRAY_LOD",
            "GATHER_TEXTURE2D_ARRAY",
            "GATHER_RED_TEXTURE2D",
            "GATHER_GREEN_TEXTURE2D",
            "GATHER_BLUE_TEXTURE2D",

            "SampleNormal",
            "SAMPLE_GI"
        };
    public List<string> compiledShaderFuncs = new List<string>() {
            "texelFetch",
            "texelFetchOffset",
            "texture",
            "textureGather",
            "textureGatherOffset",
            "textureGrad",
            "textureGradOffset",
            "textureLod",
            "textureLodOffset",
            "textureOffset",
            "textureProj",
            "textureProjGrad",
            "textureProjGradOffset",
            "textureProjLod",
            "textureProjLodOffset",
            "textureProjOffset",
            "textureSize",
        };
    public List<string> excludeShaderKeywords = new List<string>() {
            "_MAIN_LIGHT_SHADOWS",
            "_ADDITIONAL_LIGHT_SHADOWS",
            "_MAIN_LIGHT_SHADOWS_CASCADE",
            "_SCREEN_SPACE_OCCLUSION",
            "_SHADOWS_SOFT",
        };

    public List<string> includeShaderKeywords = new List<string>() {
        };

    public string[] IgnoreShaders = new string[] { "" };
    public string[] IgnoreShaderBatchs = new string[] { "AVProVideo", "Debug View", "TextMeshPro", "BatchFewStandard", "Hidden", "Universal Render Pipeline" };

    public string[] ShaderStripWhitelist = new string[] { "" };
}
