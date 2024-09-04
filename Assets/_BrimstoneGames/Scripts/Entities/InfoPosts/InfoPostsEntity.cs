using UnityEngine;

namespace _DPS
{
    [CreateAssetMenu(menuName = ("DPS/InfoPost"))]
    public class InfoPostsEntity : ScriptableObject
    {
        public int InfoPostId = -1;
        [TextArea]
        public string InfoText;
        public GameObject[] InGameObjectsToTurnOn;
    }
}
