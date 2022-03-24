using Godot;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

public class TrackedItem : Control
{
	[Export] public bool Enabled = true;
	
	[Export] public int RequiredValue = 0;
	[Export] public int MaximumCount = 0;
	[Export] public int ArrayLength = 0;
	[Export] public ulong EntryOffset = 0;
	
	[Export] public int IncreaseFactor = 1;
	
	[Export] public ulong PrimaryAddress = 0x00;
	[Export] public ulong SecondaryAddress = 0x00;
	
	[Export] public int MaximumSubCount = 0x00;
	[Export] public int SubArrayLength = 0;
	
	[Export] public ulong SubAddress = 0x00;
	
	[Export] public int TrackingMode = 0;
	[Export] public int SubTrackingMode = 0;
	
	[Export] public bool BackTrackable = true;
	[Export] public bool SubBackTrackable = true;
	
	[Export] public bool SubInclusive = false;
	
	[Export] public string TexturePath = "debug.png";
	[Export] public string SubTexturePath = "debug.png";
	
	/*
		TrackingMode:
		
		The mode of the tracked item.
		
		0 = Count-based: One address, the value denoted the count.
		1 = Incrementor-based: The address is the starting point, offsets increase by "1" with each item. 
		2 = Look-for-based: A specific value (RequiredValue) will be looked for starting from address (PrimaryAddress) for a certain amount of bytes (ArrayLength)
		3 = Bitwise-lookup: A specific value is bitwised from the address, then compared.
		4 = Bitwise-followup : A specific value is bitwised from the SecondaryAddress, then the count is read from PrimaryAddress.
		5 = Bitwise-calculation: A specific array is taken from the PrimaryAddress, then cound is calculated based on the bitwise.
		6 = Incrementor-based (Offsetted): The address is the starting point, offsets increase by a certain amount each item.. 
	*/
	
	private TextureRect _mainNode;
	private TextureRect _subNode;
	private TextureRect _shadowNode;
	private TextureRect _subShadowNode;
	private TextureRect _crossNode;
	private TextureRect _numberNode;
	private TextureRect _subNumberNode;
	
	private bool _mouseOver = false;
	
	private byte _lastCount = 0;
	private byte _subLastCount = 0;
	
	private int _displayedNumber = 0;
	private int _subDisplayedNumber = 0;
	
	private byte _loadedIcon;
	
	public void mouseEnter() => _mouseOver = true;
	public void mouseExit() => _mouseOver = false;

	public override void _Ready()
	{
		_mainNode = GetNode("MainAsset") as TextureRect;
		_subNode = GetNode("SubAsset") as TextureRect;
		_shadowNode = _mainNode.GetNode("ShadowAsset") as TextureRect;
		_subShadowNode = _subNode.GetNode("ShadowAsset") as TextureRect;
		_crossNode = GetNode("CrossAsset") as TextureRect;
		_numberNode = GetNode("NumberAsset") as TextureRect;
		_subNumberNode = GetNode("SubNumberAsset") as TextureRect;
		
		_loadedIcon = Singleton.IconMode;
		var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
		
		_mainNode.Texture.Dispose();
		_shadowNode.Texture.Dispose();
		
		_subNode.Texture.Dispose();
		_subShadowNode.Texture.Dispose();
		
		_mainNode.Texture = ResourceLoader.Load(_assetPath + TexturePath) as Texture;
		_shadowNode.Texture = ResourceLoader.Load(_assetPath + TexturePath) as Texture;
		
		if ((long)SubAddress != -1)
		{
			_subNode.Texture = ResourceLoader.Load(_assetPath + SubTexturePath) as Texture;
			_subShadowNode.Texture = ResourceLoader.Load(_assetPath + SubTexturePath) as Texture;
		}
	}
	
	private void Disable()
	{
		if (_mainNode.Modulate.a != 0.50F)
		{
			_displayedNumber = 0;
			_subDisplayedNumber = 0;
			_crossNode.Visible = true;
			_numberNode.Visible = false;
			_subNumberNode.Visible = false;
			_numberNode.Texture.Dispose();
			_subNumberNode.Texture.Dispose();
			_subNode.Visible = false;
			_mainNode.Modulate = new Godot.Color(0.30F, 0.30F, 0.30F, 0.50F);
		}
	}
	
	private void Enable()
	{
		if (_mainNode.Modulate.a != 0.25F)
		{
			_mainNode.Modulate = new Godot.Color(1, 1, 1, 0.25F);
			_crossNode.Visible = false;
		}
	}
	
	private void Activate(int Input)
	{
		if (_mainNode.Modulate.a != 1)
			_mainNode.Modulate = new Godot.Color(1, 1, 1, 1);
		
		if (Input > 1 && Input != _displayedNumber)
		{
			_displayedNumber = Input;
			
			_numberNode.Texture.Dispose();
			_numberNode.Texture = ResourceLoader.Load("res://Assets/Numbers/" + Input + ".png") as Texture;
			
			_numberNode.Visible = true;
		}
		
		else if (Input == 1)
		{
			_displayedNumber = 1;
			_numberNode.Visible = false;
			_numberNode.Texture.Dispose();
		}
		
		else if (BackTrackable && Input == 0)
		{
			_displayedNumber = 0;
			_numberNode.Visible = false;
			_numberNode.Texture.Dispose();
		}
	}

