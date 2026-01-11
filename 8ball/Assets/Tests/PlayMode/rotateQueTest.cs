using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class rotateQueTest
{
    private GameObject cueGO;
    private rotateQue cue;
    private LineRenderer lr;
    private GameObject ballGO;
    private Transform whiteBall;
    private Camera cam;

    [SetUp]
    public void Setup()
    {
        // Camera
        var camGO = new GameObject("MainCamera");
        camGO.tag = "MainCamera";
        cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.transform.position = new Vector3(0, 0, -10);

        // Cue object with LineRenderer and script
        cueGO = new GameObject("Cue");
        lr = cueGO.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.material = new Material(Shader.Find("Sprites/Default")); // simple material for LineRenderer

        cue = cueGO.AddComponent<rotateQue>();
        cue.whiteBall = null; // set later
        cue.collisionLayers = ~0; // include all layers
        cue.raycastOriginPoint = null;

        // White ball with CircleCollider2D
        ballGO = new GameObject("WhiteBall");
        whiteBall = ballGO.transform;
        var rb = ballGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var col = ballGO.AddComponent<CircleCollider2D>();
        col.radius = 0.5f; // reasonable default
        ballGO.layer = 0; // Default
        cue.whiteBall = whiteBall;

        // Initial positions
        whiteBall.position = new Vector3(0, 0, 0);
        cueGO.transform.position = new Vector3(-5f, 0f, 0f);
    }

    [TearDown]
    public void Teardown()
    {
        if (cam != null) Object.DestroyImmediate(cam.gameObject);
        if (cue != null) Object.DestroyImmediate(cue.gameObject);
        if (ballGO != null) Object.DestroyImmediate(ballGO);
    }

    [UnityTest]
    public IEnumerator DrawAimLine_NoHits_UsesTwoPoints()
    {
        // Move cue so the ray misses the ball (aim upwards)
        cueGO.transform.position = new Vector3(-5f, 5f, 0f);

        // Ensure nothing is along the direction
        // Set origin and direction such that it won't intersect the ball
        // Call Start once to init LineRenderer and camera check
        cue.SendMessage("Start");

        // Draw aim line
        cue.DrawAimLine();

        // Physics might need a frame
        yield return null;

        Assert.AreEqual(2, lr.positionCount, "LineRenderer should have 2 points when there are no hits.");

        Vector3 origin = cueGO.transform.position;
        Vector3 direction = (whiteBall.position - origin).normalized;
        Vector3 p0 = lr.GetPosition(0);
        Vector3 p1 = lr.GetPosition(1);

        // p0 should be origin, p1 in the direction scaled
        Assert.That(Vector3.Distance(p0, origin) < 0.001f, "First point should be at origin.");
        Assert.That(Vector3.Distance((p1 - origin).normalized, direction) < 0.001f, "Second point should be along the direction.");
        Assert.Greater(Vector3.Distance(p1, origin), 100f, "Second point should be far enough along the ray.");
    }

    [UnityTest]
    public IEnumerator DrawAimLine_WithBallHit_UsesThreePointsAndProjects()
    {
        // Place cue left of ball and aim directly at it to pierce through
        cueGO.transform.position = new Vector3(-5f, 0f, 0f);

        // Initialize
        cue.SendMessage("Start");

        // Draw aim line
        cue.DrawAimLine();
        yield return null;

        Assert.AreEqual(3, lr.positionCount, "LineRenderer should have 3 points when a hit occurs.");

        Vector3 p0 = lr.GetPosition(0);
        Vector3 p1 = lr.GetPosition(1);
        Vector3 p2 = lr.GetPosition(2);

        // p0 should be cue origin
        Assert.That(Vector3.Distance(p0, cueGO.transform.position) < 0.001f, "First point should be at origin.");

        // p1 should be a point on the ball collider surface near the front/exit side
        float distOriginToP1 = Vector3.Distance(p0, p1);
        Assert.Greater(distOriginToP1, 4.0f, "Hit point should be along the ray near the ball.");
        Assert.That(Mathf.Abs(p1.y - whiteBall.position.y) < 0.01f, "Hit point should align with ball center on Y for a straight shot.");

        // p2 should be ahead of p1 in the projected steering direction
        Assert.Greater(Vector3.Distance(p2, p1), 100f, "Projected point should be significantly ahead of the hit point.");

        // Direction from ball center to hit point should aim forward (positive X)
        Vector3 steeringDir = (p1 - whiteBall.position).normalized;
        Assert.Greater(steeringDir.x, 0.0f, "Steering direction X component should be positive for left-to-right shot.");
    }
}
