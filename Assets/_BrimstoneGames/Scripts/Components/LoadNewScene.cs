using UnityEngine;

namespace _DPS
{
    public class LoadNewScene : MonoBehaviour
    {
        public int SceneDestination;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.isTrigger)
            {
                GameManager.StopBall?.Invoke();
                BallController.AllowMovement = false;
                //BallController.Instance.UnsubscribeEvents();
                StartCoroutine(GameManager.Instance.LoadScene(SceneDestination));
                global::Logger.Log("Ask sceneload " + SceneDestination);
            }
        }
    }
}
