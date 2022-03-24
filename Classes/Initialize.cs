using Godot;
using System;
using System.Diagnostics;

public class Initialize : Node
{
	Color _rememberColor = new Color(0.135F, 0.135F, 0.135F);
	
	Node _sceneInstance;
	OptionButton _trackOption;
	
	WindowDialog _seekerWindow;
	WindowDialog _creditsWindow;
	
	public override void _Ready()
	{
		OS.SetWindowTitle("Auto-Tracker for KH Randomizer [v0.60] | TopazTK");
		
		_sceneInstance = GetNode("Check Panel");
		_seekerWindow = GetNode("Seeker Dialog") as WindowDialog;
		_creditsWindow = GetNode("Credits Dialog") as WindowDialog;
		_trackOption = GetNode("Settings Panel/Tracking/Selection") as OptionButton;
	}
	
	public override void _Process(float delta)
	{
		if (_trackOption.Selected == 1)
		{
			if (Hypervisor.GameProcess == null && _seekerWindow.Visible)
			{
				var _processes = Process.GetProcessesByName("KINGDOM HEARTS FINAL MIX");
				
				if (_processes.Length > 0)
				{
					Hypervisor.GameProcess = _processes[0];
					Hypervisor.GameHandle = Hypervisor.GameProcess.Handle;
					Hypervisor.ExeAddress = (ulong)Hypervisor.GameProcess.MainModule.BaseAddress;
					Hypervisor.GameAddress = Hypervisor.ExeAddress + 0x3A0606;
					
					Singleton.TrackMode = 1;
					_seekerWindow.Visible = false;
				}
			}
			
			else if (!_seekerWindow.Visible && Hypervisor.GameProcess == null)
				_trackOption.Selected = 0;
		}
	}
	
	private void iconSelected(byte index) => Singleton.IconMode = index;
	
	private void trackSelect(byte index)
	{
		if (index == 0)
		{
			Hypervisor.GameProcess = null;
			Singleton.TrackMode = 0;
		}
		
		else
			_seekerWindow.Visible = true;
		
		_sceneInstance.QueueFree();
		
		var _scene = GD.Load<PackedScene>("res://Scenes/TrackScene.tscn");
		_sceneInstance = _scene.Instance();
		
		(_sceneInstance as TrackScene).BackColor = _rememberColor;
		
		AddChild(_sceneInstance);
	}
	
	private void backColorShift(Color color)
	{
		(_sceneInstance as TrackScene).BackColor = color;
		_rememberColor = color;
	}
	
	private void creditsButton() => _creditsWindow.Visible = true;
	
	private void cancelButton()
	{
		_seekerWindow.Visible = false;
		_creditsWindow.Visible = false;
	}
}
