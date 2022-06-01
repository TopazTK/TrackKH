using Godot;
using System;

public class TrackedWorld : HBoxContainer
{
	[Export] public bool Enabled = true;
	
	[Export] public int RequiredValue = 0;
	[Export] public string TexturePath = "debug.png";
	
	private TextureRect _mainNode;
	private TextureRect _shadowNode;
	
	private bool _mouseOver = false;
	private bool _permDisable = false;
	
	private byte _lastCount = 0;
	private byte _subLastCount = 0;
	
	private int _displayedNumber = 0;
	private int _subDisplayedNumber = 0;
	
	private byte _loadedIcon;
	
	private byte _textureSwitch = 0;
	private byte _textureBitwise = 0;
	
	public void mouseEnter() => _mouseOver = true;
	public void mouseExit() => _mouseOver = false;
	
	public override void _Ready()
	{
		_mainNode = GetNode("MainAsset") as TextureRect;
		_shadowNode = _mainNode.GetNode("ShadowAsset") as TextureRect;
		
		_loadedIcon = Singleton.IconMode;
		var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
		
		_mainNode.Texture.Dispose();
		_shadowNode.Texture.Dispose();
		
		var _texturePath = TexturePath;
		
		if (_texturePath.Contains("{0}"))
			_texturePath = string.Format(_texturePath, 0);
		
		_mainNode.Texture = ResourceLoader.Load(_assetPath + _texturePath) as Texture;
		_shadowNode.Texture = ResourceLoader.Load(_assetPath + _texturePath) as Texture;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
