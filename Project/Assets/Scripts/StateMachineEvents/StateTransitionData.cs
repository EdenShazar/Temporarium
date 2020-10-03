using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace GoodBoy.StateEvents
{
    [Serializable]
    public class StateTransitionData
    {
        public float duration;
        public float offset;
        public float exitTime;
        public bool hasExitTime;
        public bool hasFixedDuration;

        public string name;
        public string userName;
        public int nameHash;
        public int userNameHash;

        public bool HasUserName { get => userNameHash != 0; }

#if UNITY_EDITOR
        public StateTransitionData(AnimatorStateTransition transition, AnimatorState fromState)
        {
            duration = transition.duration;
            offset = transition.offset;
            exitTime = transition.exitTime;
            hasExitTime = transition.hasExitTime;
            hasFixedDuration = transition.hasFixedDuration;

            name = transition.GetDisplayName(fromState);
            userName = transition.name;

            nameHash = Animator.StringToHash(name);
            userNameHash = Animator.StringToHash(userName);
        }
#endif
    }
}
