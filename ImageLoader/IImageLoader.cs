using System;

namespace AlphaOmega.Debug
{
	/// <summary>Defines a common interface for loading and reading binary images, supporting both memory-mapped modules and raw disk files.</summary>
	public interface IImageLoader : IDisposable
	{
		/// <summary>Gets a value indicating whether the module is currently mapped into memory.</summary>
		/// <remarks>When true, addresses are treated as Virtual Addresses (VAs); otherwise, they are treated as RVAs.</remarks>
		Boolean IsModuleMapped { get; }

		/// <summary>Gets the base address of the module in the process memory space when mapped.</summary>
		Int64 BaseAddress { get; }

		/// <summary>Gets the total length of the underlying image stream in bytes (when possible).</summary>
		Int64 Length { get; }

		/// <summary>Gets or sets the endianness used when decoding multi-byte structures and values.</summary>
		EndianHelper.Endian Endianness { get; set; }

		/// <summary>Marshals data from a specific position into a new structure of the specified type.</summary>
		/// <typeparam name="T">The type of the structure to populate.</typeparam>
		/// <param name="padding">The RVA or file offset where the structure begins.</param>
		/// <returns>An instance of <typeparamref name="T"/> containing the data read from the image.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the padding is beyond the image length.</exception>
		T PtrToStructure<T>(UInt32 padding) where T : struct;

		/// <summary>Reads a sequence of bytes from the image starting at the specified position.</summary>
		/// <param name="padding">The RVA or file offset to start reading from.</param>
		/// <param name="length">The number of bytes to read.</param>
		/// <returns>A byte array containing the requested data.</returns>
		Byte[] ReadBytes(UInt32 padding, UInt32 length);

		/// <summary>Reads an ANSI-encoded, null-terminated string from the specified position.</summary>
		/// <param name="padding">The RVA or file offset where the string begins.</param>
		/// <returns>The string read from the image.</returns>
		String PtrToStringAnsi(UInt32 padding);
	}
}
