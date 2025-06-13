using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class RC_SCRIPT : Control
{
	[Export] public int AMOUNT = 0;
	[Export] public int MAX_AMOUNT = 0;
	[Export] public int SUB_AMOUNT = 0;
	[Export] public int MAX_SUB_AMOUNT = 0;
	[Export] public bool SHOW_SUM_AMOUNT;
	[Export] public int ARRAY_LENGTH = 0;
	[Export] public byte TRACK_MODE;
	[Export] public ulong TRACK_ADDRESS = 0x00;
	[Export] public string ICON_PATH = "debug";
	
	TextureRect ICON_MAIN;
	TextureRect SHDW_MAIN;
	
	TextureRect ICON_NUMBER;
	TextureRect SHDW_NUMBER;
	
	AnimationPlayer ANIM_MAIN;
	AnimationPlayer ANIM_NUMBER;
	
	bool _mouseOver = false;
	string _texturePath = "Assets/Minimal/";
	
	public void mouseEnter() => _mouseOver = true;
	public void mouseExit() => _mouseOver = false;
	
	public override void _Ready()
	{
		var _iconMode = GLOBAL_VARS.ICON_CLASSIC;
		_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _fetchMainPath = ICON_PATH == "debug" ? "Assets/debug.png" : _texturePath + "Regular Checks/" + ICON_PATH + ".png";
		
		var _loadMain = ResourceLoader.Load(_fetchMainPath) as Texture2D;
		
		ICON_MAIN = GetNode("ICON_MAIN") as TextureRect;
		ICON_NUMBER = GetNode("ICON_NUMBER") as TextureRect;
		
		SHDW_MAIN = GetNode("ICON_MAIN/SHDW_MAIN") as TextureRect;
		SHDW_NUMBER = GetNode("ICON_NUMBER/SHDW_NUMBER") as TextureRect;
		
		ANIM_MAIN = GetNode("ANIM_MAIN") as AnimationPlayer;
		ANIM_NUMBER = GetNode("ANIM_NUMBER") as AnimationPlayer;
		
		ICON_MAIN.Texture = _loadMain;
		SHDW_MAIN.Texture = _loadMain;
		
		if (AMOUNT > 0)
		{
			var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + AMOUNT + ".png") as Texture2D;
			
			ICON_NUMBER.Texture = _numberTexture;
			SHDW_NUMBER.Texture = _numberTexture;
		}
		this.MouseEntered += mouseEnter;
		this.MouseExited += mouseExit;
		
		if (AMOUNT > 0)
			ANIM_MAIN.Play("MAIN_ACTIVATE");
		
		if (AMOUNT > 1)
			ANIM_NUMBER.Play("NUMBER_APPEAR");
		
		AddUserSignal("RECEIVE_SIGNAL");
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (TRACK_ADDRESS != 0x00)
		{
			switch (TRACK_MODE)
			{
				case 0:
				{
					var _fetchAmount = Hypervisor.Read<byte>(TRACK_ADDRESS);
					var _fetchRemain = MAX_AMOUNT - _fetchAmount;
					
					if (_fetchRemain < AMOUNT)
					{
						if (_fetchRemain == 0 && AMOUNT != _fetchRemain)
							ANIM_MAIN.Play("MAIN_DEACTIVATE");
						
						if (_fetchRemain > 1)
						{
							var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + _fetchRemain + ".png") as Texture2D;
							
							ICON_NUMBER.Texture = _numberTexture;
							SHDW_NUMBER.Texture = _numberTexture;
						}
						
						else if (_fetchRemain <= 1 && ICON_NUMBER.Visible)
							ANIM_NUMBER.Play("NUMBER_DISAPPEAR");
						
						AMOUNT = _fetchRemain;
						EmitSignal("RECEIVE_SIGNAL", ICON_PATH);
					}
					
					break;
				}
				
				case 3:
				{
					var _fetchArray = Hypervisor.Read<byte>(TRACK_ADDRESS, ARRAY_LENGTH);
					var _count = _fetchArray.Where(x => x == SUB_AMOUNT || x - 0x80 == SUB_AMOUNT).Count();
					
					if (MAX_SUB_AMOUNT != 0)
						_count += _fetchArray.Where(x => x == MAX_SUB_AMOUNT || x - 0x80 == MAX_SUB_AMOUNT).Count();
					
					var _fetchRemain = MAX_AMOUNT - _count;
					
					if (_fetchRemain < AMOUNT)
					{
						if (_fetchRemain == 0 && AMOUNT != _fetchRemain)
						{
							ANIM_MAIN.Play("MAIN_DEACTIVATE");
							ANIM_NUMBER.Play("NUMBER_DISAPPEAR");
						}
						
						if (_fetchRemain > 1)
						{
							var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + _fetchRemain + ".png") as Texture2D;
							
							ICON_NUMBER.Texture = _numberTexture;
							SHDW_NUMBER.Texture = _numberTexture;
						}
						
						else if (_fetchRemain <= 1 && ICON_NUMBER.Visible)
							ANIM_NUMBER.Play("NUMBER_DISAPPEAR");
						
						AMOUNT = _fetchRemain;
						EmitSignal("RECEIVE_SIGNAL", ICON_PATH);
					}
					
					break;
				}
			}
		}
	}
}
