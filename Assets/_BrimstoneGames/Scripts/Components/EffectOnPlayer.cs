using UnityEngine;

namespace _DPS
{
    public class EffectOnPlayer : MonoBehaviour
    {
        public EffectOnPlayerEntity Template;
        public BallController.BallEffect EffectType;
        public BallController.BallEffectAffects AffectType;
        public GameObject VisualPickPrefab;
        public int EffectId = -1;
        public float EffectJumpAdd;
        public float EffectPowerAdd;
        public float EffectDuration;
        public float EffectRadius = 0;
        public float EffectMagnitude = 0;

        void OnEnable()
        {
            PopulateEffect();
        }

        public void PopulateEffect()
        {
            EffectId = Template.EffectId;
            EffectType = Template.EffectType;
            EffectJumpAdd = Template.EffectJumpAdd;
            EffectPowerAdd = Template.EffectPowerAdd;
            EffectDuration = Template.EffectDuration;
            EffectRadius = Template.EffectRadius;
            EffectMagnitude = Template.EffectMagnitude;
            VisualPickPrefab = Template.VisualPickPrefab;
            AffectType = Template.AffectType;
        }
    }
}
