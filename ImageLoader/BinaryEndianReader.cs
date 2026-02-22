using System;
using System.IO;

namespace AlphaOmega.Debug
{
	/// <summary>Provides a specialized <see cref="BinaryReader"/> that automatically handles endianness conversion when reading multi-byte primitive types from a stream.</summary>
	public class BinaryEndianReader : BinaryReader
	{
		private readonly EndianHelper.Endian _endianness;

		/// <summary>Initializes a new instance of the <see cref="BinaryEndianReader"/> class with the specified stream and target endianness.</summary>
		/// <param name="input">The input stream to read from.</param>
		/// <param name="endianness">The byte order (Big or Little Endian) expected in the input stream.</param>
		public BinaryEndianReader(Stream input, EndianHelper.Endian endianness)
			: base(input)
		{
			this._endianness = endianness;
		}

		/// <summary>
		/// Factory method that creates an appropriate <see cref="BinaryReader"/> based on the target endianness.
		/// If the target matches the host system's endianness, a standard reader is returned for performance.
		/// </summary>
		/// <param name="endianness">The desired byte order to read.</param>
		/// <param name="input">The memory stream containing the image data.</param>
		/// <returns>A <see cref="BinaryReader"/> configured for the specified endianness.</returns>
		public static BinaryReader CreateReader(EndianHelper.Endian endianness, MemoryStream input)
		{
			// If the host system (BitConverter.IsLittleEndian) matches the requested format, 
			// we don't need the overhead of byte-swapping.
			if(BitConverter.IsLittleEndian == (endianness == EndianHelper.Endian.Little))
				return new BinaryReader(input);
			else
				return new BinaryEndianReader(input, endianness);
		}

		/// <summary>Reads a 128-bit decimal value from the current stream and adjusts for endianness if necessary.</summary>
		/// <returns>The decoded decimal value.</returns>
		public override Decimal ReadDecimal()
		{
			Byte[] bytes = base.ReadBytes(sizeof(Decimal));
			EndianHelper.AdjustEndianness(typeof(Decimal), bytes, this._endianness);
			var i1 = BitConverter.ToInt32(bytes, 0);
			var i2 = BitConverter.ToInt32(bytes, 4);
			var i3 = BitConverter.ToInt32(bytes, 8);
			var i4 = BitConverter.ToInt32(bytes, 12);

			return new Decimal(new Int32[] { i1, i2, i3, i4, });
		}

		/// <summary>Reads an 8-byte floating point value from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded double-precision floating point value.</returns>
		public override Double ReadDouble()
		{
			Byte[] bytes = base.ReadBytes(sizeof(Double));
			EndianHelper.AdjustEndianness(typeof(Double), bytes, this._endianness);

			return BitConverter.ToDouble(bytes, 0);
		}

		/// <summary>Reads a 2-byte signed integer from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded 16-bit signed integer.</returns>
		public override Int16 ReadInt16()
		{
			Byte[] bytes = base.ReadBytes(sizeof(Int16));
			EndianHelper.AdjustEndianness(typeof(Int16), bytes, this._endianness);

			return BitConverter.ToInt16(bytes, 0);
		}

		/// <summary>Reads a 4-byte signed integer from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded 32-bit signed integer.</returns>
		public override Int32 ReadInt32()
		{
			Byte[] bytes = base.ReadBytes(sizeof(Int32));
			EndianHelper.AdjustEndianness(typeof(Int32), bytes, this._endianness);

			return BitConverter.ToInt32(bytes, 0);
		}

		/// <summary>Reads an 8-byte signed integer from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded 64-bit signed integer.</returns>
		public override Int64 ReadInt64()
		{
			Byte[] bytes = base.ReadBytes(sizeof(Int64));
			EndianHelper.AdjustEndianness(typeof(Int64), bytes, this._endianness);

			return BitConverter.ToInt64(bytes, 0);
		}

		/// <summary>Reads a 4-byte floating point value from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded single-precision floating point value.</returns>
		public override Single ReadSingle()
		{
			Byte[] bytes = base.ReadBytes(sizeof(Single));
			EndianHelper.AdjustEndianness(typeof(Single), bytes, this._endianness);

			return BitConverter.ToSingle(bytes, 0);
		}

		/// <summary>Reads a 2-byte unsigned integer from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded 16-bit unsigned integer.</returns>
		public override UInt16 ReadUInt16()
		{
			Byte[] bytes = base.ReadBytes(sizeof(UInt16));
			EndianHelper.AdjustEndianness(typeof(UInt16), bytes, this._endianness);

			return BitConverter.ToUInt16(bytes, 0);
		}

		/// <summary>Reads a 4-byte unsigned integer from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded 32-bit unsigned integer.</returns>
		public override UInt32 ReadUInt32()
		{
			Byte[] bytes = base.ReadBytes(sizeof(UInt32));
			EndianHelper.AdjustEndianness(typeof(UInt32), bytes, this._endianness);

			return BitConverter.ToUInt32(bytes, 0);
		}

		/// <summary>Reads an 8-byte unsigned integer from the current stream and adjusts for endianness.</summary>
		/// <returns>The decoded 64-bit unsigned integer.</returns>
		public override UInt64 ReadUInt64()
		{
			Byte[] bytes = base.ReadBytes(sizeof(UInt64));
			EndianHelper.AdjustEndianness(typeof(UInt64), bytes, this._endianness);

			return BitConverter.ToUInt64(bytes, 0);
		}
	}
}