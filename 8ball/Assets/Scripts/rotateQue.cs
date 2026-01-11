using UnityEngine;


// public void DrawAimLine();
public class rotateQue : MonoBehaviour
{
    [Header("Settings")]
    public Transform whiteBall;         // Drag WhiteBall here
    
    public float distanceFromBall = 150f; // Increased for your Scale 70 objects
    public float lineLength = 2000f;    // Increased for your Scale 70 objects
    public LayerMask collisionLayers;   // Select 'BallColoision' and 'Walls' in Inspector

    public Transform reflectedObject;

    [Header("References")]
    // The child object at the tip of the cue (can leave null if using transform.position)
    public Transform raycastOriginPoint; 
    private LineRenderer lr;
    
    // Removed unused 3D variables from previous suggestions

    void Start()
    {
        Cursor.visible = false;
        // Ensure the Line Renderer component exists on this GameObject
        lr = GetComponent<LineRenderer>(); 

        if (Camera.main == null) {
            Debug.LogError("No Camera found with the 'MainCamera' tag!");
        }
    }

    void Update()
    {
        // Safety check
        if (whiteBall == null) return;

        RotateQueStick();
        DrawAimLine();
    }

    void RotateQueStick()
    {
        // 1. Get Mouse Position in World Coordinates
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); 
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0; 

        // 2. Calculate the Direction Vector from Ball to Mouse
        Vector3 fullDir = mouseWorldPos - whiteBall.position;
        float mouseDistance = fullDir.magnitude;

        if (mouseDistance < 0.1f) return;

        Vector3 dirNormalized = fullDir.normalized;

        // 3. Dynamic Gap Logic
        // Positions the cue based on mouse distance from the ball
        float dynamicGap = Mathf.Clamp(mouseDistance, 100f, 600f); 

        Vector3 targetPos = whiteBall.position + (dirNormalized * -dynamicGap);
        targetPos.z = 0; 
        transform.position = targetPos;

        // 4. Rotation Lock (2D Rotation around Z-axis)
        float angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

 public void DrawAimLine()
{
    // 1. Setup the direction
    Vector3 origin = (raycastOriginPoint != null) ? raycastOriginPoint.position : transform.position;
    Vector3 direction = (whiteBall.position - origin).normalized;

    // 2. THE PIERCING RAY: This gets EVERY hit along the line
    RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, 2000f, collisionLayers);

    if (hits.Length > 0)
    {
        lr.positionCount = 3;
        lr.SetPosition(0, origin);

        // We want the hit on the OTHER side of the ball
        // If hits[0] is the back of the ball, hits[1] is the front/exit
        RaycastHit2D targetHit = (hits.Length > 1) ? hits[1] : hits[0];

        lr.SetPosition(1, targetHit.point);

        // 3. THE STEERING HEADING: From the ball's center to the exit point
        Vector3 ballCenter = targetHit.collider.transform.position;
        Vector3 steeringDir = ((Vector3)targetHit.point - ballCenter).normalized;

        // Project the neon green steering line forward
        lr.SetPosition(2, (Vector3)targetHit.point + (steeringDir * 800f));
    }
    else
    {
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + (direction * 2000f));
    }

   
    lr.startWidth = 20f; //
    lr.endWidth = 20f;
    lr.startColor = Color.green;
    lr.endColor = Color.green;
    
   
}
}