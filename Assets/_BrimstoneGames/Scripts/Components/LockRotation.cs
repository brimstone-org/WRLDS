using UnityEngine;

namespace _DPS
{
    public class LockRotation : MonoBehaviour
    {
        void Update () 
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }
}
