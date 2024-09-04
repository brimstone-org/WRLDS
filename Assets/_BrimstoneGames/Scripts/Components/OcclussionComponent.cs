using System;
using DG.Tweening;
using UnityEngine;

namespace _DPS
{
    public class OcclussionComponent : MonoBehaviour
    {
        public float cullingRadius = 10;
        //public ParticleSystem target;

        CullingGroup m_CullingGroup;
        public Renderer[] m_ParticleRenderers;

        private SimpleDamager[] _simpleDamager;
        private Light[] m_Lights;
        [NonSerialized]
        public PlatformMover[] m_PlatformMovers;
        [NonSerialized]
        public CrushingPlatform[] m_CrushingPlatforms;
        [NonSerialized]
        public KillerRotator[] m_KillerRotators;
        [NonSerialized]
        public DOTweenAnimation[] m_DoTweens;
        public ProjectileShooter[] m_ProjectileShooters;
        private KillerPiston mp;
        private PlatformMover pm;
        private KillerRotator kr;
        private ProjectileShooter ps;

        void Awake()
        {
            _simpleDamager = GetComponentsInChildren<SimpleDamager>();
        }

        public void EnableOcclussion()
        {
            //// Do we need custom culling?
            //if (target.proceduralSimulationSupported)
            //{
            //    global::Logger.Log(name + " does not need custom culling");
            //    enabled = false;
            //    return;
            //}

            //if (m_ParticleRenderers == null)
            m_ParticleRenderers = GetComponentsInChildren<Renderer>();
            m_Lights = GetComponentsInChildren<Light>();
            m_PlatformMovers = GetComponentsInChildren<PlatformMover>();
            m_ProjectileShooters = GetComponentsInChildren<ProjectileShooter>();
            m_CrushingPlatforms = GetComponentsInChildren<CrushingPlatform>();
            m_DoTweens = GetComponentsInChildren<DOTweenAnimation>();
            //if (gameObject.name.Contains("TumbleRollAlienTwist"))
            //{
            //    global::Logger.Log("The number of shooters is "+ m_ProjectileShooters.Length);
            //}
            if (m_CullingGroup == null)
            {
                m_CullingGroup = new CullingGroup();
                m_CullingGroup.targetCamera = Camera.main;
                m_CullingGroup.SetBoundingSpheres(new[] {new BoundingSphere(transform.position, cullingRadius)});
                m_CullingGroup.SetBoundingSphereCount(1);
                m_CullingGroup.onStateChanged += OnStateChanged;
            }

            if (pm == null)
            {
                pm = GetComponent<PlatformMover>();
            }

            if (mp == null)
            {
                mp = GetComponent<KillerPiston>();
            }

            if (kr == null)
            {
                kr = GetComponent<KillerRotator>();
            }

            if (ps == null)
            {
                ps = GetComponent<ProjectileShooter>();
            }

            // We need to sync the culled state.
            Cull(m_CullingGroup.IsVisible(0));
            m_CullingGroup.enabled = true;
        }

        void OnDisable()
        {
            if (m_CullingGroup != null)
                m_CullingGroup.enabled = false;
            //global::Logger.Log("Disabling the object");
            //target.Play(true);

            //SetEnables(true);
        }

        void OnDestroy()
        {
            m_CullingGroup?.Dispose();
        }

        void OnStateChanged(CullingGroupEvent sphere)
        {
            //global::Logger.Log("State " + gameObject.name +" " + sphere.isVisible);
            Cull(sphere.isVisible);
        }

        void Cull(bool visible)
        {
            if (visible)
            {
                // We could simulate forward a little here to hide that the system was not updated off-screen.
                //target.Play(true);
                SetEnables(true);
                if (_simpleDamager != null)
                {
                    for (int i=0; i < _simpleDamager.Length; i++)
                    {
                        _simpleDamager[i].ToggleTween(true);
                    }
          
                }
            }
            else
            {
                //target.Pause(true);
                SetEnables(false);
                if (_simpleDamager != null)
                {
                    for (int i = 0; i < _simpleDamager.Length; i++)
                    {
                        _simpleDamager[i].ToggleTween(true);
                    }
                }

                if (m_ProjectileShooters != null)
                {
                    for (int i = 0; i < m_ProjectileShooters.Length; i++)
                    {
                        if (m_ProjectileShooters[i] != null)
                        {
                            if (m_ProjectileShooters[i].FiringSequence!=null)
                            StopCoroutine(m_ProjectileShooters[i].FiringSequence);
                        }
                    }
                }
                if (m_CrushingPlatforms != null)
                {
                    for (int i = 0; i < m_CrushingPlatforms.Length; i++)
                    {
                        if (m_CrushingPlatforms[i] != null)
                        {
                            m_CrushingPlatforms[i].StopMoving();
                        }
                    }
                }
                
                if (m_DoTweens != null)
                {
                    for (int i = 0; i < m_DoTweens.Length; i++)
                    {
                        if (m_DoTweens[i] != null)
                        {
                            m_DoTweens[i].DORewind();
                        }
                    }
                }
            }

            //special for platforms

            if (mp != null)
            {
                mp.enabled = visible;
            }
            if (kr != null)
            {
                kr.enabled = visible;
            }
            if (ps != null)
            {
                ps.enabled = visible;
            }
        }

        void SetEnables(bool enable)
        {
            // We also need to disable the renderer to prevent drawing the particles, such as when occlusion occurs.
            for (int i = 0; i < m_ParticleRenderers.Length; i++)
            {
                if (m_ParticleRenderers[i] != null)
                {
                    m_ParticleRenderers[i].enabled = enable;
                }
            }

            if (m_Lights != null)
            {
                for (int i = 0; i < m_Lights.Length; i++)
                {
                    if (m_Lights[i] != null)
                    {
                        m_Lights[i].enabled = enable;
                    }
                }
            }

            if (m_PlatformMovers != null)
            {
                for (int i = 0; i < m_PlatformMovers.Length; i++)
                {
                    if (m_PlatformMovers[i] != null)
                    {
                        m_PlatformMovers[i].enabled = enable;
                    }
                }
            }
            if (m_CrushingPlatforms != null)
            {
                for (int i = 0; i < m_CrushingPlatforms.Length; i++)
                {
                    if (m_CrushingPlatforms[i] != null)
                    {
                        m_CrushingPlatforms[i].enabled = enable;
                        if (enable)
                        {
                            m_CrushingPlatforms[i].StartMoving();
                        }
                    }
                }
            }
            

            if (m_ProjectileShooters != null)
            {
                for (int i = 0; i < m_ProjectileShooters.Length; i++)
                {
                    if (m_ProjectileShooters[i] != null )
                    {   
                        m_ProjectileShooters[i].enabled = enable;
                        if (enable)
                        { 
                            m_ProjectileShooters[i].StartShootingCoroutine();
                        }
                    }
                }
            }

            if (m_DoTweens != null)
            {
                for (int i = 0; i < m_DoTweens.Length; i++)
                {
                    if (m_DoTweens[i] != null)
                    {
                        if (enable)
                        {
                            m_DoTweens[i].DORestart();
                        }
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (enabled)
            {
                // Draw gizmos to show the culling sphere.
                Color col = Color.yellow;
                if (m_CullingGroup != null && !m_CullingGroup.IsVisible(0))
                    col = Color.gray;

                Gizmos.color = col;
                Gizmos.DrawWireSphere(transform.position, cullingRadius);
            }
        }
    }
}