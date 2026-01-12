using Unity.VisualScripting;
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

    void Start()
    {
        Cursor.visible = false;
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {


        if (whiteBall == null) return;

        bool isBallMoving = whiteBall.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0.1f; 

        if (isBallMoving) 
        {
           
            gameObject.SetActive(false); 
            return;
        }
        else
        {
            gameObject.SetActive(true); 
        }

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
          
             ApplyBallForce();
           
        }

        RotateQueStick(); 
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
            //ApplyBallForce();
            transform.position = cueStartPos;
        }
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
        float dynamicGap = Mathf.Clamp(mouseDistance, 0.5f, 1.5f); 

        Vector3 targetPos = whiteBall.position + (dirNormalized * -dynamicGap);
        transform.position = new Vector3(targetPos.x, targetPos.y, 0); 

        float angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);


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