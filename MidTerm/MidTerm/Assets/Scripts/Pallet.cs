using UnityEngine;

public class Pallet : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private Transform[] _positions = new Transform[4]; // 4 positions where pallet can be
    
    private int _currentPosition = 1; // Start at position 1 (middle-left)
    private bool _isMoving = false;
    
    void Start()
    {
        // Set initial position
        if (_positions[_currentPosition] != null)
        {
            transform.position = _positions[_currentPosition].position;
        }
    }

    void Update()
    {
        HandleInput();
        
        // Don't move if game is paused or exploded
        if (EggGameManager.Instance != null && (EggGameManager.Instance.IsPaused() || EggGameManager.Instance.IsExploded())) return;
        
        // Move towards target position if not already there
        if (_positions[_currentPosition] != null)
        {
            Vector3 targetPos = _positions[_currentPosition].position;
            // Allow both X and Y movement to reach all 4 positions
            
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                _isMoving = true;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
            }
            else
            {
                _isMoving = false;
                transform.position = targetPos;
            }
        }
    }
    
    private void HandleInput()
    {
        if (_isMoving) return; // Don't accept new input while moving
        
        // Don't accept input if game is paused or exploded
        if (EggGameManager.Instance != null && (EggGameManager.Instance.IsPaused() || EggGameManager.Instance.IsExploded())) return;
        
        // Simple input handling like brick breaker
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            MoveTo(0); // Position 1 - Top Left
        }
        else if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Alpha2))
        {
            MoveTo(1); // Position 2 - Top Right
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Alpha3))
        {
            MoveTo(2); // Position 3 - Bottom Left
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            MoveTo(3); // Position 4 - Bottom Right
        }
    }
    
    private void MoveTo(int positionIndex)
    {
        if (positionIndex >= 0 && positionIndex < _positions.Length && _positions[positionIndex] != null)
        {
            _currentPosition = positionIndex;
            Debug.Log($"Moving to position {positionIndex}");
        }
        else
        {
            Debug.LogWarning($"Cannot move to position {positionIndex} - Transform is null or invalid!");
        }
    }
    
    // Optional: visualize positions in scene view
    void OnDrawGizmos()
    {
        if (_positions != null)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _positions.Length; i++)
            {
                if (_positions[i] != null)
                {
                    Gizmos.DrawWireSphere(_positions[i].position, 0.5f);
                }
            }
        }
    }
}
