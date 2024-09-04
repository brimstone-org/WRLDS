using UnityEngine;

namespace _DPS
{
    public class ScorePickComponent : MonoBehaviour
    {
        public int ScoreValue;
        private FallingObjectComponent _fallingObjectComponent;
        public bool IsCought;

        void Awake()
        {
            if (_fallingObjectComponent == null)
            {
                _fallingObjectComponent = GetComponent<FallingObjectComponent>();
            }
        }

        void OnEnable()
        {
            IsCought = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !other.isTrigger)
            {
                //global::Logger.Log("pick Score " + ScoreValue + " " + transform.root.name + " " + gameObject.transform.position);
                GameManager.ScorePick?.Invoke(ScoreValue);
                    //GameManager.Instance.PlayerParams.PlayerScore += ScoreValue;
                    //HudManager.Instance.ScoreText.text = GameManager.Instance.PlayerParams.PlayerScore.ToString("N0");
                    //if (_fallingObjectComponent == null)
                    //{
                    //    gameObject.SetActive(false);
                    //}
            }
        }

    }
}
