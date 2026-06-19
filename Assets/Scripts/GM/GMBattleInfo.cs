using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarProjectDef;
using UnityEngine.UI;
using System;
using StarProject.Service.Battle;
using StarProject.Game.Entity.VitalSigns;
using StarProject.Game;
using StarProject.Service.Input;
using StarProject.Service.Cam;
using StarProject.Game.Entity.View.VitalSign;
using StarProject.Service.LocalData;
using SkillEditor;
using StarProject.Service.Business;

public class GMBattleInfo : MonoBehaviour
{
    public Text nameText;       //名称
    public Text idText;         //guid
    public Text targetText;     //攻击对象
    public Text attrInfoText;   //属性

    public Text recordItem;     //记录

    public Text buffText;       //buff

    public static GMBattleInfo Instance;

    List<int> recordSkills = new List<int>();

    List<GameObject> allRecords = new List<GameObject>();
    ulong curSelectEntityID = 0;
    NPCEntityBase curTarget = null;
    bool startRecord = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InputManager.Instance.RegisterClick(OnTapEventHandler);
        curSelectEntityID = GameManager.Instance.mainPlayerId;
    }

    void Update()
    {
        string strBuff = "";
        if (curSelectEntityID == 0)
        {
            curSelectEntityID = GameManager.Instance.mainPlayerId;
        }
        var entityBase = GameManager.Instance.GetEntityByEntityID(curSelectEntityID);
        if (entityBase != null)
        {
            nameText.text = "名称:" + entityBase.M_Name;
            idText.text = "EntityId:" + entityBase.EntityId.ToString();
            if (curTarget == null)
            {
                targetText.text = "攻击对象:无";
            }
            else
            {
                string str = GameManager.Instance.GetEntityNameById(curTarget.EntityId);
                targetText.text = "攻击对象:" + curTarget.EntityId.ToString();
            }

            var atk = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Atk); //3001
            var defence = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Defence); //3002
            var mdefence = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.MDefence); //3003
            var hp = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.curHp); //3004
            var exDodge = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.ExDodge); //3018
            var exCri = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.ExCri); //3019
            var speed = entityBase.Data.Attrs.GetAoiValue<long>(EnumAOIType.Long, AOIAttrDefine.Speed); //3026

            string str2 = $"攻击:{atk}\n防御:{defence}\n魔法防御:{mdefence}\n血量:{hp}\n额外闪避率:{exDodge}\n额外暴击率:{exCri}\n移动速度:{speed}\n";
            
            if(entityBase.EntityType == E_EntityType.Monster || entityBase.EntityType == E_EntityType.Robot)
            {
                var cfgID = entityBase.Data.Attrs.GetAoiValue<uint>(EnumAOIType.Uint, AOIAttrDefine.Index);
                MonsterDataCell monsterDataCell = LocalDataManager.Instance.GetMonsterDataCell(cfgID);

                var showLevel = entityBase.Data.Attrs.GetLuaAoiValue(EnumAOIType.Int, AOIAttrDefine.ShowLevel);
                str2 += $"怪物等级:{showLevel}\n怪物id:{monsterDataCell.ID}\n";
            }
            attrInfoText.text = str2;


            var skillBuffs = entityBase.GetSkillBuffs();
            foreach (var item in skillBuffs)
            {
                strBuff += $"|{item.BuffID} {item.StackCount} {item.LiveTime}";
            }
        }
        buffText.text = strBuff;
    }

    public void AddSkillRecord(ulong eid, int skillId)
    {
        if (!startRecord)
        {
            return;
        }

        var partnerMgr = BusinessManager.Instance.GetPartnerMDMgr();
        if (eid != curSelectEntityID && !partnerMgr.IsMyPartnerByEid(eid))
        {
            return;
        }

        recordSkills.Add(skillId);
        if (recordSkills.Count > 5)
        {
            recordSkills.RemoveAt(0);
        }
        foreach (var item in allRecords)
        {
            GameObject.Destroy(item);
        }
        allRecords.Clear();

        foreach (var item in recordSkills)
        {
            var obj = GameObject.Instantiate(recordItem.gameObject, recordItem.transform.parent);
            obj.SetActive(true);
            obj.transform.localScale = Vector3.one;


            LocalDataManager.Instance.GetSkillJson(item, (SkillEditor.SkillJson skillJson) =>
            {
                if (skillJson == null)
                {
                    return;
                }

                var cfg = skillJson.config;
                obj.GetComponent<Text>().text = $"技能id:{item}, 技能名称:{cfg.SkillDesc}";
            });

            allRecords.Add(obj);
        }
    }

    public void SetTarget(NPCEntityBase tar)
    {
        if (tar.EntityId != curSelectEntityID)
        {
            return;
        }
        curTarget = tar;
        // string str = GameManager.Instance.GetEntityNameById(tar.EntityId);
        // targetText.text = "攻击对象:" + str;
    }

    private void OnTapEventHandler(Vector3 position)
    {
        OnSelectTarget(position);
    }

    public void OnStartRecord()
    {
        startRecord = !startRecord;
    }

    private void OnSelectTarget(Vector3 position)
    {
        if (CameraManager.Instance == null)
        {
            return;
        }

        if (CameraManager.Instance.CurrentPlayCamera == null)
        {
            return;
        }

        if (GameInput.GetIsTouchDown)
        {
            return;
        }

        Ray ray = CameraManager.Instance.CurrentPlayCamera.Camera.ScreenPointToRay(position);
        RaycastHit raycast;
        if (Physics.Raycast(ray, out raycast, 100, LayerMask.GetMask("Entity")))
        {
            string tag = raycast.collider.tag;
            if (tag.Contains(E_TagType.Enemy.ToString()) || tag.Contains(E_TagType.Boss.ToString()) || tag.Contains(E_TagType.Player.ToString()))
            {
                ViewVitalNPCNormal npc = raycast.collider.gameObject.GetComponent<ViewVitalNPCNormal>();
                if (curSelectEntityID != npc.EntityID)
                {
                    foreach (var item in allRecords)
                    {
                        GameObject.Destroy(item);
                    }
                    allRecords.Clear();
                    recordSkills.Clear();
                    curSelectEntityID = npc.EntityID;
                }
            }
        }
    }
}
