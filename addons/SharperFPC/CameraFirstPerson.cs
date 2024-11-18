using Godot;

[GlobalClass]
public sealed partial class CameraFirstPerson : CharacterBody3D
{
  private Node3D Head;
  private CollisionShape3D CollisionMesh;
  private TextPanel DebugPanel;

  [ExportCategory("Character")]
  [Export] private float baseSpeed = 3.0f;
  [Export] private float sprintSpeed = 6.0f;
  [Export] private float crouchSpeed = 1.0f;
  [Export] private float acceleration = 10.0f;
  [Export] private float jumpVelocity = 4.5f;
  [Export] private float mouseSensitivity = 0.005f;
  private bool immobile = false;
  Vector3 initial_facing_direction = Vector3.Zero;

  [ExportGroup("Controls")]
  [Export] string jump = "jump";
  [Export] string left = "move_left";
  [Export] string right = "move_right";
  [Export] string forward = "move_forward";
  [Export] string backward = "move_back";
  [Export] string pause = "pause";
  [Export] string crouch = "crouch";
  [Export] string sprint = "sprint";

  [ExportGroup("Feature Settings")]
  [Export] bool debugMode = true;
  [Export] bool jumpingEnabled = true;
  [Export] bool inAirMomentum = true;
  [Export] bool motionSmoothing = true;
  [Export] bool sprintEnabled = true;
  [Export] bool crouchEnabled = true;

  // Member variables
  float speed;
  float currentSpeed = 0.0f;  // States: normal, crouching, sprinting
  string state = "normal";
  public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  /// <summary>
  /// Load references to Nodes, call for animations to start from zero, load ItemDetection references, capture the mouse and look straight ahead.
  /// </summary>
  public override void _Ready()
  {
    base._Ready();
    speed = baseSpeed;
    Head = GetNode<Node3D>("Head");
    CollisionMesh = GetNode<CollisionShape3D>("Collider");
    DebugPanel = GetNode<TextPanel>("TextPanel");
    InitItemDetection();
    InitAnimations();
    InitCamera();
  }

  /// <summary>
  /// The _PhysicsProcess() is the most important one in this Controller.
  /// First, the current speed is stored and a check is performed if an Area3D with the corresponding mask value is in front of the camera.
  /// After that, movement is processed based on keys currently pressed, gravity, delta between frames, and if something is colliding with the head.
  /// Depending on key and movement combination, one of three states is reached: normal, crouching or sprinting.
  /// Finally, headbobbing is processed and the loop starts again.
  /// </summary>
  /// <param name="delta">Time between frames.</param>
  public override void _PhysicsProcess(double delta)
  {
    currentSpeed = Vector3.Zero.DistanceTo(GetRealVelocity());
    DebugPanel.AddProperty("Speed", $"{currentSpeed:0.000}", 1);
    DebugPanel.AddProperty("Target Speed", $"{speed}", 2);
    Vector3 cv = GetRealVelocity();
    DebugPanel.AddProperty("Velocity", $"X: {cv.X:0.000} Y: {cv.Y:0.000} X: {cv.X:0.000}", 3);
    DetectItems();

    // Gravity
    // If the gravity changes during your game, uncomment this code
    // gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    Vector3 currentVelocity = Velocity;
    if (!IsOnFloor())
    {
      currentVelocity.Y -= (float)(gravity * delta);
    }
    else if (jumpingEnabled)
    {
      if (continuousJumping ? Input.IsActionPressed(jump) : Input.IsActionJustPressed(jump))
      {
        if (IsOnFloor() && !lowCeiling)
        {
          JumpAnimation?.Play("jump");
          currentVelocity.Y += jumpVelocity;
        }
      }
    }

    Velocity = currentVelocity;

    // Movement processing.
    Vector2 inputDir = immobile ? Vector2.Zero : Input.GetVector(left, right, forward, backward);
    Vector2 direction2D = inputDir.Rotated(-Head.Rotation.Y);
    Vector3 direction = new(direction2D.X, 0, direction2D.Y);
    direction = direction.Normalized();
    MoveAndSlide();
    ProcessMovementSmoothing(currentVelocity, delta, direction);

    // State processing (running, crouching, standing)
    lowCeiling = CeilingDetection.IsColliding();
    bool moving = inputDir != Vector2.Zero;
    if (sprintEnabled)
    {
      if (sprintMode == 0)
      {
        if (Input.IsActionPressed(sprint) && state != "crouching")
        {
          if (moving)
          {
            if (state != "sprinting")
            {
              SprintState();
            }
          }
          else
          {
            if (state == "sprinting")
            {
              NormalState();
            }
          }
        }
        else if (state == "sprinting")
        {
          NormalState();
        }
      }
      else if (sprintMode == 1)
      {
        if (moving)
        {
          if (Input.IsActionPressed(sprint) && state == "normal")
          {
            SprintState();
          }
          if (Input.IsActionJustPressed(sprint))
          {
            switch (state)
            {
              case "normal":
                SprintState();
                break;
              case "sprinting":
              default:
                NormalState();
                break;
            }
          }
        }
        else if (state == "sprinting")
        {
          NormalState();
        }
      }
    }

    if (crouchEnabled)
    {
      if (crouchMode == 0)
      {
        if (Input.IsActionPressed(crouch) && state != "sprinting")
        {
          if (state != "crouching")
          {
            CrouchState();
          }
        }
        else if (state == "crouching" && !CeilingDetection.IsColliding())
        {
          NormalState();
        }
      }
      else if (crouchMode == 1)
      {
        if (Input.IsActionJustPressed(crouch))
        {
          switch (state)
          {
            case "normal":
              CrouchState();
              break;
            case "crouching":
            default:
              if (!CeilingDetection.IsColliding())
              {
                NormalState();
              }
              break;
          }
        }
      }
    }

    // Movement animation
    ProcessFinalMovement(inputDir);
  }

