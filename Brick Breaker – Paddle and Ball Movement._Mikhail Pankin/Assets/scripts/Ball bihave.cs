using UnityEngine;

public class Ballbihave : MonoBehaviour
{
    [SerializeField] private float _launchForce = 5.0f;
    [SerializeField] private float _paddleInfluence = 0.5f;
    [SerializeField] private float _ballSpeedIncrement = 1.1f;

    private Rigidbody2D _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearDamping = 0.0f;
        _rb.angularDamping = 0.0f;
        _rb.gravityScale = 0.0f;

        ResetBall();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            if (!Mathf.Approximately(collision.rigidbody.linearVelocityY, 0.0f))
            {
                Vector2 direction = _rb.linearVelocity * (1 - _paddleInfluence) + collision.rigidbody.linearVelocity * _paddleInfluence;
                _rb.linearVelocity = _rb.linearVelocity.magnitude * direction.normalized * _ballSpeedIncrement;
            }
            _rb.linearVelocity *= _ballSpeedIncrement;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ScoreZone"))
        {
            ResetBall();
        }
    }

    private void ResetBall()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = Vector3.zero;
        Vector2 direction = new Vector2(getnonzerorandomfloat(), getnonzerorandomfloat()).normalized;

        _rb.AddForce(direction * _launchForce, ForceMode2D.Impulse);
    }

    // Generate a random float that's not zero to ensure the ball always moves
    float getnonzerorandomfloat(float min = -1.0f, float max = 1.0f)
    {
        float num = 0.0f;

        do
        {
            num = Random.Range(min, max);
        }
        while (Mathf.Approximately(num, 0.0f));

        return num;
    }
}
