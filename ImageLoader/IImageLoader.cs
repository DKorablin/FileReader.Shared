using System;

namespace AlphaOmega.Debug
{
	/// <summary>PE file loader interface</summary>
	public interface IImageLoader : IDisposable
	{
		/// <summary>Module is loaded in memory. All RVAs are rewritten to VAs</summary>
		Boolean IsModuleMapped { get; }

		/// <summary>Module base address in memory</summary>
		Int64 BaseAddress { get; }

		/// <summary>Gets the length of underlying stream (if possible).</summary>
		Int64 Length { get; }

		/// <summary>The required endianness.</summary>
		EndianHelper.Endian Endianness { get; set; }

		/// <summary>Gets a structure from a specific offset.</summary>
		/// <typeparam name="T">Structure type</typeparam>
		/// <param name="padding">Offset from the beginning of a file or an RVA.</param>
		/// <returns>Structure from specified offset</returns>
		T PtrToStructure<T>(UInt32 padding) where T : struct;

		/// <summary>Gets a byte array starting from the specified offset.</summary>
		/// <param name="padding">Offset from the beginning of a file or an RVA.</param>
		/// <param name="length">Required length</param>
		/// <returns>Byte array from offset and required length</returns>
		Byte[] ReadBytes(UInt32 padding, UInt32 length);

		/// <summary>Gets a string from a fixed offset.</summary>
		/// <param name="padding">Offset from the beginning of a file or an RVA.</param>
		/// <returns>String from the offset</returns>
		String PtrToStringAnsi(UInt32 padding);
	}
}
