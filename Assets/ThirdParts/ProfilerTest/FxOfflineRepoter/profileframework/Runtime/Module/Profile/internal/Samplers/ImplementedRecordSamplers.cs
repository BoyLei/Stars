// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/8 16:54:53)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// Reference:   https://gist.github.com/anarkila/f0c34e4daaac4c48b575eac04ad86064
//              https://lab.uwa4d.com/lab/62008c6ba8103dabd014777d
//              https://docs.unity3d.com/cn/2021.1/Manual/ProfilerCPU.html
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;


namespace Yoka.Galaxy.Profile
{
    public class PhysicsFixedUpdateSamplers : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "FixedUpdate.PhysicsFixedUpdate", "PhysicsFixedUpdate(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }


#if UNITY_2022_1_OR_NEWER
    public class GPUFrameSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "GPU Frame Time", "(Unity2022)GPU(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class CPUFrameTotalSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "CPU Total Frame Time", "(Unity2022)CPU Total(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }
#endif

    public class BehaviourUpdateSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "BehaviourUpdate", "Update(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class BehaviourLateUpdateSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "LateBehaviourUpdate", "LateUpdate(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class BehaviourFixedUpdateSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "FixedBehaviourUpdate", "FixedUpdate(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class GlobalIlluminationSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Internal, "Global Illumination", "Global Illumination(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class BatchesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(false, ProfilerCategory.Render, "Batches Count", "Batches", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }

    public class DrawCallSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(false, ProfilerCategory.Render, "Draw Calls Count", "Draw Call", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }

    public class SetPassCallSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(false, ProfilerCategory.Render, "SetPass Calls Count", "SetPass Call", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }

    public class TriangleSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(false, ProfilerCategory.Render, "Triangles Count", "Triangle(k)", RecorderValueType.Long, RecorderValueUnit.K);
        }
    }

    public class ParticleCountSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Particles, "ParticleSystem.Update", "ParticleSystem.Update(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class ParticleDrawSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Particles, "ParticleSystem.Draw", "ParticleSystem.Draw(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class ParticleJobSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Particles, "ParticleSystem.ScheduleGeometryJobs", "ParticleSystem.ScheduleGeometryJobs(ms)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class ShadowCastersSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Render, "Shadow Casters Count", "shadow casters", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }

    public class SkinnedMeshsSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Render, "Visible Skinned Meshes Count", "skinned meshes", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }

    /*
    public class VertexLoadSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Render, "Vertex Buffer Upload In Frame Count", "vertex load(k)", RecorderValueType.Long, RecorderValueUnit.K);
        }
    }
    */

    public class UsedBuffersCountSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Render, "Used Buffers Count", "buffer count", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }

    public class UsedBuffersBytesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Render, "Used Buffers Bytes", "buffer bytes(MB)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class RenderTextureBytesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Memory, "Render Textures Bytes", "render texture memory(MB)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class TotalUsedMemoryBytesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Memory, "Total Used Memory", "total used memory(MB)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class TextureMemoryBytesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Memory, "Texture Memory", "texture memory(MB)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class MeshMemoryBytesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Memory, "Mesh Memory", "mesh memory(MB)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class MaterialMemoryBytesSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Memory, "Material Memory", "material memory(MB)", RecorderValueType.Long, RecorderValueUnit.M);
        }
    }

    public class ObjectCountSampler : RecorderSampler
    {
        protected override void DoInit()
        {
            InitImp(true, ProfilerCategory.Memory, "Object Count", "object count", RecorderValueType.Long, RecorderValueUnit.One);
        }
    }
}
#endif