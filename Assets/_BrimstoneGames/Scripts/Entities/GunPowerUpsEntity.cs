using UnityEngine;

namespace _DPS
{
    [CreateAssetMenu(menuName = ("DPS/GunPowerUp"))]
    public class GunPowerUpsEntity : ScriptableObject
    {
        public int GunPowerUpId = -1;
        public ShootingSystem.GunTypes GunType;
    
        public Sprite GunSprite;
        public Color PowerUpBorderColor;
        public float PickUpFallingSpeed = 1;


        public ShootingSystem.FiringModes FireModes = ShootingSystem.FiringModes.Single;

        public float FireRate = 1;
        public float ProjectileSpeed = 0.01f;
    }
}
