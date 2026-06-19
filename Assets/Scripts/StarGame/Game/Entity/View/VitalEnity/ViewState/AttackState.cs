using Animancer;
using Animancer.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace StarProject.Game.Entity.View.VitalSign.State
{

    public class AttackState : VitalState
    {
        private int _AttackIndex = int.MaxValue;
        [SerializeField]
        private ClipTransition[] _AttackAnimations;
        public ClipTransition[] AttackAnimations => _AttackAnimations;
        private void OnEnable()
        {
            Character.Animancer.Animator.applyRootMotion = true;

            if (ShouldRestartCombo())
            {
                _AttackIndex = 0;
            }
            else
            {
                _AttackIndex++;
            }

            //var animation = _Equipment.Weapon.AttackAnimations[_AttackIndex];
            //CurState = Character.Animancer.Play(AttackAnimations[_AttackIndex]);
            //CurState.Events.OnEnd = Character.StateMachine.ForceSetDefaultState;
            //CurState.Events.SetCallback("Move", SkillMoveCallBack);
            //CurState.Events.SetCallback("Damage", SkillDamageEvent);
            //SetNormalizedTime
            //SetCallback
        }
        private bool ShouldRestartCombo()
        {
            var attackAnimations = AttackAnimations;//_Equipment.Weapon.AttackAnimations;

            if (_AttackIndex >= attackAnimations.Length - 1)
                return true;

            var state = attackAnimations[_AttackIndex].State;
            if (state == null ||
                state.Weight == 0)
                return true;

            return false;
        }
        private void FixedUpdate()
        {
            //if (Character.Rigidbody != null)
            //{
            //    Character.Rigidbody.velocity = default;
            //}
           
        }
        private void OnDisable()
        {
            Character.Animancer.Animator.applyRootMotion = false;
        }
        /************************************************************************************************************************/

        public override bool CanExitState => false;

        /************************************************************************************************************************/

    }
}
