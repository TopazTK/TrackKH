using Godot;
using System;
using System.Diagnostics;

public class AndroidInit : Node
{
	Color _rememberColor = new Color(0.135F, 0.135F, 0.135F);
	
	Node _sceneInstance;
	WindowDialog _creditsWindow;
	
	public override void _Ready()
	{
		OS.SetWindowTitle("Auto-Tracker for KH Randomizer [v0.60] | TopazTK");
		
		_sceneInstance = GetNode("Check Panel");
		_creditsWindow = GetNode("Credits Dialog") as WindowDialog;
	}
	
	private void iconSelected(byte index) => Singleton.IconMode = index;
	
	private void backColorShift(Color color)
	{
		(_sceneInstance as TrackScene).BackColor = color;
		_rememberColor = color;
	}
	
	private void creditsButton() => _creditsWindow.Visible = true;
	
	private void cancelButton() => _creditsWindow.Visible = false;
}
