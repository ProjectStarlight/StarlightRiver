using StarlightRiver.Abilities.Content;
using StarlightRiver.Abilities.Content.Faeflame;
using StarlightRiver.Abilities.Content.ForbiddenWinds;
using StarlightRiver.Abilities.Content.GaiasFist;
using StarlightRiver.Abilities.Content.Purify;
using StarlightRiver.Dusts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace StarlightRiver.Abilities
{
    public class AbilityHotkeys
    {
        public AbilityHotkeys(Mod mod)
        {
            this.mod = mod;
        }

        private readonly Dictionary<Type, ModHotKey> bindings = new Dictionary<Type, ModHotKey>();
        private readonly Mod mod;

        public ModHotKey this[Type type]
        {
            get
            {
                if (type == typeof(object) || type == typeof(Ability))
                {
                    throw new InvalidOperationException("Not a registered ability binding. This should never happen!");
                }
                if (bindings.TryGetValue(type, out ModHotKey ret))
                {
                    return ret;
                }
                return bindings[type] = this[type.BaseType];
            }
        }

        public ModHotKey Get<T>() => this[typeof(T)];

        public void Bind<T>(string display, string defaultKey) where T : Ability
        {
            bindings[typeof(T)] = mod.RegisterHotKey(display, defaultKey);
        }

        internal void LoadDefaults()
        {
            Bind<Dash>("Forbidden Winds", "LeftShift");
            Bind<Wisp>("Faeflame", "F");
            Bind<Purify>("Purity Crown", "N");
            Bind<Smash>("Gaia's Fist", "Z");
        }
        internal void Unload()
        {
            bindings.Clear();
        }
    }
}
