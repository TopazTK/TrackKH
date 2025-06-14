using Godot;
using System;
using System.Linq;

public partial class IC_SCRIPT : Control
{
	[Export] public int AMOUNT = 0;
	[Export] public int MAX_AMOUNT = 0;
	[Export] public int SUB_AMOUNT = 0;
	[Export] public int MAX_SUB_AMOUNT = 0;
	[Export] public bool SHOW_SUM_AMOUNT;
	[Export] public int ARRAY_LENGTH = 0;
	[Export] public int SUB_ARRAY_LENGTH = 0;
	[Export] public byte TRACK_MODE;
	[Export] public byte SUB_TRACK_MODE;
	[Export] public bool IS_IGNORED;
	[Export] public ulong TRACK_ADDRESS = 0x00;
	[Export] public ulong SUB_TRACK_ADDRESS = 0x00;
	[Export] public string ICON_PATH = "debug";
	[Export] public string SUB_ICON_PATH = "debug";

	TextureRect ICON_MAIN;
	TextureRect SHDW_MAIN;
	
	TextureRect ICON_SUBCHECK;
	TextureRect SHDW_SUBCHECK;
	
	TextureRect ICON_NUMBER;
	TextureRect SHDW_NUMBER;
	
	TextureRect ICON_SUBNUM;
	TextureRect SHDW_SUBNUM;
	
	TextureRect ICON_SPECIAL;
	TextureRect SHDW_SPECIAL;
	
	AnimationPlayer ANIM_MAIN;
	AnimationPlayer ANIM_NUMBER;
	AnimationPlayer ANIM_SUBCHECK;
	AnimationPlayer ANIM_SUBNUM;
	AnimationPlayer ANIM_SPECIAL;
	
	bool _mouseOver = false;
	string _texturePath = "Assets/Minimal/";
	
	public void mouseEnter() => _mouseOver = true;
	public void mouseExit() => _mouseOver = false;
	
	public override void _Ready()
	{
		var _iconMode = GLOBAL_VARS.ICON_CLASSIC;
		_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _fetchMainPath = ICON_PATH == "debug" ? "Assets/debug.png" : _texturePath + "Important Checks/" + ICON_PATH + (TRACK_MODE == 0x01 ? AMOUNT.ToString("_0") + ".png" : ".png");
		var _fetchSubPath = SUB_ICON_PATH == "debug" ? "Assets/debug.png" : _texturePath + "Important Checks/Sub-Checks/" + SUB_ICON_PATH + ".png";
		
		var _loadMain = ResourceLoader.Load(_fetchMainPath) as Texture2D;
		var _loadSub = ResourceLoader.Load(_fetchSubPath) as Texture2D;
		
		ICON_MAIN = GetNode("ICON_MAIN") as TextureRect;
		ICON_NUMBER = GetNode("ICON_NUMBER") as TextureRect;
		ICON_SPECIAL = GetNode("ICON_SPECIAL") as TextureRect;
		
		ICON_SUBCHECK = GetNode("ICON_SUBCHECK") as TextureRect;
		ICON_SUBNUM = GetNode("ICON_SUBNUM") as TextureRect;
		
		SHDW_MAIN = GetNode("ICON_MAIN/SHDW_MAIN") as TextureRect;
		SHDW_NUMBER = GetNode("ICON_NUMBER/SHDW_NUMBER") as TextureRect;
		SHDW_SPECIAL = GetNode("ICON_SPECIAL/SHDW_SPECIAL") as TextureRect;
		
		SHDW_SUBCHECK = GetNode("ICON_SUBCHECK/SHDW_SUBCHECK") as TextureRect;
		SHDW_SUBNUM = GetNode("ICON_SUBNUM/SHDW_SUBNUM") as TextureRect;
		
		ANIM_MAIN = GetNode("ANIM_MAIN") as AnimationPlayer;
		ANIM_NUMBER = GetNode("ANIM_NUMBER") as AnimationPlayer;
		ANIM_SUBCHECK = GetNode("ANIM_SUBCHECK") as AnimationPlayer;
		ANIM_SUBNUM = GetNode("ANIM_SUBNUM") as AnimationPlayer;
		ANIM_SPECIAL = GetNode("ANIM_SPECIAL") as AnimationPlayer;
		
		ICON_MAIN.Texture = _loadMain;
		SHDW_MAIN.Texture = _loadMain;
		
		ICON_SUBCHECK.Texture = _loadSub;
		SHDW_SUBCHECK.Texture = _loadSub;
		
		this.MouseEntered += mouseEnter;
		this.MouseExited += mouseExit;
		
		if (AMOUNT != 0)
		{
			var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (AMOUNT == MAX_AMOUNT ? "max" : AMOUNT) + ".png") as Texture2D;
			
			ICON_NUMBER.Texture = _numberTexture;
			SHDW_NUMBER.Texture = _numberTexture;
		}
		
