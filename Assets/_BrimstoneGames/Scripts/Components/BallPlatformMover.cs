using UnityEngine;

public class BallPlatformMover : MonoBehaviour
{
    public Vector2 Target;
    public float Speed = 4;

    private Rigidbody2D m_Rigidbody2D;
    private Vector2 m_PreviousPosition, m_CurrentPosition, m_NextMovement, Velocity;

    void FixedUpdate()
    {
        m_PreviousPosition = m_Rigidbody2D.position;
        m_CurrentPosition = m_PreviousPosition + m_NextMovement;
        Velocity = (m_CurrentPosition - m_PreviousPosition) / Time.deltaTime;

        m_Rigidbody2D.MovePosition(m_CurrentPosition);
        m_NextMovement = Vector2.zero;
    }
}
