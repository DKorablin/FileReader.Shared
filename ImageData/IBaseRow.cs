using System;

namespace AlphaOmega.Debug
{
	/// <summary>Represents a base contract for strongly-typed rows that wrap a generic row implementation.</summary>
	public interface IBaseRow
	{
		/// <summary>Gets the underlying generic <see cref="IRow"/> instance.</summary>
		IRow Row { get; }
	}
}