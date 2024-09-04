using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed ;

    public bool Launched;
    public bool HasBeenFired;
    public float LifeTime;

    private float _aInternalLifeTime;
    // Update is called once per frame

    public void Fire()
    {
        gameObject.SetActive(true);
        _aInternalLifeTime = LifeTime;
        HasBeenFired = true;
        Launched = true;
    }

    private IEnumerator Death()
    {
        yield return new WaitForSeconds(2);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (!gameObject.activeSelf) return;
        if (Launched)
        {
            _aInternalLifeTime -= Time.deltaTime;
            if (_aInternalLifeTime <= 0)
            {
                Launched = false;
                gameObject.SetActive(false);
                transform.localPosition = Vector2.zero;
                return;
            }
            //start timer

        }
        transform.Translate(Vector3.right * speed* Time.deltaTime, Space.Self);
    }
}
