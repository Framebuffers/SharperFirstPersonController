using Godot;

/// <summary>
/// Debug panel. Shows diagnostic data.
/// </summary>
public partial class TextPanel : Godot.PanelContainer
{
  public VBoxContainer BoxContainer { get => GetNode<VBoxContainer>("MarginContainer/VBoxContainer"); }

  /// <summary>
  /// Loads text onto the panel.
  /// </summary>
  /// <param name="title">A key to prepend before a message sent to the panel, like a title or a suffix.</param>
  /// <param name="value">Message to load.</param>
  /// <param name="order">If more messages are present, text can be loaded onto an specific place inside the text queue.</param>
  public void AddProperty(string title, string value, int order)
  {
    if (BoxContainer is VBoxContainer)
    {
      Node target = BoxContainer.FindChild(title, true, false);
      if (target == null)
      {
        Label label = new();
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        BoxContainer.AddChild(label);
        label.Name = title;
        label.Text = $"{title}: {value}";
      }
      else if (Visible)
      {
        Label label = (Label)target;
        label.Text = $"{title}: {value}";
        BoxContainer.MoveChild(target, order);
      }
    }
  }

  /// <summary>
  /// Loads text onto the panel.
  /// </summary>
  /// <param name="text">Text to display.</param>
  public void AddProperty(string text)
  {
    if (BoxContainer is VBoxContainer)
    {
      Label label = new();
      label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
      BoxContainer.AddChild(label);
      label.Text = text;

    }
  }

  /// <summary>
  /// Load a reference to the container and setup the positioning and look of the container.
  /// </summary>
  public override void _Ready()
  {
    MarginContainer container = GetNode<MarginContainer>("MarginContainer");
    Vector2 screenSize = GetViewport().GetWindow().Size;
    Vector2 targetSize = new(screenSize.X / 3, container.Size.Y);
    container.CustomMinimumSize = targetSize;
    container.AnchorRight = 1.0f;
    container.AnchorBottom = 1.0f;
    container.OffsetRight = -targetSize.X;
    container.OffsetBottom = -targetSize.Y;
  }

  /// <summary>
  /// Unload text from the container.
  /// </summary>
  public void RemoveProperties()
  {
    if (BoxContainer.GetType() == typeof(VBoxContainer))
    {
      foreach (var children in BoxContainer.GetChildren())
      {
        children.Free();
      }
    }
  }
}

