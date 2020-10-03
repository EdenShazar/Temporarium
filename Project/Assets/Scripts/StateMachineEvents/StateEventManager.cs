using System;
using System.Reflection;
using System.Collections.Generic;

namespace GoodBoy.StateEvents
{
    public static class StateEventManager
    {
        #region Event declarations

        // All events are declared here, but must be categorized in the "event categorization" region.
        // Any type of built-in or custom delegate may be used as an event handler.

        // Walk layer
        public static event Action WalkChangeDirectionEnd;
        public static event Action WalkJumpEnd;
        public static event Action WalkClimbDownEnd;

        // Window layer
        public static event Action WindowOnEnterCycle;
        public static event Action WindowMagnet2Start;
        public static event Action WindowOnEnterLastUnmagnet;
        public static event Action WindowLoupLeavingWindow;

        // Sniff layer
        public static event Action SniffEnd;

        // Pre-recontrol layer
        public static event Action PreRecontrolStartPulling;
        public static event Action PreRecontrolOnFinished;

        // Pre-recontrol: soldier with weapon
        public static event Action SoldierOnStartSpreadGas;

        // Recontrol layer
        public static event Action RecontrolPulledSuccessfully;
        public static event Action RecontrolOnOvercomePull;
        public static event Action RecontrolLookBackStart;
        public static event Action RecontrolLookBackEnd;
        public static event Action RecontrolStartedWalking;

        // Backyard layer
        public static event Action BackyardChangeFinishedUnsuccessfully;
        public static event Action BackyardChangeFinishedSuccessfully;

        // Ledge layer
        public static event Action LedgeWalkBackStart;
        public static event Action LedgeWalkBackEnd;
        public static event Action LedgeCanPullStart;
        public static event Action LedgeCanPullEnd;
        public static event Action LedgeSuccess;
        public static event Action LedgeFailure;
        
        // Memory layer
        public static event Action MemoryStopSmellT1End;

        // Carlights layer
        public static event Action CarlightsOnStart;
        public static event Action CarlightsFreeze;
        public static event Action CarlightsPulled;
        public static event Action CarlightsHitByCar;
        public static event Action CarlightsPlayerSucceeded;
        public static event Action CarlightsSuccessfulEnd;

        // Refuse to go layer
        public static event Action RefuseToGoOnFinished;

        // Jump down layer
        public static event Action JumpDownWalkStart;
        public static event Action JumpDownLand;
        public static event Action JumpDownEnd;

        // Fence Layer
        public static event Action FencePulledLeft;
        public static event Action FencePulledRight;
        public static event Action FenceRunToFenceStart;
        public static event Action FenceArriveAtFenceEnd;
        public static event Action FenceJumpEnd;
        public static event Action FenceReachedHole;
        public static event Action FenceFinishedEating;

        // Eat Food layer
        public static event Action EatFoodSwallow;
        public static event Action EatFoodOnFinishedEating;

        // Supermarket layer
        public static event Action SupermarketBattleStart;
        public static event Action SupermarketFinishedEating;
        public static event Action SupermarketCutsceneEnd;
        public static event Action SupermarketTryToGetUp;
        public static event Action SupermarketManagedToGetUp;
        
        // End layer
        public static event Action EndTookAFewSteps;
        public static event Action EndMemoryEnded;

        // Diversifer Layer
        public static event Action DiversifyOnAnimationEnd;

        // General
        public static event Action ResetGame;
        public static event Action Die;

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
                { "Walk",
                    new List<string>
                    {
                        
                    }
                },

                { "Window",
                    new List<string>
                    {
                        nameof(WindowOnEnterCycle),
                        nameof(WindowMagnet2Start),
                        nameof(WindowOnEnterLastUnmagnet)
                    }
                },

                { "Sniff",
                    new List<string>
                    {

                    }
                },

                { "Pre-recontrol",
                    new List<string>
                    {
                        nameof(PreRecontrolStartPulling)
                    }
                },

