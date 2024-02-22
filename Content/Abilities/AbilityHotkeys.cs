using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Hint;
using System;
using System.Collections.Generic;

namespace StarlightRiver.Content.Abilities
{
	public class AbilityHotkeys
	{
		public AbilityHotkeys(Mod Mod)
		{
			this.Mod = Mod;
		}

		private readonly Dictionary<Type, ModKeybind> bindings = new();
		private readonly Mod Mod;

		private ModKeybind this[Type type]
		{
			get
			{
				if (type == typeof(object) || type == typeof(Ability))
					throw new InvalidOperationException("Not a registered ability binding. This should never happen! Contact Mod devs to implement a missing key binding for the ability.");
				if (bindings.TryGetValue(type, out ModKeybind ret))
					return ret;
				return bindings[type] = this[type.BaseType];
			}
		}

		public ModKeybind Get<T>() where T : Ability
		{
			return this[typeof(T)];
		}

		public void Bind<T>(string display, string defaultKey) where T : Ability
		{
			bindings[typeof(T)] = KeybindLoader.RegisterKeybind(Mod, display, defaultKey);
		}

		internal void LoadDefaults()
		{
			Bind<HintAbility>("Starsight", "Y");
			Bind<Dash>("Forbidden Winds", "LeftShift");
			Bind<Whip>("Faeflame", "F");
		}
		internal void Unload()
		{
			bindings.Clear();
		}
	}
}