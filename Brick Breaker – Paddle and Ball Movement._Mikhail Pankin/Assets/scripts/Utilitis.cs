using UnityEngine;

// This class contains utility functions that are used throughout the game
public static class Utilities
{

    public enum GameState
    {
        Play, Pause, GameOver
    }


    public static Color[] colors =
    {
        
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.white,
        Color.gray,
    };





    // Generate a random float that's not zero to ensure the ball always moves
    //methods are static because they are not associated with any particular instance of the class
    //and can be called without creating an instance of the class
    public static float GetNonZeroRandomFloat(float min = -1.0f, float max = 1.0f)
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