                { "Recontrol",
                    new List<string>
                    { 
                        nameof(RecontrolPulledSuccessfully),
                        nameof(RecontrolOnOvercomePull),
                        nameof(RecontrolLookBackStart),
                        nameof(RecontrolStartedWalking)
                    }
                },

                { "Backyard",
                    new List<string>
                    {

                    }
                },
                
                { "Memory",
                    new List<string>
                    {

                    }
                },

                { "Carlights",
                    new List<string>
                    {
                        nameof(CarlightsOnStart),
                        nameof(CarlightsPulled),
                        nameof(CarlightsPlayerSucceeded),
                        nameof(CarlightsFreeze),
                        nameof(CarlightsHitByCar)
                    }
                },

                { "Refuse to go",
                    new List<string>
                    {

                    }
                },

                { "Jump down",
                    new List<string>
                    {
                        nameof(JumpDownWalkStart)
                    }
                },

                { "Fence",
                    new List<string>
                    {
                        nameof(FenceRunToFenceStart),
                        nameof(FenceReachedHole)
                    }
                },

                { "Ledge",
                    new List<string>
                    {
                        nameof(LedgeWalkBackStart),
                        nameof(LedgeCanPullStart)
                    }
                },

                { "Eat food",
                    new List<string>
                    {
                        
                    }
                },

                { "Supermarket",
                    new List<string>
                    {
                        nameof(SupermarketBattleStart),
                        nameof(SupermarketTryToGetUp)
                    }
                },

                { "End",
                    new List<string>
                    {

                    }
                },

                { "Diversifer",
                    new List<string>
                    {
                    
                    }
                },

                { "General",
                    new List<string>
                    {
                        
                    }
                }
            };

        #endregion

        #region Exit events

        public static Dictionary<string, List<string>> ExitEvents { get; } =
            new Dictionary<string, List<string>>
            {
                { "Walk",
                    new List<string>
                    {
                        nameof(WalkChangeDirectionEnd),
                        nameof(WalkJumpEnd),
                        nameof(WalkClimbDownEnd)
                    }
                },

                { "Window",
                    new List<string>
                    {

                    }
                },

                { "Sniff",
                    new List<string>
                    {

                    }
                },

                { "Pre-recontrol",
                    new List<string>
                    {

                    }
                },

                { "Recontrol",
                    new List<string>
                    {
                        nameof(RecontrolLookBackEnd)
                    }
                },

                { "Backyard",
                    new List<string>
                    {

                    }
                },

                { "Memory",
                    new List<string>
                    {
                        nameof(MemoryStopSmellT1End)
                    }
                },

                { "Carlights",
                    new List<string>
                    {

                    }
                },

                { "Refuse to go",
                    new List<string>
                    {

                    }
                },

                { "Jump down",
                    new List<string>
                    {
                        nameof(JumpDownLand)
                    }
                },

                { "Fence",
                    new List<string>
                    {
                        nameof(FencePulledLeft),
                        nameof(FencePulledRight),
                        nameof(FenceArriveAtFenceEnd),
                        nameof(FenceJumpEnd)
                    }
                },

                { "Ledge",
                    new List<string>
                    {
                        nameof(LedgeWalkBackEnd),
                        nameof(LedgeCanPullEnd)
                    }
                },

                { "Eat food",
                    new List<string>
                    {
                        nameof(EatFoodOnFinishedEating)
                    }
                },

                { "Supermarket",
                    new List<string>
                    {
                        nameof(SupermarketCutsceneEnd)
                    }
                },

                { "End",
                    new List<string>
                    {

                    }
                },

                { "Diversifer",
                    new List<string>
                    {
                        
                    }
                },

                { "General",
                    new List<string>
                    {

                    }
                }
            };

        #endregion

        #region Timed events

