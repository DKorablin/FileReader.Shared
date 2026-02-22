using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug
{
	/// <summary>Represents a row of data containing a collection of <see cref="ICell"/> objects.</summary>
	public interface IRow : IEnumerable<ICell>
	{
		/// <summary>Gets the zero-based index of this row within the parent table.</summary>
		UInt32 Index { get; }

		/// <summary>Gets the array of cells associated with this row.</summary>
		ICell[] Cells { get; }

		/// <summary>Gets the <see cref="ITable"/> that owns this row.</summary>
		ITable Table { get; }

		/// <summary>Gets the cell at the specified zero-based column index.</summary>
		/// <param name="columnIndex">The index of the column to retrieve.</param>
		/// <returns>The <see cref="ICell"/> at the specified index.</returns>
		ICell this[UInt16 columnIndex] { get; }

		/// <summary>Gets the cell associated with the specified column name.</summary>
		/// <param name="columnName">The name of the column to retrieve.</param>
		/// <returns>The <see cref="ICell"/> matching the specified name.</returns>
		ICell this[String columnName] { get; }

		/// <summary>Gets the cell associated with the specified column metadata.</summary>
		/// <param name="column">The column metadata object.</param>
		/// <returns>The <see cref="ICell"/> corresponding to the specified column.</returns>
		ICell this[IColumn column] { get; }
	}
}