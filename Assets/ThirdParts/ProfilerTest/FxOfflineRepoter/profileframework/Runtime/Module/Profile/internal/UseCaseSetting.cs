// -----------------------------------------------------------------------
// This file is part of  
//
// (c) ninglei   (2023/3/1 10:39:57)
// 
// For the full copyright and license information, please view the LICENSE
// file that was distributed with this source code.
// -----------------------------------------------------------------------
#if EFFECT_PROFILER
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Yoka.Galaxy.Configure;
using System.Reflection;
using System;


namespace Yoka.Galaxy.Profile
{
    public class UseCaseSetting : ProfileBaseSettings<UseCaseSetting>
    {
        public const string LIST_EDITOR_USAGE = "��Ҫ����˳�����ɾ����Ԫ�أ����л����б�ģʽ��";

        [Space]
        [PropertyOrder(1)]
        [InfoBox("������Ҫ���ӵ������࣬ȫ�ֹ���")]
        [ValueDropdown("_useCaseClasses")/*, InlineButton("Add")*/]
        public string addClass;

        [Space]
        [InfoBox("���������б�������ִ�С�"+ LIST_EDITOR_USAGE)]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, CustomAddFunction = "AddCase", CustomRemoveIndexFunction= "RemoveCase", HideAddButton = true, OnBeginListElementGUI = "BeginDrawListElement", OnEndListElementGUI = "EndDrawListElement"), InlineButton("Add"), TableList(ShowIndexLabels = true, AlwaysExpanded = true, /*HideAddButton = true, HideRemoveButton = true,*/ CellPadding=10, ShowPaging=true)]
        [PropertyOrder(2)]
        public List<ProfilerUseCase> cases;

        [InfoBox("ˢ����Ϸ��Դ��ɾ������������ͬ���������֣���������")]
        [Button, PropertyOrder(0)]
        void Sync()
        {
            HashSet<ProfilerUseCase> usingCases = new HashSet<ProfilerUseCase>();
            foreach (var root in cases)
            {
                root.Sync(usingCases);
            }
            OnCaseSynced?.Invoke(usingCases);
        }

        private void BeginDrawListElement(int index)
        {
            //SirenixEditorGUI.BeginBox(this.InjectListElementGUI[index].SomeString);
        }
        private void EndDrawListElement(int index)
        {
            //SirenixEditorGUI.EndBox();
        }

        public UnityEngine.Object AddCase()
        {
            Debug.Log("AddCase triggered");

            return null;
        }

        public void RemoveCase(int index)
        {
            ProfilerUseCase useCase = cases[index];
            cases.RemoveAt(index);
            OnCaseRemoved(useCase);
        }

        public static void InitializeFromEditor(Action<ScriptableObject> onCaseAdded, Action<ScriptableObject> onCaseRemoved, Action<HashSet<ProfilerUseCase>> onCaseSynced)
        {
            Debug.Log("DEBUG!! InitializeFromEditor");
            var stringList = new ValueDropdownList<string>()
            {

            };
            _classNameToTypeDict = new Dictionary<string, Type>();
            List<Assembly> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());
            Assembly assembly = Assembly.GetExecutingAssembly();
            assemblies.Remove(assembly);
            assemblies.Insert(0, assembly);
            for (int i = 0; i < assemblies.Count; i++)
            {
                Type[] types = assemblies[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    Type type = types[j];
                    if(type.IsAbstract)
                    {
                        continue;
                    }
                    if (!type.IsSubclassOf(typeof(ProfilerUseCase)))
                    {
                        continue;
                    }
                    string convertName = type.FullName.Replace(".", "/");
                    stringList.Add(convertName, type.FullName);
                    Debug.Log("DEBUG!! type match: " + type.FullName);
                    _classNameToTypeDict.Add(type.FullName, type);
                }
            }
            _useCaseClasses = stringList;
            Debug.Log("DEBUG!! UseCaseClasses assigned");

            OnCaseAdded = onCaseAdded;
            Debug.Log("DEBUG!! OnCaseAdded assigned");

            OnCaseRemoved = onCaseRemoved;
            Debug.Log("DEBUG!! OnCaseRemoved assigned");

            OnCaseSynced = onCaseSynced;
            Debug.Log("DEBUG!! OnCaseSynced assigned");
        }

        private static IEnumerable _useCaseClasses = null;

        private static Dictionary<string, Type> _classNameToTypeDict = null;

        private static Action<ScriptableObject> OnCaseAdded = null;

        private static Action<ScriptableObject> OnCaseRemoved = null;

        private static Action<HashSet<ProfilerUseCase>> OnCaseSynced = null;

        private void AddImp(Action<ProfilerUseCase> callback)
        {
            if (_classNameToTypeDict.TryGetValue(addClass, out var foundType))
            {
                var instance = ScriptableObject.CreateInstance(foundType) as ProfilerUseCase;
                instance.name = foundType.Name;
                OnCaseAdded(instance);
                callback?.Invoke(instance);
            }
            else
            {
                Debug.LogErrorFormat("Cannot find class {0}", addClass);
            }
        }

        private void Add()
        {
            AddImp((instance) =>
            {
                cases.Add(instance);
            });
        }

        public void AddToCustom(Action<ProfilerUseCase> callback)
        {
            AddImp(callback);
        }    

        public void RemoveFromCustom(ProfilerUseCase useCase)
        {
            OnCaseRemoved(useCase);
        }
    }

}

#endif