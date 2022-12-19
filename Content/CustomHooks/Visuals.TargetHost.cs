using System;

namespace StarlightRiver.Content.CustomHooks
{
	class TargetHost : HookGroup
	{
		//Hook where Maps are loaded, as well as easier access to maps. Window Resizing may be an issue :p
		public static Map Maps;

		public static MapPass GetMap(string String)
		{
			return Maps?.Get(String);
		}

		public static MapPass GetMap<T>() where T : MapPass
		{
			return Maps?.Get<T>();
		}

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

		public override void Unload()
		{
			Maps = null;
		}
	}
}