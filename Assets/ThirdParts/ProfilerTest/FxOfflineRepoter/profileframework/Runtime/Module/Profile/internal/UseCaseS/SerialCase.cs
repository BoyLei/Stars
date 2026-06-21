// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/7 15:15:5)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Runtime.CompilerServices;
using System;


namespace Yoka.Galaxy.Profile.Core
{
    public class SerialCase : ProfilerUseCase
    {
        [VerticalGroup("Detail")]
        [InfoBox("��������������������������" + UseCaseSetting.LIST_EDITOR_USAGE)]
        //[ShowIf("active"), ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, CustomAddFunction = "AddCase", CustomRemoveIndexFunction = "RemoveCase", HideAddButton = true), InlineButton("Add"), TableList(ShowIndexLabels = false, AlwaysExpanded = true, HideAddButton = true, HideRemoveButton = true, CellPadding = 8, ShowPaging = true)]
        public List<ProfilerUseCase> serialCases;

        private ProfilerUseCase _runningChildCase = null;

        public UnityEngine.Object AddCase()
        {
            Debug.Log("AddCase triggered");

            return null;
        }

        public UnityEngine.Object Add()
        {
            Debug.Log("ParallelCase AddCase triggered");
            UseCaseSetting.Fetch().AddToCustom((instance) =>
            {
                serialCases.Add(instance);
            });
            return null;
        }

        public void RemoveCase(int index)
        {
            ProfilerUseCase useCase = serialCases[index];
            serialCases.RemoveAt(index);
            UseCaseSetting.Fetch().RemoveFromCustom(useCase);
        }


        public override UniTask Run()
        {
            UniTask all = UniTask.RunOnThreadPool(async () =>
            {
                await UniTask.SwitchToMainThread();
                foreach (var c in serialCases)
                {
                    if(c.active)
                    {
                        _runningChildCase = c;
                        try
                        {
                            await c.Run();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                        _runningChildCase = null;
                    }
                }
            });
            return all;
        }

        public override string ReportStatus(IProfilerOutputFormatter formatter)
        {
            if(_runningChildCase!=null)
            {
                return base.ReportStatus(formatter) + formatter.GetCharacter(FormatterCharacter.Child, string.Empty) + _runningChildCase.ReportStatus(formatter);
            }
            return base.ReportStatus(formatter);
        }

        public override void Sync(HashSet<ProfilerUseCase> usingCases)
        {
            base.Sync(usingCases);
            foreach(var child in serialCases)
            {
                child.Sync(usingCases);
            }
        }
    }

}
#endif