using System.Collections;
using UnityEngine;

namespace _DPS
{
    [RequireComponent(typeof(Collider2D))]
    public class KillerPiston : MonoBehaviour
    {
        [Header("leave zero first for starting from spot")]
        public Vector3[] LocalDestinations;
        public float MovingSpeed, Wait;
        public bool IsTriggered;
        /// <summary>
        /// -1 = infinite
        /// </summary>
        public int Loops = -1;

        private int _destinationsCount, _currentDest;
        private bool isWaiting, allow;
        private Coroutine movingCoroutine;
        private int _loop;
        private bool isActive;
        
        void Start()
        {
            //get destinationscount
            
            GameManager.TriggerPlatformMovement += OnTriggerPlatformmovement;
           
            BuildDestinations();
            _destinationsCount = LocalDestinations.Length;
            transform.position = new Vector3(transform.position.x + RandomSign() * Random.Range(5, 9), transform.position.y, transform.position.z);
            if (!IsTriggered)
            {
                movingCoroutine = StartCoroutine(StartWaiting());
                isActive = true;
            }
            _loop = Loops;
        }

        public void OnTriggerPlatformmovement()
        {
            if(isActive) return;
            movingCoroutine = StartCoroutine(StartWaiting());
        }

        
        private void BuildDestinations()
        {
            global::Logger.Log("calculating destiantis");
            for (int i = 0; i < LocalDestinations.Length; i++)
            {
                if (i == 0)
                {
                    if ((LocalDestinations[i] - Vector3.zero).sqrMagnitude < 0 || (LocalDestinations[i] - Vector3.zero).sqrMagnitude > 0)
                    {
                        var loc = LocalDestinations[i];
                        LocalDestinations[i] = transform.localPosition - loc;
                    }
                    else
                    {
                        LocalDestinations[i] -= transform.localPosition;
                    }
                }
                else
                {
                    LocalDestinations[i] += transform.localPosition;
                }
            }

            allow = true;
        }

        void OnDisable()
        {
            isActive = false;
            GameManager.TriggerPlatformMovement -= OnTriggerPlatformmovement;
            if (movingCoroutine != null)
            {
                StopCoroutine(movingCoroutine);
            }

            transform.localPosition = LocalDestinations[0];
            allow = false;
        }

        private IEnumerator StartWaiting()
        {
            if (Wait <= 0)
            {
                isWaiting = false;
                yield break;
            }
            var t = Wait;
            while (t > 0)
            {
                t -= Time.deltaTime;
                isWaiting = true;
                yield return new WaitForSeconds(Time.deltaTime);
            }

            isWaiting = false;
        }

        void Update()
        {
            if(!allow || _loop == 0 || isWaiting) return;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, LocalDestinations[_currentDest],
                MovingSpeed * Time.deltaTime);
            if ((transform.localPosition - LocalDestinations[_currentDest]).sqrMagnitude < 0.1f)
            {
                if (_loop > 0)
                {
                    _loop--;

                }

                _currentDest++;
                if (_currentDest == _destinationsCount)
                {
                    _currentDest = 0;
                }
                if(Wait <= 0) return;

                if (movingCoroutine != null)
                {
                    StopCoroutine(movingCoroutine);
                }

                movingCoroutine = StartCoroutine(StartWaiting());
            } 
        }

        private int RandomSign()
        {
            return Random.value < 0.5f ? 1 : -1;
        }
    }


}
