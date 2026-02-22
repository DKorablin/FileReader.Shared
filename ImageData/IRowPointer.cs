using System;

namespace AlphaOmega.Debug
{
	/// <summary>Represents a pointer or reference to a specific row within a dynamic table collection.</summary>
	public interface IRowPointer
	{
		/// <summary>Gets the root collection containing the target table.</summary>
		ITables Root { get; }

		/// <summary>Gets the identifier or type of the target table.</summary>
		Object TableType { get; }

		/// <summary>Gets the index of the row being referenced.</summary>
		UInt32 Index { get; }

		/// <summary>Resolves the reference and returns the corresponding <see cref="IRow"/>.</summary>
		/// <returns>The referenced <see cref="IRow"/> instance.</returns>
		IRow GetReference();
	}
}