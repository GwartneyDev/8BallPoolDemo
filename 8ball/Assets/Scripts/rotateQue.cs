using UnityEngine;

public class rotateQue : MonoBehaviour
{
    [Header("Settings")]
    public Transform whiteBall;
    public LayerMask collisionLayers;
    public float maxPullDistance = 5f; 
    public float forceMultiplier = 20f; 

    [Header("References")]
    public Transform raycastOriginPoint; 
    private LineRenderer lr;
    private Rigidbody2D whiteBallRb;

    private float currentPullDistance = 0f;
    private Vector3 currentPullDir = Vector3.zero;
    private bool isPulling = false; 

    void Start()
    {
        Cursor.visible = false;
        lr = GetComponent<LineRenderer>();
        if (whiteBall != null) whiteBallRb = whiteBall.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (whiteBall == null) return;

        // Check if ball is moving
        bool isBallMoving = whiteBallRb.linearVelocity.magnitude > 0.1f; 

        // Instead of SetActive(false), we just disable the visual renderer 
        // so the script keeps running to check if the ball stopped.
        ToggleVisuals(!isBallMoving);

        if (isBallMoving) return;

        // INPUT HANDLING
        if (Input.GetMouseButtonDown(0))
        {
            isPulling = true;
        }

        if (isPulling)
        {
            HandleSpringPull();
            if (Input.GetMouseButtonUp(0))
            {
                ApplyBallForce();
                isPulling = false;
            }
        }
        else
        {
            RotateQueStick(); // Only rotate when NOT pulling
        }

        DrawAimLine();
    }

    private void HandleSpringPull()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        
        // We calculate direction FROM the ball TO the mouse
        Vector3 pullVector = mouseWorldPos - whiteBall.position;
        
        // Clamp the distance
        currentPullDistance = Mathf.Min(pullVector.magnitude, maxPullDistance);
        currentPullDir = -pullVector.normalized; // Direction to shoot is opposite of pull

        // Position the cue: Ball position + (Inverse of shoot direction * distance)
        Vector3 targetPos = whiteBall.position + (pullVector.normalized * currentPullDistance);
        transform.position = new Vector3(targetPos.x, targetPos.y, 0);

        // Rotate to face the ball
        float angle = Mathf.Atan2(pullVector.y, pullVector.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void RotateQueStick()
    {
        Vector3 mouseWorldPos = GetMouseWorldPos();
        Vector3 dir = mouseWorldPos - whiteBall.position;
        
        float mouseDistance = dir.magnitude;
        if (mouseDistance < 0.1f) return;

        // Visual gap between cue and ball
        float dynamicGap = Mathf.Clamp(mouseDistance, 0.5f, 1.5f); 

        Vector3 targetPos = whiteBall.position + (dir.normalized * dynamicGap);
        transform.position = new Vector3(targetPos.x, targetPos.y, 0); 

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void ApplyBallForce()
    {
        // currentPullDir is already normalized and pointing toward the shot target
        Vector2 force = (Vector2)currentPullDir * currentPullDistance * forceMultiplier;
        whiteBallRb.AddForce(force, ForceMode2D.Impulse);

        // Reset
        currentPullDistance = 0f;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    private void ToggleVisuals(bool show)
    {
        // This keeps the script alive but hides the cue and line
        if (lr) lr.enabled = show;
        // Assuming the cue has a SpriteRenderer or MeshRenderer
        Renderer r = GetComponent<Renderer>();
        if (r) r.enabled = show;
        
        // If your cue has children (the stick graphics), toggle them:
        foreach (Transform child in transform) child.gameObject.SetActive(show);
    }

    public void DrawAimLine()
    {
        if (lr == null || !lr.enabled) return;

        Vector3 origin = whiteBall.position;
        // Direction is where the stick is pointing (away from the stick toward the ball)
        Vector3 direction = currentPullDir.magnitude < 0.001f ? (whiteBall.position - transform.position).normalized : currentPullDir;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, 20f, collisionLayers);

        lr.positionCount = 2;
        lr.SetPosition(0, origin);

        if (hit.collider != null)
        {
            lr.SetPosition(1, hit.point);
        }
        else
        {
            lr.SetPosition(1, origin + (direction * 20f));
        }
    }
}