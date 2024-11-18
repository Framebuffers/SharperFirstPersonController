using Godot;

public sealed partial class CameraFirstPerson : CharacterBody3D
{
  private Camera3D Camera;

  [ExportGroup("Camera Limits")]
  [Export(PropertyHint.Range, "-90.0, 90.0")] private float TiltUpperLimit = 60.0f;
  [Export(PropertyHint.Range, "-90.0, 90.0")] private float TiltLowerLimit = -60.0f;

  /// <summary>
  /// Set the camera rotation to whatever initial_facing_direction is, as long as it's not Vector3.zero
  /// </summary>
  private void InitCamera()
  {
    Camera = GetNode<Camera3D>("Head/Camera");
    Input.MouseMode = Input.MouseModeEnum.Captured;

    if (!initial_facing_direction.Equals(Vector3.Zero))
      Head.RotationDegrees = initial_facing_direction;
  }

  /// <summary>
  /// When running, changes FOV to make it look nicer. 
  /// </summary>
  void UpdateCameraFOV() =>
    Camera.Fov = Mathf.Lerp(Camera.Fov, state == "sprinting" ? 85 : 75, 0.3f);

  /// <summary>
  /// Clamps the camera to a given range, so it doesn't go upside down. 
  /// </summary>
  private void ClampCamera()
  {
    // NOTE: 
    float x = Mathf.Clamp(Head.Rotation.X, Mathf.DegToRad(TiltLowerLimit), Mathf.DegToRad(TiltUpperLimit));
    float y = Head.Rotation.Y;
    float z = Head.Rotation.Z;
    Head.Rotation = new Vector3(x, y, z);
  }
}

