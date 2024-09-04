
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class PlatfomCatchComponent : MonoBehaviour
{
    private Rigidbody2D _selfRigidbody2D;
    private Rigidbody2D _caughtPlayerRigidbody2D;
    private Vector3 _contactPoint;
    private List<ContactPoint2D> _contacts = new List<ContactPoint2D>();
    private bool _isCought;

    void Awake()
    {
        _selfRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isCought = true;
            _caughtPlayerRigidbody2D = collision.rigidbody;
            //_contactPoint = collision.GetContact(0).point;
            //collision.GetContacts(_contacts);
            //global::Logger.Log("cought2 " + _contacts.Count);
        }
    }  
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //_isCought = false;
            //_caughtPlayerRigidbody2D = null;
        }
    }

    void FixedUpdate()
    {
        if(!_isCought) return;
        global::Logger.Log("cought " + _selfRigidbody2D.velocity.x);
        _caughtPlayerRigidbody2D.transform.position = new Vector2(/*_contacts[0].point.x*/ _caughtPlayerRigidbody2D.transform.position.x - _selfRigidbody2D.position.x,
            _caughtPlayerRigidbody2D.transform.position.y);
        //_caughtPlayerRigidbody2D.velocity = new Vector2(_caughtPlayerRigidbody2D.velocity.x + _selfRigidbody2D.velocity.x, 0);
        //var previousPosition = _caughtPlayerRigidbody2D.position;
        //var currentPosition =  new Vector2(/*_contacts[0].point.x*/_selfRigidbody2D.position.x, _caughtPlayerRigidbody2D.transform.position.y);;
        //_caughtPlayerRigidbody2D.MovePosition((_caughtPlayerRigidbody2D.position + currentPosition) * 2f * Time.fixedDeltaTime);
    }
}
