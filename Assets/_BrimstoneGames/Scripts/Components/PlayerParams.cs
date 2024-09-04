using System;
using System.Collections.Generic;

namespace _DPS
{
    [Serializable]
    public class PlayerParams
    {
        public int NumberOfLives;
        public int NumberOfArmor;
        public int PlayerScore;
        public int LastScene;
        /// <summary>
        /// holds the catalogue id of the effect and float = remaining duration at the time of save
        /// </summary>
        public Dictionary<int, float> EffectsOnPlayer;

        public PlayerParams()
        {
            NumberOfLives = 3;
            NumberOfArmor = 0;
            PlayerScore = 0;
            LastScene = 1;
            EffectsOnPlayer = new Dictionary<int, float>();
        }
    }
}
