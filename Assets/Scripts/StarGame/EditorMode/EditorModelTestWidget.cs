using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using StarProject;
using StarProject.Service.LocalData;
using StarProjectDef;
using UnityEngine;
using UnityEngine.UI;

namespace SGF.UI.Framework
{
    public class EditorModelTestWidget : UIWidget
    {
        public InputField inputField;

        public Button button;

        public Dropdown dropdown;

        public long curMonsterID;

        /// <summary>
        /// 创建木桩怪的 输入框
        /// </summary>
        public InputField monsterInputField;
        /// <summary>
        /// 创建 木桩怪物的 按钮
        /// </summary>
        public Button createMonster;
        /// <summary>
        /// 需要创建的 怪物的 id
        /// </summary>
        public int createMonsterID = 1;

        protected override void Awake()
        {
            var list = new Dropdown.OptionDataList();
            List<long> idList = new();
            LocalDataManager.Instance.M_MonsterData.StaticMonsterDatas.ForEach((item) =>
            {
                long monsterId = item.Key;
                string monsterName = item.Value.Name;

                list.options.Add(new Dropdown.OptionData($"{monsterName}_{monsterId}"));
                idList.Add(monsterId);
            });

            dropdown.AddOptions(list.options);

            dropdown.onValueChanged.AddListener((int idx) =>
            {
                curMonsterID = idList[idx];
                inputField.text = curMonsterID.ToString();

            });

            inputField.onValueChanged.AddListener((string v) =>
            {
                if (string.IsNullOrWhiteSpace(v))
                {
                    return;
                }
                if (long.TryParse(v, out long id))
                {
                    curMonsterID = id;
                }
                else
                {
                    curMonsterID = 0;
                }
            });


            button.onClick.AddListener(() =>
            {
                if (curMonsterID == 0) return;
                EditorModeTest.EditorMode.Instance.localServer.SwitchMonster(curMonsterID);
            });

            monsterInputField.onValueChanged.AddListener((string v) =>
            {
                if (string.IsNullOrWhiteSpace(v))
                {
                    return;
                }
                if (int.TryParse(v, out int id))
                {
                    createMonsterID = id;
                }

            });


            createMonster.onClick.AddListener(() =>
            {
                if (createMonsterID == 0) return;
                GlobalEvent.OnClientReqLocalServerEvent.Invoke(ClientEventReq.CreateMonster, new object[] { createMonsterID });

            });
        }

    }
}
