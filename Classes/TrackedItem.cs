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
	[Export] public ulong FallbackAddress = 0x00;
	[Export] public ulong SecondaryAddress = 0x00;
	
	[Export] public int MaximumSubCount = 0x00;
	[Export] public int SubArrayLength = 0;
	
	[Export] public ulong SubAddress = 0x00;
	
	[Export] public int TrackingMode = 0;
	[Export] public int SubTrackingMode = 0;
	[Export] public bool SubSoloTrackable = false;
	
	[Export] public bool BackTrackable = true;
	[Export] public bool SubBackTrackable = true;
	
	[Export] public bool SubInclusive = false;
	
	[Export] public string TexturePath = "debug.png";
	[Export] public string SubTexturePath = "debug.png";
	[Export] public string[] FileList;
	
	/*
		TrackingMode:
		
		The mode of the tracked item.
		
		0 = Count-based: One address, the value denoted the count.
		1 = Incrementor-based: The address is the starting point, offsets increase by "1" with each item. 
		2 = Look-for-based: A specific value (RequiredValue) will be looked for starting from address (PrimaryAddress) for a certain amount of bytes (ArrayLength)
		3 = Bitwise-lookup: A specific value is bitwised from the address, then compared.
		4 = Bitwise-followup : A specific value is bitwised from the SecondaryAddress, then the count is read from PrimaryAddress.
		5 = Bitwise-calculation: A specific array is taken from the PrimaryAddress, then cound is calculated based on the bitwise.
		6 = Incrementor-based (Offsetted): The address is the starting point, offsets increase by a certain amount each item.
		7 = Incrementor-based (Bitwise): Same as "1", but the values change the bitwise. Unique to the emblem piece.
		8 = Incrementor-based (Specific): Same as "1", but the values that we seek will be specific.
	*/
	
	private TextureRect _mainNode;
	private TextureRect _subNode;
	private TextureRect _shadowNode;
	private TextureRect _subShadowNode;
	private TextureRect _crossNode;
	private TextureRect _numberNode;
	private TextureRect _subNumberNode;
	
	private AnimationPlayer _mainAnim;
	private AnimationPlayer _numberAnim;
	private AnimationPlayer _crossAnim;
	private AnimationPlayer _subAnim;
	private AnimationPlayer _subNumberAnim;
	
	private bool _mouseOver = false;
	private bool _permDisable = false;
	
	private byte _lastCount = 0;
	private byte _subLastCount = 0;
	
	private int _displayedNumber = 0;
	private int _subDisplayedNumber = 0;
	
	private byte _loadedIcon;
	private bool _isActive;
	
	private byte _textureSwitch = 0;
	private byte _textureBitwise = 0;
	
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
		
		_mainAnim = GetNode("MainAnim") as AnimationPlayer;
		_numberAnim = GetNode("NumberAnim") as AnimationPlayer;
		_crossAnim = GetNode("CrossAnim") as AnimationPlayer;
		_subAnim = GetNode("SubAnim") as AnimationPlayer;
		_subNumberAnim = GetNode("SubNumberAnim") as AnimationPlayer;
		
		_loadedIcon = Singleton.IconMode;
		var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
		
		_mainNode.Texture.Dispose();
		_shadowNode.Texture.Dispose();
		
		_subNode.Texture.Dispose();
		_subShadowNode.Texture.Dispose();
		
		var _texturePath = TexturePath;
		
		if (_texturePath.Contains("{0}"))
			_texturePath = string.Format(_texturePath, 0);
		
		_mainNode.Texture = ResourceLoader.Load(_assetPath + _texturePath) as Texture;
		_shadowNode.Texture = ResourceLoader.Load(_assetPath + _texturePath) as Texture;
		
		if ((long)SubAddress != -1)
		{
			_subNode.Texture = ResourceLoader.Load(_assetPath + SubTexturePath) as Texture;
			_subShadowNode.Texture = ResourceLoader.Load(_assetPath + SubTexturePath) as Texture;
		}
		
		if (!Enabled)
			_permDisable = true;
	}
	
	private void Disable()
	{
		if (_mainNode.Modulate.r != 0.20F)
		{
			_displayedNumber = 0;
			_subDisplayedNumber = 0;
			
			if (!_crossNode.Visible)
				_crossAnim.Play("PromptCross");
			
			if (_numberNode.Visible)
				_numberAnim.Play("HideNum");
				
			if (_subNode.Visible)
				_subAnim.Play("HideSub");
			
			if (_subNumberNode.Visible)
				_subNumberAnim.Play("HideSubNum");
			
			if (_mainNode.Modulate.r != 1F)
				_mainAnim.Play("DisableItemHalf");
				
			else
				_mainAnim.Play("DisableItemFull");
			
			_numberNode.Texture.Dispose();
			_subNumberNode.Texture.Dispose();
			
			if (TexturePath.Contains("{0}"))
			{
				var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
				
				_mainNode.Texture.Dispose();
				_shadowNode.Texture.Dispose();
				
				_mainNode.Texture = ResourceLoader.Load(_assetPath + string.Format(TexturePath, 0)) as Texture;
				_shadowNode.Texture = ResourceLoader.Load(_assetPath + string.Format(TexturePath, 0)) as Texture;
			}
			
			_subNode.Visible = false;
			_mainNode.Modulate = new Godot.Color(0.20F, 0.20F, 0.20F, 1);
		}
	}
	
	private void Enable()
	{
		if (_mainNode.Modulate.r != 0.45F && !_permDisable)
		{
			if (TexturePath.Contains("{0}"))
			{
				var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
				
				_mainNode.Texture.Dispose();
				_shadowNode.Texture.Dispose();
				
				_mainNode.Texture = ResourceLoader.Load(_assetPath + string.Format(TexturePath, _textureBitwise)) as Texture;
				_shadowNode.Texture = ResourceLoader.Load(_assetPath + string.Format(TexturePath, _textureBitwise)) as Texture;
			}
			
			if (!_isActive)
				_mainAnim.Play("EnableItemHalf");
			
			_crossAnim.Play("HideCross");
		}
	}
	
	private void Activate(int Input)
	{
		if (_mainNode.Modulate.r != 1)
			_mainAnim.Play("PromptItem");
		
		if (Input > 1 && Input != _displayedNumber)
		{
			_displayedNumber = Input;
			
			_numberNode.Texture.Dispose();
			_numberNode.Texture = ResourceLoader.Load("res://Assets/Numbers/" + Input + ".png") as Texture;
			
			if (!_numberNode.Visible)
				_numberAnim.Play("PromptNum");
		}
		
		else if (Input == 1)
		{
			_displayedNumber = 1;
			
			if (_numberNode.Visible)
			_numberAnim.Play("HideNum");
		}
		
		else if (BackTrackable && Input == 0)
		{
			_displayedNumber = 0;
			
			if (_numberNode.Visible)
			_numberAnim.Play("HideNum");
		}
		
		_isActive = true;
	}
	
	private void SwapTexture()
	{
		if (_textureSwitch != _textureBitwise)
		{
			var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
			
			_mainNode.Texture.Dispose();
			_shadowNode.Texture.Dispose();
			
			_mainNode.Texture = ResourceLoader.Load(_assetPath + string.Format(TexturePath, _textureBitwise)) as Texture;
			_shadowNode.Texture = ResourceLoader.Load(_assetPath + string.Format(TexturePath, _textureBitwise)) as Texture;
			
			_textureSwitch = _textureBitwise;
		}
	}
	
	private void Deactivate()
	{
		if (_mainNode.Modulate.r > 0.46F)
		{
			_displayedNumber = 0;
			
			if (_numberNode.Visible)
				_numberAnim.Play("HideNum");
				
			if (_subNode.Visible)
				_subAnim.Play("HideSub");
			
			if (_subNumberNode.Visible)
				_subNumberAnim.Play("HideSubNum");
			
			_mainAnim.Play("HideItem");
			_isActive = false;
		}
	}
	
	private void SubActivate(int Input)
	{
		if (!_subNode.Visible)
			_subAnim.Play("PromptSub");
		
		if (MaximumSubCount > 1)
		{
			if (Input >= 1 && Input != _subDisplayedNumber)
			{
				_subDisplayedNumber = Input;
				_subNumberNode.Texture = ResourceLoader.Load("res://Assets/Numbers/" + Input + ".png") as Texture;
				
				if (!_subNumberNode.Visible)
					_subNumberAnim.Play("PromptSubNum");
			}
			
			else if (SubBackTrackable && Input == 0)
			{
				_subDisplayedNumber = 0;
				
				if (_subNumberNode.Visible)
					_subNumberAnim.Play("HideSubNum");
			}
		}
	}

	private void SubDeactivate()
	{
		if (_subNode.Visible)
		{
			_subDisplayedNumber = 0;
			
			if (_subNumberNode.Visible)
					_subNumberAnim.Play("HideSubNum");
			
			_subAnim.Play("HideSub");
		}
	}
	
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("track_toggle") && _mouseOver && !_permDisable)
			this.Enabled = !this.Enabled;
		
		if (_mainNode.RectSize.x < 96 && _shadowNode.SelfModulate.a > 0.20F)
		{
			_subNode.RectPosition = new Vector2(_subNode.RectPosition.x + 10, _subNode.RectPosition.y + 5);
			_shadowNode.SelfModulate = new Color(0, 0, 0, 0.20F);
		}
		
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
				
				if ((_lastCount > 0 || SubSoloTrackable) && Input.IsActionJustPressed("track_subclick") && SubTrackingMode != 0x0F)
				{
					if (_subLastCount - 1 >= 0 && Input.IsActionPressed("track_modifier"))
						_subLastCount -= 1;
						
					else if (_subLastCount + 1 <= MaximumSubCount)
						_subLastCount += 1;
				}
				
				if (TexturePath.Contains("{0}") && _textureBitwise != _lastCount)
				{
					_textureBitwise = _lastCount;
					SwapTexture();
				}
			}
			
			if (_lastCount == 0 && !SubSoloTrackable)
				_subLastCount = 0;
		}
		
		if (SubTrackingMode == 0x0F && Enabled)
		{
			if (_mouseOver)
			{
				if (Input.IsActionJustReleased("track_subup"))
				{
					if (_subLastCount + 1 <= MaximumSubCount)
						_subLastCount += 1;
						
					else
						_subLastCount = 0;
				}
				
				if (_lastCount > 0 && Input.IsActionJustReleased("track_subdown"))
				{
					if (_subLastCount - 1 > -1)
						_subLastCount -= 1;
						
					else
						_subLastCount = (byte)MaximumSubCount;
				}
			}
		}
	}
	
	public override void _PhysicsProcess(float delta)
	{
		if (_loadedIcon != Singleton.IconMode)
		{
			_loadedIcon = Singleton.IconMode;
			var _assetPath = _loadedIcon == 0 ? "res://Assets/Simplified/" : "res://Assets/Classic/";
			
			var _texturePath = TexturePath;
			
			if (_texturePath.Contains("{0}"))
				_texturePath = string.Format(_texturePath, 0);
			
			_mainNode.Texture.Dispose();
			_shadowNode.Texture.Dispose();
			
			_mainNode.Texture = ResourceLoader.Load(_assetPath + _texturePath) as Texture;
			_shadowNode.Texture = ResourceLoader.Load(_assetPath + _texturePath) as Texture;
			
			_textureSwitch = 0x00;
		}
		
		if (SubTrackingMode == 0x0F && Enabled)
		{
			if (_subLastCount != _subDisplayedNumber)
			{
				_subDisplayedNumber = _subLastCount;
				
				if (_subLastCount == 0 && _subNode.Visible)
					_subAnim.Play("HideSub");
				
				else
				{
					_subNode.Texture = ResourceLoader.Load("res://Assets/Simplified/"  + FileList[_subLastCount - 1]) as Texture;
					_subShadowNode.Texture = ResourceLoader.Load("res://Assets/Simplified/"  + FileList[_subLastCount - 1]) as Texture;
					
					if (!_subNode.Visible)
						_subAnim.Play("PromptSub");
				}
			}
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
								
								if (SubBackTrackable)
									_subLastCount = _count;
							}
							
							else if (SubBackTrackable)
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
						var _fallCount = Hypervisor.Read<byte>(FallbackAddress);
						
						if (FallbackAddress != 0xFFFFFFFFFFFFFFFF)
							_count += _fallCount;
						
						_lastCount = _lastCount < _count ? _count : _lastCount;
						
						if (SubInclusive && _count <= _subLastCount && _count + _subLastCount >= _lastCount)
						_lastCount = (byte)(_count + _subLastCount);
						
						if (_fallCount == RequiredValue)
							BackTrackable = false;
						
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
						
						if (SubInclusive && _count < _subLastCount && _count + _subLastCount > _lastCount)
						_lastCount = (byte)(_count + _subLastCount);
						
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
						
						if (SubInclusive && _count < _subLastCount && _count + _subLastCount > _lastCount)
						_lastCount = (byte)(_count + _subLastCount);
						
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
						
						if (SubInclusive && _count < _subLastCount && _count + _subLastCount > _lastCount)
						_lastCount = (byte)(_count + _subLastCount);
						
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
						
						if (SubInclusive && _count < _subLastCount && _count + _subLastCount > _lastCount)
						_lastCount = (byte)(_count + _subLastCount);
						
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
					
					case 0x07:
					{
						var _value = Hypervisor.ReadArray(PrimaryAddress, ArrayLength);
						byte _bitwise = 0;
						
						for (int i = 0; i < _value.Length; i++)
						{
							if (_value[i] != 0)
							_bitwise += (byte)(Math.Pow(2, i));
						}
						
						if (FallbackAddress != 0xFFFFFFFFFFFFFFFF)
						{
							var _fallVal = Hypervisor.ReadArray(FallbackAddress, ArrayLength);
							byte _fallWise = 0;
							
							for (int i = 0; i < _fallVal.Length; i++)
							{
								if (_fallVal[i] == RequiredValue)
								_fallWise += (byte)(Math.Pow(2, i));
							}
							
							_bitwise += _fallWise;
						}
						
						if (_textureBitwise < _bitwise || BackTrackable)
							_textureBitwise = _bitwise;
						
						Activate(1);
						SwapTexture();
						
						break;
					}
					
					case 0x08:
					{
						var _value = Hypervisor.ReadArray(PrimaryAddress, ArrayLength);
						
						var _count = (byte)_value.Where(x => x == RequiredValue || x - 0x80 == RequiredValue).ToArray().Length;
						
						if (EntryOffset != 0xFFFFFFFFFFFFFFFF)
						{
							var _offCalc = (byte)(RequiredValue + (int)EntryOffset);
							_count += (byte)_value.Where(x => x == _offCalc || x - 0x80 == _offCalc).ToArray().Length;
						}
						
						_lastCount = _lastCount < _count ? _count : _lastCount;
						
						if (SubInclusive && _count < _subLastCount && _count + _subLastCount > _lastCount)
						_lastCount = (byte)(_count + _subLastCount);
						
						if (_lastCount > 0)
						{
							Activate(_lastCount);
								
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
				if (!TexturePath.Contains("{0}"))
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
				
				else
					Activate(1);
			}
		}
		
		else if (!_crossNode.Visible)
			Disable();
	}
}
