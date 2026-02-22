using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>Represents a collection of rows with a uniform structure.</summary>
	public interface ITable
	{
		/// <summary>Gets all rows contained within the table.</summary>
		IEnumerable<IRow> Rows { get; }

		/// <summary>Gets the total number of rows in the table.</summary>
		UInt32 RowsCount { get; }

		/// <summary>Gets the metadata for all columns in the table.</summary>
		IColumn[] Columns { get; }

		/// <summary>Gets the implementation-specific type identifier for this table.</summary>
		Object Type { get; }

		/// <summary>Gets the row at the specified zero-based index.</summary>
		/// <param name="rowIndex">The index of the row to retrieve.</param>
		/// <returns>The <see cref="IRow"/> at the specified index.</returns>
		IRow this[UInt32 rowIndex] { get; }
	}
}