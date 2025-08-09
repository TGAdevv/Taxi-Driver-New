using UnityEngine;
using System.Collections.Generic;

public class TaxiLaneSwitcher : MonoBehaviour
{
    // Stel de X-posities van de rijbanen in (links, midden, rechts)
    private int currentLane = 1; // Start in het midden (index 1)
    public float Base_laneSwitchSpeed = 1f; // Hoe snel de taxi naar de nieuwe baan beweegt
    public int maxLanes;

    public Transform center;

    [SerializeField] AnimationCurve laneSwitchSpeedOverCarSpeed;

    public float carSpeed = 0;
    float centerSpeed = 0;

    public float accelerationSpeed = 5;
    public float brakeSpeed = 20;
    public float maxSpeed = 20;
    public float laneWidth;

    float distanceCenter;
    float distanceTransform;
    float degreesBeforeRot;

    public LineRenderer[] renderedLanes = new LineRenderer[4];

    bool busyGenerating = true;
    void generateLanes(int steps, float simulatedSpeed) 
    {
        busyGenerating = true;
        Vector3 centerPosBefore = center.position;
        Quaternion centerRotBefore = center.rotation;
        Vector3 transformPosBefore = transform.position;
        Quaternion transformRotBefore = transform.rotation;
        float distBefore = distanceCenter;
        float distTransBefore = distanceTransform;
        float carSpeedBefore = carSpeed;
        carSpeed = simulatedSpeed;

        List<Vector3> lane0 = new List<Vector3>();
        List<Vector3> lane1 = new List<Vector3>();
        List<Vector3> lane2 = new List<Vector3>();
        List<Vector3> lane3 = new List<Vector3>();

        for (int i = 0; i < steps; i++)
        {
            RotateTick(10, 20, new Vector3(0, .5f, -42), true);

            distanceCenter    += centerSpeed * Time.deltaTime;
            distanceTransform += carSpeed * Time.deltaTime;

            center.position -= Vector3.up * .5f;

            lane0.Add(center.position + (center.right * -3));
            lane1.Add(center.position + (center.right * -1));
            lane2.Add(center.position + (center.right));
            lane3.Add(center.position + (center.right * 3));

            center.position += Vector3.up * .5f;
        }

        foreach (LineRenderer line in renderedLanes)
            line.positionCount = lane0.Count;

        renderedLanes[0].SetPositions(lane0.ToArray());
        renderedLanes[1].SetPositions(lane1.ToArray());
        renderedLanes[2].SetPositions(lane2.ToArray());
        renderedLanes[3].SetPositions(lane3.ToArray());

        center.SetPositionAndRotation(centerPosBefore, centerRotBefore);
        transform.SetPositionAndRotation(transformPosBefore, transformRotBefore);

        distanceCenter = distBefore;
        distanceTransform = distTransBefore;
        carSpeed = carSpeedBefore;
        busyGenerating = false;
        t = 0;
        completedTurn = false;
    }

    private void Start()
    {
        generateLanes(10000, 0.5f);
    }

    float t = 0;
    bool completedTurn = false;
    void RotateTick(float beginDist, float radius, Vector3 beginPoint, bool rightTurn, bool affectTransform = true) 
    {
        Vector3 origin = beginPoint + (Vector3.right * radius);

        float endDist = beginDist + (.5f * Mathf.PI * radius);

        float turnDist = endDist - beginDist;

        float radiusCurLane = radius - (((float)currentLane - 1.0f)) * laneWidth;
        float turnDistCurLane = .5f * Mathf.PI * radiusCurLane;

        if (t == 1)
        {
            center.rotation = Quaternion.Euler(0, degreesBeforeRot + (rightTurn ? 90 : -90), 0);
            transform.rotation = Quaternion.Euler(0, degreesBeforeRot + (rightTurn ? 90 : -90), 0);
            t = 0;
            completedTurn = true;
        }
        if (distanceTransform > beginDist && t != 1 && !completedTurn)
        {
            centerSpeed = carSpeed * (turnDist / turnDistCurLane);
            t += carSpeed / turnDistCurLane * Time.deltaTime;

            t = Mathf.Clamp(t, 0, 1);

            float angle = Mathf.PI * (1 - (.5f*t));
            float lerpRadiusPos = Mathf.Lerp(Vector3.Distance(transform.position, origin), radiusCurLane, Time.deltaTime * laneSwitchSpeed);

            if (affectTransform) 
            {
                transform.rotation = Quaternion.Euler(0, t*90, 0);
                transform.position = origin + new Vector3(Mathf.Cos(angle) * lerpRadiusPos, 0, Mathf.Sin(angle) * lerpRadiusPos);
            }
            center.rotation = Quaternion.Euler(0, t*90, 0);
            center.position = origin + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }
        else 
        {
            transform.position += carSpeed * Time.deltaTime * transform.forward;
            center.position    += carSpeed * Time.deltaTime * transform.forward;
        }
    }

    float dist;
    float laneSwitchSpeed;

    void Update()
    {
        laneSwitchSpeed = laneSwitchSpeedOverCarSpeed.Evaluate(carSpeed / maxSpeed) * Base_laneSwitchSpeed;

        if (busyGenerating)
            return;

        Vector3 posBefore = transform.position;
        float vertical = Input.GetAxisRaw("Vertical");

        // Links
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0 && laneSwitchSpeed > 0.5f)
                currentLane--;
        }
        // Rechts
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < maxLanes - 1 && laneSwitchSpeed > 0.5f)
                currentLane++;
        }

        if (vertical > 0)
            carSpeed += accelerationSpeed * Time.deltaTime * vertical;
        if (vertical < 0 && carSpeed > 0)
            carSpeed += brakeSpeed * Time.deltaTime * vertical;
        carSpeed = Mathf.Clamp(carSpeed, 0, maxSpeed);

        RotateTick(10, 20, new Vector3(0, .5f, -42), true);

        // Soepele overgang naar de nieuwe rijbaan
        Vector3 targetPosition = center.position + (currentLane - 1) * center.right * laneWidth;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * laneSwitchSpeed);

        distanceCenter    += centerSpeed * Time.deltaTime;
        distanceTransform += carSpeed * Time.deltaTime;

        dist += Vector3.Distance(transform.position, posBefore);
        //print(dist);
    }
}