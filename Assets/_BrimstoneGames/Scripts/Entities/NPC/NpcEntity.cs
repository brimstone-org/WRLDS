using UnityEngine;

namespace _DPS
{
    [CreateAssetMenu(menuName = ("DPS/NPC"))]
    public class NpcEntity : ScriptableObject
    {
        public int NpcId = -1;
        //public Sprite CharSprite;
        public GameObject CharPrefab;
        public int Health = 1;
        public int BossVersionHealth = 100;
        public float BossScaleMod = 3;
        public int Damage = 1;
        public float FireRate = 1;
        public int SpawnAmount = 1;
        public GameManager.NpcTypes NpcType;

        //public bool HasSight;
        //public float PatrollingLength;
        //public float WalkingSpeed = 3;
        //public float JumpSpeed = 3;
        //public float WalkDelay = 2;
    }
}
