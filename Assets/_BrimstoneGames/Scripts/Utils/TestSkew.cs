using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _DPS
{
    public class TestSkew : MonoBehaviour
    {
        public int End = 5;
        public int NumberOfTries = 1000;
        public float SkewFactor = 6f;

        private Dictionary<int, int>resultsList = new Dictionary<int, int>();

        public void TestSkewMethod()
        {
            resultsList.Clear();
            for (var i = 0; i < NumberOfTries; i++)
            {
                var rng = Utils.SkewedRandomRange(0, End, SkewFactor);
                if (resultsList.ContainsKey(rng))
                {
                    resultsList[rng].Add(resultsList[rng]++);
                }
                else
                {
                    resultsList.Add(rng,1);
                }
            }
            var ordered = resultsList.OrderBy(x => x.Key);
            foreach (var key in ordered.ToDictionary(t=>t.Key, t=>t.Value))
            {
                global::Logger.Log("~~~~~~~~~ Number " + key.Key + " resulted " + key.Value + " times." + " odds: " + ((float)key.Value *100f / (float)NumberOfTries).ToString("N2") +"%");
            }
        }
    }
}
