using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D.IK;

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
        
        // THE FIX: Get the actual distance (magnitude)
        float mouseDistance = fullDir.magnitude;

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
        if (lr == null) return;

        Vector3 origin = (raycastOriginPoint != null) ? raycastOriginPoint.position : transform.position;
        Vector3 direction = (whiteBall.position - origin).normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 2000f, collisionLayers);

        if (hit.collider != null)
        {
            lr.positionCount = 3; 
            lr.SetPosition(0, origin);
            lr.SetPosition(1, hit.point); 

            Vector3 targetBallCenter = hit.collider.transform.position;
            Vector3 trajectoryDir = (targetBallCenter - (Vector3)hit.point).normalized;
            lr.SetPosition(2, (Vector3)hit.point + (trajectoryDir * 1200f)); 
        }
        else
        {
            lr.positionCount = 2;
            lr.SetPosition(0, origin);
            lr.SetPosition(1, origin + (direction * 2000f));
        }

        lr.startWidth = 25f; 
        lr.endWidth = 25f;
        lr.startColor = Color.white; 
        lr.endColor = Color.white;
    }
}