	private void Deactivate()
	{
		if (_mainNode.Modulate.a != 0.25F)
		{
			_displayedNumber = 0;
			_numberNode.Visible = false;
			_numberNode.Texture.Dispose();
			_mainNode.Modulate = new Godot.Color(1, 1, 1, 0.25F);
		}
	}
	
	private void SubActivate(int Input)
	{
		if (!_subNode.Visible)
			_subNode.Visible = true;
		
		if (Input >= 1 && Input != _subDisplayedNumber)
		{
			_subDisplayedNumber = Input;
			
			_subNumberNode.Texture.Dispose();
			_subNumberNode.Texture = ResourceLoader.Load("res://Assets/Numbers/" + Input + ".png") as Texture;
			
			_subNumberNode.Visible = true;
		}
		
		else if (SubBackTrackable && Input == 0)
		{
			_subDisplayedNumber = 0;
			_subNumberNode.Visible = false;
			_subNumberNode.Texture.Dispose();
		}
	}

	private void SubDeactivate()
	{
		if (_subNode.Visible)
		{
			_subDisplayedNumber = 0;
			_subNumberNode.Visible = false;
			_subNumberNode.Texture.Dispose();
			_subNode.Visible = false;
		}
	}
	
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("track_toggle") && _mouseOver)
			this.Enabled = !this.Enabled;
			
		if (Singleton.TrackMode == 0)
		{
			if (_mouseOver)
			{
				if (Input.IsActionJustPressed("track_click"))
				{
					if (_lastCount - 1 >= 0 && Input.IsActionPressed("track_modifier"))
						_lastCount -= (byte)IncreaseFactor;
						
					else if (_lastCount + 1 <= MaximumCount && !Input.IsActionPressed("track_modifier"))
						_lastCount += (byte)IncreaseFactor;
				}
				
				if (_lastCount > 0 && Input.IsActionJustPressed("track_subclick"))
				{
					if (_subLastCount - 1 >= 0 && Input.IsActionPressed("track_modifier"))
						_subLastCount -= 1;
						
					else if (_subLastCount + 1 <= MaximumSubCount)
						_subLastCount += 1;
				}
			}
			
			if (_lastCount == 0)
				_subLastCount = 0;
		}
	}
	
	public override void _PhysicsProcess(float delta)
	{
		if (_loadedIcon != Singleton.IconMode)
		{
			_loadedIcon = Singleton.IconMode;
			var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
			
			_mainNode.Texture.Dispose();
			_shadowNode.Texture.Dispose();
			
			_mainNode.Texture = ResourceLoader.Load(_assetPath + TexturePath) as Texture;
			_shadowNode.Texture = ResourceLoader.Load(_assetPath + TexturePath) as Texture;
		}
		
		if (Enabled)
		{
			if (_crossNode.Visible)
				Enable();
			
			if (Singleton.TrackMode == 1)
			{
				if ((long)SubAddress != -1)
				{
					switch (SubTrackingMode)
					{
						case 0x00:
						{
							var _count = Hypervisor.Read<byte>(SubAddress);
							_subLastCount = _subLastCount < _count ? _count : _subLastCount;
							
							if (_subLastCount > 0)
							{
								if (_subLastCount < MaximumCount)
									SubActivate(_subLastCount);
								
								else
									SubActivate(MaximumCount);
								
								if (BackTrackable)
									_subLastCount = _count;
							}
							
							else if (BackTrackable)
								SubDeactivate();
							
							break;
						}
						
						case 0x01:
						{
							var _value = Hypervisor.ReadArray(SubAddress, SubArrayLength);
							var _count = (byte)_value.Where(x => x > 0x00).ToArray().Length;
							_subLastCount = _subLastCount < _count ? _count : _subLastCount;
							
							if (_subLastCount > 0)
							{
								SubActivate(_subLastCount);
								
								if (SubBackTrackable)
									_subLastCount = _count;
							}
							
							else if (SubBackTrackable)
								SubDeactivate();
							
							break;
						}
					}
				}
				
				switch (TrackingMode)
				{
					case 0x00:
					{
						var _count = Hypervisor.Read<byte>(PrimaryAddress);
						_lastCount = _lastCount < _count ? _count : _lastCount;
						
						if (SubInclusive && _lastCount < _subLastCount)
						_lastCount += _subLastCount;
						
						if (_lastCount > 0)
						{
							if (RequiredValue == -1)
							{
								if (_lastCount < MaximumCount)
									Activate(_lastCount);
								
								else
									Activate(MaximumCount);
							}
							
							else if (_lastCount >= RequiredValue)
								Activate(1);
							
							if (BackTrackable)
								_lastCount = _count;
						}
						
						else if (BackTrackable)
							Deactivate();
						
						break;
					}
					
					case 0x01:
					{
						var _value = Hypervisor.ReadArray(PrimaryAddress, ArrayLength);
						var _count = (byte)_value.Where(x => x > 0x00).ToArray().Length;
						_lastCount = _lastCount < _count ? _count : _lastCount;
						
						if (SubInclusive && _lastCount < _subLastCount)
						_lastCount += _subLastCount;
						
						if (_lastCount > 0)
						{
							if (RequiredValue == -1)
								Activate(_lastCount);
							
							else if (_lastCount >= RequiredValue)
								Activate(1);
							
							if (BackTrackable)
								_lastCount = _count;
						}
						
						else if (BackTrackable)
							Deactivate();
						
						break;
					}
					
					case 0x02:
					{
						var _value = Hypervisor.ReadArray(PrimaryAddress, ArrayLength);
						
						try
						{
							_value.First(x => x == (byte)RequiredValue);
							Activate(1);
						}
						
						catch (InvalidOperationException)
						{
							if (BackTrackable)
								Deactivate();
						}
						
						break;
					}
					
					case 0x03:
					{
						var _count = Hypervisor.Read<byte>(PrimaryAddress);
						var _bitwise = _count & RequiredValue;
						
						if (SubInclusive && _count < _subLastCount)
						_count += _subLastCount;
						
						if (_bitwise == RequiredValue)
							Activate(1);
						
						else if (BackTrackable)
							Deactivate();
						
						break;
					}
					
					case 0x04:
					{
						var _count = Hypervisor.Read<byte>(PrimaryAddress);
						var _bitCount = Hypervisor.Read<byte>(SecondaryAddress);
						
						var _bitwise = _bitCount & RequiredValue;
						_lastCount = _lastCount < _count ? _count : _lastCount;
						
						if (SubInclusive && _lastCount < _subLastCount)
						_lastCount += _subLastCount;
						
						if (_bitwise == RequiredValue)
						{
							Activate(1);
							
							if (_lastCount > 1)
							{
								Activate(_lastCount);
								
								if (_lastCount > _count && BackTrackable)
									_lastCount = _count;
							}
							
							else if (BackTrackable)
								_numberNode.Visible = false;
						}
						
						else if (BackTrackable)
							Deactivate();
						
						break;
					}
					
					case 0x05:
					{
						var _bitArray = Hypervisor.ReadArray(PrimaryAddress, ArrayLength);
						int _count = 0;
						
						for (int i = 0; i < _bitArray.Length; i++)
						{
							var _value = _bitArray[i];
							
							_count += (_value & 0x01) == 0x01 ? 0x01 : 0x00;
							_count += (_value & 0x02) == 0x02 ? 0x01 : 0x00;
							_count += (_value & 0x04) == 0x04 ? 0x01 : 0x00;
							_count += (_value & 0x08) == 0x08 ? 0x01 : 0x00;
							_count += (_value & 0x10) == 0x10 ? 0x01 : 0x00;
							_count += (_value & 0x20) == 0x20 ? 0x01 : 0x00;
							_count += (_value & 0x40) == 0x40 ? 0x01 : 0x00;
							_count += (_value & 0x80) == 0x80 ? 0x01 : 0x00;
						}
						
						if (_count % IncreaseFactor != 0)
							_count = (int)Math.Round(_count / (double)IncreaseFactor) * IncreaseFactor;
						
						if (_count > MaximumCount)
						_count = MaximumCount;
						
						_lastCount = _lastCount < _count ? (byte)_count : _lastCount;
						
						if (SubInclusive && _lastCount < _subLastCount)
						_lastCount += _subLastCount;
						
						if (_lastCount > 0)
						{
							Activate(1);
							
							if (_lastCount > 1)
							{
								Activate(_lastCount);
								
								if (_lastCount > _count && BackTrackable)
									_lastCount = (byte)_count;
							}
							
							else if (BackTrackable)
								_numberNode.Visible = false;
						}
						
						else if (BackTrackable)
							Deactivate();
						
						break;
					}
					
					case 0x06:
					{
						var _value = new List<byte>();
						
						for (ulong i = 0; i < (ulong)ArrayLength; i++)
							_value.Add(Hypervisor.Read<byte>(PrimaryAddress + EntryOffset * i));
						
						var _count = (byte)_value.Where(x => x > 0x00).ToArray().Length;
						_lastCount = _lastCount < _count ? _count : _lastCount;
						
						if (SubInclusive && _lastCount < _subLastCount)
						_lastCount += _subLastCount;
						
						if (_count > 0)
						{
							if (RequiredValue == -1)
								Activate(_lastCount);
							
							else if (_lastCount >= RequiredValue)
								Activate(1);
							
							if (BackTrackable)
								_lastCount = _count;
						}
						
						else if (BackTrackable)
							Deactivate();
						
						break;
					}
				}
			}
			
			else
			{
				if (_lastCount > 0)
					Activate(_lastCount);
				
				else
					Deactivate();
					
				if (_subLastCount > 0)
					SubActivate(_subLastCount);
				
				else
					SubDeactivate();
			}
		}
		
		else if (!_crossNode.Visible)
			Disable();
	}
}
