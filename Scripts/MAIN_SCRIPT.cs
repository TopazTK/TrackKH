using Godot;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public partial class MAIN_SCRIPT : Control
{
	AnimationPlayer ANIM_SPLASH;
	
	public override void _Ready()
	{
		GetWindow().Title = "Auto-Tracker for KH Randomizer [v3.00] | TopazTK";
		
		ANIM_SPLASH = GetNode("ANIM_SPLASH") as AnimationPlayer;
		ANIM_SPLASH.Play("SPLASH_INIT");
		
		Task.Run(() => 
		{
			var _fetchProcess = Process.GetProcessesByName("KINGDOM HEARTS FINAL MIX");
			
			while(_fetchProcess.Length == 0)
				_fetchProcess = Process.GetProcessesByName("KINGDOM HEARTS FINAL MIX");
			
			while (ANIM_SPLASH.CurrentAnimation == "SPLASH_INIT");
			
			while (ANIM_SPLASH.CurrentAnimation == "SPLASH_LOOP")
				if (ANIM_SPLASH.CurrentAnimationPosition <= 0.3F)
					ANIM_SPLASH.Play("SPLASH_FINISH");
			
			var _fetchModules = _fetchProcess[0].Modules.OfType<ProcessModule>();
			var _checkSteam = _fetchModules.Any(x => x.ModuleName.Contains("steam_api64"));
			
			if (!_checkSteam)
				Hypervisor.AttachProcess(_fetchProcess[0], 0xA00);
			
			else
				Hypervisor.AttachProcess(_fetchProcess[0]);
		});
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("track_save"))
		{
			var _checkNode = GetNode("CHECK_SCENE") as PANEL_SCRIPT;
			
			foreach (var _child in _checkNode.GetChildren())
				_child.SetOwner(_checkNode);
			
			var _saveScene = new PackedScene();
			_saveScene.Pack(_checkNode);
			
			ResourceSaver.Save(_saveScene, "user://saveTrack.tscn");
			
			var _saveDialog = new AcceptDialog();
			
			_saveDialog.DialogText = "Tracker State has been saved successfully!";
			_saveDialog.Title = "Save Successful";
			
			_saveDialog.Connect("canceled", new Callable(_saveDialog, "queue_free"));
			_saveDialog.Connect("confirmed", new Callable(_saveDialog, "queue_free"));
			
			AddChild(_saveDialog);
			_saveDialog.PopupCentered();
		}
		
		if (Input.IsActionJustPressed("track_load"))
		{
			if (!FileAccess.FileExists("user://saveTrack.tscn"))
			{
				var _loadDialog = new AcceptDialog();
				
				_loadDialog.DialogText = "Cannot load the Tracker State, it does not exist!";
				_loadDialog.Title = "Save Does Not Exist";
				
				_loadDialog.Connect("canceled", new Callable(_loadDialog, "queue_free"));
				_loadDialog.Connect("confirmed", new Callable(_loadDialog, "queue_free"));
				
				AddChild(_loadDialog);
				_loadDialog.PopupCentered();
			}
			
			else
			{
				var _checkNode = GetNode("CHECK_SCENE") as PANEL_SCRIPT;
				RemoveChild(_checkNode);
				
				var _loadScene = GD.Load<PackedScene>("user://saveTrack.tscn");
				var _instantiateScene = _loadScene.Instantiate() as PANEL_SCRIPT;
				
				AddChild(_instantiateScene);
			}
		}
	}
}