		if (SUB_AMOUNT != 0)
		{
			var _subNumTexture = ResourceLoader.Load("Assets/General/Numbers/" + (SUB_AMOUNT == MAX_SUB_AMOUNT ? "max" : SUB_AMOUNT) + ".png") as Texture2D;
			
			ICON_SUBNUM.Texture = _subNumTexture;
			SHDW_SUBNUM.Texture = _subNumTexture;
		}
		
		if (TRACK_MODE == 1)
			ICON_MAIN.Modulate = new Godot.Color(1, 1, 1);
		
		if (!IS_IGNORED && TRACK_MODE != 1)
		{
			if (SUB_TRACK_ADDRESS != 0x00)
			{
				if (SUB_AMOUNT > 0)
					ANIM_SUBCHECK.Play("SUBCHECK_APPEAR");
				
				if (SUB_AMOUNT >= 1)
					ANIM_SUBNUM.Play("SUBNUM_APPEAR");
			}
			
			if (AMOUNT > 0)
				ANIM_MAIN.Play("MAIN_ACTIVATE");
			
			if (AMOUNT > 1)
				ANIM_NUMBER.Play("NUMBER_APPEAR");
		}
		
		else if (IS_IGNORED)
		{
			ANIM_MAIN.Play("MAIN_DEACTIVATE");
			
			if (TRACK_MODE == 1)
			{
				var _fetchPath = _texturePath + "Important Checks/" + ICON_PATH + "_0.png";
				var _loadTexture = ResourceLoader.Load(_fetchPath) as Texture2D;
				
				ICON_MAIN.Texture = _loadTexture;
				SHDW_MAIN.Texture = _loadTexture;
			}
			
			ANIM_SPECIAL.Play("SPECIAL_APPEAR");
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("track_toggle") && _mouseOver)
		{
			if (AMOUNT > 0)
				ANIM_MAIN.Play(IS_IGNORED ? "MAIN_ACTIVATE" : "MAIN_DEACTIVATE");
			
			if (AMOUNT > 1 && TRACK_MODE != 1)
				ANIM_NUMBER.Play(IS_IGNORED ? "NUMBER_APPEAR" : "NUMBER_DISAPPEAR");
			
			if (SUB_AMOUNT > 0 && TRACK_MODE != 3)
			{
				ANIM_SUBCHECK.Play(IS_IGNORED ? "SUBCHECK_APPEAR" : "SUBCHECK_DISAPPEAR");
				ANIM_SUBNUM.Play(IS_IGNORED ? "SUBNUM_APPEAR" : "SUBNUM_DISAPPEAR");
			}
			
			if (TRACK_MODE == 1)
			{
				var _fetchMainPath = IS_IGNORED ? _texturePath + "Important Checks/" + ICON_PATH + AMOUNT.ToString("_0") + ".png" : _texturePath + "Important Checks/" + ICON_PATH + "_0.png";
				var _loadMain = ResourceLoader.Load(_fetchMainPath) as Texture2D;
				
				ICON_MAIN.Texture = _loadMain;
				SHDW_MAIN.Texture = _loadMain;
			}
			
			ANIM_SPECIAL.Play(IS_IGNORED ? "SPECIAL_DISAPPEAR" : "SPECIAL_APPEAR");
			
			IS_IGNORED = !IS_IGNORED;
		}
		
