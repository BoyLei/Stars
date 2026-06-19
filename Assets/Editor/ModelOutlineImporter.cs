// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using Unity.Collections;
// using Unity.Jobs;
// using System.IO;
// using System.Linq;
// using Unity.Collections.LowLevel.Unsafe;
// 
// public class ModelOutlineImporter : AssetPostprocessor
// {
//     // 在模型导入前调用
//     void OnPreprocessModel()
//     {
//         
//     }
//    
//  
// 
//    
//  
//     // Color[] ComputeSmoothedNormal(Mesh smoothedMesh, Mesh originalMesh, int maxOverlapvertices = 10)
//     // {
//     //     Vector3[] normals=new Vector3[originalMesh.vertexCount];
//     //     Vector4[] tangents=new Vector4[originalMesh.vertexCount];
//     //     Color[] colors = new Color[originalMesh.vertexCount];
//     //     //Vector3[] smoothedNormals ;
//     //     //将SmoothMesh模型的顶点和法线数据加入Dictionary
//     //     Dictionary<Vector3, Vector3> smoothVerNorDictionary = new Dictionary<Vector3, Vector3>();
//     //     for (int i = 0; i < smoothedMesh.vertexCount; i++)
//     //     {
//     //         if (smoothVerNorDictionary.ContainsKey(smoothedMesh.vertices[i]))
//     //         {
//     //             Vector3 v3T = smoothVerNorDictionary[smoothedMesh.vertices[i]];
//     //             v3T = v3T + smoothedMesh.normals[i];
//     //             smoothVerNorDictionary.Remove(smoothedMesh.vertices[i]);
//     //             smoothVerNorDictionary.Add(smoothedMesh.vertices[i],v3T);
//     //             //Debug.Log("smoothVerNorDictionary add"+smoothedMesh.vertices[i] + v3T);
//     //         }
//     //         else
//     //         {
//     //             smoothVerNorDictionary.Add(smoothedMesh.vertices[i],smoothedMesh.normals[i]);
//     //             //Debug.Log("smoothVerNorDictionary add"+smoothedMesh.vertices[i] + smoothedMesh.normals[i]);
//     //         }
//     //         
//     //     }
//     //     /*for (int i = 0; i < result.Length; i++)
//     //     {
//     //         if (result[i].normal != Vector3.zero)
//     //             smoothedNormals += result[i].normal;
//     //         else
//     //             break;
//     //     }*/
//     //     //smoothedNormals = smoothedNormals.normalized;
//     //     //smoothedNormals = smoothedMesh.normals;
//     //     normals = originalMesh.normals;
//     //     tangents = originalMesh.tangents;
//     //     for (int index = 0; index < originalMesh.vertexCount; index++)
//     //     {
//     //         //对于OriginalMesh内的每一个顶点 查找SmoothMesh内对应的顶点的法线
//     //         if (smoothVerNorDictionary.ContainsKey(originalMesh.vertices[index]))
//     //         {
//     //             //Debug.Log("查找到顶点:"+index+"的位置为:"+originalMesh.vertices[index]);
//     //             Vector3 smoothNormal = smoothVerNorDictionary[originalMesh.vertices[index]];
//     //                         smoothNormal = smoothNormal.normalized;
//     //                         var binormal = (Vector3.Cross(normals[index], tangents[index]) * tangents[index].w).normalized;
//     //                         
//     //                         var tbn = new Matrix4x4(
//     //                             tangents[index],
//     //                             binormal,
//     //                             normals[index],
//     //                             Vector4.zero);
//     //                         tbn = tbn.transpose;
//     //                 
//     //                         var bakedNormal = tbn.MultiplyVector(/*smoothedNormals[index]*/smoothNormal).normalized;
//     //                 
//     //                         Color color = new Color();
//     //                         color.r = (bakedNormal.x * 0.5f) + 0.5f;
//     //                         color.g = (bakedNormal.y * 0.5f) + 0.5f;
//     //                         color.b = colors[index].b;
//     //                         color.a = colors[index].a;
//     //                 
//     //                         colors[index] = color;
//     //         }
//     //         else
//     //         {
//     //              //11Debug.Log("无法找2到顶点:"+index+"的位置为:"+originalMesh.vertices[index]);
//     //         }
//     //     }
//     //   
//     //     /*Debug.Log("平滑模型顶点数为："+smoothedMesh.vertexCount);
//     //     Debug.Log("原模型顶点数为："+originalMesh.vertexCount);*/
//     //     return colors;
//     // }
//     
// }