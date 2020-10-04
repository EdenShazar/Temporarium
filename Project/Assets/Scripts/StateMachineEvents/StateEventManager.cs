using System;
using System.Reflection;
using System.Collections.Generic;

public static class StateEventManager
{
    #region Event declarations

    // All events are declared here, but must be categorized in the "event categorization" region.
    // Any type of built-in or custom delegate may be used as an event handler.

    // Creature layer
    public static event Action<int> OnFinishedHatching;
    public static event Action<int> OnLayEgg;
    public static event Action<int> OnDeath;

    #endregion

    #region Event categorization

    // Events are defined in separate dictionaries - each a main category. In each dictionary,
    // the events are then further categorized by animation layer. General events can be used
    // as any type of event.
    // The event names must be identical to the names of their corresponding event variables;
    // use the nameof() expression to ensure this equality and still support easy refactoring.

    #region Enter events

    public static Dictionary<string, List<string>> EnterEvents { get; } =
        new Dictionary<string, List<string>>
        {
            { "Creature",
                new List<string>
                {
                    nameof(OnDeath)
                }
            }
        };

    #endregion

    #region Exit events

    public static Dictionary<string, List<string>> ExitEvents { get; } =
        new Dictionary<string, List<string>>
        {
            { "Creature",
                new List<string>
                {
                    nameof(OnFinishedHatching)
                }
            }
        };

    #endregion

    #region Timed events

    public static Dictionary<string, List<string>> TimedEvents { get; } =
        new Dictionary<string, List<string>>
        {
            { "Creature",
                new List<string>
                {
                    nameof(OnLayEgg)
                }
            }
        };

    #endregion

    #region General events

    public static Dictionary<string, List<string>> GeneralEvents { get; } =
        new Dictionary<string, List<string>>
        {
            { "Creature",
                new List<string>
                {
                        
                }
            }
        };

    #endregion

    #endregion

    public static void Raise(string eventName, int gameObjectID)
    {
        if (eventName == "")
            return;

        if (!eventFields.ContainsKey(eventName))
            throw new Exception("No event named " + eventName + " exists.");

        var eventDelegate = (MulticastDelegate)eventFields[eventName].GetValue(null);
        eventDelegate?.DynamicInvoke(gameObjectID);
    }

    #region Implementation details

    static Dictionary<string, FieldInfo> eventFields;

    static StateEventManager()
    {
        SetEventFields();
    }

    static void SetEventFields()
    {
        eventFields = new Dictionary<string, FieldInfo>();

        BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;
        FieldInfo[] fields = typeof(StateEventManager).GetFields(bindingFlags);

        foreach (FieldInfo field in fields)
            if (typeof(MulticastDelegate).IsAssignableFrom(field.FieldType))
                eventFields.Add(field.Name, field);
    }

    #endregion
}
