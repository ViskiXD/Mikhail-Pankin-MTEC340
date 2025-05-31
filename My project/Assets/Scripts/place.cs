using UnityEngine;

public class Console : MonoBehaviour
{
    public int x = 5;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(message: $"My number is {x}!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

