using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaOmega.Debug
{
	/// <summary>Image loader from file or stream</summary>
	public class StreamLoader : IDisposable, IImageLoader
	{
		/// <summary>File reader</summary>
		private BinaryReader _reader;

		/// <summary>Module mapped to memory</summary>
		public Boolean IsModuleMapped { get => false; }

		/// <summary>Base PE file address</summary>
		public Int64 BaseAddress { get => 0; }

		/// <summary>Required endianness</summary>
		public EndianHelper.Endian Endianness { get; set; }

		/// <summary>Read image from stream</summary>
		/// <param name="input">Stream with image</param>
		/// <exception cref="ArgumentNullException"><paramref name="input"/> stream is null</exception>
		/// <exception cref="ArgumentException">the stream must be searchable and readable</exception>
		public StreamLoader(Stream input)
		{
			_ = input ?? throw new ArgumentNullException(nameof(input));

			if(!input.CanSeek || !input.CanRead)
				throw new ArgumentException("The stream does not support reading and/or seeking", nameof(input));

			this._reader = new BinaryReader(input);
		}

		/// <summary>Read PE image from file</summary>
		/// <param name="filePath">Path to the file</param>
		/// <exception cref="ArgumentNullException"><paramref name="filePath"/> is null or empty string</exception>
		/// <exception cref="FileNotFoundException">file not found</exception>
		/// <returns>PE loader</returns>
		public static StreamLoader FromFile(String filePath)
		{
			if(String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));
			else if(!File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);

			FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			return new StreamLoader(stream);
		}

		/// <summary>Read PE image from memory</summary>
		/// <param name="input">Array of bytes</param>
		/// <returns>PE loader</returns>
		public static StreamLoader FromMemory(Byte[] input)
		{
			if(input == null || input.Length == 0)
				throw new ArgumentNullException(nameof(input));

			MemoryStream stream = new MemoryStream(input, false);
			return new StreamLoader(stream);
		}

		/// <summary>Get bytes from specific padding and specific length</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <param name="length">Length of bytes to read</param>
		/// <exception cref="ArgumentOutOfRangeException">padding + length more than size of image</exception>
		/// <returns>Read bytes</returns>
		public virtual Byte[] ReadBytes(UInt32 padding, UInt32 length)
		{
			Stream stream = this._reader.BaseStream;
			if(padding + length > stream.Length)
				throw new ArgumentOutOfRangeException(nameof(padding));

			stream.Seek(checked((Int64)padding), SeekOrigin.Begin);
			return this._reader.ReadBytes((Int32)length);
		}

		/// <summary>Get structure from specific padding from the beginning of the image</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <returns>Read structure from image</returns>
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

		/// <summary>Get ACSII string from specific padding from the beginning of the image</summary>
		/// <param name="padding">Padding from the beginning of the image</param>
		/// <exception cref="ArgumentOutOfRangeException">padding more than size of image</exception>
		/// <returns>String from pointer</returns>
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

		/// <summary>Close PE reader</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>Dispose managed objects</summary>
		/// <param name="disposing">Dispose managed objects</param>
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