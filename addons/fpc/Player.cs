using Godot;

namespace Premonition.Camera;

public static class ControlNames
{
  public const string Jump = "jump";
  public const string Left = "move_left";
  public const string Right = "move_right";
  public const string Forward = "move_forward";
  public const string Backward = "move_back";
  public const string Pause = "pause";
  public const string Crouch = "crouch";
  public const string Sprint = "sprint";
}


public sealed partial class Player : CharacterBody3D
{

  [ExportCategory("Character")]
  [Export]
  float baseSpeed = 3.0f;
  [Export]
  float sprintSpeed = 6.0f;
  [Export]
  float crouchSpeed = 1.0f;
  [Export]
  float acceleration = 10.0f;
  [Export]
  float jumpVelocity = 4.5f;
  [Export]
  float mouseSensitivity = 0.005f;
  [Export]
  bool immobile = false;
  //[Export(PropertyHint.File)]
  //public string defaultReticle ;
  [Export]
  Vector3 initial_facing_direction = Vector3.Zero;

  [ExportGroup("Nodes")]
  [Export]
  Node3D Head;
  [Export]
  Camera3D Camera;
  [Export]
  AnimationPlayer HeadbobAnimation;
  [Export]
  AnimationPlayer JumpAnimation;
  [Export]
  AnimationPlayer CrouchAnimation;
  [Export]
  CollisionShape3D CollisionMesh;
  [Export]
  ShapeCast3D CeilingDetection;
  MeshInstance3D ShaderSurface;
  //Reticle Reticle ;
  //  [Export]
  //  Control UserInterface;
  //  [Export]
  //  VBoxContainer DebugPanel;

  [ExportGroup("Controls")]
  [Export]
  string JUMP = "jump";
  [Export]
  string LEFT = "move_left";
  [Export]
  string RIGHT = "move_right";
  [Export]
  string FORWARD = "move_forward";
  [Export]
  string BACKWARD = "move_back";
  [Export]
  string PAUSE = "pause";
  [Export]
  string CROUCH = "crouch";
  [Export]
  string SPRINT = "sprint";

  [ExportGroup("Feature Settings")]
  [Export]
  bool debugMode = true;
  [Export]
  bool jumpingEnabled = true;
  [Export]
  bool inAirMomentum = true;
  [Export]
  bool motionSmoothing = true;
  [Export]
  bool sprintEnabled = true;
  [Export]
  bool crouchEnabled = true;
  [Export(PropertyHint.Enum, "Hold to Crouch,Toggle Crouch")]
  public int crouchMode = 0;
  [Export(PropertyHint.Enum, "Hold to Sprint,Toggle Sprint")]
  public int sprintMode = 0;
  [Export]
  bool dynamicFOV = true;
  [Export]
  bool continuousJumping = true;
  [Export]
  bool viewBobbing = true;
  [Export]
  bool jumpAnimation = true;

  // Member variables
  float speed;
  float currentSpeed = 0.0f;

  // States: normal, crouching, sprinting
  string state = "normal";
  bool lowCeiling = false; // This is for when the ceiling is too low and the player needs to crouch.
  bool wasOnFloor = true;
  // Get the gravity from the project settings to be synced with RigidBody nodes.
  public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

  public override void _Ready()
  {
    base._Ready();
    speed = baseSpeed;

    // Get all nodes.
    Head = GetNode<Node3D>("Head");
    Camera = GetNode<Camera3D>("Head/Camera");
    ShaderSurface = GetNode<MeshInstance3D>("Head/Camera/ShaderSurface");
    CollisionMesh = GetNode<CollisionShape3D>("Collision");
    CrouchAnimation = GetNode<AnimationPlayer>("CrouchAnimation");
    HeadbobAnimation = GetNode<AnimationPlayer>("Head/HeadbobAnimation");
    JumpAnimation = GetNode<AnimationPlayer>("Head/JumpAnimation");
    CeilingDetection = GetNode<ShapeCast3D>("CrouchCeilingDetection");

    Input.MouseMode = Input.MouseModeEnum.Captured;

    // Set the camera rotation to whatever initial_facing_direction is, as long as it's not Vector3.zero
    if (!initial_facing_direction.Equals(Vector3.Zero))
    {
      Head.RotationDegrees = initial_facing_direction;
    }

    /*if (defaultReticle != null)
    {
        change_reticle(defaultReticle) ;
    }*/

    HeadbobAnimation.Play("RESET");
    JumpAnimation.Play("RESET");
    CrouchAnimation.Play("RESET");
  }

