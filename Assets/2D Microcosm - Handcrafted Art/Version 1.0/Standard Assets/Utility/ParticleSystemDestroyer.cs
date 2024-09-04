using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Utility
{
    public class ParticleSystemDestroyer : MonoBehaviour
    {
        // allows a particle system to exist for a specified duration,
        // then shuts off emission, and waits for all particles to expire
        // before destroying the gameObject

        public float minDuration = 8;
        public float maxDuration = 10;

        private float m_MaxLifetime;
        private bool m_EarlyStop;


        private IEnumerator Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();

            // find out the maximum lifetime of any particles in this effect
            for (int i = 0; i < systems.Length; i++)
            {
                m_MaxLifetime = Mathf.Max(systems[i].main.startLifetimeMultiplier , m_MaxLifetime);
            }
            //foreach (var system in systems)
            //{
            //    m_MaxLifetime = Mathf.Max(system.main.startLifetimeMultiplier , m_MaxLifetime);
            //}

            // wait for random duration

            float stopTime = Time.time + Random.Range(minDuration, maxDuration);

            while (Time.time < stopTime || m_EarlyStop)
            {
                yield return null;
            }
            //global::Logger.Log("stopping " + name);

            // turn off emission
            for (int i = 0; i < systems.Length; i++)
            {
                systems[i].Stop(true);
            }
            //foreach (var system in systems)
            //{
            //    system.enableEmission = false;
            //}
            BroadcastMessage("Extinguish", SendMessageOptions.DontRequireReceiver);

            // wait for any remaining particles to expire
            yield return new WaitForSeconds(m_MaxLifetime);

            Destroy(gameObject);
        }


        public void Stop()
        {
            // stops the particle system early
            m_EarlyStop = true;
        }
    }
}
