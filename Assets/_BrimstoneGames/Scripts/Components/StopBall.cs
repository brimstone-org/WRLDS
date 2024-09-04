using UnityEngine;

namespace _DPS
{

    [RequireComponent(typeof(Collider2D))]
    public class StopBall : MonoBehaviour
    {
        [Tooltip("Use this to make the platform start moving only when the player touches it")]
        public bool TriggerPlatformMovement;
        [Tooltip("Use this to make the platform start moving only when the player touches it and keeps doing it")]
        public bool TriggerPlatformMovementYoyo;
        [Tooltip("Use this to make the platform stop moving and remain in place when the player leaves the platform")]
        public bool TriggerStop;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player") && !other.isTrigger && !BallController.IsOnPlatform)
            {
                BallController.IsOnPlatform = true;
                global::Logger.Log("!@#!#!@#!# STOPPING VERTICAL");
                GameManager.StopBallVertical?.Invoke(0.05f);
            }
        }


        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player") && !other.isTrigger && BallController.IsOnPlatform)
            {
                BallController.IsOnPlatform = false;
                var pm = transform.parent.GetComponent<PlatformMover>();
                if (pm != null)
                {
                    pm.IsMoving = false;
                }
                global::Logger.Log("---------------exiting collider in the second half-------------");
                GameManager.Instance.StopCoroutineThatStopsBall();

                //other.attachedRigidbody.freezeRotation = true;
                //other.attachedRigidbody.bodyType = RigidbodyType2D.Kinematic;
                if (!TriggerStop) return;

                if (GetComponent<PlatformMover>())
                {
                    GetComponent<PlatformMover>().PlatformPause();
                }
            }
        }
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player") && !other.collider.isTrigger)
            {
                
                other.transform.SetParent(transform);
                BallController.CurrentParentOcclussionComponent = transform.GetComponentInParent<OcclussionComponent>();
                //In case we jump on the same platform and we are in the trigger we should stop the ball wehn touching the platform
                if (BallController.IsOnPlatform)
                {
                    GameManager.StopBallVertical?.Invoke(0.05f);
                }
                if (TriggerPlatformMovement)
                {
//                    GameManager.TriggerPlatformMovement?.Invoke();
                    if (GetComponent<KillerPiston>())
                    {
                        GetComponent<KillerPiston>().OnTriggerPlatformmovement();
                    }

                    if (GetComponent<PlatformMover>())
                    {
                        GetComponent<PlatformMover>().StartMoving();
                    }
                    
                }
                if (TriggerPlatformMovementYoyo )
                {
//                    GameManager.TriggerPlatformMovement?.Invoke();
                    if (GetComponent<KillerPiston>())
                    {
                        GetComponent<KillerPiston>().OnTriggerPlatformmovement();
                    }

                    var pm = GetComponent<PlatformMover>();
                    if (pm != null)
                    {
                        pm.IsYoyo = true;
                        pm.StartMoving();
                    }
                    
                }

                //other.attachedRigidbody.freezeRotation = true;
                //other.attachedRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }
        void OnCollisionExit2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player") && !other.collider.isTrigger)
            {
                other.transform.SetParent(transform.root);
               
            }
        }
    }
}
