using UnityEngine;
using System.Collections.Generic;

public class TaxiLaneSwitcher : MonoBehaviour
{
    // Stel de X-posities van de rijbanen in (links, midden, rechts)
    private int currentLane = 1; // Start in het midden (index 1)
    public float laneSwitchSpeed = 10f; // Hoe snel de taxi naar de nieuwe baan beweegt
    public int maxLanes;

    public Transform center;

    public float carSpeed = 0;
    float centerSpeed = 0;

    public float accelerationSpeed = 5;
    public float brakeSpeed = 20;
    public float maxSpeed = 20;
    public float laneWidth;

    float distance;
    float degreesBeforeRot;

    public LineRenderer[] renderedLanes = new LineRenderer[4];

    bool busyGenerating = true;
    void generateLanes(int steps, float simulatedSpeed) 
    {
        busyGenerating = true;
        Transform centerBefore = center;
        Transform transformBefore = transform;
        float distBefore = distance;
        float carSpeedBefore = carSpeed;
        carSpeed = simulatedSpeed;

        List<Vector3> lane0 = new List<Vector3>();
        List<Vector3> lane1 = new List<Vector3>();
        List<Vector3> lane2 = new List<Vector3>();
        List<Vector3> lane3 = new List<Vector3>();

        for (int i = 0; i < steps; i++)
        {
            RotateTick(10, 50, 90, false);
            center.position += center.forward * centerSpeed * Time.deltaTime;
            distance += centerSpeed * Time.deltaTime;

            lane0.Add(center.position + (center.right * -3));
            lane1.Add(center.position + (center.right * -1));
            lane2.Add(center.position + (center.right));
            lane3.Add(center.position + (center.right * 3));
        }

        foreach (LineRenderer line in renderedLanes)
            line.positionCount = lane0.Count;

        renderedLanes[0].SetPositions(lane0.ToArray());
        renderedLanes[1].SetPositions(lane1.ToArray());
        renderedLanes[2].SetPositions(lane2.ToArray());
        renderedLanes[3].SetPositions(lane3.ToArray());

        for (int i = 0; i < 1000; i++)
        {
            center.position = centerBefore.position;
            center.rotation = centerBefore.rotation;

            transform.position = transformBefore.position;
            transform.rotation = transformBefore.rotation;
        }

        distance = distBefore;
        carSpeed = carSpeedBefore;
        busyGenerating = false;
    }

    private void Start()
    {
        generateLanes(1000, 60);
        print("r: " + (40)/(.5f*Mathf.PI));
    }

    void RotateTick(float beginDist, float endDist, float degrees, bool affectTransform = true) 
    {
        float turnDist = endDist - beginDist;
        float radius = turnDist / (.5f * Mathf.PI);
        float turnDistCurLane = .5f * Mathf.PI * (radius - (((float)currentLane - 1.0f)) * laneWidth);

        //print(turnDist + " vs " + turnDistCurLane);

        if (distance > beginDist && distance < endDist)
        {
            centerSpeed = carSpeed * (turnDist / turnDistCurLane);
            float rotSpeed = (carSpeed / turnDistCurLane) * degrees * Time.deltaTime;
            if (affectTransform)
                transform.Rotate(rotSpeed * Vector3.up);
            center.Rotate   (rotSpeed * Vector3.up);
        }
        else
            centerSpeed = carSpeed;

        if (transform.rotation.eulerAngles.y > degreesBeforeRot + degrees && affectTransform) 
        {
            transform.rotation = Quaternion.Euler(0, degreesBeforeRot + degrees, 0);
        }
        if (center.rotation.eulerAngles.y > degreesBeforeRot + degrees)
        {
            center.rotation = Quaternion.Euler(0, degreesBeforeRot + degrees, 0);
        }
    }

    float dist;

    void Update()
    {
        if (busyGenerating)
            return;

        Vector3 posBefore = transform.position;
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical > 0)
            carSpeed += accelerationSpeed * Time.deltaTime * vertical;
        if (vertical < 0 && carSpeed > 0)
            carSpeed += brakeSpeed * Time.deltaTime * vertical;
        carSpeed = Mathf.Clamp(carSpeed, 0, maxSpeed);

        RotateTick(10, 50, 90);

        // Links
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0)
                currentLane--;
        }
        // Rechts
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < maxLanes - 1)
                currentLane++;
        }

        // Soepele overgang naar de nieuwe rijbaan
        Vector3 targetPosition = center.position + (currentLane - 1) * center.right * laneWidth;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * laneSwitchSpeed);

        transform.position += transform.forward * carSpeed    * Time.deltaTime;
        center.position    += center.forward    * centerSpeed * Time.deltaTime;

        distance += centerSpeed * Time.deltaTime;

        dist += Vector3.Distance(transform.position, posBefore);
        //print(dist);
    }
}