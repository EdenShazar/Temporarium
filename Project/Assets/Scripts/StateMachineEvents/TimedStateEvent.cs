using UnityEngine;

namespace GoodBoy.StateEvents
{
    public class TimedStateEvent : StateMachineBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] string eventName;
        [SerializeField] float eventTime;
        [SerializeField] bool normalizedTime;
        [SerializeField] bool everyLoop;
#pragma warning restore CS0659

        bool raised;
        float previousLoopedTime;

#if UNITY_EDITOR
        [SerializeField] string layerName;
#endif

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            raised = false;
            previousLoopedTime = 0;

            if (!normalizedTime && eventTime > stateInfo.length)
                Debug.LogWarning("Event " + eventName + " will never be raised; it is set to " +
                    eventTime + " seconds, but the animation is " + stateInfo.length + " seconds long");
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (everyLoop)
            {
                EveryLoopCheck(animator, stateInfo);
                return;
            }

            FirstLoopCheck(animator, stateInfo);
        }

        void EveryLoopCheck(Animator animator, AnimatorStateInfo stateInfo)
        {
            float loopedTime = normalizedTime ? stateInfo.NormalizedTimeLooped() : stateInfo.RealTimeLooped();

            bool firstUpdateOfLoop = loopedTime <= previousLoopedTime;
            if (firstUpdateOfLoop)
            {
                // Raise event of previous loop if missed
                if (previousLoopedTime < eventTime)
                    StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());

                raised = false;
            }

            if (loopedTime >= eventTime)
            {
                StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());
                raised = true;
            }
        }

        void FirstLoopCheck(Animator animator, AnimatorStateInfo stateInfo)
        {
            if (raised)
                return;

            float time = normalizedTime ? stateInfo.normalizedTime : stateInfo.RealTime();

            if (time >= eventTime)
            {
                StateEventManager.Raise(eventName, animator.gameObject.GetInstanceID());
                raised = true;
            }
        }
    }
}
