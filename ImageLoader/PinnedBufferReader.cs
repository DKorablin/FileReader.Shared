using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Reader from memory allocated bytes array</summary>
	[DefaultProperty(nameof(Length))]
	public class PinnedBufferReader : IDisposable
	{
		private GCHandle _gcHandle;

		/// <summary>Allocated handle</summary>
		private readonly IntPtr _gcPointer;

		private readonly Byte[] _buffer;

		/// <summary>Read byte from buffer</summary>
		/// <param name="index">Index in the buffer array</param>
		/// <returns>One byte from the buffer</returns>
		public Byte this[UInt32 index] { get => this._buffer[index]; }

		/// <summary>Length of the buffer</summary>
		public Int32 Length { get => this._buffer.Length; }

		/// <summary>Create instance of <see cref="PinnedBufferReader"/> class</summary>
		/// <param name="buffer">Buffer</param>
		public PinnedBufferReader(Byte[] buffer)
		{
			this._buffer = buffer;
			this._gcHandle = GCHandle.Alloc(this._buffer, GCHandleType.Pinned);

			// Get the address of the data array
			this._gcPointer = this._gcHandle.AddrOfPinnedObject();
		}

		/// <summary>Overlaying a structure into an array of bytes and increasing the indentation by the size of array</summary>
		/// <typeparam name="T">Mapped structure type</typeparam>
		/// <param name="padding">Indent from the beginning of the byte array and indent from the beginning of the array + end of the structure</param>
		/// <returns>Mapped structure with data</returns>
		public T BytesToStructure<T>(ref UInt32 padding) where T : struct
		{
			T result = this.BytesToStructure<T>(padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Overlaying structure into an array of bytes</summary>
		/// <typeparam name="T">Mapped structure type</typeparam>
		/// <param name="padding">Indent from the beginning of the byte array</param>
		/// <returns>Mapped structure with data</returns>
		public T BytesToStructure<T>(UInt32 padding) where T : struct
			=> this.BytesToStructure<T>(padding, out Int32 _);

		/// <summary>Overlaying structure into an array of bytes</summary>
		/// <typeparam name="T">Mapped structure type</typeparam>
		/// <param name="padding">Indent from the beginning of the byte array</param>
		/// <param name="length">Size of the resulting array</param>
		/// <exception cref="ArgumentOutOfRangeException">padding+structure size is out of range of byte array</exception>
		/// <returns>Mapped structure with data</returns>
		public T BytesToStructure<T>(UInt32 padding, out Int32 length) where T : struct
		{
			length = Marshal.SizeOf(typeof(T));
			if(length + padding > this._buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			return (T)this.BytesToStructureI(padding, typeof(T));
		}

		/// <summary>Overlaying structure into an array of bytes and returns overflowed bytes</summary>
		/// <remarks>Used in SystemFirmware project</remarks>
		/// <param name="structType">Type of structure to map</param>
		/// <param name="padding">Basic offset from beginning of the array</param>
		/// <param name="dataLength">Length of current block from witch structure will extracted and extra bytes will be returned</param>
		/// <param name="exBytes">Returned extra bytes</param>
		/// <returns>returns mapped object &amp; extra bytes from current window</returns>
		public Object BytesToStructure2(Type structType, UInt32 padding, UInt32 dataLength, out Byte[] exBytes)
		{
			UInt32 structLength = (UInt32)Marshal.SizeOf(structType);
			Byte[] bytes = new Byte[structLength > dataLength ? structLength : dataLength];//If you take an array smaller than the structure, then garbage will be written to the tail of the structure.
			Array.Copy(this._buffer, padding, bytes, 0, dataLength);

			using(PinnedBufferReader reader = new PinnedBufferReader(bytes))
			{
				exBytes = structLength < dataLength
					? reader.GetBytes(structLength, dataLength - structLength)
					: null;
				return reader.BytesToStructureI(0, structType);
			}
		}

		private Object BytesToStructureI(UInt32 padding, Type structType)
		{
			IntPtr ptr = padding == 0
				? this._gcPointer
				: new IntPtr(this._gcPointer.ToInt64() + padding);

			return Marshal.PtrToStructure(ptr, structType);
		}

		/// <summary>Converts byte array to string from indent</summary>
		/// <param name="padding">Indent from the beginning of array</param>
		/// <returns>Result string</returns>
		public String BytesToStringUni(UInt32 padding)
			=> this.BytesToStringUni(padding, out Int32 _);

		/// <summary>Converts byte array from indent to string</summary>
		/// <param name="padding">Indent from the beginning of the byte array, whose after reading will be cursor location at the end of a string</param>
		/// <returns>Result string</returns>
		public String BytesToStringUni(ref UInt32 padding)
		{
			String result = this.BytesToStringUni(padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Converts byte array from indent to string</summary>
		/// <param name="padding">Indent from the beginning if the byte array</param>
		/// <param name="length">Result string length</param>
		/// <exception cref="ArgumentOutOfRangeException">Bytes array is smaller than padding</exception>
		/// <returns>Result string</returns>
		public String BytesToStringUni(UInt32 padding, out Int32 length)
		{
			if(padding > this._buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			IntPtr ptr = padding == 0
				? this._gcPointer
				: new IntPtr(this._gcPointer.ToInt64() + padding);

			String result = Marshal.PtrToStringUni(ptr);
			length = (result.Length + 1) * Marshal.SystemDefaultCharSize;
			return result;
		}

		/// <summary>Converts byte array to string</summary>
		/// <param name="padding">Indent from the beginning of the array</param>
		/// <returns>Result string</returns>
		public String BytesToStringAnsi(UInt32 padding)
			=> this.BytesToStringAnsi(padding, out _);

		/// <summary>Converting a byte array from padding to a string</summary>
		/// <param name="padding">Padding from the beginning of the array, which after returning will become padding from the end of the string</param>
		/// <returns>The resulting string</returns>
		public String BytesToStringAnsi(ref UInt32 padding)
		{
			String result = this.BytesToStringAnsi(padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Converting a byte array from padding to a string</summary>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Resulting string size</param>
		/// <exception cref="ArgumentOutOfRangeException">Bytes array is smaller than padding</exception>
		/// <returns>Resulting string</returns>
		public String BytesToStringAnsi(UInt32 padding, out Int32 length)
		{
			if(padding > this._buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			IntPtr ptr = padding == 0
				? this._gcPointer
				: new IntPtr(this._gcPointer.ToInt64() + padding);

			String result = Marshal.PtrToStringAnsi(ptr);
			length = (result.Length + 1);//ANSII length == 1
			return result;
		}

		/// <summary>Get bytes from data</summary>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Length</param>
		/// <exception cref="ArgumentOutOfRangeException">index>Index and length is larger than bytes array length</exception>
		/// <returns>Bytes from index</returns>
		public Byte[] GetBytes(UInt32 padding, UInt32 length)
		{
			if(padding + length > this._buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			Byte[] result = new Byte[length];
			Array.Copy(this._buffer, padding, result, 0, result.Length);
			return result;
		}

		/// <summary>Overlaying a structure into an array of bytes and increasing the indentation by the size of the array</summary>
		/// <typeparam name="T">Overlay structure type</typeparam>
		/// <param name="buffer">An array of bytes to apply the structure</param>
		/// <param name="padding">Indent from the beginning of the byte array and indent from the beginning of the array + end of the structure</param>
		/// <returns>Overlay structure with data</returns>
		public static T BytesToStructure<T>(Byte[] buffer, ref UInt32 padding) where T : struct
		{
			T result = PinnedBufferReader.BytesToStructure<T>(buffer, padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Overlaying a structure into an array of bytes</summary>
		/// <typeparam name="T">Overlay structure type</typeparam>
		/// <param name="buffer">An array of bytes to apply the structure</param>
		/// <param name="padding">Indent from the beginning of array</param>
		/// <returns>Overlay structure with data</returns>
		public static T BytesToStructure<T>(Byte[] buffer, UInt32 padding) where T : struct
			=> PinnedBufferReader.BytesToStructure<T>(buffer, padding, out Int32 _);

		/// <summary>Overlaying a structure into an array of bytes</summary>
		/// <typeparam name="T">Overlay structure type</typeparam>
		/// <param name="buffer">An array of bytes to apply the structure</param>
		/// <param name="padding">Indent from the beginning of array</param>
		/// <param name="length">The size of the resulting array</param>
		/// <returns>Overlay structure with data</returns>
		public static T BytesToStructure<T>(Byte[] buffer, UInt32 padding, out Int32 length) where T : struct
		{
			using(PinnedBufferReader reader = new PinnedBufferReader(buffer))
				return reader.BytesToStructure<T>(padding, out length);
		}

		/// <summary>Convert byte array from indent to string</summary>
		/// <param name="buffer">Convert byte array from indent to string</param>
		/// <param name="padding">Indent from the beginning of array</param>
		/// <param name="length">The size of the resulting array</param>
		/// <returns>Result string</returns>
		public static String BytesToStringUni(Byte[] buffer, UInt32 padding, out Int32 length)
		{
			using(PinnedBufferReader reader = new PinnedBufferReader(buffer))
				return reader.BytesToStringUni(padding, out length);
		}

		/// <summary>Convert byte array from indent to string</summary>
		/// <param name="buffer">Byte array</param>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Resulting string size</param>
		/// <returns>Result string</returns>
		public static String BytesToStringAnsi(Byte[] buffer, UInt32 padding, out Int32 length)
		{
			using(PinnedBufferReader reader = new PinnedBufferReader(buffer))
				return reader.BytesToStringAnsi(padding, out length);
		}

		/// <summary>Converting a structure from memory to an array of bytes</summary>
		/// <typeparam name="T">Structure to be converted</typeparam>
		/// <param name="structure">Structure to transform</param>
		/// <returns>Byte array</returns>
		public static Byte[] StructureToArray<T>(T structure) where T : struct
		{
			Int32 length = Marshal.SizeOf(structure);
			Byte[] result = new Byte[length];

			IntPtr ptr = Marshal.AllocHGlobal(length);
			try
			{
				Marshal.StructureToPtr(structure, ptr, true);
				Marshal.Copy(ptr, result, 0, length);
			} finally
			{
				Marshal.FreeHGlobal(ptr);
			}
			return result;
		}

		/// <summary>Release allocated memory</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Release allocated memory</summary>
		/// <param name="disposing">Free managed resources</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this._gcHandle.IsAllocated)
				this._gcHandle.Free();
		}
	}
}
