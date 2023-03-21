using System;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	internal class BossRushStage
	{
		readonly string structurePath;
		readonly int bossType;
		readonly Vector2 spawnOffset;
		readonly Action<Vector2> spawnBoss;
		readonly Action<Point16> onGenerate;

		public Vector2 actualPosition;

		public BossRushStage(string structurePath, int bossType, Vector2 spawnOffset, Action<Vector2> spawnBoss, Action<Point16> onGenerate)
		{
			this.structurePath = structurePath;
			this.bossType = bossType;
			this.spawnOffset = spawnOffset;
			this.spawnBoss = spawnBoss;
			this.onGenerate = onGenerate;
		}

		public void Generate(ref Vector2 pos)
		{
			var dims = new Point16();
			StructureHelper.Generator.GetDimensions(structurePath, StarlightRiver.Instance, ref dims);

			StructureHelper.Generator.GenerateStructure(structurePath, pos.ToPoint16(), StarlightRiver.Instance);
			actualPosition = pos * 16;

			onGenerate(pos.ToPoint16());

			pos.X += dims.X;

			if (pos.X > Main.maxTilesX)
			{
				pos.X = 100;
				pos.Y += dims.Y;
			}
		}

		public void EnterArena(Player player)
		{
			player.Center = actualPosition + spawnOffset;
		}

		public void BeginFight()
		{
			ModContent.GetInstance<BossRushSystem>().trackedBossType = bossType;
			spawnBoss(actualPosition);
		}

		public void Save(TagCompound tag)
		{
			tag["pos"] = actualPosition;
		}

		public void Load(TagCompound tag)
		{
			actualPosition = tag.Get<Vector2>("pos");
		}
	}
}
