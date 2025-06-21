using Godot;
using System;

public partial class DEATH_SCRIPT : Control
{
	[Export] public int AMOUNT = 0;
	[Export] public string ICON_PATH = "debug";
	
	TextureRect ICON_MAIN;
	TextureRect SHDW_MAIN;
	
	TextureRect ICON_NUMBER;
	TextureRect SHDW_NUMBER;
	
	TextureRect ICON_SPECIAL;
	TextureRect SHDW_SPECIAL;
	
	AnimationPlayer ANIM_MAIN;
	AnimationPlayer ANIM_NUMBER;
	AnimationPlayer ANIM_SPECIAL;
	
	bool _isDead = false;
	
	public override void _Ready()
	{
		var _iconMode = GLOBAL_VARS.ICON_CLASSIC;
		var _texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _loadMain = ResourceLoader.Load(_texturePath + "Important Checks/death.dds") as Texture2D;
		
		ICON_MAIN = GetNode("ICON_MAIN") as TextureRect;
		ICON_NUMBER = GetNode("ICON_NUMBER") as TextureRect;
		ICON_SPECIAL = GetNode("ICON_SPECIAL") as TextureRect;
		
		SHDW_MAIN = GetNode("ICON_MAIN/SHDW_MAIN") as TextureRect;
		SHDW_NUMBER = GetNode("ICON_NUMBER/SHDW_NUMBER") as TextureRect;
		SHDW_SPECIAL = GetNode("ICON_SPECIAL/SHDW_SPECIAL") as TextureRect;
		
		ANIM_MAIN = GetNode("ANIM_MAIN") as AnimationPlayer;
		ANIM_NUMBER = GetNode("ANIM_NUMBER") as AnimationPlayer;
		ANIM_SPECIAL = GetNode("ANIM_SPECIAL") as AnimationPlayer;
		
		ICON_MAIN.Texture = _loadMain;
		SHDW_MAIN.Texture = _loadMain;
		
		if (AMOUNT > 0)
		{
			ANIM_MAIN.Play("MAIN_ACTIVATE");
			
			var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (AMOUNT + 1) + ".png") as Texture2D;
			
			ICON_NUMBER.Texture = _numberTexture;
			SHDW_NUMBER.Texture = _numberTexture;
			
			if (!ICON_NUMBER.Visible)
				ANIM_NUMBER.Play("NUMBER_APPEAR");
		}
		
		AddUserSignal("AUTOSAVE");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		var _checkDeath = Hypervisor.Read<ulong>(0x2382568);
		
		if (_checkDeath == Hypervisor.PureAddress + 0x23DB2F0 && !_isDead)
		{
			if (AMOUNT == 0)
				ANIM_MAIN.Play("MAIN_ACTIVATE");
			
			var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (AMOUNT + 1) + ".png") as Texture2D;
			
			ICON_NUMBER.Texture = _numberTexture;
			SHDW_NUMBER.Texture = _numberTexture;
			
			if (!ICON_NUMBER.Visible)
				ANIM_NUMBER.Play("NUMBER_APPEAR");
				
			AMOUNT++;
			_isDead = true;
			
			if (GLOBAL_VARS.IS_AUTOSAVE)
				EmitSignal("AUTOSAVE");
		}
		
		if (_checkDeath != Hypervisor.PureAddress + 0x23DB2F0 && _isDead)
			_isDead = false;
	}
}