  /*void change_reticle(string reticlePath)
  {
      if (Reticle!=null)
      {
          Reticle.QueueFree();
      }

      Reticle = GD.Load<PackedScene>(reticlePath).Instantiate<Reticle>() ;
      Reticle.character = this ;
      UserInterface.AddChild(Reticle) ;
  }*/

  public override void _PhysicsProcess(double delta)
  {
    currentSpeed = Vector3.Zero.DistanceTo(GetRealVelocity());

    /*DebugPanel.AddProperty("Speed", $"{currentSpeed:0.000}", 1) ;
    DebugPanel.AddProperty("Target Speed", $"{speed}", 2) ;
    Vector3 cv = GetRealVelocity() ;
    DebugPanel.AddProperty("Velocity", $"X: {cv.X:0.000} Y: {cv.Y:0.000} X: {cv.X:0.000}", 3) ;
    */
    // Gravity
    //  If the gravity changes during your game, uncomment this code
    // gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    Vector3 currentVelocity = Velocity;
    if (!IsOnFloor())
    {
      currentVelocity.Y -= (float)(gravity * delta);
    }

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


    Vector2 inputDir = immobile ? Vector2.Zero : Input.GetVector(LEFT, RIGHT, FORWARD, BACKWARD);
    Vector2 direction2D = inputDir.Rotated(-Head.Rotation.Y);
    Vector3 direction = new Vector3(direction2D.X, 0, direction2D.Y);
    direction = direction.Normalized(); // ?
    MoveAndSlide();

    if (!inAirMomentum || IsOnFloor())
    {
      currentVelocity = Vector3.Zero;
      currentVelocity.X = motionSmoothing ? Mathf.Lerp(Velocity.X, direction.X * speed, (float)(acceleration * delta)) : direction.X * speed;
      currentVelocity.Z = motionSmoothing ? Mathf.Lerp(Velocity.Z, direction.Z * speed, (float)(acceleration * delta)) : direction.Z * speed;
      Velocity = currentVelocity;
    }


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


    if (dynamicFOV) { UpdateCameraFOV(); }

    if (viewBobbing) { RunHeadbobAnimation(inputDir != Vector2.Zero); }

    if (jumpAnimation)
    {
      if (!wasOnFloor && IsOnFloor()) // Just Landed
      {
        JumpAnimation.Play((GD.Randi() % 2) == 1 ? "land_left" : "land_right");
      }
      wasOnFloor = IsOnFloor(); //This must always be at the end of physics_process
    }
  }

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

  private void CrouchState()
  {
    string previousState = state;
    state = "crouching";
    speed = crouchSpeed;
    CrouchAnimation.Play("crouch");
  }

  void UpdateCameraFOV()
  {
    Camera.Fov = Mathf.Lerp(Camera.Fov, state == "sprinting" ? 85 : 75, 0.3f);
  }

  void RunHeadbobAnimation(bool moving)
  {
    if (moving && IsOnFloor())
    {
      string useRunHeadbobAnimation = (state == "normal" || state == "crouching") ? "walk" : "sprint";
      bool wasPlaying = HeadbobAnimation.CurrentAnimation == useRunHeadbobAnimation;

      HeadbobAnimation.Play(useRunHeadbobAnimation, 0.25f);
      HeadbobAnimation.SpeedScale = (currentSpeed / baseSpeed) * 1.75f;

      if (!wasPlaying) { HeadbobAnimation.Seek((double)(GD.Randi() % 2)); }
    }
    else
    {
      HeadbobAnimation.Play("RESET", 0.25);
      HeadbobAnimation.SpeedScale = 1;
    }
  }

  public override void _Process(double delta)
  {
    //DebugPanel.AddProperty("FPS", $"{Performance.GetMonitor(Performance.Monitor.TimeFps)}", 0) ;
    //DebugPanel.AddProperty("state", $"{state}" + (!IsOnFloor() ? " in the air" : ""), 4) ;
    if (Input.IsActionJustPressed(PAUSE))
    {
      Input.MouseMode = (Input.MouseMode == Input.MouseModeEnum.Captured) ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
    {
      InputEventMouseMotion iemm = (InputEventMouseMotion)@event;
      Vector3 currentRotation = Head.Rotation;
      currentRotation.Y -= iemm.Relative.X * mouseSensitivity;
      currentRotation.X -= iemm.Relative.Y * mouseSensitivity;
      Head.Rotation = currentRotation;

    }
  }
}
