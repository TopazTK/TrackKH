using Godot;
using System;

public partial class GHOST_SCRIPT : Control
{
	[Export] public string ICON_PATH = "debug";
	
	TextureRect ICON_MAIN;
	TextureRect SHDW_MAIN;
	
	AnimationPlayer ANIM_MAIN;
	
	public override void _Ready()
	{
		var _iconMode = GLOBAL_VARS.ICON_CLASSIC;
		var _texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _loadMain = ResourceLoader.Load(_texturePath + ICON_PATH + ".png") as Texture2D;
		
		ICON_MAIN = GetNode("ICON_MAIN") as TextureRect;
		SHDW_MAIN = GetNode("ICON_MAIN/SHDW_MAIN") as TextureRect;
		
		ANIM_MAIN = GetNode("ANIM_MAIN") as AnimationPlayer;
		
		ICON_MAIN.Texture = _loadMain;
		SHDW_MAIN.Texture = _loadMain;
		
		ANIM_MAIN.Play("MAIN_ACTIVATE");
	}
}
