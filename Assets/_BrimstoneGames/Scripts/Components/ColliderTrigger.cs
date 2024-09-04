using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTrigger : MonoBehaviour
{
    public Collider2D TargetCollider, TargetCollider2;
    public bool Activate;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") && !collider.isTrigger)
        {   
            if (TargetCollider!=null)
                TargetCollider.enabled = Activate;
            if (TargetCollider2 != null)
                TargetCollider2.enabled = !Activate;
        }
    }
}
