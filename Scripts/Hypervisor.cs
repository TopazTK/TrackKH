using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class Hypervisor
{
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flNewProtect, ref int lpflOldProtect);
	
	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);
	
	[DllImport("kernel32.dll")]
	static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
	
	public static IntPtr Handle;
	public static Process Process;
	public static ulong PureAddress;
	public static ulong MemoryOffset;
	public static ulong BaseOffset;
	
	public static void Log(string Input, byte Type)
	{
		var _dateStr = DateTime.Now.ToString("dd-MM-yyyy");
		var _timeStr = "[" + DateTime.Now.ToString("hh:mm:ss") + "] ";
		
		var _typeStr = "";
		
		switch (Type)
		{
			case 0:
				_typeStr = "MESSAGE";
				break;
				
			case 1:
				_typeStr = "WARNING";
				break;
				
			case 2:
				_typeStr = "ERROR";
				break;
		}
		
		Console.Write(_timeStr);
		Console.ForegroundColor = Type == 0x00 ? ConsoleColor.Green : (Type == 0x01 ? ConsoleColor.Yellow : ConsoleColor.Red);
		Console.Write(_typeStr + ": ");
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine(Input);
	}
	
	public static void Log(Exception Input)
	{
		var _dateStr = DateTime.Now.ToString("dd-MM-yyyy");
		var _timeStr = DateTime.Now.ToString("hh:mm:ss");
		
		var _exString = Input.ToString().Replace("   ", "").Replace(System.Environment.NewLine, " ");
		
		Console.Write("[" + _timeStr + "] ");
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Write("EXCEPTION: ");
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine(_exString);
	}
	
	/// <summary>
	/// Initialize the Hypervisor on a process.
	/// </summary>
	/// <param name="Input">The input process.</param>
	public static void AttachProcess(Process Input, ulong Offset = 0x00)
	{
		Process = Input;
		Handle = Input.Handle;
		PureAddress = (ulong)Input.MainModule.BaseAddress;
		MemoryOffset = PureAddress & 0x7FFF00000000;
		
		BaseOffset = Offset;
	}
	
	/// <summary>
	/// Reads a value with the type of T from an address.
	/// Unsafe, must be used with caution.
	/// </summary>
	/// <typeparam name="T">Type of the value to read.</typeparam>
	/// <param name="Address">The address of the value to read.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	/// <returns>The value as it is read from memory.</returns>
	public static T Read<T>(ulong Address, bool Absolute = false) where T : struct
	{
		var _address = (IntPtr)Address;
		
		if (!Absolute)
			_address = (IntPtr)(PureAddress + Address + BaseOffset);
		
		var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
		ILGenerator _ilGen = _dynoMethod.GetILGenerator();
		
		_ilGen.Emit(OpCodes.Sizeof, typeof(T));
		_ilGen.Emit(OpCodes.Ret);
		
		var _outSize = (int)_dynoMethod.Invoke(null, null);
		
		var _outArray = new byte[_outSize];
		int _outRead = 0;
		
		ReadProcessMemory(Handle, _address, _outArray, _outSize, ref _outRead);
		
		var _outType = typeof(T);
		
		if (_outType.IsEnum)
		{
			var _gcHandle = GCHandle.Alloc(_outArray, GCHandleType.Pinned);
			var _retData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), Enum.GetUnderlyingType(_outType));
			
			_gcHandle.Free();
			
			return _retData;
		}
		
		else
		{
			var _gcHandle = GCHandle.Alloc(_outArray, GCHandleType.Pinned);
			var _retData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), typeof(T));
			
			_gcHandle.Free();
			
			return _retData;
		}
	}
	
	/// <summary>
	/// Reads an array with the type of T[] from an address.
	/// Unsafe, must be used with caution.
	/// </summary>
	/// <typeparam name="T">Type of the array to read.</typeparam>
	/// <param name="Address">The address of the value to read.</param>
	/// <param name="Size">The size of the array to read.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	/// <returns>The array as it is read from memory.</returns>
	public static T[] Read<T>(ulong Address, int Size, bool Absolute = false) where T : struct
	{
		var _address = (IntPtr)Address;
		
		if (!Absolute)
			_address = (IntPtr)(PureAddress + Address + BaseOffset);
		
		var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
		ILGenerator _ilGen = _dynoMethod.GetILGenerator();
		
		_ilGen.Emit(OpCodes.Sizeof, typeof(T));
		_ilGen.Emit(OpCodes.Ret);
		
		var _outSize = (int)_dynoMethod.Invoke(null, null);
		
		var _outArray = new byte[Size * _outSize];
		int _outRead = 0;
		
		ReadProcessMemory(Handle, _address, _outArray, Size * _outSize, ref _outRead);
		
		var _outType = typeof(T);
		
		if (_outType.IsEnum)
		{
			var _enumType = Enum.GetUnderlyingType(_outType);
			var _retArray = MemoryMarshal.Cast<byte, T>(_outArray);
			
			return _retArray.ToArray();
		}
		
		else
		{
			var _retArray = MemoryMarshal.Cast<byte, T>(_outArray);
			return _retArray.ToArray();
		}
	}
	
	/// <summary>
	/// Writes a value with the type of T to an address.
	/// Unsafe, must be used with caution.
	/// </summary>
	/// <typeparam name="T">Type of the value to write. Must have a size.</typeparam>
	/// <param name="Address">The address which the value will be written to.</param>
	/// <param name="Value">The value to write.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	public static void Write<T>(ulong Address, T Value, bool Absolute = false) where T : struct
	{
		var _address = (IntPtr)Address;
		
		if (!Absolute)
			_address = (IntPtr)(PureAddress + Address + BaseOffset);
		
		UnlockBlock(Address, Absolute: Absolute);
		
		var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
		ILGenerator _ilGen = _dynoMethod.GetILGenerator();
		
		_ilGen.Emit(OpCodes.Sizeof, typeof(T));
		_ilGen.Emit(OpCodes.Ret);
		
		var _inSize = (int)_dynoMethod.Invoke(null, null);
		int _inWrite = 0;
		var _inType = typeof(T);
		
		if (_inSize > 1)
		{
			if (_inType.IsEnum)
				_inType = Enum.GetUnderlyingType(_inType);
			
			
			var _inArray = (byte[])typeof(BitConverter).GetMethod("GetBytes", new[] { _inType }).Invoke(null, new object[] { Value });
			WriteProcessMemory(Handle, _address, _inArray, _inArray.Length, ref _inWrite);
		}
		
		else
		{
			var _inArray = new byte[] { (byte)Convert.ChangeType(Value, typeof(byte)) };
			WriteProcessMemory(Handle, _address, _inArray, _inArray.Length, ref _inWrite);
		}
	}
	
	/// <summary>
	/// Writes an array with the type of T to an address.
	/// Unsafe, must be used with caution.
	/// </summary>
	/// <typeparam name="T">Type of the array to write. Must have a size.</typeparam>
	/// <param name="Address">The address which the Array will be written to.</param>
	/// <param name="Value">The array to write.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	public static void Write<T>(ulong Address, T[] Value, bool Absolute = false) where T : struct
	{
		var _address = (IntPtr)Address;
		
		if (!Absolute)
			_address = (IntPtr)(PureAddress + Address + BaseOffset);
		
		UnlockBlock(Address, Absolute: Absolute);
		
		var _dynoMethod = new DynamicMethod("SizeOfType", typeof(int), []);
		ILGenerator _ilGen = _dynoMethod.GetILGenerator();
		
		_ilGen.Emit(OpCodes.Sizeof, typeof(T));
		_ilGen.Emit(OpCodes.Ret);
		
		var _inSize = (int)_dynoMethod.Invoke(null, null);
		int _inWrite = 0;
		
		var _writeArray = MemoryMarshal.Cast<T, byte>(Value).ToArray();
		WriteProcessMemory(Handle, _address, _writeArray, _writeArray.Length, ref _inWrite);
	}
	
	/// <summary>
	/// Reads a terminated string from address.
	/// </summary>
	/// <param name="Address">The address which the value will be read from.</param>
	/// <param name="Absolute">Whether the address is an absolute address or not. Defaults to false.</param>
	/// <returns></returns>
	public static string ReadString(ulong Address, bool Absolute = false)
	{
		IntPtr _address = (IntPtr)(PureAddress + Address + BaseOffset);
		
		if (Absolute)
			_address = (IntPtr)(Address);
		
		var _length = 0;
		
		while (Read<byte>((ulong)(_address + _length), true) != 0x00)
			_length++;
		
		var _outArray = new byte[_length];
		int _outRead = 0;
		
		ReadProcessMemory(Handle, _address, _outArray, _length, ref _outRead);
		
		return Encoding.Default.GetString(_outArray);
	}
	
	/// <summary>
	/// Writes a string to an address.
	/// </summary>
	/// <param name="Address">The address which the value will be written to.</param>
	/// <param name="Value">The string to write.</param>
	/// <param name="Absolute">Whether the address is an absolute address or not. Defaults to false.</param>
	public static void Write(ulong Address, string Value, bool Absolute = false, bool Unicode = false)
	{
		IntPtr _address = (IntPtr)(PureAddress + Address + BaseOffset);
		
		if (Absolute)
			_address = (IntPtr)(Address);
		
		UnlockBlock(Address, Absolute: Absolute);
		
		int _inWrite = 0;
		
		var _stringArray = Encoding.Default.GetBytes(Value);
		
		if (Unicode)
			_stringArray = Encoding.Unicode.GetBytes(Value);
		
		
		WriteProcessMemory(Handle, _address, _stringArray, _stringArray.Length, ref _inWrite);
	}

	/// <summary>
	/// Calculated a 64-bit pointer with the given offsets.
	/// All offsets are added and the resulting address is read.
	/// </summary>
	/// <param name="Address">The starting point to the pointer.</param>
	/// <param name="Offsets">All the offsets of the pointer, null by default.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	/// <returns>The final calculated pointer.</returns>
	public static ulong GetPointer64(ulong Address, int[] Offsets = null, bool Absolute = false)
	{
		var _returnPoint = Read<ulong>(Address, Absolute);
		
		if (Offsets == null)
			return _returnPoint;
		
		for (int i = 0; i < Offsets.Length - 1; i++)
			_returnPoint = Read<ulong>((ulong)((long)_returnPoint + Offsets[i]), true);
		
		return (ulong)((long)_returnPoint + Offsets.Last());
	}
	
	/// <summary>
	/// Calculated a 32-bit pointer with the given offsets.
	/// All offsets are added and the resulting address is read.
	/// </summary>
	/// <param name="Address">The starting point to the pointer.</param>
	/// <param name="Offsets">All the offsets of the pointer, null by default.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	/// <returns>The final calculated pointer.</returns>
	public static uint GetPointer32(ulong Address, uint[] Offsets = null, bool Absolute = false)
	{
		var _returnPoint = Read<uint>(Address, Absolute);
		
		if (Offsets == null)
			return _returnPoint;
		
		for (int i = 0; i < Offsets.Length - 1; i++)
			_returnPoint = Read<uint>(_returnPoint + Offsets[i], true);
		
		return _returnPoint + Offsets.Last();
	}
	
	/// <summary>
	/// Unlocks a particular block to be written.
	/// </summary>
	/// <param name="Address">The address of the subject block.</param>
	/// <param name="Absolute">If the address is absolute, false by default.</param>
	public static void UnlockBlock(ulong Address, int Size = 0x100000, bool Absolute = false)
	{
		var _address = (IntPtr)Address;
		
		if (!Absolute)
			_address = (IntPtr)(PureAddress + Address + BaseOffset);
			
		int _oldProtect = 0;
		VirtualProtectEx(Handle, _address, Size, 0x40, ref _oldProtect);
	}
}
