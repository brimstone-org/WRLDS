using System.Collections.Generic;

namespace _DPS
{
    public class GunsPowerUpsaCatalogue : Singleton<GunsPowerUpsaCatalogue>
    {
        protected GunsPowerUpsaCatalogue(){}
        /// <summary>
        /// Entities with their info, populate with all needed scriptableObjects
        /// </summary>
        public List<GunPowerUpsEntity> GunPowerUpsEntities = new List<GunPowerUpsEntity>();
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

        /// <summary>
        /// Will populate entity ids
        /// </summary>
        void Update()
        {
            if (GunPowerUpsEntities != null && _lastLength != GunPowerUpsEntities.Count)
            {
                foreach (var gun in GunPowerUpsEntities)
                {
                    gun.GunPowerUpId = GunPowerUpsEntities.IndexOf(gun);
                }
            }
        }
    }
}
