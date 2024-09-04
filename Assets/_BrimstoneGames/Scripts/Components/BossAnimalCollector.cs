using DG.Tweening;
using UnityEngine;

namespace _DPS
{

    public class BossAnimalCollector : MonoBehaviour
    {
        public bool IsCought;
        public Transform Catcher;
        [SerializeField] private float magnetSpeed = 25f;
        private bool isSet;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.transform == Catcher && !other.isTrigger)
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!IsCought) return;
            if (!isSet)
            {
                isSet = true;
                transform.SetParent(transform.root);
                GetComponent<SpriteRenderer>().sortingOrder = 3;
                transform.DOScale(0.4f, 5f).SetEase(Ease.Linear).SetAutoKill();
            }
            transform.position = Vector3.MoveTowards(transform.position, Catcher.transform.position, Time.deltaTime * magnetSpeed);

        }
    }
}