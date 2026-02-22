using System;

namespace AlphaOmega.Debug
{
	/// <summary>Defines the metadata for a column within a dynamic structure.</summary>
	public interface IColumn
	{
		/// <summary>Gets the name of the column as defined in the constant metadata structures.</summary>
		String Name { get; }

		/// <summary>Gets the zero-based index of this column within its parent table.</summary>
		UInt16 Index { get; }
	}
}