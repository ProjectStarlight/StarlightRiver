global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using StarlightRiver.Core;
global using Terraria;
global using Terraria.ModLoader;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bestiary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StarlightRiver
{
	public class TemporaryFix : PreJITFilter
	{
		public override bool ShouldJIT(MemberInfo member)
		{
			return false;
		}
	}

	public partial class StarlightRiver : Mod
	{
		private List<IOrderedLoadable> loadCache;

		private List<IRecipeGroup> recipeGroupCache;

		public static bool debugMode = false;

		//debug hook to view RTs
		//public override void PostDrawInterface(SpriteBatch spriteBatch)
		//{
		//    spriteBatch.Draw(Content.CustomHooks.HotspringMapTarget.hotspringMapTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Purple * 0.5f);
		//    spriteBatch.Draw(Content.CustomHooks.HotspringMapTarget.hotspringShineTarget, new Rectangle(Main.screenWidth - (Main.screenWidth / 4), 0, Main.screenWidth / 4, Main.screenHeight / 4), Color.White * 0.5f);
		//}

		public static StarlightRiver Instance { get; set; }

		public AbilityHotkeys AbilityKeys { get; private set; }

		public StarlightRiver()
		{
			Instance = this;
			PreJITFilter = new TemporaryFix();
		}

		public bool useIntenseMusic = false; //TODO: Make some sort of music handler at some point for this

		private Vector2 lastScreenSize; //Putting these in StarlightRiver incase anything else wants to use them (which is likely)

		public static void SetLoadingText(string text)
		{
			FieldInfo Interface_loadMods = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.Interface")!.GetField("loadMods", BindingFlags.NonPublic | BindingFlags.Static)!;
			MethodInfo UIProgress_set_SubProgressText = typeof(Mod).Assembly.GetType("Terraria.ModLoader.UI.UIProgress")!.GetProperty("SubProgressText", BindingFlags.Public | BindingFlags.Instance)!.GetSetMethod()!;

			UIProgress_set_SubProgressText.Invoke(Interface_loadMods.GetValue(null), new object[] { text });
		}

		public override void Load()
		{
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
				SetLoadingText("Loading " + loadCache[k].GetType().Name);
			}

			recipeGroupCache = new List<IRecipeGroup>();

			foreach (Type type in Code.GetTypes())
			{
				if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(IRecipeGroup)))
				{
					object instance = Activator.CreateInstance(type);
					recipeGroupCache.Add(instance as IRecipeGroup);
				}

				recipeGroupCache.Sort((n, t) => n.Priority > t.Priority ? 1 : -1);
			}

			if (!Main.dedServ)
			{
				lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);

				//Hotkeys
				AbilityKeys = new AbilityHotkeys(this);
				AbilityKeys.LoadDefaults();
			}
		}

		public override void Unload()
		{
			foreach (IOrderedLoadable loadable in loadCache)
			{
				loadable.Unload();
			}

			loadCache = null;

			if (!Main.dedServ)
			{
				Instance = null;
				AbilityKeys.Unload();

				SLRSpawnConditions.Unload();
			}
		}

		public override void AddRecipeGroups()/* tModPorter Note: Removed. Use ModSystem.AddRecipeGroups */
		{
			foreach (IRecipeGroup group in recipeGroupCache)
			{
				group.AddRecipeGroups();
			}
		}

		public void CheckScreenSize()
		{
			if (!Main.dedServ && !Main.gameMenu)
				lastScreenSize = new Vector2(Main.screenWidth, Main.screenHeight);
		}

		public override void PostSetupContent()
		{
			Compat.BossChecklist.BossChecklistCalls.CallBossChecklist();

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