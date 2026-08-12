global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using StarlightRiver.Core;
global using StarlightRiver.Helpers;
global using Terraria;
global using Terraria.Localization;
global using Terraria.ModLoader;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bestiary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.ID;

namespace StarlightRiver
{
	public class StarlightRiver : Mod
	{
		private List<IOrderedLoadable> loadCache;

		public static bool debugMode = false;

		public static StarlightRiver Instance { get; set; }

		public AbilityHotkeys AbilityKeys { get; private set; }

		public StarlightRiver()
		{
			Instance = this;
		}

		public override void Load()
		{
#if DEBUG
			debugMode = true;
#endif

			loadCache = new List<IOrderedLoadable>();

			foreach (Type type in Code.GetTypes())
			{
				if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IOrderedLoadable)))
				{
					object instance = Activator.CreateInstance(type);
					loadCache.Add(instance as IOrderedLoadable);
				}

				loadCache.Sort((n, t) => n.Priority.CompareTo(t.Priority));
			}

			for (int k = 0; k < loadCache.Count; k++)
			{
				loadCache[k].Load();
				Terraria.ModLoader.UI.Interface.loadMods.SubProgressText = "Loading " + loadCache[k].GetType().Name;
			}

			if (!Main.dedServ)
			{
				//Hotkeys
				AbilityKeys = new AbilityHotkeys(this);
				AbilityKeys.LoadDefaults();
			}
		}

		public override void Unload()
		{
			if (loadCache != null)
			{
				foreach (IOrderedLoadable loadable in loadCache)
				{
					loadable.Unload();
				}

				loadCache = null;
			}
			else
			{
				Logger.Warn("load cache was null, IOrderedLoadable's may not have been unloaded...");
			}

			if (!Main.dedServ)
			{
				Instance ??= null;
				AbilityKeys?.Unload();

				SLRSpawnConditions.Unload();
			}
		}

		public override void PostSetupContent()
		{
			Compat.BossChecklist.BossChecklistCalls.CallBossChecklist();
			Compat.Wikithis.WikithisWrapper.AddStarlightRiverWikiUrl();

			NetEasy.NetEasy.Register(this);

			foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IPostLoadable)))
				{
					object toLoad = Activator.CreateInstance(type);

					((IPostLoadable)toLoad).PostLoad();
				}
			}
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			NetEasy.NetEasy.HandleModule(reader, whoAmI);
		}
	}
}