// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/3 16:41:2)
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
using System.Text;
using System;

namespace Yoka.Galaxy.Profile.Core
{
    public class ParallelCase : ProfilerUseCase
    {
        [VerticalGroup("Detail")]
        [InfoBox("����������������ͬʱ������" + UseCaseSetting.LIST_EDITOR_USAGE)]
        //[ShowIf("active"), ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, CustomAddFunction = "AddCase", CustomRemoveIndexFunction = "RemoveCase", HideAddButton=true), InlineButton("Add"), TableList(ShowIndexLabels = false, AlwaysExpanded = true, HideAddButton = true, HideRemoveButton = true, CellPadding = 8, ShowPaging = true)]
        public List<ProfilerUseCase> parallelCases;

        private List<ProfilerUseCase> _runningChildCases = null;
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
                parallelCases.Add(instance);
            });
            return null;
        }

        public void RemoveCase(int index)
        {
            ProfilerUseCase useCase = parallelCases[index];
            parallelCases.RemoveAt(index);
            UseCaseSetting.Fetch().RemoveFromCustom(useCase);
        }


        public override UniTask Run()
        {
            UniTask all = UniTask.RunOnThreadPool(async () =>
            {
                _runningChildCases = new List<ProfilerUseCase>();
                await UniTask.SwitchToMainThread();
                List<UniTask> tasks = new List<UniTask>();
                foreach (var p in parallelCases)
                {
                    if(p.active)
                    {
                        try
                        {
                            var t = p.Run();
                            tasks.Add(t);
                        }
                        catch(Exception ex)
                        { 
                            Debug.LogException(ex);
                        }
                        _runningChildCases.Add(p);
                    }
                }
                foreach (var st in tasks)
                {
                    await st;
                }
                _runningChildCases.Clear();
                _runningChildCases = null;
            });
            return all;
        }

        public override string ReportStatus(IProfilerOutputFormatter formatter)
        {
            if(_runningChildCases!=null)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var task in _runningChildCases)
                {
                    if(sb.Length > 0)
                    {
                        sb.Append(formatter.GetCharacter(FormatterCharacter.Sibling, string.Empty));
                    }
                    sb.Append(task.ReportStatus(formatter));
                }
                return base.ReportStatus(formatter) + formatter.GetCharacter(FormatterCharacter.Child, string.Empty) + sb.ToString();
            }
            return base.ReportStatus(formatter);
        }

        public override void Sync(HashSet<ProfilerUseCase> usingCases)
        {
            base.Sync(usingCases);
            foreach(var child in parallelCases)
            {
                child.Sync(usingCases);
            }
        }
    }

}
#endif