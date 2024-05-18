using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsychOutDestined
{
    public partial class GameManager : MonoBehaviour
    {
        public IEnumerator ExecuteCoroutinesConcurrently(List<IEnumerator> coroutines)
        {
            if(coroutines == null || !coroutines.Any())
            {
                Debug.LogWarning("coroutine list found empty");
                yield break;
            }
                
            // Create a list to hold references to the started coroutines
            List<Coroutine> runningCoroutines = new List<Coroutine>();

            // Start all coroutines and add them to the running list
            foreach (IEnumerator coroutine in coroutines)
                runningCoroutines.Add(StartCoroutine(coroutine));

            // Wait until all coroutines have completed
            foreach (Coroutine runningCoroutine in runningCoroutines)
                yield return runningCoroutine;

            // At this point, all coroutines have finished
            Debug.Log("All coroutines have completed.");
        }

        public IEnumerator WaitCoroutine(float time)
        {
            yield return new WaitForSeconds(time);
        }
    }
}