using System;

namespace AlphaOmega.Debug
{
	/// <summary>Generic column for dynamic structures</summary>
	public interface IColumn
	{
		/// <summary>Name of the column. From CONSTANT structures</summary>
		String Name { get; }

		/// <summary>Zero based index from the beginning of structure</summary>
		UInt16 Index { get; }
	}
}