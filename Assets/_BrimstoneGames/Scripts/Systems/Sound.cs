using UnityEngine;

namespace _DPS
{
    [System.Serializable]
//The class that is used inside Audio manager.
    public class Sound {

        public AudioClip clip;

        public string name;

        [Range(0f, 1f)]
        public float volume;
        [Range(.1f, 3f)]
        public float pitch = 1f;

        [HideInInspector]
        public AudioSource source;


    }
}
