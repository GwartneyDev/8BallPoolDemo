using UnityEngine;

public class RotateQueStick : MonoBehaviour
{
    public Transform stick;
    public Transform Ball;

    public Vector3 start;
    public Vector3 RayDirectionEnd;
    public Vector3 mPos;

    private float currentPullDistance = 0f;
    private Vector3 currentPullDir = Vector3.zero;

    // Adjust this to change shot power
    public float powerScale = 20f;

    void ApplyBallForce()
    {
        if (Ball == null) return;

        // Calculate the direction from the stick to the ghost ball
        // This ensures the ball moves toward where you are aiming
        currentPullDir = (mPos - Ball.position).normalized;
        
        // For a simple click-to-shoot, we use a fixed distance or 
        // you can calculate it based on mouse distance if desired.
        currentPullDistance = 1f; 

        Vector2 force = currentPullDir * currentPullDistance * powerScale;

        Rigidbody2D rb2d = Ball.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.AddForce(force, ForceMode2D.Impulse);
        }
        else
        {
            Rigidbody rb = Ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(new Vector3(force.x, force.y, 0f), ForceMode.Impulse);
            }
        }

        Debug.Log("Ball hit in direction: " + currentPullDir);
    }

    public void Update()
    {
        if (Ball == null || stick == null) return;

        // 1. Get Mouse Pos and force it to the same Z as the White Ball
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        mouseWorldPos.z = 0;
        mPos = mouseWorldPos;

        // 2. Detection - ONLY check for things on the "Ball" or "Wall" layers
        int mask = LayerMask.GetMask("Ball", "Wall");
        Collider2D hit = Physics2D.OverlapCircle(mPos, 0.5f, mask);

        if (hit != null)
        {
            mPos = hit.ClosestPoint(mPos);
            mPos.z = Ball.position.z;

            Vector3 incomingDir = (mPos - start).normalized;
            string layerName = LayerMask.LayerToName(hit.gameObject.layer);

            if (layerName == "Ball")
            {
                Vector3 targetBallPath = (hit.transform.position - mPos).normalized;
                Debug.DrawRay(hit.transform.position, targetBallPath * 5f, Color.black);

                Vector3 perpendicular = new Vector3(-targetBallPath.y, targetBallPath.x, 0);
                Vector3 normal = (mPos - hit.transform.position).normalized;
                float hitDirectness = Vector3.Dot(incomingDir, normal);
                
                if (hitDirectness < -0.98f)
                {
                    perpendicular = -incomingDir;
                }
                else
                {
                    if (Vector3.Dot(perpendicular, incomingDir) < 0) perpendicular = -perpendicular;
                }
                Debug.DrawRay(mPos, perpendicular * 3f, Color.magenta);
            }
            else if (layerName == "Wall")
            {
                RaycastHit2D ray = Physics2D.Raycast(start, incomingDir, 20f, LayerMask.GetMask("Wall"));
                if (ray.collider != null)
                {
                    Vector3 reflectDir = Vector3.Reflect(incomingDir, ray.normal);
                    Debug.DrawRay(mPos, reflectDir * 3f, Color.magenta);
                }
            }
        }

        // 3. Stick Positioning
        // Forces the stick to look at the ghost ball (mPos)
        stick.rotation = Quaternion.LookRotation(Vector3.forward, mPos - Ball.position);
        stick.position = Ball.position - (stick.up * 2.0f);
        start = stick.position;

        // White Path
        Debug.DrawLine(start, mPos, Color.white);

        // 4. Input
        if (Input.GetMouseButtonDown(0))
        {
            ApplyBallForce();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mPos, 0.5f);
    }
}