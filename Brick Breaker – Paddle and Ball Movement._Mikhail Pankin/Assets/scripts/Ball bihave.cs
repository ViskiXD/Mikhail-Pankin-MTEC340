using UnityEngine;

public class Ballbihave : MonoBehaviour
{
    public Vector2 Speed = new (10.0f, 10.0f);

    void Start()
    {
        int randomDirection = Random.Range(0, 10);
        
        switch (randomDirection)
        {
            case 0:
                Speed = new Vector2(-4.0f, -10.0f);
                break;
            case 1:
                Speed = new Vector2(-3.0f, -10.0f);
                break;
            case 2:
                Speed = new Vector2(-2.0f, -10.0f);
                break;
            case 3:
                Speed = new Vector2(-1.0f, -10.0f);
                break;
            case 4:
                Speed = new Vector2(-0.5f, -10.0f);
                break;
            case 5:
                Speed = new Vector2(0.5f, -10.0f);
                break;
            case 6:
                Speed = new Vector2(1.0f, -10.0f);
                break;
            case 7:
                Speed = new Vector2(2.0f, -10.0f);
                break;
            case 8:
                Speed = new Vector2(3.0f, -10.0f);
                break;
            case 9:
                Speed = new Vector2(4.0f, -10.0f);
                break;
        }
    }
//tried to make ball to go more random then just same direction every time, only possible option for me to make it was switches cause of lack of expirience
    
    void Update()
    {
        transform.position += (Vector3)Speed * Time.deltaTime;
    }
}
