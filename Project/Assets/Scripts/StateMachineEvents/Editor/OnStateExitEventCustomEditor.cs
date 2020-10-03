using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace GoodBoy.StateEvents.Editor
{
    [CustomEditor(typeof(OnStateExitEvent))]
    public class OnStateExitEventCustomEditor : UnityEditor.Editor
    {
        OnStateExitEvent targetObject;

        SerializedProperty layerName;
        SerializedProperty eventName;

        SerializedProperty raiseOnTransitionStart;
        SerializedProperty raiseOnAnimationEndOnly;

        void OnEnable()
        {
            layerName = serializedObject.FindProperty("layerName");
            eventName = serializedObject.FindProperty("eventName");

            raiseOnTransitionStart = serializedObject.FindProperty("raiseOnTransitionStart");
            raiseOnAnimationEndOnly = serializedObject.FindProperty("raiseOnAnimationEndOnly");

            // The following is not properly defined during play mode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            targetObject = (OnStateExitEvent)target;
            var context = AnimatorController.FindStateMachineBehaviourContext(targetObject)[0];

            // Only states are relevant; state machines are ignored
            bool isStateMachine = context.animatorObject is AnimatorStateMachine;
            if (isStateMachine)
            {
                Debug.LogWarning("OnStateExitEvent will not work on state machine " +
                    context.animatorObject.name + ". Use it on a state instead.");
                return;
            }

            AnimatorState state = context.animatorObject as AnimatorState;

            targetObject.transitions = new List<StateTransitionData>();
            foreach (AnimatorStateTransition transition in state.transitions)
            {
                var transitionData = new StateTransitionData(transition, state);

                bool duplicate = targetObject.transitions.Any(
                    t => t.nameHash == transitionData.nameHash &&
                    t.userNameHash == transitionData.userNameHash
                    );

                if (duplicate)
                {
                    Debug.LogWarning("Found transitions with identical names (\"" + transitionData.name + 
                        "\") and user-defined names (\"" + transitionData.userName + "\"). " +
                        "Only the first will be used to raise events on transition start.");
                    continue;
                }

                targetObject.transitions.Add(transitionData);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw event selection title
            EditorGUILayout.LabelField(
                "Event selection",
                style: EditorStyles.boldLabel
                );

            // Draw layer popup
            EditorGUI.indentLevel++;

            List<string> layerNames = StateEventManager.ExitEvents.Keys.ToList();
            layerNames.AddRange(StateEventManager.GeneralEvents.Keys.ToList());

            if (layerNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No layers available", MessageType.Warning);
                layerName.stringValue = "";
                eventName.stringValue = "";

                raiseOnTransitionStart.boolValue = false;

                serializedObject.ApplyModifiedProperties();
                return;
            }

            int selectedLayerIndex;
            if (!layerNames.Contains(layerName.stringValue))
                selectedLayerIndex = 0;
            else
                selectedLayerIndex = layerNames.IndexOf(layerName.stringValue);

            selectedLayerIndex = EditorGUILayout.Popup(
                selectedIndex: selectedLayerIndex,
                displayedOptions: layerNames.ToArray(),
                label: "Layer"
                );

            layerName.stringValue = layerNames[selectedLayerIndex];

            // Draw event popup
            List<string> eventNames = StateEventManager.ExitEvents[layerName.stringValue];
            eventNames.AddRange(StateEventManager.GeneralEvents[layerName.stringValue]);

            if (eventNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No events in layer", MessageType.Warning);
                eventName.stringValue = "";

                raiseOnTransitionStart.boolValue = false;

                serializedObject.ApplyModifiedProperties();
                return;
            }

            int selectedEventIndex;
            if (!eventNames.Contains(eventName.stringValue))
                selectedEventIndex = 0;
            else
                selectedEventIndex = eventNames.IndexOf(eventName.stringValue);

            int newEventIndex = EditorGUILayout.Popup(
                selectedIndex: selectedEventIndex,
                displayedOptions: eventNames.ToArray(),
                label: "Event"
                );

            eventName.stringValue = eventNames[newEventIndex];

            EditorGUI.indentLevel--;

            // Draw options title
            EditorGUILayout.LabelField(
                "Options",
                style: EditorStyles.boldLabel
                );

            // Draw raise on transition start toggle
            EditorGUI.indentLevel++;

            raiseOnTransitionStart.boolValue = EditorGUILayout.ToggleLeft(
                value: raiseOnTransitionStart.boolValue,
                label: "Raise when starting transition from state"
                );

            // Draw raise on animation end only toggle
            raiseOnAnimationEndOnly.boolValue = EditorGUILayout.ToggleLeft(
                value: raiseOnAnimationEndOnly.boolValue,
                label: "Raise only when triggered by end of animation"
                );

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}