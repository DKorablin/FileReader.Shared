using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>
	/// Provides an implementation of <see cref="IImageLoader"/> for reading binary images 
	/// from a <see cref="Stream"/>, such as a file on disk or a byte array in memory.
	/// </summary>
	public class StreamLoader : IImageLoader
	{
		private BinaryReader _reader;

		/// <inheritdoc/>
		/// <remarks>For <see cref="StreamLoader"/>, this is always false as it operates on raw streams.</remarks>
		Boolean IImageLoader.IsModuleMapped { get => false; }

		/// <inheritdoc/>
		/// <remarks>Since the module is not mapped, the base address is consistently 0.</remarks>
		Int64 IImageLoader.BaseAddress { get => 0; }

		/// <inheritdoc/>
		Int64 IImageLoader.Length { get => this._reader.BaseStream.Length; }

		/// <inheritdoc/>
		public EndianHelper.Endian Endianness { get; set; }

		/// <summary>Initializes a new instance of the <see cref="StreamLoader"/> class using the specified stream.</summary>
		/// <param name="input">The input stream containing the image data.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the stream does not support seeking or reading.</exception>
		public StreamLoader(Stream input)
		{
			_ = input ?? throw new ArgumentNullException(nameof(input));

			if(!input.CanSeek || !input.CanRead)
				throw new ArgumentException("The stream does not support reading and/or seeking", nameof(input));

			this._reader = new BinaryReader(input);
		}

		/// <summary>Creates a <see cref="StreamLoader"/> from a file on disk.</summary>
		/// <param name="filePath">The full path to the binary file.</param>
		/// <returns>A new instance of <see cref="StreamLoader"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null or empty.</exception>
		/// <exception cref="FileNotFoundException">Thrown if the file does not exist at the specified path.</exception>
		public static StreamLoader FromFile(String filePath)
		{
			if(String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			else if(!File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);

			FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return new StreamLoader(stream);
		}

		/// <summary>Creates a <see cref="StreamLoader"/> from an array of bytes.</summary>
		/// <param name="input">The byte array representing the binary image.</param>
		/// <returns>A new instance of <see cref="StreamLoader"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is null or empty.</exception>
		public static StreamLoader FromMemory(Byte[] input)
		{
			if(input == null || input.Length == 0)
				throw new ArgumentNullException(nameof(input));

			MemoryStream stream = new MemoryStream(input, false);
			return new StreamLoader(stream);
		}

		/// <summary>Reads a block of bytes from the underlying stream at the specified file offset.</summary>
		/// <param name="padding">The file offset (padding) from the beginning of the stream.</param>
		/// <param name="length">The number of bytes to retrieve.</param>
		/// <returns>An array containing the requested bytes.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the requested range exceeds the stream length.</exception>
		public virtual Byte[] ReadBytes(UInt32 padding, UInt32 length)
		{
			Stream stream = this._reader.BaseStream;
			if(padding + length > stream.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			stream.Seek(checked((Int64)padding), SeekOrigin.Begin);
			return this._reader.ReadBytes((Int32)length);
		}

		/// <summary>Reads a structure of type <typeparamref name="T"/> from the stream at the specified file offset.</summary>
		/// <typeparam name="T">The type of the structure to read.</typeparam>
		/// <param name="padding">The file offset where the structure begins.</param>
		/// <returns>The structure read from the image.</returns>
		public virtual T PtrToStructure<T>(UInt32 padding) where T : struct
		{
			Byte[] bytes = this.ReadBytes(padding, (UInt32)Marshal.SizeOf(typeof(T)));
			EndianHelper.AdjustEndianness(typeof(T), bytes, this.Endianness);

			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			try
			{
				T result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
				return result;
			} finally
			{
				handle.Free();
			}
		}

		/// <summary>Reads a null-terminated ANSI string from the stream at the specified file offset.</summary>
		/// <param name="padding">The file offset where the string begins.</param>
		/// <returns>The string read from the image.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the offset is beyond the stream length.</exception>
		public virtual String PtrToStringAnsi(UInt32 padding)
		{
			Stream stream = this._reader.BaseStream;
			if(padding > stream.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			stream.Seek(checked((Int64)padding), SeekOrigin.Begin);
			List<Byte> result = new List<Byte>();

			Byte b = this._reader.ReadByte();
			while(b != 0x00)
			{
				result.Add(b);
				b = this._reader.ReadByte();
			}
			return Encoding.ASCII.GetString(result.ToArray());
		}

		/// <summary>Releases all resources used by the <see cref="StreamLoader"/>.</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="StreamLoader"/> and optionally releases the managed resources.</summary>
		/// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this._reader != null)
			{
				this._reader.Close();
				this._reader = null;
			}
		}
	}
}