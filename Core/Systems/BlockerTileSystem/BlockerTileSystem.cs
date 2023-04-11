using StarlightRiver.Core.Loaders.TileLoading;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.BlockerTileSystem
{
	internal class BlockerTileSystem : ModSystem
	{
		private static readonly List<Blocker> blockers = new();

		public override void Load()
		{
			On_Main.Update += UpdateCollision;
		}

		public override void Unload()
		{
			On_Main.Update -= UpdateCollision;
		}

		private void UpdateCollision(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			if (Main.gameMenu && Main.netMode != NetmodeID.Server)
				return;

			foreach (Blocker barrier in blockers)
			{
				Main.tileSolid[barrier.Type] = barrier.activeFunction();
			}
		}

		public static void LoadBarrier(string internalName, Func<bool> activeFunction)
		{
			var instance = new Blocker(internalName, activeFunction);

			StarlightRiver.Instance.AddContent(instance);
			StarlightRiver.Instance.AddContent(new LoaderTileItem(internalName + "Item", internalName + "Item", "debug item", internalName, -1, "StarlightRiver/Assets/Default", true, 0));
			blockers.Add(instance);
		}
	}

	[Autoload(false)]
	internal class Blocker : ModTile
	{
		private readonly string internalName;

		public readonly Func<bool> activeFunction;

		public override string Texture => AssetDirectory.Invisible;

		public override string Name => internalName;

		public Blocker() { }

		public Blocker(string internalName, Func<bool> activeFunction)
		{
			this.internalName = internalName;
			this.activeFunction = activeFunction;
		}

		public override void SetStaticDefaults()
		{
			TileID.Sets.DrawsWalls[Type] = true;
			Main.tileBlockLight[Type] = false;
			MinPick = int.MaxValue;
		}
	}
}