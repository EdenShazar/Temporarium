using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace GoodBoy.StateEvents.Editor
{
    [CustomEditor(typeof(TimedStateEvent))]
    public class TimedStateEventCustomEditor : UnityEditor.Editor
    {
        SerializedProperty layerName;
        SerializedProperty eventName;
        
        SerializedProperty eventTime;
        SerializedProperty normalizedTime;
        SerializedProperty everyLoop;

        void OnEnable()
        {
            layerName = serializedObject.FindProperty("layerName");
            eventName = serializedObject.FindProperty("eventName");
            
            eventTime = serializedObject.FindProperty("eventTime");
            normalizedTime = serializedObject.FindProperty("normalizedTime");
            everyLoop = serializedObject.FindProperty("everyLoop");
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

            List<string> layerNames = StateEventManager.TimedEvents.Keys.ToList();
            layerNames.AddRange(StateEventManager.GeneralEvents.Keys.ToList());

            if (layerNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No layers available", MessageType.Warning);
                layerName.stringValue = "";
                eventName.stringValue = "";
                
                eventTime.floatValue = 0;
                everyLoop.boolValue = false;

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
            List<string> eventNames = StateEventManager.TimedEvents[layerName.stringValue];
            eventNames.AddRange(StateEventManager.GeneralEvents[layerName.stringValue]);

            if (eventNames.Count == 0)
            {
                EditorGUILayout.HelpBox("No events in layer", MessageType.Warning);
                eventName.stringValue = "";

                eventTime.floatValue = 0;
                everyLoop.boolValue = false;

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

            EditorGUIUtility.labelWidth = 100;

            // draw event time field
            EditorGUI.indentLevel++;

            normalizedTime.boolValue = EditorGUILayout.ToggleLeft(
                value: normalizedTime.boolValue,
                label: "Normalized time"
                );

            float newEventTime = EditorGUILayout.FloatField(
                value: eventTime.floatValue,
                label: "Event time"
                );

            eventTime.floatValue = Mathf.Max(0, newEventTime);

            // draw every loop field
            everyLoop.boolValue = EditorGUILayout.ToggleLeft(
                value: everyLoop.boolValue,
                label: "Every loop"
                );

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}