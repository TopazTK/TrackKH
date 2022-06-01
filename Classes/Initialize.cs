using Godot;
using System;
using System.Diagnostics;

public class Initialize : Node
{
	int _frameElapse = 0;
	Color _rememberColor = new Color(0.135F, 0.135F, 0.135F);
	
	Node _sceneInstance;
	
	CheckButton _trackOption;
	CheckButton _iconOption;
	
	Control _multiDialog;
	Control _seekerDialog;
	
	AnimationPlayer _splashAnim;
	AnimationPlayer _messageAnim;
	
	public override void _Ready()
	{
		OS.SetWindowTitle("Auto-Tracker for KH Randomizer [v1.25] | TopazTK");
		
		_sceneInstance = GetNode("Check Panel");
		
		_multiDialog = GetNode("Multi Dialog") as Control;
		_seekerDialog = GetNode("Seeker Dialog") as Control;
		
		_trackOption = GetNode("Multi Dialog/Tabs/General/Track Toggle") as CheckButton;
		_iconOption = GetNode("Multi Dialog/Tabs/General/Icon Toggle") as CheckButton;
		
		_splashAnim = GetNode("SplashAnims") as AnimationPlayer;
		_messageAnim = GetNode("MessageAnims") as AnimationPlayer;
		
		_splashAnim.Play("Logo Fade");
	}
	
	public override void _Process(float delta)
	{
		if (_trackOption.Pressed)
		{
			_frameElapse++;
			
			if (_frameElapse == 85)
			{
				if (Hypervisor.GameProcess == null && _seekerDialog.Visible)
				{
					var _processes = Process.GetProcessesByName("KINGDOM HEARTS FINAL MIX");
					
					if (_processes.Length > 0)
					{
						Hypervisor.GameProcess = _processes[0];
						Hypervisor.GameHandle = Hypervisor.GameProcess.Handle;
						Hypervisor.ExeAddress = (ulong)Hypervisor.GameProcess.MainModule.BaseAddress;
						Hypervisor.GameAddress = Hypervisor.ExeAddress + 0x3A0606;
						
						Singleton.TrackMode = 1;
						_messageAnim.Play("SeekerHide");
					}
				}
				
				_frameElapse = 0;
			}
		}
		
		if (Input.IsActionJustPressed("settings_toggle") && !_seekerDialog.Visible)
		{
			if (!_multiDialog.Visible)
				_messageAnim.Play("DialogShow");
				
			else
				_messageAnim.Play("DialogHide");
		}
	}
	
	private void iconSelect(byte index) => Singleton.IconMode = index;
	
	private void trackSelect()
	{
		if (!_trackOption.Pressed)
		{
			Hypervisor.GameProcess = null;
			Singleton.TrackMode = 0;
		}
		
		else
		{
			if (!_seekerDialog.Visible)
				_messageAnim.Play("SeekerShow");
		}
		
		_sceneInstance.QueueFree();
		
		var _scene = GD.Load<PackedScene>("res://Scenes/TrackKH1.tscn");
		_sceneInstance = _scene.Instance();
		
		(_sceneInstance as TrackKH1).BackColor = _rememberColor;
		
		AddChild(_sceneInstance);
		MoveChild(_sceneInstance, 1);
	}
	
	private void colorShift(Color color)
	{
		(_sceneInstance as TrackKH1).BackColor = color;
		_rememberColor = color;
	}
	
	private void seekerAbort()
	{
		_trackOption.Pressed = false;
		_messageAnim.Play("SeekerHide");
	}
	
	private void iconToggle() => Singleton.IconMode = _iconOption.Pressed == true ? (byte)1 : (byte)0;
}
