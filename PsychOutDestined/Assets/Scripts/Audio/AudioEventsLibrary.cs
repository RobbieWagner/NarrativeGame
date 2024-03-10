using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using FMODUnity;
using UnityEngine;

namespace PsychOutDestined
{
    public enum GroundType
    {
        None,
        Stone,
        Wood,
        Dirt,
        Grass,
        Water,
        Sand,
        Gravel
    }
    public class AudioEventsLibrary : MonoBehaviour
    {

        [field: Header("UI")]
        [field: SerializeField] public EventReference MenuNavigation {get; private set;}

        [field: Header("Dialogue")]
        [field: SerializeField] public EventReference NextDialogueLine {get; private set;}
        [field: SerializeField] public EventReference Talking {get; private set;}

        [field: Header("Combat Ambience")]
        [field: SerializeField] public EventReference DungeonCombatAmbience {get; private set;}

        [field: Header("Combat Action Effects")]
        [field: SerializeField] public EventReference MagicalFire {get; private set;}

        [field: Header("Combat Impact")]
        [field: SerializeField] public EventReference UnitBludgeoned {get; private set;}

        [field: Header("Exploration Ambience")]
        [field: SerializeField] public EventReference DungeonExplorationAmbience {get; private set;}

        [field: Header("Exploration Footstep Sounds")]
        [field: SerializeField] public EventReference DefaultFootsteps {get; private set;}
        [field: SerializeField] public EventReference StoneFootsteps {get; private set;}
        private Dictionary<GroundType, EventReference> footstepSounds = null;

        [field: Header("Exploration Objects")] // Objects in exploration scenes
        [SerializeField][SerializedDictionary("Name", "Sound Event")] private SerializedDictionary<string, EventReference> explorationObjectSounds;
        [field: SerializeField] public EventReference defaultObjectSound {get; private set;}

        public Dictionary<GroundType, EventReference> FootstepSounds
        {
            get
            {
                if (footstepSounds == null)
                {
                    footstepSounds = new Dictionary<GroundType, EventReference>()
                    {
                        {GroundType.None, DefaultFootsteps},
                        {GroundType.Stone, StoneFootsteps}
                    };
                }
                return footstepSounds;
            }
        }

        [field: Header("Exploration Interactions")]
        [field: SerializeField] public EventReference PullLever {get; private set;}

        [field: Header("Music Combat")]
        [field: SerializeField] public EventReference EncounterMusic {get; private set;}

        [field: Header("Music Dialogue")]
        [field: SerializeField] public EventReference RestingMusic {get; private set;}

        [field: Header("Music Exploration")]
        [field: SerializeField] public EventReference DungeonMusic {get; private set;}

        [field: Header("Music Other")]
        [field: SerializeField] public EventReference MainTheme {get; private set;}

        public static AudioEventsLibrary Instance {get; private set;}

        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
            } 
            else 
            { 
                Instance = this; 
            }     
        }

        public EventReference FindExplorationObjectSound(string soundName)
        {
            var result = explorationObjectSounds.Where(s => s.Key.Equals(soundName, System.StringComparison.CurrentCultureIgnoreCase));
            return result.Any() ? result.First().Value: defaultObjectSound;
        }
    }
}
