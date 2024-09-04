using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _DPS
{

    public class StaticScorePick : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _scoreSprite;
        [SerializeField]
        private float magnetSpeed = 25f;
        [SerializeField]
        private GameObject _effects;
        [HideInInspector]
        public bool isCought;
        [HideInInspector]
        public int ScoreValue;
        void Awake()
        {
            isCought = false;
            SetScoreParams();
        }

        private void SetScoreParams()
        {
            var element = ShootingSystem.Instance._fallingObjList[(int) ShootingSystem.FallingObjTypes.Animals]
                .FallingObjList[Random.Range(0,ShootingSystem.Instance._fallingObjList[(int) ShootingSystem.FallingObjTypes.Animals].FallingObjList.Length)];
            var rng = Random.Range(0, element.FallingObjScore.Length);
            ScoreValue = element.FallingObjScore[rng];
            _scoreSprite.sprite = element.FallingObjSprite[rng];
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.isTrigger)
            {
                isCought = false;
                GameManager.ScorePick?.Invoke(ScoreValue);
                _scoreSprite.enabled = false;
                GetComponent<CircleCollider2D>().enabled = false;
                _effects.SetActive(true);
                StartCoroutine(DespawnWithDelay());
            }
        }

        private IEnumerator DespawnWithDelay()
        {
            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
        
            if (isCought)
            {
                transform.DOKill();
                transform.position = Vector3.MoveTowards(transform.position, LevelBuilder.Player.transform.position, Time.deltaTime *magnetSpeed);
            }
        }
    }
}