using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace PsychOutDestined
{
    public class TurnTracker : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnText;
        private const string TURN_STRING ="Turn ";

        public void InitializeTurnTracker()
        {
            turnText.text = TURN_STRING + CombatManagerBase.Instance.CurrentTurn;
            CombatManagerBase.Instance.OnStartTurn += UpdateTurnTracker;
        }

        private void UpdateTurnTracker()
        {
            turnText.text = TURN_STRING + CombatManagerBase.Instance.CurrentTurn;
        }
    }
}