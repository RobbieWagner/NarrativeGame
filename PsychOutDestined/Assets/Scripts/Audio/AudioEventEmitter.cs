using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace PsychOutDestined
{
    [RequireComponent(typeof(StudioEventEmitter))]
    public class AudioEventEmitter : MonoBehaviour
    {
        [SerializeField] private StudioEventEmitter eventEmitter;
        [SerializeField] private EventReference sound;

        private void Awake()
        {
            eventEmitter.EventReference = sound;
            eventEmitter.Play();
        }

        private void OnDestroy()
        {
            eventEmitter.Stop();
        }
    }
}
