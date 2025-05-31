using UnityEngine;
using UnityEngine;

public class ControlFlow : MonoBehaviour
{
    // 1. Public boolean field for Inspector
    public bool flag;

    // 2. Start() method
    void Start()
    {
        // Check the flag and print the appropriate message
        if (flag)
        {
            Debug.Log("Boolean flag is set");
        }
        else
        {
            Debug.Log("Boolean flag isn’t set");
        }

        // 3. Loop to print the first ten powers of 2
        for (int i = 1; i <= 10; i++)
        {
            int powerOfTwo = (int)Mathf.Pow(2, i);
            Debug.Log($"The {i} power of 2 is {powerOfTwo}");
        }
    }
}

