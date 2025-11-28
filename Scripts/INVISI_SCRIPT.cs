using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;

public partial class INVISI_SCRIPT : Control
{
	[Export] public int AMOUNT = 0;
	[Export] public int MAX_AMOUNT = 0;
	[Export] public int SUB_AMOUNT = 0;
	[Export] public int MAX_SUB_AMOUNT = 0;
	[Export] public bool SHOW_SUM_AMOUNT;
	[Export] public int ARRAY_LENGTH = 0;
	[Export] public byte TRACK_MODE;
	[Export] public ulong TRACK_ADDRESS = 0x00;
	[Export] public string CAT_PATH = "debug";
	[Export] public string ICON_PATH = "debug";
	
	bool _isInit = false;
	bool _mouseOver = false;
	bool _iconMode = false;
	string _texturePath = "Assets/Minimal/";
	
	public void mouseEnter() => _mouseOver = true;
	public void mouseExit() => _mouseOver = false;
	
	public override void _Ready()
	{
		_isInit = false;
		
		_iconMode = GLOBAL_VARS.ICON_CLASSIC;
		_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
		
		var _fetchMainPath = ICON_PATH == "debug" ? "Assets/debug.png" : _texturePath + CAT_PATH + "/" + ICON_PATH + ".dds";
		
		AddUserSignal("AUTOSAVE");
		AddUserSignal("RECEIVE_SIGNAL");
		
		_isInit = true;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (_iconMode != GLOBAL_VARS.ICON_CLASSIC)
		{
			_iconMode = GLOBAL_VARS.ICON_CLASSIC;
			_texturePath = _iconMode ? "Assets/Classic/" : "Assets/Minimal/";
			
			var _fetchMainPath = ICON_PATH == "debug" ? "Assets/debug.png" : _texturePath + CAT_PATH + "/" + ICON_PATH + (TRACK_MODE == 0x01 ? AMOUNT.ToString("_0") + ".dds" : ".dds");
		}
		
		if (TRACK_ADDRESS != 0x00)
		{
			switch (TRACK_MODE)
			{
				case 0:
				{
					var _fetchAmount = Hypervisor.Read<byte>(TRACK_ADDRESS);
					var _fetchRemain = MAX_AMOUNT - _fetchAmount;
					
					if (_fetchAmount > MAX_AMOUNT)
						_fetchRemain = 0;
					
					if (_fetchRemain < AMOUNT && AMOUNT > 0)
					{  
						EmitSignal("RECEIVE_SIGNAL", ICON_PATH, Math.Abs(AMOUNT - _fetchRemain), CAT_PATH);
						
						AMOUNT = _fetchRemain;
						
						if (GLOBAL_VARS.IS_AUTOSAVE && _isInit)
							EmitSignal("AUTOSAVE");
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
					
					if (_fetchRemain < AMOUNT && AMOUNT > 0)
					{
						EmitSignal("RECEIVE_SIGNAL", ICON_PATH, Math.Abs(AMOUNT - _fetchRemain), CAT_PATH);
						AMOUNT = _fetchRemain;
						
						if (GLOBAL_VARS.IS_AUTOSAVE && _isInit)
							EmitSignal("AUTOSAVE");
					}
					
					break;
				}
			}
		}
	}
}
