using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

public static class Hypervisor
{
	[DllImport("kernel32.dll")]
	public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
	
	public static Process GameProcess;
	public static IntPtr GameHandle;
	
	public static ulong GameAddress;
	public static ulong ExeAddress;
	
	public unsafe static T Read<T>(ulong Address, bool Absolute = false) where T : unmanaged
	{
		IntPtr _address = (IntPtr)(GameAddress + Address);
		
		if (Absolute)
			_address = (IntPtr)(Address);
		
		var _outSize = sizeof(T);
		
		var _outArray = new byte[_outSize];
		int _outRead = 0;
		
		ReadProcessMemory(GameHandle, _address, _outArray, _outSize, ref _outRead);
		
		var _gcHandle = GCHandle.Alloc(_outArray, GCHandleType.Pinned);
		var _retData = (T)Marshal.PtrToStructure(_gcHandle.AddrOfPinnedObject(), typeof(T));
		
		_gcHandle.Free();  
		
		return _retData;
	}
	
	public static byte[] ReadArray(ulong Address, int Length, bool Absolute = false)
	{
		IntPtr _address = (IntPtr)(GameAddress + Address);
		
		if (Absolute)
			_address = (IntPtr)(Address);
		
		var _outArray = new byte[Length];
		int _outRead = 0;
		
		ReadProcessMemory(GameHandle, _address, _outArray, Length, ref _outRead);
		
		return _outArray;
	}
}
