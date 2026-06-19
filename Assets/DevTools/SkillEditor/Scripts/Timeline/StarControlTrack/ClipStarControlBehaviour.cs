///--------------------------------------------------------------------
/// 文件名   :   ClipStarControlAsset.cs
/// 内  容   :   
/// 说  明   :  
/// 创建日期 :   2022/09/13 14:49:19
/// 创建人   :   赵尔东
/// 版权所有 :   游卡网络科技技术有限公司 
///--------------------------------------------------------------------

using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using Unity.Mathematics;
using StarProject.OffLine;

namespace SkillEditor
{
    [System.Serializable]
    public class ClipStarControlBehaviour : PlayableBehaviour
    {
        [Space]
        [LabelText("配置数据")]
        [ShowInInspector]
        public SpecEffect configEffect;

        [HideInInspector]
        public GameObject owner;

        [HideInInspector]
        public GameObject EffectRoot;

        [HideInInspector]
        public GameObject HangPoint;

        public override void OnPlayableCreate(Playable playable)
        {
            base.OnPlayableCreate(playable);
        }


        public override void OnGraphStart(Playable playable)
        {
            base.OnGraphStart(playable);
        }

        public override void OnGraphStop(Playable playable)
        {
            base.OnGraphStop(playable);
        }

#if UNITY_EDITOR
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            base.PrepareFrame(playable, info);
        }

        public override void PrepareData(Playable playable, FrameData info)
        {
            base.PrepareData(playable, info);
        }

        public override void OnBehaviourDelay(Playable playable, FrameData info)
        {
            base.OnBehaviourDelay(playable, info);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            base.OnPlayableDestroy(playable);
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            base.OnBehaviourPause(playable, info);
        }


        public void SetTrans(ClipStarControlAsset clipStarControlAsset)
        {
            //如果是空的，需要创建一个对应的effectroot节点，并设置对应的source
            if (EffectRoot == null)
            {
                GameObject root = new GameObject("EffectRoot");
                root.transform.parent = SkillEditorGlobal.Instance.Effect.transform;
                EffectRoot = root;
                ExposedReference<GameObject> reference = new ExposedReference<GameObject>();
                reference.defaultValue = root;
                clipStarControlAsset.sourceGameObject = reference;
            }

            //拿到模型，设置特效挂点
            ModelOffLineData modelOffLineData = SkillEditorData.curExplorerItem.ExplorereItemModel.GetComponent<ModelOffLineData>();

            if (!modelOffLineData.BindDummyPos.ContainsKey(configEffect.HangPoint.ToString()))
            {
                return;
            }
            HangPoint = modelOffLineData.BindDummyPos[configEffect.HangPoint.ToString()].gameObject;


            Vector3 rootPos = Vector3.zero;
            Quaternion rootQuaternion = Quaternion.identity;

            Vector3 posOffset = new Vector3(configEffect.XOffset / 100f, configEffect.YOffset / 100f, configEffect.ZOffset / 100f);
            Quaternion rotOffset = Quaternion.Euler(configEffect.XOffsetTowards, configEffect.YOffsetTowards, configEffect.ZOffsetTowards);

            //特效节点有几种方式
            //1.既不跟随位置又不跟随旋转
            //2.跟随位置但不跟随旋转
            //3.跟随位置且跟随旋转

            if (!configEffect.IsFollowMove && !configEffect.IsFollowRot)
            {
                rootPos += posOffset;
                rootQuaternion = rootQuaternion * rotOffset;
            }
            else if (configEffect.IsFollowMove && !configEffect.IsFollowRot)
            {
                rootPos = HangPoint.transform.position + posOffset;
                rootQuaternion = rootQuaternion * rotOffset;
            }
            else if (configEffect.IsFollowMove && configEffect.IsFollowRot)
            {
                rootPos = HangPoint.transform.position + posOffset;
                rootQuaternion = HangPoint.transform.rotation * rotOffset;
            }

            EffectRoot.transform.position = rootPos;
            EffectRoot.transform.rotation = rootQuaternion;
            EffectRoot.transform.localScale = new Vector3(configEffect.XScale / 100f, configEffect.YScale / 100f, configEffect.ZScale / 100f);
        }
#endif

    }
}