using System.Collections.Generic;

//[ExecuteInEditMode]
namespace _DPS
{
    public class EffectsCatalogue : Singleton<EffectsCatalogue>
    {
        protected EffectsCatalogue(){}
        public List<EffectOnPlayerEntity> Effects = new List<EffectOnPlayerEntity>();
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
            if (Effects != null && _lastLength != Effects.Count)
            {
                _lastLength = Effects.Count;
                foreach (var effect in Effects)
                {
                    effect.EffectId = Effects.IndexOf(effect);
                }
            }
        }
    }
}
