using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace StarlightRiver.Helpers
{
	internal static class ILHelper
	{
		/// <summary>
		/// Adds a <see cref="VariableDefinition"/> corresponding to the <see cref="Type"/> of <typeparamref name="T"/> to <see cref="ILContext.Body"/>'s <see cref="MethodBody.Variables"/> collection. <br />
		/// This effectively creates a new local variable to work with.
		/// </summary>
		/// <param name="il">The <see cref="ILContext"/> to add a local variable to.</param>
		/// <typeparam name="T">The local variable's type.</typeparam>
		/// <returns>The index of the local variable.</returns>
		public static int MakeLocalVariable<T>(this ILContext il)
		{
			return il.MakeLocalVariable(typeof(T));
		}

		/// <summary>
		/// Adds a <see cref="VariableDefinition"/> corresponding to the given <paramref name="type"/> to <see cref="ILContext.Body"/>'s <see cref="MethodBody.Variables"/> collection. <br />
		/// This effectively creates a new local variable to work with.
		/// </summary>
		/// <param name="il">The <see cref="ILContext"/> to add a local variable to.</param>
		/// <param name="type">The local variable's type.</param>
		/// <returns>The index of the local variable.</returns>
		public static int MakeLocalVariable(this ILContext il, Type type)
		{
			il.Body.Variables.Add(new VariableDefinition(il.Import(type)));
			return il.Body.Variables.Count - 1;
		}
	}
}
