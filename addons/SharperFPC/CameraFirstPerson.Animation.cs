using Godot;

public sealed partial class CameraFirstPerson : CharacterBody3D
{
  private AnimationPlayer HeadbobAnimation;
  private AnimationPlayer JumpAnimation;
  private AnimationPlayer CrouchAnimation;

  [Export(PropertyHint.Enum, "Hold to Crouch, Toggle Crouch")] public int crouchMode = 0;
  [Export(PropertyHint.Enum, "Hold to Sprint, Toggle Sprint")] public int sprintMode = 0;
  [Export] bool dynamicFOV = true;
  [Export] bool continuousJumping = true;
  [Export] bool viewBobbing = true;
  [Export] bool jumpAnimation = true;

  private void InitAnimations()
  {
    CrouchAnimation = GetNode<AnimationPlayer>("CrouchAnimation");
    HeadbobAnimation = GetNode<AnimationPlayer>("Head/HeadbobAnimation");
    JumpAnimation = GetNode<AnimationPlayer>("Head/JumpAnimation");

    HeadbobAnimation.Play("RESET");
    JumpAnimation.Play("RESET");
    CrouchAnimation.Play("RESET");
  }

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

  // NOTE: Headbobbing animation while moving.
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

