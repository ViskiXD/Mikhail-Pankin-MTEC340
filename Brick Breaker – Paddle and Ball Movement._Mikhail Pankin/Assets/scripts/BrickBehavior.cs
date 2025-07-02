using UnityEngine;

public class BrickBehavior : MonoBehaviour
{
    [SerializeField] private int _health = 2;
    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            
            _health--;
            
            
            if (_health == 1)
            {
                _spriteRenderer.color = Color.red;
            }
            
            else if (_health <= 0)
            {
                
                if (GameBehavior.Instance != null)
                {
                    
                    GameBehavior.Instance.ScorePoint(0); 
                    Debug.Log("Brick destroyed! Score updated.");
                }
                
                
                Destroy(gameObject);
            }
        }
    }

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = Utilities.colors[Random.Range(0, Utilities.colors.Length)];
        _spriteRenderer.color = _originalColor;
    }
}

