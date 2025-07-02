using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    [SerializeField] private float _launchForce = 5.0f;

    private Rigidbody2D _rb;
    private bool _isPaused = false;
    private Vector2 _previousVelocity;

    private AudioSource _audioSource;

    [SerializeField] private AudioClip _wallHitClip;
    [SerializeField] private AudioClip _paddleHitClip;
    [SerializeField] private AudioClip _scoreClip;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.linearDamping = 0.0f;
        _rb.angularDamping = 0.0f;
        _rb.gravityScale = 0.0f;

        // Set ball size from Game Manager
        if (GameBehavior.Instance != null)
        {
            transform.localScale = Vector3.one * GameBehavior.Instance.ballSize;
        }

        ResetBall();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            // Get paddle influence from Game Manager
            float paddleInfluence = GameBehavior.Instance != null ? GameBehavior.Instance.paddleInfluence : 0.5f;
            
            if (!Mathf.Approximately(collision.rigidbody.linearVelocityY, 0.0f))
            {
                Vector2 direction = _rb.linearVelocity * (1 - paddleInfluence) + collision.rigidbody.linearVelocity * paddleInfluence;
                float ballSpeedIncrease = GameBehavior.Instance != null ? 1f + GameBehavior.Instance.ballSpeedIncrease : 1.1f;
                _rb.linearVelocity = _rb.linearVelocity.magnitude * direction.normalized * ballSpeedIncrease;
            }
            else
            {
                float ballSpeedIncrease = GameBehavior.Instance != null ? 1f + GameBehavior.Instance.ballSpeedIncrease : 1.1f;
                _rb.linearVelocity *= ballSpeedIncrease;
            }
            _audioSource.clip = _paddleHitClip; //Update the audio source to the paddle hit clip
            _audioSource.Play(); //
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            _audioSource.clip = _wallHitClip;
            _audioSource.Play();
        }
        else if (collision.gameObject.CompareTag("ScoreZone"))
        {
            _audioSource.clip = _scoreClip;
            _audioSource.Play();
        }
        else
        {
            _audioSource.clip = _wallHitClip;
            _audioSource.Play();    
        }
    }

    private void Update()
    {
        if (GameBehavior.Instance != null && GameBehavior.Instance.CurrentState == Utilities.GameState.Pause && !_isPaused)
        {
            _isPaused = true;
            _previousVelocity = _rb.linearVelocity;
            _rb.linearVelocity = Vector2.zero;
        }
        else if (GameBehavior.Instance != null && GameBehavior.Instance.CurrentState == Utilities.GameState.Play && _isPaused)
        {
            _rb.linearVelocity = _previousVelocity;
            _isPaused = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ScoreZone"))
        {
            // increment score when ball drops into scoring area
            if (GameBehavior.Instance != null)
            {
                GameBehavior.Instance.ScorePoint(0); // using player index 0 for single player scoring
            }
            ResetBall();

            _audioSource.PlayOneShot(_scoreClip);
        }
    }

    private void ResetBall()
    {
        _rb.linearVelocity = Vector2.zero;
        transform.position = Vector3.zero;
        Vector2 direction = new Vector2(Utilities.GetNonZeroRandomFloat(), Utilities.GetNonZeroRandomFloat()).normalized;

        // Use ball speed from Game Manager
        float ballSpeed = GameBehavior.Instance != null ? GameBehavior.Instance.ballSpeed : _launchForce;
        _rb.AddForce(direction * ballSpeed, ForceMode2D.Impulse);
    }
}
