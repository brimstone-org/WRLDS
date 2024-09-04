using UnityEngine;
using DG.Tweening;

public class CrushingPlatform : MonoBehaviour
{
    public Vector3 Target;
    public float SlamDuration = 1f;
    public float RetractDuration = 4f;
    public int Loops = -1;
    public bool isJointed;

    private Vector3 _originalLocalPosition, _originalRotation;
    private bool falling ;

    private void Start()
    {
        _originalLocalPosition = transform.localPosition;
        _originalRotation = transform.rotation.eulerAngles;
        //StartMoving();
    }
    public void StartMoving()
    {
        falling = true;
        if (!isJointed)
        { transform.DOLocalMove(Target, SlamDuration).SetDelay(0.5f).SetEase(Ease.Linear).OnComplete((() =>
        {
            falling = false;
            transform.DOLocalMove(_originalLocalPosition, RetractDuration).SetDelay(1f).SetAutoKill(true).OnComplete(StartMoving);
        }));
        }
        else
        {
            transform.DORotate(Target, SlamDuration).SetDelay(0.5f).SetEase(Ease.Linear).OnComplete((() =>
            {
                falling = false;
                transform.DORotate(_originalRotation, RetractDuration).SetDelay(1f).SetAutoKill(true).OnComplete(StartMoving);
            }));
        }
    }

    public void StopMoving()
    {
        transform.DOPause();
    }

   /* void OnCollisionEnter2D(Collision2D collision2D)
    {
        if (falling == true && collision2D.gameObject.CompareTag("Player"))
        {
            StartCoroutine(GameManager.Instance.KillAndReload());
        }
    }*/

}
