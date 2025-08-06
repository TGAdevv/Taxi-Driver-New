using UnityEngine;

public class TaxiController : MonoBehaviour
{
    public float movementSpeed;
    public int maxLanes = 3;

    int lane = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            if (lane > -1)
                lane--;

            transform.position -= transform.right * 2;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (lane < maxLanes-1)
                lane++;

            transform.position += transform.right * 2;
        }

        transform.Translate(transform.forward * movementSpeed);
    }
}
