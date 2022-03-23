using Godot;
using System;

public class TrackScene : Panel
{
	[Export] public Color BackColor = new Color(0.13F, 0.13F, 0.13F, 1);
	
	private StyleBoxFlat _styleBox;
	
	public override void _Ready()
	{
		_styleBox = GetStylebox("panel") as StyleBoxFlat;
	}
	
	public override void _Process(float delta)
	{
		_styleBox.BgColor = BackColor;
	}
}
