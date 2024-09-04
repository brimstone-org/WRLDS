using System.Collections.Generic;

namespace _DPS
{
    public class NpcCatalogue : Singleton<NpcCatalogue>
    {
        protected NpcCatalogue(){}
        public List<NpcEntity> NpcList = new List<NpcEntity>();
        private static int _lastLength;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
        }

        void Update()
        {
            if (NpcList != null && _lastLength != NpcList.Count)
            {
                _lastLength = NpcList.Count;
                foreach (var effect in NpcList)
                {
                    effect.NpcId = NpcList.IndexOf(effect);
                }
            }
        }
    }
}
