using UnityEngine;
 

    public class RotateQueStick : MonoBehaviour{

         public Transform  stick; 
         public Transform  Ball;
         
         public Vector3 start;
         public Vector3 RayDirectionEnd;
         public Vector3 mPos;

        public static void RotateQueStickMethod(){
           
        }

        public static void DrawAimLine(){
            

        }

        public void Start()
        {
            
        }
 
    public void Update()
    {
        // 1. Get Mouse Pos and force it to the same Z as the White Ball
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
        mouseWorldPos.z = 0;
        mPos = mouseWorldPos;

        // 2. Detection - ONLY check for things on the "Ball" or "Wall" layers
        int mask = LayerMask.GetMask("Ball", "Wall");
        Collider2D hit = Physics2D.OverlapCircle(mPos, 0.5f, mask);

        if (hit != null)
        {
            // Snap mPos to the edge of the hit object
            mPos = hit.ClosestPoint(mPos);
            mPos.z = Ball.position.z; // Keep Z consistent!

            Vector3 incomingDir = (mPos - start).normalized;
            string layerName = LayerMask.LayerToName(hit.gameObject.layer);

            if (layerName == "Ball")
            {
                // BLACK LINE: From hit ghost center THROUGH target ball center
                Vector3 targetBallPath = (hit.transform.position - mPos).normalized;
                Debug.DrawRay(hit.transform.position, targetBallPath * 5f, Color.black);

                // MAGENTA LINE: 90-degree rule
                Vector3 perpendicular = new Vector3(-targetBallPath.y, targetBallPath.x, 0);
                Vector3 normal = (mPos - hit.transform.position).normalized;

                // THE STRAIGHT-BACK CHECK
                float hitDirectness = Vector3.Dot(incomingDir, normal);
                
                // If hitting head-on (within a small margin)
                if (hitDirectness < -0.98f)
                {
                    perpendicular = -incomingDir; // Point straight back
                }
                else
                {
                    // Ensure perpendicular points away from the player
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
        stick.rotation = Quaternion.LookRotation(Vector3.forward, mPos - Ball.position);
        stick.position = Ball.position - (stick.up * 2.0f);
        start = stick.position;

        // White Path
        Debug.DrawLine(start, mPos, Color.white);
    }

    void OnDrawGizmos()
    {
        // Draw the red ghost ball
        Gizmos.color = Color.red;
        // Draw it slightly closer to camera (z - 0.1) than the balls so it's ALWAYS visible
        Vector3 displayPos = new Vector3(0, 0, 10F);
        
        Gizmos.DrawWireSphere(mPos, 0.5f);
    }



    }
    