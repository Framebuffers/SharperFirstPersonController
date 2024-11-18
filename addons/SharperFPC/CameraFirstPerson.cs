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

  // TODO: decouple controls from strings into const strings.
  [ExportGroup("Controls")]
  [Export] string JUMP = "jump";
  [Export] string LEFT = "move_left";
  [Export] string RIGHT = "move_right";
  [Export] string FORWARD = "move_forward";
  [Export] string BACKWARD = "move_back";
  [Export] string PAUSE = "pause";
  [Export] string CROUCH = "crouch";
  [Export] string SPRINT = "sprint";

  [ExportGroup("Feature Settings")]
  [Export] bool debugMode = true; // NOTE: sends data to a Control node, with FPS data.
  [Export] bool jumpingEnabled = true;
  [Export] bool inAirMomentum = true;
  [Export] bool motionSmoothing = true;
  [Export] bool sprintEnabled = true;
  [Export] bool crouchEnabled = true;


  // TODO: Clean up repeated member variables.
  // Member variables
  float speed;
  float currentSpeed = 0.0f;
  // TODO: handle states cleaner.
  // States: normal, crouching, sprinting
  string state = "normal"; // NOTE: use an enum.

  // Get the gravity from the project settings to be synced with RigidBody nodes.
  // TODO: push all the static paths onto a separate file for easier access.
  public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

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

  public override void _PhysicsProcess(double delta)
  {
    currentSpeed = Vector3.Zero.DistanceTo(GetRealVelocity());
    DebugPanel.AddProperty("Speed", $"{currentSpeed:0.000}", 1);
    DebugPanel.AddProperty("Target Speed", $"{speed}", 2);
    Vector3 cv = GetRealVelocity();
    DebugPanel.AddProperty("Velocity", $"X: {cv.X:0.000} Y: {cv.Y:0.000} X: {cv.X:0.000}", 3);
    DetectItems();

    // Gravity
    //  If the gravity changes during your game, uncomment this code
    // gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    // NOTE: Gravity calc.
    Vector3 currentVelocity = Velocity;
    if (!IsOnFloor())
    {
      currentVelocity.Y -= (float)(gravity * delta);
    }

    // NOTE: jump check
    else if (jumpingEnabled)
    {
      if (continuousJumping ? Input.IsActionPressed(JUMP) : Input.IsActionJustPressed(JUMP))
      {
        if (IsOnFloor() && !lowCeiling)
        {
          if (JumpAnimation != null)
          {
            JumpAnimation.Play("jump");
          }
          currentVelocity.Y += jumpVelocity;
        }
      }
    }
    Velocity = currentVelocity;

    // NOTE: movement processing
    Vector2 inputDir = immobile ? Vector2.Zero : Input.GetVector(LEFT, RIGHT, FORWARD, BACKWARD);
    Vector2 direction2D = inputDir.Rotated(-Head.Rotation.Y);
    Vector3 direction = new Vector3(direction2D.X, 0, direction2D.Y);
    direction = direction.Normalized();
    MoveAndSlide();
    ProcessMovementSmoothing(currentVelocity, delta, direction);

    // NOTE: state processing (running, crouching, standing)
    // not touching this anytime soon because it breaks so easily.
    lowCeiling = CeilingDetection.IsColliding();
    bool moving = inputDir != Vector2.Zero;
    if (sprintEnabled)
    {
      if (sprintMode == 0)
      {
        if (Input.IsActionPressed(SPRINT) && state != "crouching")
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
          if (Input.IsActionPressed(SPRINT) && state == "normal")
          {
            SprintState();
          }
          if (Input.IsActionJustPressed(SPRINT))
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
        if (Input.IsActionPressed(CROUCH) && state != "sprinting")
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
        if (Input.IsActionJustPressed(CROUCH))
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

    // NOTE: Movement animation
    ProcessFinalMovement(inputDir);
  }

  // NOTE: state processing: standing up doing nothing.
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

  // NOTE: state processing: running.
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

  // NOTE: state processing: crouching.
  private void CrouchState()
  {
    string previousState = state;
    state = "crouching";
    speed = crouchSpeed;
    CrouchAnimation.Play("crouch");
  }

  private void HandlePause()
  {
    // NOTE: releases the mouse from the window.
    if (Input.IsActionJustPressed(PAUSE))
      Input.MouseMode = (Input.MouseMode == Input.MouseModeEnum.Captured) ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
  }

  public override void _Process(double delta)
  {
    HandlePause();
    ClampCamera();

    DebugPanel.AddProperty("FPS", $"{Performance.GetMonitor(Performance.Monitor.TimeFps)}", 0);
    DebugPanel.AddProperty("state", $"{state}" + (!IsOnFloor() ? " in the air" : ""), 4);
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    // NOTE: rotates the camera upon receiving a mouse input.
    //
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