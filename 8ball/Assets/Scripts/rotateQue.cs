using UnityEngine;

public class rotateQue : MonoBehaviour
{
    [Header("Settings")]
    public Transform whiteBall;
    public LayerMask collisionLayers;

    [Header("Spring & Strike Settings")]
    public float springTension = 0.1f; 
    public float strikeSpeed = 800f;   
    public float maxPullDistance = 20f; 

    [Header("References")]
    public Transform raycastOriginPoint; 
    private LineRenderer lr;

    private Vector3 initialMousePos;
    private Vector3 cueStartPos;
    
    // ERROR FIX: Variable name must match capitalization used in Update (isPulling)
    private bool isPulling = false; 

    void Start()
    {
        Cursor.visible = false;
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (whiteBall == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isPulling = true;
            initialMousePos = Input.mousePosition;
            cueStartPos = transform.position; 
        }

        if (isPulling)
        {
            HandleSpringPull();
        }
        else
        {  
            // ERROR FIX: Removed the transform.position line here because 'targetPos' 
            // does not exist in Update. RotateQueStick() handles its own positioning.
            RotateQueStick(); 
        }

        DrawAimLine();
    }

    void Han         }umousePosition;
        Vector3 mouseDelta = currentMousePos - initialMousePos;

        float pullDistance = Mathf.Clamp(mouseDelta.magnitude * 0.02f, 0f, Mathf.Abs(maxPullDistance));

        Vector3 dirFromBall = (transform.position - whiteBall.position).normalized;
        Vector3 targetPos = cueStartPos + (dirFromBall * pullDistance);
        
        transform.position = new Vector3(targetPos.x, targetPos.y, 0); 
    }

    void RotateQueStick()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z); 
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        
        Vector3 fullDir = mouseWorldPos - whiteBall.position;
        float mouseDistance = fullDir.magnitude;
        if (mouseDistance < 0.1f) return;

        Vector3 dirNormalized = fullDir.normalized;
        float dynamicGap = Mathf.Clamp(mouseDistance, 100f, 600f); 

        Vector3 targetPos = whiteBall.position + (dirNormalized * -dynamicGap);
        transform.position = new Vector3(targetPos.x, targetPos.y, 0); 

        float angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void ApplyBallForce() {}

    public void DrawAimLine()
    {
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