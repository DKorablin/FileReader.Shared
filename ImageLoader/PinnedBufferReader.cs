using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Reader from memory allocated bytes array</summary>
	[DefaultProperty("Length")]
	public class PinnedBufferReader : IDisposable
	{
		#region Fields
		private GCHandle _gcHandle;
		private IntPtr _gcPointer;
		private readonly Byte[] _buffer;
		#endregion Fields
		#region Properties
		/// <summary>Bytes array</summary>
		private Byte[] Buffer { get { return this._buffer; } }
		
		/// <summary>Allocated handle</summary>
		private IntPtr Handle { get { return this._gcPointer; } }
		
		/// <summary>Read byte from buffer</summary>
		/// <param name="index">Index in the buffer array</param>
		/// <returns>One byte from the buffer</returns>
		public Byte this[UInt32 index] { get { return this.Buffer[index]; } }

		/// <summary>Length of the buffer</summary>
		public Int32 Length { get { return this.Buffer.Length; } }
		#endregion Properties
		
		/// <summary>Create instance of bytesreader class</summary>
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
			Int32 length;
			T result = this.BytesToStructure<T>(padding, out length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Overlaying structure into an array of bytes</summary>
		/// <typeparam name="T">Mapped structure type</typeparam>
		/// <param name="padding">Indent from the beginnning of the byte array</param>
		/// <returns>Mapped structure with data</returns>
		public T BytesToStructure<T>(UInt32 padding) where T : struct
		{
			Int32 length;
			return this.BytesToStructure<T>(padding, out length);
		}

		/// <summary>Overlaying structure into an array of bytes</summary>
		/// <typeparam name="T">Mapped structure type</typeparam>
		/// <param name="padding">Indent from the beginning of the byte array</param>
		/// <param name="length">Size of the resulting array</param>
		/// <exception cref="T:ArgumentOutOfRangeException">padding+structure size is out of range of byte array</exception>
		/// <returns>Mapped structure with data</returns>
		public T BytesToStructure<T>(UInt32 padding, out Int32 length) where T : struct
		{
			length = Marshal.SizeOf(typeof(T));
			if(length + padding > this.Buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			return (T)this.BytesToStructureI(padding, typeof(T));
		}

		/// <summary>banana banana banana</summary>
		/// <param name="structType"></param>
		/// <param name="padding"></param>
		/// <param name="dataLength"></param>
		/// <param name="exBytes"></param>
		/// <returns></returns>
		[Obsolete("Unknown method", true)]
		public Object BytesToStructure2(Type structType, UInt32 padding, UInt32 dataLength, out Byte[] exBytes)
		{
			UInt32 structLength = (UInt32)Marshal.SizeOf(structType);
			Byte[] bytes = new Byte[structLength > dataLength ? structLength : dataLength];//Если брать массив меньше чем структура, то в хвост структуры запишется мусор
			Array.Copy(this.Buffer, padding, bytes, 0, dataLength);
			
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
				? this.Handle
				: new IntPtr(this.Handle.ToInt64() + padding);

			return Marshal.PtrToStructure(ptr, structType);
		}

		/// <summary>Converts byte array to string from indent</summary>
		/// <param name="padding">Indent from the beginning of array</param>
		/// <returns>Result string</returns>
		public String BytesToStringUni(UInt32 padding)
		{
			Int32 length;
			return this.BytesToStringUni(padding, out length);
		}

		/// <summary>Converts byte array from indent to string</summary>
		/// <param name="padding">Indent from the beginning of the byte array, whose after reading will be cursor location at the end of a string</param>
		/// <returns>Result string</returns>
		public String BytesToStringUni(ref UInt32 padding)
		{
			Int32 length;
			String result = this.BytesToStringUni(padding, out length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Converts byte array from indent to string</summary>
		/// <param name="padding">Indent from the beginning if the byte array</param>
		/// <param name="length">Result string length</param>
		/// <exception cref="T:ArgumentOutOfRangeException">Bytes array is smaller than padding</exception>
		/// <returns>Result string</returns>
		public String BytesToStringUni(UInt32 padding, out Int32 length)
		{
			if(padding > this.Buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			IntPtr ptr = padding == 0
				? this.Handle
				: new IntPtr(this.Handle.ToInt64() + padding);

			String result = Marshal.PtrToStringUni(ptr);
			length = (result.Length + 1) * Marshal.SystemDefaultCharSize;
			return result;
		}

		/// <summary>Convets byte array to string</summary>
		/// <param name="padding">Indent from the beginning of the array</param>
		/// <returns>Result string</returns>
		public String BytesToStringAnsi(UInt32 padding)
		{
			Int32 length;
			return this.BytesToStringAnsi(padding, out length);
		}

		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Отступ от начала массива, который после возврата станет отступом от конца строки</param>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringAnsi(ref UInt32 padding)
		{
			Int32 length;
			String result = this.BytesToStringAnsi(padding, out length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Результатирующий размер строки</param>
		/// <exception cref="T:ArgumentOutOfRangeException">Bytes array is smaller than padding</exception>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringAnsi(UInt32 padding, out Int32 length)
		{
			if(padding > this.Buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			IntPtr ptr = padding == 0
				? this.Handle
				: new IntPtr(this.Handle.ToInt64() + padding);

			String result = Marshal.PtrToStringAnsi(ptr);
			length = (result.Length + 1);//ANSII length == 1
			return result;
		}

		/// <summary>Get bytes from data</summary>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Length</param>
		/// <exception cref="T:ArgumentOutOfRangeException">index>Index and length is larger than bytes array length</exception>
		/// <returns>Bytes from index</returns>
		public Byte[] GetBytes(UInt32 padding, UInt32 length)
		{
			if(padding + length > this.Buffer.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			Byte[] result = new Byte[length];
			Array.Copy(this.Buffer, padding, result, 0, result.Length);
			return result;
		}

		/// <summary>Overlaying a structure into an array of bytes and increasing the indentation by the size of the array</summary>
		/// <typeparam name="T">Overlay structure type</typeparam>
		/// <param name="buffer">An arry of bytes to apply the structure</param>
		/// <param name="padding">Indent from the beginning of the byte array and indent from the beginning of the array + end of the structure</param>
		/// <returns>Overlay structure with data</returns>
		public static T BytesToStructure<T>(Byte[] buffer, ref UInt32 padding) where T : struct
		{
			Int32 length;
			T result = PinnedBufferReader.BytesToStructure<T>(buffer, padding, out length);
			padding += (UInt32)length;
			return result;
		}

		/// <summary>Overlaying a structure into an array of bytes</summary>
		/// <typeparam name="T">Overlay structure type</typeparam>
		/// <param name="buffer">An array of bytes to apply the structure</param>
		/// <param name="padding">Indent from the beginning of array</param>
		/// <returns>Overlay structure with data</returns>
		public static T BytesToStructure<T>(Byte[] buffer, UInt32 padding) where T : struct
		{
			Int32 length;
			return PinnedBufferReader.BytesToStructure<T>(buffer, padding, out length);
		}

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