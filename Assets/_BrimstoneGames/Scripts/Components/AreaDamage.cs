using UnityEngine;

namespace _DPS
{
    public class AreaDamage : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {

            if (other.GetComponent<FallingObjectComponent>() != null && !other.GetComponent<FallingObjectComponent>().isCollectible)
            {
                var arrow = other.GetComponent<FallingObjectComponent>();
                other.enabled = false;
            
                arrow.ObjIsShot = true;
                ShootingSystem.FallingObjEvent?.Invoke(arrow);
                arrow.DisableFallingObj();
            
            }
        }
    }
}
