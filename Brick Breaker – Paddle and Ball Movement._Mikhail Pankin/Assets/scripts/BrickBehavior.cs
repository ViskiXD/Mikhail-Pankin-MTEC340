using UnityEngine;

public class BrickBehavior : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            // Notify Game Manager that player scored
            if (Gamebihave.Instance != null)
            {
                // increment the score when a brick is destroyed
                Gamebihave.Instance.ScorePoint(0); // using player index 0 for single player scoring
                Debug.Log("Brick destroyed! Score updated.");
            }
            
            // Destroy this brick when hit by the ball
            Destroy(gameObject);
        }
    }
}
