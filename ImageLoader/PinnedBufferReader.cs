using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Provides a high-performance reader for a byte array by pinning it in memory  to allow direct pointer-based marshaling into structures and strings.</summary>
	[DefaultProperty(nameof(Length))]
	public class PinnedBufferReader : IDisposable
	{
		private GCHandle _gcHandle;

		/// <summary>The fixed memory address of the pinned buffer.</summary>
		private readonly IntPtr _gcPointer;

		private readonly Byte[] _buffer;

		/// <summary>Gets the byte at the specified index directly from the underlying buffer.</summary>
		/// <param name="index">The zero-based index in the buffer.</param>
		/// <returns>The byte at the specified index.</returns>
		public Byte this[UInt32 index] { get => this._buffer[index]; }

		/// <summary>Gets the total number of bytes in the buffer.</summary>
		public Int32 Length { get => this._buffer.Length; }

		/// <summary>Initializes a new instance of the <see cref="PinnedBufferReader"/> class and pins the buffer in memory.</summary>
		/// <param name="buffer">The byte array to be read.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="buffer"/> is null.</exception>
		public PinnedBufferReader(Byte[] buffer)
		{
			this._buffer = buffer;
			this._gcHandle = GCHandle.Alloc(this._buffer, GCHandleType.Pinned);

			// Get the address of the data array
			this._gcPointer = this._gcHandle.AddrOfPinnedObject();
		}

		/// <summary>Marshals data from the buffer into a structure and advances the offset by the size of the structure.</summary>
		/// <typeparam name="T">The type of the structure to populate.</typeparam>
		/// <param name="padding">The current offset in the buffer; updated to the end of the structure upon return.</param>
		/// <returns>The populated structure.</returns>
		public T BytesToStructure<T>(ref UInt32 padding) where T : struct
		{
			T result = this.BytesToStructure<T>(padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Marshals data from the buffer into a structure starting at the specified offset.</summary>
		/// <typeparam name="T">The type of the structure to populate.</typeparam>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <returns>The populated structure.</returns>
		public T BytesToStructure<T>(UInt32 padding) where T : struct
			=> this.BytesToStructure<T>(padding, out Int32 _);

		/// <summary>Marshals data from the buffer into a structure starting at the specified offset and returns the structure size.</summary>
		/// <typeparam name="T">The type of the structure to populate.</typeparam>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <param name="length">The size of the structure in bytes.</param>
		/// <returns>The populated structure.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the structure exceeds the buffer bounds.</exception>
		public T BytesToStructure<T>(UInt32 padding, out Int32 length) where T : struct
		{
			length = Marshal.SizeOf(typeof(T));
			if(length + padding > this._buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			return (T)this.BytesToStructureI(padding, typeof(T));
		}

		/// <summary>Extracts a structure and any remaining bytes from a specific data window.</summary>
		/// <remarks>Used in SystemFirmware project</remarks>
		/// <param name="structType">The type of the structure to map.</param>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <param name="dataLength">The length of the data block to process.</param>
		/// <param name="exBytes">The bytes remaining in the window after the structure.</param>
		/// <returns>The mapped structure object.</returns>
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

		/// <summary>Reads a Unicode string from the buffer at the specified offset.</summary>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <returns>The decoded string.</returns>
		public String BytesToStringUni(UInt32 padding)
			=> this.BytesToStringUni(padding, out Int32 _);

		/// <summary>Reads a Unicode string from the buffer and advances the offset past the string and its null terminator.</summary>
		/// <param name="padding">The current offset; updated to the end of the string upon return.</param>
		/// <returns>The decoded string.</returns>
		public String BytesToStringUni(ref UInt32 padding)
		{
			String result = this.BytesToStringUni(padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Reads a Unicode string from the buffer and returns its length in bytes.</summary>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <param name="length">The total length of the string in bytes, including the null terminator.</param>
		/// <returns>The decoded string.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is beyond the buffer length.</exception>
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

		/// <summary>Reads an ANSI string from the buffer at the specified offset.</summary>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <returns>The decoded string.</returns>
		public String BytesToStringAnsi(UInt32 padding)
			=> this.BytesToStringAnsi(padding, out _);

		/// <summary>Reads an ANSI string from the buffer and advances the offset past the string and its null terminator.</summary>
		/// <param name="padding">The current offset; updated to the end of the string upon return.</param>
		/// <returns>The decoded string.</returns>
		public String BytesToStringAnsi(ref UInt32 padding)
		{
			String result = this.BytesToStringAnsi(padding, out Int32 length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Reads an ANSI string from the buffer and returns its length in bytes.</summary>
		/// <param name="padding">The offset from the beginning of the buffer.</param>
		/// <param name="length">The total length of the string in bytes, including the null terminator.</param>
		/// <returns>The decoded string.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is beyond the buffer length.</exception>
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

		/// <summary>Retrieves a sub-array of bytes from the buffer.</summary>
		/// <param name="padding">The starting offset in the buffer.</param>
		/// <param name="length">The number of bytes to retrieve.</param>
		/// <returns>A new byte array containing the requested data.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the requested range is outside the buffer.</exception>
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

		/// <summary>Serializes a structure into a byte array by marshaling it through unmanaged memory.</summary>
		/// <typeparam name="T">The type of the structure to convert.</typeparam>
		/// <param name="structure">The structure instance to convert.</param>
		/// <returns>A byte array representing the structure.</returns>
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

		/// <summary>Unpins the buffer and releases the GC handle.</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Releases the GC handle if it is allocated.</summary>
		/// <param name="disposing">True to release managed resources.</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this._gcHandle.IsAllocated)
				this._gcHandle.Free();
		}
	}
}
