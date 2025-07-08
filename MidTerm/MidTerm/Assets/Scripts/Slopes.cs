using UnityEngine;

public class Slopes : MonoBehaviour
{
    [SerializeField] private PhysicsMaterial2D _slopeMaterial;
    
    void Start()
    {
        // Ensure the slope has a collider for physics interactions
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Apply physics material if provided
        if (_slopeMaterial != null)
        {
            collider.sharedMaterial = _slopeMaterial;
        }
    }
    
    // Slopes don't need update logic - they just exist for physics
}
