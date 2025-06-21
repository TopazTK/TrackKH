using Godot;
using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public partial class MAIN_SCRIPT : Control
{
	Control OPTIONS;
	
	AnimationPlayer ANIM_SPLASH;
	AnimationPlayer ANIM_WINDOW;
	
	bool _isArchipealgo = false;
	
	public static void ToggleIcons(bool Input) => GLOBAL_VARS.ICON_CLASSIC = Input;
	public static void ToggleAutosave(bool Input) => GLOBAL_VARS.IS_AUTOSAVE = Input;
	
	public void ArchipelagoEvent(bool Input)
	{
		var _checkNode = GetNode("CHECK_SCENE") as PANEL_SCRIPT;
		var _backNode = GetNode("BACKGROUND") as ColorRect;
		
		RemoveChild(_checkNode);
		
		var _loadScene = GD.Load<PackedScene>("res://Scenes/" + (Input ? "ARCHIPELAGO_PANEL" : "CHECK_PANEL") + ".tscn");
		var _instantiateScene = _loadScene.Instantiate() as PANEL_SCRIPT;
	
		_instantiateScene.MouseFilter = MouseFilterEnum.Pass;
		_instantiateScene.OffsetLeft = 10;
		_instantiateScene.OffsetTop = 10;
		_instantiateScene.OffsetRight = -15;
		_instantiateScene.OffsetBottom = -10;
		
		AddChild(_instantiateScene);
		MoveChild(_instantiateScene, 1);
		
		DisplayServer.WindowSetSize(new Vector2I(570, Input ? 1340 : 1255));
		GetWindow().ContentScaleSize = new Vector2I(570, Input ? 1340 : 1255);
		this.SetPosition(new Vector2(0, 0));
		
		var _worldNode = GetNode("CHECK_SCENE/MAIN_CONTAINER/WORLD_CONTAINER") as Container;
		var _countChecks = GetNode("CHECK_SCENE/MAIN_CONTAINER/COUNTABLES") as Container;
		var _trackChecks = GetNode("CHECK_SCENE/MAIN_CONTAINER/TRACKABLES") as Container;
		
		foreach (var _item in _worldNode.GetChildren())
			_item.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
		
		foreach (var _parent in _countChecks.GetChildren())
			foreach (var _item in _parent.GetChildren())
				_item.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
		
		foreach (var _parent in _trackChecks.GetChildren())
			foreach (var _item in _parent.GetChildren())
				_item.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
		
		if (Input)
		{
			var _archiNode = GetNode("CHECK_SCENE/MAIN_CONTAINER/ARCH_CONTAINER/ARCHIPELAGO") as WORLD_SCRIPT;
			_archiNode.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
		}
		
		_isArchipealgo = Input;
	}
	
	public void ChangeBackground(Color Input)
	{
		var _backgroundCheck = GetNode("CHECK_SCENE") as Panel;
		_backgroundCheck.SelfModulate = Input;
		
		var _backImage = GetNode("CHECK_SCENE/BACKGROUND") as TextureRect;
		_backImage.Visible = false;
	}
	
	public void BackPopupEvent() => (GetNode("OPTIONS/BACKPANEL/TABS/Options/BCK_DIALOG") as FileDialog).Popup();
	
	public void BackChangeEvent(string Input)
	{
		var _backImage = GetNode("CHECK_SCENE/BACKGROUND") as TextureRect;
		_backImage.Texture = ResourceLoader.Load(Input) as CompressedTexture2D;
		_backImage.Visible = true;
	}
	
	public void DoAutosave()
	{
		var _checkNode = GetNode("CHECK_SCENE") as PANEL_SCRIPT;
		
		foreach (var _child in _checkNode.GetChildren())
			_child.SetOwner(_checkNode);
		
		var _saveScene = new PackedScene();
		_saveScene.Pack(_checkNode);
		
		ResourceSaver.Save(_saveScene, "user://saveTrack.tscn");
	}
	
	public override void _Ready()
	{
		GetWindow().Title = "Auto-Tracker for KH Randomizer [v4.00] | TopazTK";
		
		OPTIONS = GetNode("OPTIONS") as Control;
		ANIM_WINDOW = GetNode("ANIM_WINDOW") as AnimationPlayer;
		
		ANIM_SPLASH = GetNode("ANIM_SPLASH") as AnimationPlayer;
		ANIM_SPLASH.Play("SPLASH_INIT");
		
		var _backButton = GetNode("OPTIONS/BACKPANEL/TABS/Options/BCK_BUTTON") as Button;
		var _backDialog = GetNode("OPTIONS/BACKPANEL/TABS/Options/BCK_DIALOG") as FileDialog;
		var _classicButton = GetNode("OPTIONS/BACKPANEL/TABS/Options/CLASSIC_CHECK") as CheckButton;
		var _autosaveButton = GetNode("OPTIONS/BACKPANEL/TABS/Options/AUTOSAVE_CHECK") as CheckButton;
		var _archipelagoButton = GetNode("OPTIONS/BACKPANEL/TABS/Options/ARCHI_CHECK") as CheckButton;
		var _colorPicker = GetNode("OPTIONS/BACKPANEL/TABS/Options/BCK_COLORPICK") as ColorPickerButton;
		
		_backButton.Pressed += BackPopupEvent;
		_classicButton.Toggled += ToggleIcons;
		_colorPicker.ColorChanged += ChangeBackground;
		_backDialog.FileSelected += BackChangeEvent;
		_archipelagoButton.Toggled += ArchipelagoEvent;
		_autosaveButton.Toggled += ToggleAutosave;
		
		Task.Run(() => 
		{
			while (ANIM_SPLASH.CurrentAnimation != "SPLASH_LOOP");
			while (ANIM_SPLASH.CurrentAnimationPosition < 0.3F);
			
			Callable.From(() => 
			{
				var _worldNode = GetNode("CHECK_SCENE/MAIN_CONTAINER/WORLD_CONTAINER") as Container;
				var _countChecks = GetNode("CHECK_SCENE/MAIN_CONTAINER/COUNTABLES") as Container;
				var _trackChecks = GetNode("CHECK_SCENE/MAIN_CONTAINER/TRACKABLES") as Container;
				
				foreach (var _item in _worldNode.GetChildren())
					_item.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
				
				foreach (var _parent in _countChecks.GetChildren())
					foreach (var _item in _parent.GetChildren())
						_item.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
				
				foreach (var _parent in _trackChecks.GetChildren())
					foreach (var _item in _parent.GetChildren())
						_item.Connect("AUTOSAVE", new Callable(this, MethodName.DoAutosave));
			}).CallDeferred();
		});
		
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
			var _checkEpic = _fetchModules.Any(x => x.ModuleName.Contains("EOSSDK-Win64-Shipping"));
			
			if (_checkEpic)
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
				
				_instantiateScene.MouseFilter = MouseFilterEnum.Pass;
				
				AddChild(_instantiateScene);
				MoveChild(_instantiateScene, 1);
			}
		}
		
		if (Input.IsActionJustPressed("track_settings"))
			ANIM_WINDOW.Play(OPTIONS.Visible ? "WINDOW_DISAPPEAR" : "WINDOW_APPEAR");
	}
}
