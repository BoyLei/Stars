using StarProjectDef;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

internal class ShaderVariantLoading
{
    static ShaderVariantLoading _instance = null;
    public static ShaderVariantLoading Instance 
    {
        get { 
            if (_instance == null) 
                _instance = new ShaderVariantLoading(); 
            return _instance;
        }
    }

    public bool IsLoad;

    public void Init()
    {
        Reset();
        CanPreLoad();
    }

    private void Reset()
    {
        IsLoad = false;
    }

    public bool CanPreLoad()
    {
        if (!IsLoad)
        {
            Prepare();
            return true;
        }

        return false;
    }

    private void Prepare()
    {
        // loading preloading asset
        LoaidngPreLoadingAssets((string assetPath, UnityEngine.Object @object) =>
        {
            if (@object != null)
            {
                ShaderVariantWarmUp(@object);
            }
        });
    }

    private void LoaidngPreLoadingAssets(Action<String, UnityEngine.Object> itemLoadCb)
    {
        var qualityLevel = (int)GameConfig.MachineQualityLevel;
        string shaderVarAssetPath = string.Format("Shader/ShaderVariant/shadervariants_{0}", qualityLevel);
        StarProject.Service.Resource.ResourceFormalManager.Instance.LoadResourceUniRefAsync<UnityEngine.Object>(shaderVarAssetPath,
        (UnityEngine.Object @object) =>
        {
            if (@object == null)
            {
                SGF.Debuger.LogWarning($"@object ²»´æÔÚ Â·¾¶: {shaderVarAssetPath}");
                return;
            }
            itemLoadCb?.Invoke(shaderVarAssetPath, @object);
        });
    }

    private void ShaderVariantWarmUp(UnityEngine.Object @object)
    {
        Debug.Log("ShaderVariantWarmUp:>" + @object);
        try
        {
            var coll = @object as ShaderVariantCollection;
            coll?.WarmUp();

            Addressables.Release(@object);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    internal void Loading(Action<string, float> process, Action complete)
    {
        int totalCount = 1;
        int finishCount = 0;
        if (totalCount > 0)
        {
            // loading preloading asset
            LoaidngPreLoadingAssets((string assetPath, UnityEngine.Object @object) =>
            {
                if (@object != null)
                {
                    ShaderVariantWarmUp(@object);
                }
                finishCount++;
                RefreshLoaidng(finishCount, totalCount, assetPath, process, complete);
            });
        }
        else
        {
            complete?.Invoke();
            IsLoad = true;
        }
    }

    private void RefreshLoaidng(int finishCount, int totalCount, string assetPath, Action<string, float> process, Action complete)
    {
        float p = finishCount * 1.0f / totalCount;
        // SGF.Debuger.Log($"{finishCount}_{totalCount}");
        process?.Invoke(assetPath, p);
        if (finishCount == totalCount)
        {
            complete?.Invoke();
            IsLoad = true;
        }

    }
}
