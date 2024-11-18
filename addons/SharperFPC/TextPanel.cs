using Godot;

public partial class TextPanel : Godot.PanelContainer
{
  public VBoxContainer BoxContainer { get => GetNode<VBoxContainer>("MarginContainer/VBoxContainer"); }

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

