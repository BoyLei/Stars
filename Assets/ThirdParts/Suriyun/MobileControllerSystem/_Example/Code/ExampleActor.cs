using SGF.Network;
using SGF.Unity;
using StarProject.UI.SkillBtn;
using StarProjectDef;
using System;
using UnityEngine;

public class ExampleActor : MonoBehaviour
{
    public static ExampleActor Instance;
    public UniversalButton inputMove;
    public SkillCanceller skillCanceller;
    public UniversalButton[] skillButtons;

    public SkillSetting[] skillSettings;
    public float[] cooldowns;

    public bool lerpStopping = false;
    public Transform dirMarker;
    public float moveSpeed;

    public MCS_Contol.MessageBox msg;

    private CharacterController m_CharacterController;

    private void Awake()
    {
        Instance = this;
        m_CharacterController = GameObjectUtils.EnsureComponent<CharacterController>(gameObject);
        moveSpeed = 400;
    }

    protected virtual void Start()
    {
        Application.targetFrameRate = 60;

        cooldowns = new float[3];

        /*    for (int i = 0; i < skillButtons.Length; i++)
            {
                if (skillButtons[i] == null)
                {
                    continue;
                }
                skillButtons[i].SetActiveState(true);
                skillButtons[i].SetText("");
                skillButtons[i].onPointerDown.AddListener(OnSkillButtonPressed);
                skillButtons[i].onDrag.AddListener(OnSkillButtonDragged);
                skillButtons[i].onActivateSkill.AddListener(OnActivateSkill);
                skillButtons[i].onCancelSkill.AddListener(OnCancelSkill);

                skillButtons[i].onEndDrag.AddListener(onEndDrag);


                skillSettings[i].skillMarker.transform.position = this.transform.position;
                skillSettings[i].skillMarker.SetActive(false);

                cooldowns[i] = 0f;

            }*/
    }

    protected Vector3 cachedInput;
    protected virtual void Update()
    {
        //空即为松手
        //if (inputMove != null && inputMove.isFingerDown)
        //{
        //    cachedInput = inputMove.directionXZ;
        //    transform.forward = cachedInput;
        //}
        //else
        //{
        //    if (lerpStopping)
        //    {
        //        cachedInput = Vector3.Lerp(cachedInput, Vector3.zero, moveSpeed * Time.deltaTime);
        //    }
        //    else
        //    {
        //        cachedInput = Vector3.zero;
        //    }
        //}
        ////transform.Translate(cachedInput * moveSpeed * Time.deltaTime, Space.World);
        //m_CharacterController.SimpleMove(cachedInput * moveSpeed * Time.deltaTime);
        //dirMarker.position = transform.position + cachedInput;
        //if (msg != null)
        //{
        //    msg.UpdatePosition(transform.position);
        //}

        //if (skillCanceller != null && skillCanceller.isAnyFingerDown)
        //{
        //    for (int i = 0; i < skillButtons.Length; i++)
        //    {
        //        skillSettings[i].skillMarker.transform.position = GetSkillMarkerPosition(i);
        //    }
        //}

        //UpdateCooldown();
    }

    private void FixedUpdate()
    {
        //if (inputMove != null && inputMove.isFingerDown)
        //{
        //    SendSyncMoveMsg();
        //}
    }
    /// <summary>
    /// 发送移动同步消息
    /// </summary>
    private void SendSyncMoveMsg()
    {
        //ProtoMsg.MoveMsg moveMsg = new ProtoMsg.MoveMsg();
        //ProtoMsg.Vector3 vector3 = new ProtoMsg.Vector3();
        //vector3.X = transform.position.x;
        //vector3.Y = transform.position.y;
        //vector3.Z = transform.position.z;
        //moveMsg.Ispos = true;
        ////moveMsg.Rot = 126; // TODO:曲 先用不到不传了
        ////moveMsg.Isrot = true;
        //moveMsg.Pos = vector3;
        //moveMsg.TimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        //NetworkManager.Instance.gameSocket.SendRPCMsg(ServerType.ServerTypeScene, moveMsg, false);
    }

