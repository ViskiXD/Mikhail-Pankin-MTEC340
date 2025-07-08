using UnityEngine;

public static class Utilities
{
    public enum GameState
    {
        Play,
        Pause,
        GameOver
    }
    
    // Helper function to get a non-zero random float between -1 and 1
    public static float GetNonZeroRandomFloat()
    {
        float value = 0f;
        while (Mathf.Approximately(value, 0f))
        {
            value = Random.Range(-1f, 1f);
        }
        return value;
    }
    
    // Helper function to get random spawn timing variation
    public static float GetRandomSpawnDelay(float baseDelay, float variation = 0.3f)
    {
        return baseDelay + Random.Range(-variation, variation);
    }
    
    // Helper function to choose random element from array
    public static T GetRandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);
            
        return array[Random.Range(0, array.Length)];
    }
    
    // Helper to check if game object is within screen bounds
    public static bool IsWithinScreenBounds(Transform transform, Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;
            
        Vector3 screenPoint = camera.WorldToViewportPoint(transform.position);
        return screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1;
    }
} 