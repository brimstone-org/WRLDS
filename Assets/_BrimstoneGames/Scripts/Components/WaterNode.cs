using UnityEngine;

namespace _DPS
{
    public class WaterNode : MonoBehaviour
    {
        public GameObject Effect;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.isTrigger)
            {   
                global::Logger.Log("HitPlayer");
                if (LevelBuilder.Instance.SceneType == LevelBuilder.SceneTypes.FlappyBird)
                {
                    GameManager.StopBall?.Invoke();
                    BallController.AllowMovement = false;
                }

                if (GameManager.Instance.kill == null)
                {
                    GameManager.Instance.kill= StartCoroutine(GameManager.Instance.KillAndReload());
                }
                if (Effect != null)
                {
                    Effect.SetActive(true);
                }
            }
        }

    }
}