    protected virtual void UpdateCooldown()
    {
        //for (int i = 0; i < cooldowns.Length; i++)
        //{
        //    if (skillButtons[i] != null)
        //    {
        //        if (cooldowns[i] > 0f)
        //        {
        //            cooldowns[i] -= Time.deltaTime;


        //            if (cooldowns[i] < 1f)
        //            {
        //                skillButtons[i].SetCountdown(cooldowns[i].ToString("F1"));
        //            }
        //            else
        //            {
        //                skillButtons[i].SetCountdown("" + (int)cooldowns[i]);
        //            }
        //            if (skillButtons[i].state == E_SkillBtnState.Active)
        //            {
        //                skillButtons[i].SetActiveState(false);
        //            }


        //        }
        //        else
        //        {
        //            if (skillButtons[i].state == E_SkillBtnState.Inactive)
        //            {
        //                skillButtons[i].SetCountdown("");
        //                skillButtons[i].SetActiveState(true);
        //            }
        //        }
        //    }

        //}
    }


    protected virtual void OnSkillButtonDragged(int i)
    {
        this.UpdateSkillMarkersState(i);
    }

    protected virtual void UpdateSkillMarkersState(int i)
    {
        //if (skillCanceller.M_State == UniversalButton.ButtonState.Pressed)
        //{
        //    skillSettings[i].SetMarkerCanCastSkill(false);
        //}
        //else
        //{
        //    skillSettings[i].SetMarkerCanCastSkill(true);
        //}
    }

    protected virtual void OnActivateSkill(int i)
    {
        //skillSettings[i].SpawnSkillAt(skillSettings[i].skillMarker.transform.position);
        //skillSettings[i].skillMarker.SetActive(false);
        //skillSettings[i].skillMarker.transform.position = this.transform.position;
        //cooldowns[i] = skillSettings[i].cooldown;
        //this.skillButtons[i].directionXZ = Vector3.zero;

        //msg.PopText("Activated skill " + i);
    }

    protected virtual void OnCancelSkill(int i)
    {
        //skillSettings[i].skillMarker.SetActive(false);
        //skillSettings[i].skillMarker.transform.position = this.transform.position;
        //this.skillButtons[i].directionXZ = Vector3.zero;

        //msg.PopText("Canceled skill " + i);
    }

    protected virtual void onEndDrag(int i)
    {
        Debug.Log("onEndDrag skill " + i);
    }

    protected Vector3 GetSkillMarkerPosition(int i)
    {
        return this.transform.position;
        //return this.transform.position +
        //    skillButtons[i].directionXZ * skillSettings[i].range;
    }
    public void ResetPosition()
    {
        this.transform.position = Vector3.up;
    }

    [Serializable]
    public class SkillSetting
    {
        public GameObject skillPrefab;
        public float rotationSpeed;
        public float startingSize;
        public float sizeDecaySpeed;
        public float range;
        public GameObject skillMarker;
        public Material markerActivateSkillTrue;
        public Material markerActivateSkillFalse;
        public float cooldown;

        protected MeshRenderer renderer;

        protected SkillRotatingBox skill;

        public void SetMarkerCanCastSkill(bool can)
        {
            if (renderer == null)
            {
                renderer = skillMarker.GetComponent<MeshRenderer>();
            }

            if (can)
            {
                renderer.material = markerActivateSkillTrue;
            }
            else
            {
                renderer.material = markerActivateSkillFalse;
            }
        }

        public void SpawnSkillAt(Vector3 position)
        {
            skill = this.skillPrefab.GetComponent<SkillRotatingBox>();
            skill.size0 = this.startingSize;
            skill.sizeDecaySpeed = this.sizeDecaySpeed;

            Instantiate(skillPrefab, position, Quaternion.identity);

        }
    }

}
