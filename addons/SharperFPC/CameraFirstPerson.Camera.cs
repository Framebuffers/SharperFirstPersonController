using Godot;

public sealed partial class CameraFirstPerson : CharacterBody3D
{
  private Camera3D Camera;

  // TODO: Clean up the camera clamping code.
  [ExportGroup("Camera Limits")]
  [Export(PropertyHint.Range, "-90.0, 90.0")] private float TiltUpperLimit = 60.0f;
  [Export(PropertyHint.Range, "-90.0, 90.0")] private float TiltLowerLimit = -60.0f;

  private void InitCamera()
  {
    Camera = GetNode<Camera3D>("Head/Camera");
    Input.MouseMode = Input.MouseModeEnum.Captured;

    // Set the camera rotation to whatever initial_facing_direction is, as long as it's not Vector3.zero
    if (!initial_facing_direction.Equals(Vector3.Zero))
      Head.RotationDegrees = initial_facing_direction;
  }

  // NOTE: when running, changes FOV to make it look nicer.
  void UpdateCameraFOV() =>
    Camera.Fov = Mathf.Lerp(Camera.Fov, state == "sprinting" ? 85 : 75, 0.3f);

  private void ClampCamera()
  {
    // NOTE: clamps the camera to a given range, so it doesn't go upside down.
    float x = Mathf.Clamp(Head.Rotation.X, Mathf.DegToRad(TiltLowerLimit), Mathf.DegToRad(TiltUpperLimit));
    float y = Head.Rotation.Y;
    float z = Head.Rotation.Z;
    Head.Rotation = new Vector3(x, y, z);
  }
}

