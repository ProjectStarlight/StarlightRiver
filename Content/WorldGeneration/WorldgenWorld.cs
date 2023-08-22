﻿using StarlightRiver.Core.Systems.ChestLootSystem;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;

namespace StarlightRiver.Core
{
	public partial class StarlightWorld : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int ShiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
			int SurfaceIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Sunflowers"));
			int HellIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Lakes"));
			int DesertIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			int TrapsIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Traps"));
			int EndIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Tile Cleanup"));

			if (ShiniesIndex != -1)
			{
				tasks.Insert(DesertIndex + 1, new PassLegacy("Starlight River Temples", UndergroundTempleGen));

				tasks.Insert(DesertIndex + 1, new PassLegacy("Starlight River Shrines", ShrineGen));
				tasks.Insert(DesertIndex + 1, new PassLegacy("Starlight River Springs", SpringGen));
				tasks.Insert(DesertIndex + 2, new PassLegacy("Starlight River Permafrost", PermafrostGen));
				tasks.Insert(DesertIndex + 4, new PassLegacy("Starlight River Vitric Desert", VitricGen));
				tasks.Insert(DesertIndex + 5, new PassLegacy("Starlight River Artifacts", ArtifactGen));

				tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Ivy", ForestHerbGen));
				tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Desert", DesertGen));
				tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Surface Items", SurfaceItemPass));
				tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Ankh Chests", AnkhChestPass));
				tasks.Insert(SurfaceIndex + 1, new PassLegacy("Starlight River Drop Pods", DropPodGen));
				tasks.Insert(EndIndex + 1, new PassLegacy("Starlight River Big Trees", BigTreeGen));
				tasks.Insert(EndIndex + 1, new PassLegacy("Starlight River Bouncy Mushrooms", BouncyMushroomGen));
				tasks.Insert(EndIndex + 1, new PassLegacy("Starlight River Twisted Greatsword", TwistSwordGen));
				tasks.Insert(EndIndex + 1, new PassLegacy("Starlight River Salt Gen", SeaSaltPass));
			}
		}

		public override void PostWorldGen()
		{
			ChestLootSystem.Instance.PopulateAllChests();
			LootWraithGen();
		}
	}
}