        public static Dictionary<string, List<string>> TimedEvents { get; } =
            new Dictionary<string, List<string>>
            {
                { "Walk",
                    new List<string>
                    {

                    }
                },

                { "Window",
                    new List<string>
                    {
                        nameof(WindowLoupLeavingWindow)
                    }
                },

                { "Sniff",
                    new List<string>
                    {
                        nameof(SniffEnd)
                    }
                },

                { "Pre-recontrol",
                    new List<string>
                    {
                        nameof(SoldierOnStartSpreadGas),
                        nameof(PreRecontrolOnFinished)
                    }
                },

                { "Recontrol",
                    new List<string>
                    {

                    }
                },

                { "Backyard",
                    new List<string>
                    {
                        nameof(BackyardChangeFinishedSuccessfully),
                        nameof(BackyardChangeFinishedUnsuccessfully)
                    }
                },

                { "Memory",
                    new List<string>
                    {

                    }
                },

                { "Carlights",
                    new List<string>
                    {
                        nameof(CarlightsSuccessfulEnd)
                    }
                },

                { "Refuse to go",
                    new List<string>
                    {
                        nameof(RefuseToGoOnFinished)
                    }
                },

                { "Jump down",
                    new List<string>
                    {
                        nameof(JumpDownEnd)
                    }
                },

                { "Fence",
                    new List<string>
                    {
                        nameof(FenceFinishedEating)
                    }
                },

                { "Ledge",
                    new List<string>
                    {
                        nameof(LedgeSuccess),
                        nameof(LedgeFailure)
                    }
                },

                {
                    "Eat food",
                    new List<string>
                    {
                        nameof(EatFoodSwallow)
                    }
                },

                { "Supermarket",
                    new List<string>
                    {
                        nameof(SupermarketFinishedEating),
                        nameof(SupermarketManagedToGetUp)
                    }
                },

                { "End",
                    new List<string>
                    {
                        nameof(EndTookAFewSteps),
                        nameof(EndMemoryEnded)
                    }
                },

                { "Diversifer",
                    new List<string>
                    {
                        nameof(DiversifyOnAnimationEnd)
                    }
                },

                { "General",
                    new List<string>
                    {
                        
                    }
                }
            };

        #endregion

        #region General events

        public static Dictionary<string, List<string>> GeneralEvents { get; } =
            new Dictionary<string, List<string>>
            {
                { "Walk",
                    new List<string>
                    {

                    }
                },

                { "Window",
                    new List<string>
                    {

                    }
                },

                { "Sniff",
                    new List<string>
                    {

                    }
                },

                { "Pre-recontrol",
                    new List<string>
                    {
                        
                    }
                },

                { "Recontrol",
                    new List<string>
                    {
                        
                    }
                },

                { "Backyard",
                    new List<string>
                    {

                    }
                },

                { "Memory",
                    new List<string>
                    {

                    }
                },

                { "Carlights",
                    new List<string>
                    {

                    }
                },

                { "Refuse to go",
                    new List<string>
                    {
                        
                    }
                },

                { "Jump down",
                    new List<string>
                    {

                    }
                },

                { "Fence",
                    new List<string>
                    {

                    }
                },

                { "Ledge",
                    new List<string>
                    {
                        
                    }
                },

                { "Diversifer",
                    new List<string>
                    {
                    
                    }
                },

                { "Eat food",
                    new List<string>
                    {

                    }
                },

                { "Supermarket",
                    new List<string>
                    {

                    }
                },

                { "End",
                    new List<string>
                    {

                    }
                },

                { "General",
                    new List<string>
                    {
                        nameof(ResetGame),
                        nameof(Die)
                    }
                }
            };

        #endregion

        #endregion

        public static void Raise(string eventName)
        {
            if (eventName == "")
                return;

            if (!eventFields.ContainsKey(eventName))
                throw new Exception("No event named " + eventName + " exists.");

            var eventDelegate = (MulticastDelegate)eventFields[eventName].GetValue(null);
            eventDelegate?.DynamicInvoke();
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
    }

    #endregion
}
