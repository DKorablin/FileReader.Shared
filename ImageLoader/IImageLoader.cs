using System;

namespace AlphaOmega.Debug
{
	/// <summary>PE file loader interface</summary>
	public interface IImageLoader : IDisposable
	{
		/// <summary>Module loaded in memory. All RVA rewritten to VA</summary>
		Boolean IsModuleMapped { get; }

		/// <summary>Module base address in memory</summary>
		Int64 BaseAddress { get; }

		/// <summary>Required endianness</summary>
		EndianHelper.Endian Endianness { get; set; }

		/// <summary>Gets structure from specific offset</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Offset from beginning of a file or RVA</param>
		/// <returns>Structure from specified offset</returns>
		T PtrToStructure<T>(UInt32 padding) where T : struct;

		/// <summary>Gets byte array from beginning of the offset</summary>
		/// <param name="padding">Offset from beginning of a file or RVA</param>
		/// <param name="length">Required length</param>
		/// <returns>Byte array from offset and required length</returns>
		Byte[] ReadBytes(UInt32 padding, UInt32 length);

		/// <summary>Gets string from fixed offset</summary>
		/// <param name="padding">Offset from beginning of a file or RVA</param>
		/// <returns>String from the offset</returns>
		String PtrToStringAnsi(UInt32 padding);
	}
}
