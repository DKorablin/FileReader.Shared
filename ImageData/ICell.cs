using System;

namespace AlphaOmega.Debug
{
	/// <summary>Represents an individual data cell within a dynamic structure row.</summary>
	public interface ICell
	{
		/// <summary>Gets the interpreted value stored in the cell (e.g., a string, structure, or pointer).</summary>
		Object Value { get; }

		/// <summary>Gets the raw numerical value of the cell, which may represent a literal value, a length, or a physical index into another table.</summary>
		UInt32 RawValue { get; }

		/// <summary>Gets the <see cref="IColumn"/> metadata that defines this cell.</summary>
		IColumn Column { get; }
	}
}