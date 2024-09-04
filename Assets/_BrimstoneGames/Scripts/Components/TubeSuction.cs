using _DPS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeSuction : MonoBehaviour
{
    public PhysicsMaterial2D BounceMaterial;
    public PhysicsMaterial2D BallMaterial;
    public Transform HoverPoint;
    private float _playerGravityScale;
    private float _playerDrag;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !other.isTrigger)
        {
            BallController.IsInsideTube = true;
            Rigidbody2D rgbd = other.GetComponent<Rigidbody2D>();
            rgbd.velocity =Vector2.zero;
            rgbd.angularVelocity = 0;
            _playerDrag = rgbd.drag;
            rgbd.drag = 0.3f;
            _playerGravityScale = rgbd.gravityScale;
            rgbd.gravityScale = 0;
           // rgbd.sharedMaterial = BounceMaterial;
            StartCoroutine(other.GetComponent<BallController>().MoveUpTube(HoverPoint.position, 1f));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !other.isTrigger )
        {
            Rigidbody2D rgbd = other.GetComponent<Rigidbody2D>();
            BallController.IsInsideTube = false;
            rgbd.gravityScale = _playerGravityScale;
            rgbd.drag = _playerDrag;
            //rgbd.sharedMaterial = BallMaterial;
        }
    }
}
