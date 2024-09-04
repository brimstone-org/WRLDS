using System.Collections.Generic;
using UnityEngine;

namespace _DPS
{
    [ExecuteInEditMode]
    public class InfoPostsCatalogue : MonoBehaviour
    {
        public static InfoPostsCatalogue _instance;
        public List<InfoPostsEntity> InfoPosts = new List<InfoPostsEntity>();
        private static int _lastLength;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }

        void Update()
        {
            if(_lastLength == InfoPosts.Count) return;
            if (InfoPosts != null)
            {
                _lastLength = InfoPosts.Count;
                foreach (var effect in InfoPosts)
                {
                    effect.InfoPostId = InfoPosts.IndexOf(effect);
                }
            }
        }
    }
}
