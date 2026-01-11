# Pool Cue Rotation and Aiming (rotateQue.cs)

Controls the rotation and aiming visualization of a 2D pool cue relative to the white ball, including dynamic positioning based on cursor distance and a line renderer to illustrate the shot path.

## Usage
- Attach this script to the cue GameObject.
- Ensure a `LineRenderer` component is present on the cue.
- The script hides the cursor, rotates the cue to face the mouse position, positions the cue behind the ball with a dynamic gap, and draws an aiming line that accounts for collider hits.

## Inspector Fields
- `whiteBall` (Transform): Reference to the white ball Transform the cue should aim around. Assign in the Inspector.
- `distanceFromBall` (float): Desired distance from the ball to the cue. Used for initial spacing in larger-scale scenes.
- `lineLength` (float): Maximum length of the aim line drawn by the `LineRenderer`.
- `collisionLayers` (LayerMask): Layer mask used by the raycasts to detect collisions (e.g., balls and walls). Select relevant layers (e.g., BallCollision, Walls).
- `reflectedObject` (Transform, optional): Optional Transform to visualize reflections or other derived objects from the aiming logic.
- `raycastOriginPoint` (Transform, optional): Optional origin point for raycasts at the tip of the cue. If null, the cue's Transform position is used.

## Methods
- `Start()`: Initializes required components and verifies that a main camera exists. Ensures the `LineRenderer` is available and warns if the `MainCamera` is missing.
- `Update()`: Per-frame update that rotates the cue and draws the aiming line if a white ball is assigned. Skips processing if the white ball reference is missing.
- `RotateQueStick()`: Rotates and positions the cue based on the mouse position in world space, maintaining a dynamic gap behind the white ball.
  - Converts the mouse screen position to 2D world coordinates.
  - Computes direction from the ball to the mouse and normalizes it.
  - Applies a clamped dynamic gap to position the cue behind the ball.
  - Rotates the cue around Z to face the computed direction.
- `DrawAimLine()`: Draws the aiming line using a `LineRenderer`.
  - Casts a ray through the ball to detect collisions along the shot path.
  - Uses the exit point on the ball to determine the steering direction.
  - Extends a projected guidance line forward.
  - If no hits are detected, a straight line is drawn in the aim direction.
  - Line width and color (neon green) are configured here.

## Notes
- Ensure colliders are set up on balls and table walls to visualize accurate aim collisions.
- Configure the `collisionLayers` to include only relevant gameplay layers.
- Tune `distanceFromBall` and `lineLength` for your scene scale.
