using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ToggleProgressColor : MonoBehaviour
{
    [SerializeField] private Image _bubble;
    public bool IsToggled;
    public void ToggleColor(bool turnOn)
    {
        IsToggled = turnOn;
        if (turnOn)
        {
            _bubble.DOComplete();
            _bubble.DOColor(Color.green, 0.75f);
        }
        else
        {
            _bubble.DOPlayBackwards();
            IsToggled = false;
        }
    }
}
