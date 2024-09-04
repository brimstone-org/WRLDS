using DG.Tweening;
using UnityEngine;

namespace _DPS
{


    public class SimpleDamager : MonoBehaviour
    {
        [SerializeField]
        private DOTweenAnimation _doTweenAnimation;
        private Collider2D _collider2D;

        public void ToggleTween(bool turnOn)
        {
            if (_doTweenAnimation != null)
            {
                if (turnOn)
                {
                    _doTweenAnimation.DORestart();
                }
                else
                {
                    _doTweenAnimation.DOPause();
                }
            }
        }

        void OnCollisionEnter2D(Collision2D collision2D)
        {
            if (collision2D.gameObject.CompareTag("Player"))
            {
                if (GameManager.Instance.kill == null)
                {
                    GameManager.Instance.kill= StartCoroutine(GameManager.Instance.KillAndReload());
                }
            }
        }
    }
}