  /// <summary>
  /// State processing: standing up doing nothing.
  /// </summary>
  private void NormalState()
  {
    string previousState = state;
    if (previousState == "crouching")
    {
      CrouchAnimation.PlayBackwards("crouch");
    }
    state = "normal";
    speed = baseSpeed;
  }

  /// <summary>
  /// State processing: running. 
  /// </summary>
  private void SprintState()
  {
    string previousState = state;
    if (previousState == "crouching")
    {
      CrouchAnimation.PlayBackwards("crouch");
    }
    state = "sprinting";
    speed = sprintSpeed;
  }

  /// <summary>
  /// State processing: crouching. 
  /// </summary>
  private void CrouchState()
  {
    string previousState = state;
    state = "crouching";
    speed = crouchSpeed;
    CrouchAnimation.Play("crouch");
  }

  /// <summary>
  /// State processing: pause. If the pause key is pressed (in this case, P), mouse is released.
  /// </summary>
  private void HandlePause()
  {
    if (Input.IsActionJustPressed(pause))
      Input.MouseMode = (Input.MouseMode == Input.MouseModeEnum.Captured) ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
  }

  /// <summary>
  /// Check if the game is paused, make sure the camera does not go too far back as to turn it upside down, and load debug details if needed.
  /// </summary>
  /// <param name="delta"></param>
  public override void _Process(double delta)
  {
    HandlePause();
    ClampCamera();
    DebugPanel.AddProperty("FPS", $"{Performance.GetMonitor(Performance.Monitor.TimeFps)}", 0);
    DebugPanel.AddProperty("state", $"{state}" + (!IsOnFloor() ? " in the air" : ""), 4);
  }

  /// <summary>
  /// Handle camera rotation and item detection events.
  /// </summary>
  /// <param name="event"></param>
  public override void _UnhandledInput(InputEvent @event)
  {
    switch (@event)
    {
      case InputEventMouseMotion:
        if (Input.MouseMode == Input.MouseModeEnum.Captured)
        {
          InputEventMouseMotion iemm = (InputEventMouseMotion)@event;
          Vector3 currentRotation = Head.Rotation;
          currentRotation.Y -= iemm.Relative.X * mouseSensitivity;
          currentRotation.X -= iemm.Relative.Y * mouseSensitivity;
          Head.Rotation = currentRotation;
        }
        break;
      case InputEventMouseButton:
        HandleItemDetection();
        break;
    }
  }
}