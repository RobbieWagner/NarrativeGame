namespace PsychOutDestined
{
    public enum UnitStat
    {
        //Main Stats
        None = -1,
        Brawn,
        Agility,
        Defense,
        Psych,
        Focus,
        Heart,

        //Computed stats
        HP, // Health (10 +  HalfStrength + HalfDefense)
        Stress, // Max Stamina a user can take before PSYCH OUT (100)
        Initiative, // Order in Combat (1d6 + HalfAgility)
        PCrit, // Physical Crit Chance (.001 * (HalfStrength + Agility))
        MCrit, // Mental Crit Chance (.001 * (HalfPsych + Focus))
        Resistance, // Resistance to gaining stress (Focus + Heart + HalfDefense)
    }
    public class UnitStatDetails
    {

        public readonly UnitStat statType;
        private int statValue;

        public int StatValue
        {
            get
            {
                return (int) ((statValue + raiseReduction) * multiplier);
            }
            private set => statValue = value;
        }

        private int raiseReduction = 0;
        public int RaiseReduction
        {
            get => raiseReduction;
            set 
            {
                if (value == raiseReduction)
                    return;
                raiseReduction = value;
                OnStatChanged?.Invoke(statValue);
            }
        }

        private float multiplier = 1;
        public float Multiplier
        {
            get => multiplier;
            set
            {
                if (value == multiplier)
                    return;
                multiplier = value;
                OnStatChanged?.Invoke(statValue);
            }
        }

        public UnitStatDetails(UnitStat stat, int startingValue)
        {
            statType = stat;
            statValue = startingValue;
        }

        public delegate void OnStatChangedDelegate(int value);
        public event OnStatChangedDelegate OnStatChanged;
    }
}
