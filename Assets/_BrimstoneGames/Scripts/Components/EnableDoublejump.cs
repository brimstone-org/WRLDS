using UnityEngine;

namespace _DPS
{
    public class EnableDoublejump : MonoBehaviour
    {
        public GameObject Effect;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player")  && !other.isTrigger)
            {
                Effect.SetActive(true);
                var ball = other.GetComponent<BallController>();
                ball.DoubleJump = ball.DblJumpOn = true;
            
            }
        }
    }
}
