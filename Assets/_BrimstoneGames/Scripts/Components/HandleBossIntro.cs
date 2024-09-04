using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _DPS
{
    public class HandleBossIntro : MonoBehaviour
    {
        private float _yPosition;
        public bool stop;
        private bool isCaught;

        void Start()
        {
            _yPosition = transform.position.y;
        }

        public void StopAndParentToBall()
        {
            if(isCaught) return;
            isCaught = true;
            stop = true;
            AudioManager.Instance.Play("bossDeath");
            //transform.SetParent(LevelBuilder.Player.transform, true);
            //GetComponent<LockRotation>().enabled = true;
            transform.DOLocalMove(LevelBuilder.Instance.Exit.transform.position + new Vector3(35,28,0), 15f).SetEase(Ease.Linear).SetAutoKill();
            transform.DOScale(0.1f, 8f).SetEase(Ease.OutQuad).SetAutoKill();
            ToggleBossLaughing(false);
        }

        
        public void ToggleBossLaughing(bool turnOn)
        {
            if (turnOn)
            {
                if (_bossLaughing != null)
                {
                    StopCoroutine(_bossLaughing);
                }
                _bossLaughing = StartCoroutine(BossLaughing());
            }
            else
            {
                if (_bossLaughing != null)
                {
                    StopCoroutine(_bossLaughing);
                }
            }
        }

        private Coroutine _bossLaughing;
        private IEnumerator BossLaughing()
        {
            while (true)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                AudioManager.Instance.Play("bossEnrage");
                var t = 10f;
                while (t > 0)
                {
                    if (Time.timeScale < .1)
                    {
                        yield return StartCoroutine(GameManager.HandlePauseState());
                    }
                    t -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        void Update()
        {
            if (!stop)
            {
                transform.position = new Vector3(LevelBuilder.Player.transform.position.x + 15, _yPosition, transform.position.z);
            }
        }
    }
}