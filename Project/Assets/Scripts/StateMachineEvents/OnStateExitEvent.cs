using System.Collections.Generic;
using UnityEngine;

namespace GoodBoy.StateEvents
{
    public class OnStateExitEvent : StateMachineBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] string eventName;
        [SerializeField] bool raiseOnTransitionStart;
        [SerializeField] bool raiseOnAnimationEndOnly;
#pragma warning restore CS0659

        public List<StateTransitionData> transitions = new List<StateTransitionData>();
        bool eventRaised;
        bool transitioningOnAnimationEnd;

#if UNITY_EDITOR
        [SerializeField] string layerName;
#endif

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            eventRaised = false;
            transitioningOnAnimationEnd = false;
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (raiseOnAnimationEndOnly)
                transitioningOnAnimationEnd = IsTransitionOnAnimationEnd(animator, layerIndex);

            if (!raiseOnTransitionStart || eventRaised || !IsTransitioningFromState(animator, layerIndex))
            return;

            if (raiseOnAnimationEndOnly && !transitioningOnAnimationEnd)
            return;

            StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());
            eventRaised = true;
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (eventRaised)
                return;

            if (raiseOnAnimationEndOnly && !transitioningOnAnimationEnd)
                return;

            StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());
        }

        bool IsTransitioningFromState(Animator animator, int layerIndex)
        {
            StateTransitionData transitionData = GetCurrentTransition(animator, layerIndex);
            return (transitionData != null);
        }

        bool IsTransitionOnAnimationEnd(Animator animator, int layerIndex)
        {
            StateTransitionData transitionData = GetCurrentTransition(animator, layerIndex);

            if (transitionData == null)
                return false;

            return transitionData.hasExitTime && transitionData.exitTime % 1 == 0;
        }

        StateTransitionData GetCurrentTransition(Animator animator, int layerIndex)
        {
            foreach (StateTransitionData transition in transitions)
                if (animator.GetAnimatorTransitionInfo(layerIndex).nameHash == transition.nameHash &&
                    animator.GetAnimatorTransitionInfo(layerIndex).userNameHash == transition.userNameHash)
                    return transition;

            return null;
        }
    }
}
