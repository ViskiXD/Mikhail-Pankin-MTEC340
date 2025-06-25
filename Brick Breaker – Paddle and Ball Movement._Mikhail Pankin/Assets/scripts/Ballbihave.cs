using UnityEngine;

public class Ballbihave : MonoBehaviour
{
    [SerializeField] private float _launchForce = 5.0f;

    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearDamping = 0.0f;
        _rb.angularDamping = 0.0f;
        _rb.gravityScale = 0.0f;

        // Set ball size from Game Manager
        if (Gamebihave.Instance != null)
        {
            transform.localScale = Vector3.one * Gamebihave.Instance.ballSize;
        }

        ResetBall();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Get paddle influence from Game Manager
            float paddleInfluence = Gamebihave.Instance != null ? Gamebihave.Instance.paddleInfluence : 0.5f;
            
            if (!Mathf.Approximately(collision.rigidbody.linearVelocityY, 0.0f))
            {
                Vector2 direction = _rb.linearVelocity * (1 - paddleInfluence) + collision.rigidbody.linearVelocity * paddleInfluence;
                float ballSpeedIncrease = Gamebihave.Instance != null ? 1f + Gamebihave.Instance.ballSpeedIncrease : 1.1f;
                _rb.linearVelocity = _rb.linearVelocity.magnitude * direction.normalized * ballSpeedIncrease;
            }
            else
            {
                float ballSpeedIncrease = Gamebihave.Instance != null ? 1f + Gamebihave.Instance.ballSpeedIncrease : 1.1f;
                _rb.linearVelocity *= ballSpeedIncrease;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ScoreZone"))
        {
            // increment score when ball drops into scoring area
            if (Gamebihave.Instance != null)
            {
                Gamebihave.Instance.ScorePoint(0); // using player index 0 for single player scoring
            }
            ResetBall();
        }
    }

    private void ResetBall()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = Vector3.zero;
        Vector2 direction = new Vector2(Utilitis.GetNonZeroRandomFloat(), Utilitis.GetNonZeroRandomFloat()).normalized;

        // Use ball speed from Game Manager
        float ballSpeed = Gamebihave.Instance != null ? Gamebihave.Instance.ballSpeed : _launchForce;
        _rb.AddForce(direction * ballSpeed, ForceMode2D.Impulse);
    }
}
