//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using System.Text.RegularExpressions;
using UnityEditor;

/// <summary>
/// Component that manages a single combined mesh. 
/// 
/// This class is a Component. It must be added to a GameObject to use it. It is a wrapper for MB2_MeshCombiner which contains the same functionality but is not a component
/// so it can be instantiated like a normal class.
/// </summary>
public class MB3_MeshBaker : MB3_MeshBakerCommon
{

    [SerializeField] protected MB3_MeshCombinerSingle _meshCombiner = new MB3_MeshCombinerSingle();

    public override MB3_MeshCombiner meshCombiner
    {
        get { return _meshCombiner; }
    }

    public void BuildSceneMeshObject()
    {
        _meshCombiner.BuildSceneMeshObject();
    }

    public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs)
    {
        return _meshCombiner.ShowHideGameObjects(gos, deleteGOs);
    }

    public virtual void ApplyShowHide()
    {
        _meshCombiner.ApplyShowHide();
    }

    public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
    {
        //		if ((_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoSceneObject || (_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoPrefab && _meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer) )) BuildSceneMeshObject();
        _meshCombiner.name = name + "-mesh";
        return _meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
    }

    public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
    {
        //		if ((_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoSceneObject || (_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoPrefab && _meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer) )) BuildSceneMeshObject();
        _meshCombiner.name = name + "-mesh";
        return _meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOinstanceIDs, disableRendererInSource);
    }

    public void InitCubeInfo(Bounds sourceObjectBounds, GameObject obj, Vector3 pos, Vector3 scale, Vector3 org, List<Renderer> gaws)
    {
        if (transform.childCount == 0)
        {
            var newObj = GameObject.Instantiate(obj);
            newObj.transform.parent = transform;

            if(gaws != null && gaws.Count > 0)
            {
                newObj.name = gaws[0].sharedMaterials[0].name;
            }
        }

        var trans = transform.GetChild(0);
        trans.position = pos + scale/2 + org;
        trans.localScale = scale;
    }
}
