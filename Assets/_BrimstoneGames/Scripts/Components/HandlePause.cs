using UnityEngine;

namespace _DPS
{
    public class HandlePause : MonoBehaviour
    {

        void OnEnable()
        {
            Time.timeScale = 0;
        }
        void OnDisable()
        {

            Time.timeScale = 1;
        }

    }
}
