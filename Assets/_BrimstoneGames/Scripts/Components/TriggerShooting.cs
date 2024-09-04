using System;
using Cinemachine;
using Gamekit2D;
using UnityEngine;


namespace _DPS
{
    public class TriggerShooting : MonoBehaviour
    {
        [SerializeField]
        private float _velocityMultiplier = 2;

        [SerializeField] private Transform _emmiter;
        [SerializeField] private Transform _projectilePoolHolder;
        [SerializeField] private Transform _fallingObjPoolHolder;
        [SerializeField] private BoxCollider2D _projectileCatcher;
        [SerializeField] private Collider2D _fallingCatcher;
        public Transform ExitEncounterHolder;
        [Header("OverrideNpcEnemy, leave blank if you want random boss")] 
        [SerializeField]
        private NpcEntity _overrideNpcEnemy;
        [NonSerialized]
        public float m_OriginalScreenY, m_OriginalScreenX;


        void OnTriggerEnter2D(Collider2D other)
        {
            if (other == other.CompareTag("Player") && !other.isTrigger)
            {
                global::Logger.Log("Swinger");
                BallController.Swinger = true;
                LevelBuilder.Instance.LastSetup = LevelBuilder.Instance.CamSetup;
                var transposer = LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>();
                m_OriginalScreenY = transposer.m_ScreenY;
                m_OriginalScreenX = transposer.m_ScreenX;

                transposer.m_ScreenY = .82f;
                transposer.m_ScreenX = .5f;
                BallController.VelocityMulti = _velocityMultiplier;
                //start shooting sequence
                StartShootingChallenge();
            }
        }

        private void StartShootingChallenge()
        {
            var npc = _overrideNpcEnemy == null ? ShootingSystem.Instance.GetRandomNpcEntity() : _overrideNpcEnemy;
            StartCoroutine(ShootingSystem.Instance.StartShootingSequence(this, npc, _emmiter, _projectileCatcher, _fallingCatcher, _projectilePoolHolder, _fallingObjPoolHolder));
        }
    }
}
