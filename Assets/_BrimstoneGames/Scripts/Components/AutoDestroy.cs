using UnityEngine;

namespace _DPS
{
    public class AutoDestroy : MonoBehaviour
    {

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
