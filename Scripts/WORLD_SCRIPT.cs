using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class WORLD_SCRIPT : Control
{
	/*
	 *
	 * A LIST OF WORLD IDS FOR USE:
	 *
	 * 0x00 - N/A
	 * 0x01 - Destiny Islands
	 * 0x02 - N/A
	 * 0x03 - Traverse Town
	 * 0x04 - Wonderland
	 * 0x05 - Deep Jungle
	 * 0x06 - 100 Acre Woods
	 * 0x07 - Agrabah [Unused?]
	 * 0x08 - Agrabah
	 * 0x09 - Atlantica
	 * 0x0A - Halloween Town
	 * 0x0B - Olympus Coliseum
	 * 0x0C - Monstro
	 * 0x0D - Neverland
	 * 0x0E - Hollow Bastion [Unused?]
	 * 0x0F - Hollow Bastion
	 * 0x10 - End of the World
	 *
	 */
	
	[Export] public int AMOUNT = 0;
	[Export] public byte WORLD_ID;
	[Export] public bool IS_ACTIVE = false;
	[Export] public bool IS_LOCKED = false;
	[Export] public bool IS_IGNORED = false;
	[Export] public ulong AMOUNT_ADDRESS = 0x00;
	[Export] public ulong UNLOCK_ADDRESS = 0x00;
	[Export] public string ICON_PATH = "debug";
	
	[Export] public string[] GHOST_NAMES = new string[0];
	
	TextureRect BACKDROP;
	
	TextureRect ICON_MAIN;
	TextureRect SHDW_MAIN;
	
	TextureRect ICON_NUMBER;
	TextureRect SHDW_NUMBER;
	
	TextureRect ICON_SPECIAL;
	TextureRect SHDW_SPECIAL;
	
	AnimationPlayer ANIM_MAIN;
	AnimationPlayer ANIM_NUMBER;
	AnimationPlayer ANIM_SPECIAL;
	AnimationPlayer ANIM_BACKDROP;
	
	GridContainer CHECK_CONTAIN;
	
	bool _mouseOver = false;
	bool _signalConnect = false;
	
	string _texturePath = "Assets/Minimal/";

	public void mouseEnter() => _mouseOver = true;
	public void mouseExit() => _mouseOver = false;
	
	public override void _Ready()
	{
		var _iconMode = GLOBAL_VARS.ICON_CLASSIC;
		_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _loadMain = ResourceLoader.Load(_texturePath + "Worlds/" + ICON_PATH + ".png") as Texture2D;
		var _ignoreTexture = ResourceLoader.Load("Assets/General/ignore.png") as Texture2D;
		
		IS_LOCKED = false;
		IS_ACTIVE = false;
		
		BACKDROP = GetNode("BACKDROP") as TextureRect;
		
		ICON_MAIN = GetNode("ICON_MAIN") as TextureRect;
		ICON_NUMBER = GetNode("ICON_NUMBER") as TextureRect;
		ICON_SPECIAL = GetNode("ICON_SPECIAL") as TextureRect;
		
		SHDW_MAIN = GetNode("ICON_MAIN/SHDW_MAIN") as TextureRect;
		SHDW_NUMBER = GetNode("ICON_NUMBER/SHDW_NUMBER") as TextureRect;
		SHDW_SPECIAL = GetNode("ICON_SPECIAL/SHDW_SPECIAL") as TextureRect;
		
		ANIM_MAIN = GetNode("ANIM_MAIN") as AnimationPlayer;
		ANIM_NUMBER = GetNode("ANIM_NUMBER") as AnimationPlayer;
		ANIM_SPECIAL = GetNode("ANIM_SPECIAL") as AnimationPlayer;
		ANIM_BACKDROP = GetNode("ANIM_BACKDROP") as AnimationPlayer;
		
		CHECK_CONTAIN = GetNode("CHECK_CONTAIN") as GridContainer;
		
		ICON_MAIN.Texture = _loadMain;
		SHDW_MAIN.Texture = _loadMain;
		
		this.MouseEntered += mouseEnter;
		this.MouseExited += mouseExit;
		
		if (IS_IGNORED)
		{
			ICON_SPECIAL.Texture = _ignoreTexture;
			SHDW_SPECIAL.Texture = _ignoreTexture;
			
			ANIM_MAIN.Play("MAIN_DISABLE");
			ANIM_SPECIAL.Play("SPECIAL_APPEAR");
		}
		
		foreach(var _ghostName in GHOST_NAMES)
		{
			var _fetchScene = GD.Load<PackedScene>("res://Scenes/GHOST_CHECK.tscn");
			var _ghostCheck = _fetchScene.Instantiate() as GHOST_SCRIPT;
			_ghostCheck.ICON_PATH = "Regular Checks/" + _ghostName;
			
			CHECK_CONTAIN.AddChild(_ghostCheck);
		}
		
		if (AMOUNT > 0)
		{
			var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + AMOUNT + ".png") as Texture2D;
			
			ICON_NUMBER.Texture = _numberTexture;
			SHDW_NUMBER.Texture = _numberTexture;
			
			ANIM_NUMBER.Play("NUMBER_APPEAR");
		}
		
		ExecuteLogic();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (!_signalConnect)
		{
			var _parentNode = GetParent().GetParent();
			var _trackableNode = _parentNode.GetNode("TRACKABLES");
			
			foreach (var _node in _trackableNode.GetChildren())
			{
				foreach (var _subNode in _node.GetChildren())
					_subNode.Connect("RECEIVE_SIGNAL", new Callable(this, MethodName.ApplyCheck));
			}
			
			_signalConnect = true;
		}
		
		if (Input.IsActionJustPressed("track_toggle") && _mouseOver)
		{
			var _ignoreTexture = ResourceLoader.Load("Assets/General/ignore.png") as Texture2D;
			var _lockTexture = ResourceLoader.Load("Assets/General/lock.png") as Texture2D;
			
			ICON_SPECIAL.Texture = _ignoreTexture;
			SHDW_SPECIAL.Texture = _ignoreTexture;
			
			if (!IS_LOCKED)
			{
				ANIM_MAIN.Play(IS_IGNORED ? "MAIN_ENABLE" : "MAIN_DISABLE");
				ANIM_SPECIAL.Play(IS_IGNORED ? "SPECIAL_DISAPPEAR" : "SPECIAL_APPEAR");
				
				if (AMOUNT > 0)
					ANIM_NUMBER.Play(IS_IGNORED ? "NUMBER_APPEAR" : "NUMBER_DISAPPEAR");
				
				if (IS_ACTIVE)
					ANIM_BACKDROP.Play(IS_IGNORED ? "BACKDROP_ACTIVE" : "BACKDROP_IDLE");
			}
			
			else if (IS_IGNORED)
			{
				ICON_SPECIAL.Texture = _lockTexture;
				SHDW_SPECIAL.Texture = _lockTexture;
			}
			
			IS_IGNORED = !IS_IGNORED;
		}
		
		ExecuteLogic();
	}
	
	public void ExecuteLogic()
	{
		if (!IS_IGNORED)
		{
			var _worldRead = Hypervisor.Read<byte>((Hypervisor.BaseOffset == 0xA00 ? 0x234045CU : 0x233FE84U));
			var _checkGummi = Hypervisor.Read<long>((Hypervisor.BaseOffset == 0xA00 ? 0x5079A8U : 0x05076A8U));
			
			var _amountRead = AMOUNT_ADDRESS == 0x00 ? 0x00 : Hypervisor.Read<byte>(AMOUNT_ADDRESS);
			
			if (UNLOCK_ADDRESS != 0x00)
			{
				var _unlockAmount = Hypervisor.Read<byte>(UNLOCK_ADDRESS);
				
				if (_unlockAmount == 0x00 && !IS_LOCKED)
				{
					var _lockTexture = ResourceLoader.Load("Assets/General/lock.png") as Texture2D;
					
					ICON_SPECIAL.Texture = _lockTexture;
					SHDW_SPECIAL.Texture = _lockTexture;
					
					ANIM_MAIN.Play("MAIN_DISABLE");
					ANIM_SPECIAL.Play("SPECIAL_APPEAR");
					
					IS_LOCKED = true;
				}
				
				else if (_unlockAmount > 0x00 && IS_LOCKED)
				{
					ANIM_MAIN.Play("MAIN_ENABLE");
					ANIM_SPECIAL.Play("SPECIAL_DISAPPEAR");
					
					IS_LOCKED = false;
				}
			}
			
			if (_amountRead > AMOUNT)
			{
				var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + _amountRead + ".png") as Texture2D;
				
				ICON_NUMBER.Texture = _numberTexture;
				SHDW_NUMBER.Texture = _numberTexture;
				
				if (_amountRead != 0x00 && !ICON_NUMBER.Visible)
					ANIM_NUMBER.Play("NUMBER_APPEAR");
				
				else if (_amountRead == 0x00 && ICON_NUMBER.Visible)
					ANIM_NUMBER.Play("NUMBER_DISAPPEAR");
				
				AMOUNT = _amountRead;
			}
			
			if (WORLD_ID != 0x00 && !IS_LOCKED)
			{
				if (_checkGummi == 0x00 && _worldRead == WORLD_ID && !IS_ACTIVE)
				{
					ANIM_BACKDROP.Play("BACKDROP_ACTIVE");
					IS_ACTIVE = true;
				}
				
				else if ((_checkGummi != 0x00 || _worldRead != WORLD_ID) && IS_ACTIVE)
				{
					ANIM_BACKDROP.Play("BACKDROP_IDLE");
					IS_ACTIVE = false;
				}
			}
		}
	}
	
	public void ApplyCheck(string CHECK_NAME)
	{
		var _fetchScene = GD.Load<PackedScene>("res://Scenes/GHOST_CHECK.tscn");
		
		var _textPointer = Hypervisor.Read<ulong>(0x283B3C0);
		var _checkText = Hypervisor.Read<byte>(_textPointer, 0x05, true);
		
		var _checkActive = Hypervisor.Read<byte>(0x283B380);
		var _checkColor = Hypervisor.Read<int>(0x0527A10, 0x04);
		
		var _checkShowing = Hypervisor.Read<byte>(0x283B390);
		
		var _colorEnsure = _checkColor[0] == 0xCA && _checkColor[1] == 0x43 && _checkColor[2] == 0x31 && _checkColor[3] == 0x80;
		
		if (_checkText.SequenceEqual<byte>([ 0x3D, 0x53, 0x56, 0x45, 0x00 ]) && _colorEnsure && _checkActive > 0 && _checkShowing > 0)
		{
			if (ICON_PATH == "levels")
			{
				var _ghostCheck = _fetchScene.Instantiate() as GHOST_SCRIPT;
				_ghostCheck.ICON_PATH = "Regular Checks/" + CHECK_NAME;
				CHECK_CONTAIN.AddChild(_ghostCheck);
				
				var _ghostList = new List<string>();
				_ghostList.AddRange(GHOST_NAMES);
				_ghostList.Add(CHECK_NAME);
				
				GHOST_NAMES = _ghostList.ToArray();
			}
			
			else
				return;
		}
		
		else if (IS_ACTIVE)
		{
			var _ghostCheck = _fetchScene.Instantiate() as GHOST_SCRIPT;
			_ghostCheck.ICON_PATH = "Regular Checks/" + CHECK_NAME;
			CHECK_CONTAIN.AddChild(_ghostCheck);
			
			var _ghostList = new List<string>();
			_ghostList.AddRange(GHOST_NAMES);
			_ghostList.Add(CHECK_NAME);
				
			GHOST_NAMES = _ghostList.ToArray();
		}
	}
}
