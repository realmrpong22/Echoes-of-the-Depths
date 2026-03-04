using UnityEngine;

public class RoomTransitionTrigger : MonoBehaviour
{
    public enum TransitionDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public TransitionDirection direction;
    public Collider2D newBounds;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float playerX = other.GetComponent<Rigidbody2D>().velocity.x;

        if (direction == TransitionDirection.Right && playerX <= 0) return;
        if (direction == TransitionDirection.Left && playerX >= 0) return;

        RoomTransitionManager.Instance.StartTransition(direction, newBounds);
    }
}