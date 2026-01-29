
 
using UnityEngine;
 

public class rotateQue : MonoBehaviour
{
    [Header("Settings")]
    public Transform whiteBall;
    public LayerMask collisionLayers;

    [Header("Spring & Strike Settings")]
   
    public float maxPullDistance = 10f; 

    [Header("References")]
    public Transform raycastOriginPoint; 
    private LineRenderer lr;

    private Vector3 initialMousePos;
    private Vector3 cueStartPos;
    private float currentPullDistance = 0f;
    private Vector3 currentPullDir = Vector3.zero;
    
    // ERROR FIX: Variable name must match capitalization used in Update (isPulling)
    private bool isPulling = false; 

    public Vector2 pushDir;


    public float mouseDistance;
Vector2 blackRayTipPos;
    Vector2 ghostPos;
    Vector2 slideDir;

    [Header("References")]
 
    public Transform tipCircle; // DRAG YOUR YELLOW CIRCLE SPRITE HERE

    private Vector3 lastSphereCenter;
    private float sphereRadius = 16f;
    private bool shouldDrawSphere = false;

    void Start()
    {
       
        Cursor.visible = true;
        lr = GetComponent<LineRenderer>();
    }

        void Update()
{
    if (whiteBall == null) return;

    // 1. Check if the ball is moving
    // If moving, we hide everything and stop all input logic
    bool isBallMoving = whiteBall.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0.1f; 

    if (isBallMoving)
    {
        // Hide cue and line while ball is in motion
        GetComponent<SpriteRenderer>().enabled = false;
        lr.enabled = false;
        isPulling = false; // Reset pull state so it doesn't "stick"
        return; // Exit early so no clicks are processed
    }
    else 
    {
        // Show cue and line when ball is still
        GetComponent<SpriteRenderer>().enabled = true;
        lr.enabled = true;
    }

    // 2. Click Handling (Only runs if ball is NOT moving)
    if (Input.GetMouseButtonDown(0))
    {
        isPulling = true;
        initialMousePos = Input.mousePosition;
        cueStartPos = transform.position;
    }

    // 3. State Management
    if (isPulling)
    {
        HandleSpringPull();
        // Keep visual feedback during the pull if you want, 
        // or hide them here if you want it invisible DURING the drag.
    }
    else
    {
        RotateQueStick(); 
    }

    // 4. Draw the aim line
    DrawAimLine();
}

    private void HandleSpringPull()
    {
          Vector3 mouseworldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));
          Vector3 pullVector = mouseworldPos - whiteBall.position;
          currentPullDistance = Mathf.Min(pullVector.magnitude, maxPullDistance);
          currentPullDir = pullVector.normalized;
          Vector3 targetPos = whiteBall.position + (currentPullDir * -currentPullDistance);
          transform.position = new Vector3(targetPos.x, targetPos.y, 0);
          transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(currentPullDir.y, currentPullDir.x) * Mathf.Rad2Deg);


        if (Input.GetMouseButtonUp(0))
        {
            isPulling = false;
            ApplyBallForce();
            transform.position = cueStartPos;
        }
    }

    void RotateQueStick()
    {
       
        // Convert mouse position to world position
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); 
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0; 

        // Get the vector from the ball to your mouse
        Vector3 fullDir = mouseWorldPos - whiteBall.position;
        
        // Get the actual length (magnitude)
        mouseDistance = fullDir.magnitude;

        if (mouseDistance < 0.1f) return;

        // Get the direction
        Vector3 dirNormalized = fullDir / mouseDistance;

        // APPLY MAGNITUDE: Use the mouse distance to set the cue's gap
        // Adjust 150 and 800 to fit your Scale 70 table
        float dynamicGap = Mathf.Clamp(mouseDistance, 100, 200); 

        transform.position = whiteBall.position + (dirNormalized * -dynamicGap);

        transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        // ROTATION
        float angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        
        DrawAimLine();
}

    void ApplyBallForce()
    {
        if (whiteBall == null) return;

        Vector2 force = currentPullDir * currentPullDistance * 20f;

        Rigidbody2D rb2d = whiteBall.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.AddForce(force, ForceMode2D.Impulse);
        }
        else
        {
            Rigidbody rb = whiteBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(new Vector3(force.x, force.y, 0f), ForceMode.Impulse);
            }
        }

        currentPullDistance = 0f;
        currentPullDir = Vector3.zero;
    }

public void DrawAimLine()
{
    if (whiteBall == null || raycastOriginPoint == null) return;

    Vector2 cueTip = raycastOriginPoint.position;
    Vector2 whitePos = whiteBall.position;

    // Direction from white ball forward (away from cue)
    Vector2 aimDir = (whitePos - cueTip).normalized;

    float ballRadius = 16f;
    float castDistance = 1000f;

    // Always draw cue → white ball
    Debug.DrawLine(cueTip, ghostPos, Color.magenta, 0f, false);

    // Always draw white ball forward direction
    //Debug.DrawRay(whitePos, aimDir * castDistance, Color.yellow, 0f, false);
   
    // Start cast slightly in front of white ball
    Vector2 castStart = whitePos + aimDir * ballRadius;

    RaycastHit2D hit = Physics2D.CircleCast(
        castStart,
        ballRadius,
        aimDir,
        castDistance,
        collisionLayers
    );


 
 
if (hit.collider != null)
{
    // hit.centroid is the exact center of the ghost ball when it "touches" the target
   
    // Debug.DrawRay(whitePos, aimDir * castDistance, Color.yellow, 0f, false);
    
    // If we hit a ball, store sphere data for OnDrawGizmos
    if (hit.collider)
    {
        lastSphereCenter = (Vector3)whitePos + (Vector3)aimDir * hit.distance;
        shouldDrawSphere = true;
        ghostPos = hit.point - hit.normal * ballRadius;
        pushDir = hit.normal.normalized;
        slideDir = new Vector2(-pushDir.y, pushDir.x);

        blackRayTipPos = (Vector3)hit.point + (new Vector3(pushDir.x, pushDir.y, 0) * mouseDistance);
        
        Debug.DrawLine(whitePos, ghostPos, Color.green, 0f, false);
        Debug.DrawRay(hit.point, pushDir * mouseDistance , Color.black, 0f, false);
      

        if (tipCircle != null)
            tipCircle.position = ghostPos;
    }
   
    
}

void OnDrawGizmos(){
    // Draw the last sphere cast position
        if (shouldDrawSphere)
        {

            Gizmos.color = Color.blue;
            
            Gizmos.DrawWireSphere(lastSphereCenter, sphereRadius);
 
}
        }

    }
}