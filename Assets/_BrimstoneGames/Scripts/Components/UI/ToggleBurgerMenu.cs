using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _DPS
{
    public class ToggleBurgerMenu : MonoBehaviour
    {
        [SerializeField] private Transform burgerOuter;
        [SerializeField] private float MenuSizeMultiplier = 1f;
        [SerializeField] private Image arrow, theButtonImage;
        [SerializeField] private Sprite arrowUp, arrowDown;
        
        private Button button;
        private bool _enable;

        void Start()
        {
            arrow.sprite = arrowUp;
            button = GetComponent<Button>();
            button.interactable = true;
            theButtonImage.alphaHitTestMinimumThreshold = 0.5f;

        }

        public void ToggleMenu()
        {
            _enable = !_enable;
            if (_enable)
            {
                AudioManager.Instance.Play("menuOpen");
                button.interactable = false;
                Time.timeScale = 0;
                GameManager.Instance.SavePlayerParams(GameManager.Instance.PlayerParams);
                burgerOuter.DOScale(Vector3.one * MenuSizeMultiplier, 0.5f).SetUpdate(true).SetEase(Ease.OutBounce).OnComplete((() =>
                {
                    //radialLayoutGroup.ForceUpdate();
                    arrow.transform.localScale = Vector3.zero;
                    arrow.sprite = arrowDown;
                    arrow.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBounce)
                        .OnComplete((() => button.interactable = true));
                }));
            }
            else
            {
                AudioManager.Instance.Play("menuClose");
                button.interactable = false;
                Time.timeScale = 1;
                burgerOuter.DOScale(Vector3.zero, 0.5f).SetUpdate(true).SetEase(Ease.InQuad).OnComplete((() =>
                {
                    arrow.transform.localScale = Vector3.zero;
                    arrow.sprite = arrowUp;
                    arrow.transform.DOScale(Vector3.one, 0.5f).SetUpdate(true).SetEase(Ease.OutBounce)
                        .OnComplete((() => button.interactable = true));
                    ;
                }));
                ;
            }
        }
    }
}