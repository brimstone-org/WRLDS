using UnityEngine;

namespace _DPS
{
    [CreateAssetMenu(menuName = ("DPS/Effect"))]
    public class EffectOnPlayerEntity : ScriptableObject
    {
        public BallController.BallEffect EffectType;
        public BallController.BallEffectAffects AffectType;
        public GameObject VisualPickPrefab;
        public Sprite HudImagePrefab;
        public int EffectId = -1;
        public float EffectJumpAdd = 0;
        public float EffectPowerAdd = 0;
        public float EffectRadius = 0;
        public float EffectMagnitude = 0;


        public float EffectDuration;
    }
}
