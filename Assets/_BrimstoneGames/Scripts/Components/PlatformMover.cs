using DG.Tweening;
using UnityEngine;

namespace _DPS
{
    public class PlatformMover : MonoBehaviour
    {
        public Vector3 Target;
        public float Duration = 4f;
        public int Loops = -1;
        private Vector3 _originalLocalPosition;
        public bool IsYoyo;
        public bool IsMoving;
        private Tween m_mover, m_backer;

        private void Start()
        {
            _originalLocalPosition = transform.localPosition;
        }

        public void StartMoving()
        {
            if (!IsMoving)
            {
                IsMoving = true;
                m_mover = transform.DOLocalMove(Target, Duration).SetDelay(0.5f).SetEase(Ease.Linear).OnComplete((() =>
                {
                    m_backer = transform.DOLocalMove(_originalLocalPosition, Duration).SetDelay(1.5f).OnComplete((() =>
                    {
                        IsMoving = false;
                        if (IsYoyo)
                        {
                            StartMoving();
                        }
                    }));
                }));
            }
        }
    


        //public void StartMovingYoyo()
        //{
        //    transform.DOKill();
        //    transform.DOLocalMove(Target, Duration).SetDelay(0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);

        //}

        public void ResetToStart()
        {
            global::Logger.Log("Called restart on " + gameObject.name);
            m_mover.Kill();
            m_backer.Kill();
            IsYoyo = false;
            GameManager.Instance.StopCoroutineThatStopsBall();
            transform.DOLocalMove(_originalLocalPosition, Duration).SetEase(Ease.Linear).SetAutoKill(true).OnComplete((() =>
            {
                IsMoving = false;
            })); 
        }

        public void PlatformPause()
        {
            transform.DOPause();
        }
    }

}

