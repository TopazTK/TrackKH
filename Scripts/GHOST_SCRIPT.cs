using Godot;
using System;

public partial class GHOST_SCRIPT : Control
{
	[Export] public string ICON_PATH = "debug";
	
	TextureRect ICON_MAIN;
	TextureRect SHDW_MAIN;
	
	AnimationPlayer ANIM_MAIN;
	
	bool _iconMode = false;
	string _texturePath = "Assets/Minimal/";
	
	public override void _Ready()
	{
		_iconMode = GLOBAL_VARS.ICON_CLASSIC;
		_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _loadMain = ResourceLoader.Load(_texturePath + ICON_PATH + ".dds") as Texture2D;
		
		ICON_MAIN = GetNode("ICON_MAIN") as TextureRect;
		SHDW_MAIN = GetNode("ICON_MAIN/SHDW_MAIN") as TextureRect;
		
		ANIM_MAIN = GetNode("ANIM_MAIN") as AnimationPlayer;
		
		ICON_MAIN.Texture = _loadMain;
		SHDW_MAIN.Texture = _loadMain;
		
		ANIM_MAIN.Play("MAIN_ACTIVATE");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (_iconMode != GLOBAL_VARS.ICON_CLASSIC)
		{
			_iconMode = GLOBAL_VARS.ICON_CLASSIC;
			_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
			
			var _fetchMainPath = ICON_PATH == "debug" ? "Assets/debug.png" : _texturePath + ICON_PATH + ".dds";
			var _loadMain = ResourceLoader.Load(_fetchMainPath) as Texture2D;
			
			ICON_MAIN.Texture = _loadMain;
			SHDW_MAIN.Texture = _loadMain;
		}
	}
}
