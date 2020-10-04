using UnityEngine;

namespace GoodBoy.StateEvents
{
    public class OnStateEnterEvent : StateMachineBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] string eventName;
        [SerializeField] bool raiseOnTransitionEnd;
#pragma warning restore CS0659

        bool eventRaised;

#if UNITY_EDITOR
        [SerializeField] string layerName;
#endif

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            bool transitionIsInstant = !animator.IsInTransition(layerIndex);
            if (raiseOnTransitionEnd && !transitionIsInstant)
                return;

            StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());
            eventRaised = true;

            if (stateInfo.shortNameHash == Animator.StringToHash("Idle"))
                Debug.Log(stateInfo.normalizedTime);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!raiseOnTransitionEnd || eventRaised || animator.IsInTransition(layerIndex))
                return;

            StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());
            eventRaised = true;

            if (stateInfo.shortNameHash == Animator.StringToHash("Idle"))
                Debug.Log(stateInfo.normalizedTime);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            eventRaised = false;
        }
    }
}
