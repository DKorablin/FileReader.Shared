using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>Represents a collection of <see cref="ITable"/> objects, typically representing a full database or metadata stream.</summary>
	public interface ITables : IEnumerable<ITable>
	{
		/// <summary>Gets a specific table by its type identifier.</summary>
		/// <param name="type">The identifier for the table type.</param>
		/// <returns>The <see cref="ITable"/> corresponding to the specified type.</returns>
		ITable this[Object type] { get; }

		/// <summary>Gets the total number of tables in the collection.</summary>
		UInt32 Count { get; }

		/// <summary>Retrieves a row using a global index across the entire collection if applicable.</summary>
		/// <param name="rowIndex">The index of the row to find.</param>
		/// <returns>The found <see cref="IRow"/>.</returns>
		/// <exception cref="Exception">Thrown if the index is invalid.</exception>
		IRow GetRowByIndex(UInt32 rowIndex);
	}
}