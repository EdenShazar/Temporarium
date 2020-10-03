using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace GoodBoy.StateEvents.Editor
{
    [CustomEditor(typeof(OnStateEnterEvent))]
    public class OnStateEnterEventCustomEditor : UnityEditor.Editor
    {
        SerializedProperty layerName;
        SerializedProperty eventName;

        SerializedProperty raiseOnTransitionEnd;

        void OnEnable()
        {
            layerName = serializedObject.FindProperty("layerName");
            eventName = serializedObject.FindProperty("eventName");

            raiseOnTransitionEnd = serializedObject.FindProperty("raiseOnTransitionEnd");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw event selection title
            EditorGUILayout.LabelField(
                "Event selection",
                style: EditorStyles.boldLabel
                );

            // draw layer popup
            EditorGUI.indentLevel++;

            List<string> layerNames = StateEventManager.EnterEvents.Keys.ToList();
            layerNames.AddRange(StateEventManager.GeneralEvents.Keys.ToList());

            if (layerNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No layers available", MessageType.Warning);
                layerName.stringValue = "";
                eventName.stringValue = "";

                raiseOnTransitionEnd.boolValue = false;

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

            // draw event popup
            List<string> eventNames = StateEventManager.EnterEvents[layerName.stringValue];
            eventNames.AddRange(StateEventManager.GeneralEvents[layerName.stringValue]);

            if (eventNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No events in layer", MessageType.Warning);
                eventName.stringValue = "";

                raiseOnTransitionEnd.boolValue = false;

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

            // Draw raise on transition end toggle
            EditorGUI.indentLevel++;

            raiseOnTransitionEnd.boolValue = EditorGUILayout.ToggleLeft(
                value: raiseOnTransitionEnd.boolValue,
                label: "Raise when ending transition to state"
                );

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}