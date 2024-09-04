using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _DPS
{

    public class ScrollButtonParams : MonoBehaviour
    {
        public int SceneId;
        public Button ScrollButton;
        public Image ScrollButtonImage;


        private void OnEnable()
        {
            ScrollButton.onClick.AddListener((() =>
            {
                AudioManager.Instance.Play("buttonOk");
                SceneLoader.Instance.MainMenu.GetComponent<DOTweenAnimation>().DORestart();
                SceneLoader.Instance.LoadScene(SceneId);
                ScrollButton.interactable = false;
            }));
        }

        private void OnDisable()
        {
            ScrollButton.onClick.RemoveAllListeners();
            ScrollButton.interactable = true;
        }
    }
}