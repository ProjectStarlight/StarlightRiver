using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class TargetHost : HookGroup
    {
        //Hook where Maps are loaded, as well as easier access to maps. Window Resizing may be an issue :p
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public static Map Maps;

        public static MapPass GetMap(string String) => Maps?.Get(String);
        public static MapPass GetMap<T>() where T : MapPass => Maps?.Get<T>();

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Maps = new Map();

            Mod Mod = ModContent.GetInstance<StarlightRiver>();

            foreach (Type t in Mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(MapPass)))
                {
                    var state = (MapPass)Activator.CreateInstance(t);
                    Maps.AddMap(t.Name, state);
                }
            }
        }
    }
}