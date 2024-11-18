using Godot;

/// <summary>
/// A character which includes movement, a camera, and item detection into one Node.
/// </summary>
public sealed partial class CameraFirstPerson : CharacterBody3D
{
  private AnimationPlayer HeadbobAnimation;
  private AnimationPlayer JumpAnimation;
  private AnimationPlayer CrouchAnimation;

  [Export(PropertyHint.Enum, "Hold to Crouch, toggle Crouch")] public int crouchMode = 0;
  [Export(PropertyHint.Enum, "Hold to Sprint, toggle Sprint")] public int sprintMode = 0;
  [Export] bool dynamicFOV = true;
  [Export] bool continuousJumping = true;
  [Export] bool viewBobbing = true;
  [Export] bool jumpAnimation = true;

  /// <summary>
  /// Loads references to AnimationPlayer nodes inside the Scene.
  /// </summary>
  private void InitAnimations()
  {
    CrouchAnimation = GetNode<AnimationPlayer>("CrouchAnimation");
    HeadbobAnimation = GetNode<AnimationPlayer>("Head/HeadbobAnimation");
    JumpAnimation = GetNode<AnimationPlayer>("Head/JumpAnimation");

    HeadbobAnimation.Play("RESET");
    JumpAnimation.Play("RESET");
    CrouchAnimation.Play("RESET");
  }

  /// <summary>
  /// Loop to determine which animations to play each cycle.
  /// </summary>
  /// <param name="inputDir">Direction on which the player's moving towards.</param>
  private void ProcessFinalMovement(Vector2 inputDir)
  {
    if (dynamicFOV) { UpdateCameraFOV(); }

    if (viewBobbing) { RunHeadbobAnimation(inputDir != Vector2.Zero); }

    if (jumpAnimation)
    {
      if (!wasGrounded && IsOnFloor()) // Just Landed
      {
        JumpAnimation.Play((GD.Randi() % 2) == 1 ? "land_left" : "land_right");
      }
      wasGrounded = IsOnFloor(); //This must always be at the end of physics_process
    }
  }

  /// <summary>
  /// Checks for conditions and plays the headbobbing animation. Note that the playback is randomised to create a more natural bobbing effect.
  /// </summary>
  /// <param name="moving">Is the player moving?</param>
  private void RunHeadbobAnimation(bool moving)
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

  /// <summary>
  /// If motion smoothing is enabled, performs a linear interpolation between points of origin and destination, weighted by the acceleration factor and the delta between frames.
  /// </summary>
  /// <param name="currentVelocity">How fast is the player moving?</param>
  /// <param name="delta">Delta between frames obtained from _PhysicsProcess()</param>
  /// <param name="direction">Where is the player going?</param>
  private void ProcessMovementSmoothing(Vector3 currentVelocity, double delta, Vector3 direction)
  {
    // NOTE: motion smoothing: lerps direction changes
    if (!inAirMomentum || IsOnFloor())
    {
      currentVelocity = Vector3.Zero;
      currentVelocity.X = motionSmoothing ? Mathf.Lerp(Velocity.X, direction.X * speed, (float)(acceleration * delta)) : direction.X * speed;
      currentVelocity.Z = motionSmoothing ? Mathf.Lerp(Velocity.Z, direction.Z * speed, (float)(acceleration * delta)) : direction.Z * speed;
      Velocity = currentVelocity;
    }
  }
}