		ExecuteLogic();
	}
	
	public void ExecuteLogic()
	{
		if (!IS_IGNORED)
		{
			if (SUB_TRACK_ADDRESS != 0x00)
			{
				switch (SUB_TRACK_MODE)
				{
					case 0:
					{
						var _fetchAmount = Hypervisor.Read<byte>(SUB_TRACK_ADDRESS);
						
						if (_fetchAmount > SUB_AMOUNT)
						{
							if (_fetchAmount > 0)
							{
								if (SUB_AMOUNT == 0)
									ANIM_SUBCHECK.Play("SUBCHECK_APPEAR");
								
								var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (_fetchAmount == MAX_SUB_AMOUNT ? "max" : _fetchAmount) + ".png") as Texture2D;
								
								ICON_SUBNUM.Texture = _numberTexture;
								SHDW_SUBNUM.Texture = _numberTexture;
								
								if (_fetchAmount > 1 && !ICON_SUBNUM.Visible)
									ANIM_SUBNUM.Play("SUBNUM_APPEAR");
							}
							
							SUB_AMOUNT = _fetchAmount;
						}
						
						break;
					}
					
					case 1:
					{
						var _fetchAmount = Hypervisor.Read<byte>(SUB_TRACK_ADDRESS, SUB_ARRAY_LENGTH);
						var _count = _fetchAmount.Where(x => x > 0x00).Count();
						
						if (_count > SUB_AMOUNT)
						{
							if (_count > 0)
							{
								if (SUB_AMOUNT == 0)
									ANIM_SUBCHECK.Play("SUBCHECK_APPEAR");
								
								var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (_count == MAX_SUB_AMOUNT ? "max" : _count) + ".png") as Texture2D;
								
								ICON_SUBNUM.Texture = _numberTexture;
								SHDW_SUBNUM.Texture = _numberTexture;
								
								if (_count > 1 && !ICON_SUBNUM.Visible)
									ANIM_SUBNUM.Play("SUBNUM_APPEAR");
							}
							
							SUB_AMOUNT = _count;
						}
						
						break;
					}
				}
			}
			
			if (TRACK_ADDRESS != 0x00)
			{
				switch (TRACK_MODE)
				{
					case 0:
					{
						var _fetchAmount = Hypervisor.Read<byte>(TRACK_ADDRESS) + (SHOW_SUM_AMOUNT ? SUB_AMOUNT : 0);
						
						if (_fetchAmount > AMOUNT)
						{
							if (_fetchAmount > 0 && AMOUNT == 0)
								ANIM_MAIN.Play("MAIN_ACTIVATE");
							
							if (_fetchAmount > 1)
							{
								var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (_fetchAmount == MAX_AMOUNT ? "max" : _fetchAmount) + ".png") as Texture2D;
								
								ICON_NUMBER.Texture = _numberTexture;
								SHDW_NUMBER.Texture = _numberTexture;
								
								if (!ICON_NUMBER.Visible)
									ANIM_NUMBER.Play("NUMBER_APPEAR");
							}
							
							AMOUNT = _fetchAmount;
						}
						
						break;
					}
					
					case 1:
					{
						var _fetchArray = Hypervisor.Read<byte>(TRACK_ADDRESS, ARRAY_LENGTH);
						double _valueBitwise = 0 + (SHOW_SUM_AMOUNT ? SUB_AMOUNT : 0);
						
						for (int i = 0; i < _fetchArray.Length; i++)
						{
							if (_fetchArray[i] != 0)
							_valueBitwise += Math.Pow(2, i);
						}
						
						if (_valueBitwise > AMOUNT)
						{
							var _mainTexture = ResourceLoader.Load(_texturePath + "Important Checks/" + ICON_PATH + _valueBitwise.ToString("_0") + ".png") as Texture2D;
							
							ICON_MAIN.Texture = _mainTexture;
							SHDW_MAIN.Texture = _mainTexture;
							
							AMOUNT = (int)_valueBitwise;
						}
						
						break;
					}
					
					case 2:
					{
						var _fetchArray = Hypervisor.Read<byte>(TRACK_ADDRESS, ARRAY_LENGTH);
						double _count = 0 + (SHOW_SUM_AMOUNT ? SUB_AMOUNT : 0);
						
						for (int i = 0; i < _fetchArray.Length; i++)
						{
							var _value = _fetchArray[i];
							
							_count += (_value & 0x01) == 0x01 ? 0x01 : 0x00;
							_count += (_value & 0x02) == 0x02 ? 0x01 : 0x00;
							_count += (_value & 0x04) == 0x04 ? 0x01 : 0x00;
							_count += (_value & 0x08) == 0x08 ? 0x01 : 0x00;
							_count += (_value & 0x10) == 0x10 ? 0x01 : 0x00;
							_count += (_value & 0x20) == 0x20 ? 0x01 : 0x00;
							_count += (_value & 0x40) == 0x40 ? 0x01 : 0x00;
							_count += (_value & 0x80) == 0x80 ? 0x01 : 0x00;
						}
						
						if (_count > AMOUNT)
						{
							if (_count > 0 && AMOUNT == 0)
								ANIM_MAIN.Play("MAIN_ACTIVATE");
							
							if (_count > 1)
							{
								var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (_count == MAX_AMOUNT ? "max" : _count) + ".png") as Texture2D;
								
								ICON_NUMBER.Texture = _numberTexture;
								SHDW_NUMBER.Texture = _numberTexture;
								
								if (!ICON_NUMBER.Visible)
									ANIM_NUMBER.Play("NUMBER_APPEAR");
							}
							
							AMOUNT = (int)_count;
						}
						
						break;
					}
					
					case 3:
					{
						var _fetchArray = Hypervisor.Read<byte>(TRACK_ADDRESS, ARRAY_LENGTH);
						var _count = _fetchArray.Where(x => x == SUB_AMOUNT || x - 0x80 == SUB_AMOUNT).Count();
						
						if (MAX_SUB_AMOUNT != 0)
							_count += _fetchArray.Where(x => x == MAX_SUB_AMOUNT || x - 0x80 == MAX_SUB_AMOUNT).Count();
						
						if (_count > AMOUNT)
						{
							if (_count > 0 && AMOUNT == 0)
								ANIM_MAIN.Play("MAIN_ACTIVATE");
							
							if (_count > 1)
							{
								var _numberTexture = ResourceLoader.Load("Assets/General/Numbers/" + (_count == MAX_AMOUNT ? "max" : _count) + ".png") as Texture2D;
								
								ICON_NUMBER.Texture = _numberTexture;
								SHDW_NUMBER.Texture = _numberTexture;
								
								if (!ICON_NUMBER.Visible)
									ANIM_NUMBER.Play("NUMBER_APPEAR");
							}
							
							AMOUNT = (int)_count;
						}
						
						break;
					}
				}
			}
		}
	}
}
