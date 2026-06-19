using Animancer;
using Animancer.FSM;
using StarProjectDef;
using System;

namespace StarProject.Game.Entity.View.VitalSign.State
{
    public interface I_VVitalAnim
    {
        AnimancerComponent Animancer { get; }
        StateMachine<VitalState>.WithDefault StateMachine { get; }

        void PlayAnim(E_ULayerSubState subState, I_AnimParam animParam, Action endStateCB);
    }



}