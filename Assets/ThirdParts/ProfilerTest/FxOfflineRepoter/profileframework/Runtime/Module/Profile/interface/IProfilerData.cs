// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/2/21 16:5:20)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Yoka.Galaxy.Profile
{
    /// <summary>
    /// �������ݣ��������Ʋ���
    /// </summary>
    public interface IProfilerInputData
    {

    }


    /// <summary>
    /// д����Խ��
    /// </summary>
    public interface IProfilerOutputData
    {
        void Begin(string path);

        void Write(string content);

        void End();
    }

    public enum FormatterCharacter
    {
        None = 0,
        Child,
        Sibling,
        Count,
    }

    /// <summary>
    /// ��ʽ������
    /// </summary>
    public interface IProfilerOutputFormatter
    {
        void Begin(IProfilerOutputData outputData, IEnumerable<IProfilerSampler> samplers);

        void Tag(string tag);

        void TakeSample(IProfilerOutputData outputData, IEnumerable<IProfilerSampler> samplers, ProfilerUseCase rootCase, int frame);

        void End(IProfilerOutputData outputData);

        string GetCharacter(FormatterCharacter character, string inputString);
    }

}
#endif