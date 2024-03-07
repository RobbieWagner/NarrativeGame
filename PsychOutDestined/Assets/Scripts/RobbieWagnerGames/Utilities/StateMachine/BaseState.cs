using UnityEngine;
using System;

namespace RobbieWagnerGames
{
    public abstract class BaseState<EState> where EState : Enum
    {
        public BaseState(EState key)
        {
            StateKey = key;
        }

        public EState StateKey {get; set;}

        public abstract void EnterState();
        public abstract void ExitState();
        public abstract void UpdateState();
        public abstract EState GetNextState();
    }
}