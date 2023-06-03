using System;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.BossRushSystem
{
	/// <summary>
	/// Object to represent one stage of the boss rush mode
	/// </summary>
	internal class BossRushStage
	{
		/// <summary>
		/// The structure this stage should generate for its arena. The size of this structure is used to organize the arenas in the world.
		/// </summary>
		readonly string structurePath;
		/// <summary>
		/// The boss of this stage. If there are no NPCs of this type alive, the stage is considered complete
		/// </summary>
		readonly int bossType;
		/// <summary>
		/// Where the player should be placed into the world relative to the arena
		/// </summary>
		readonly Vector2 spawnOffset;
		/// <summary>
		/// What this stage needs to do to spawn its boss. You can also set up other boss-specific things here like moving biomes into the correct place
		/// </summary>
		readonly Action<Vector2> spawnBoss;
		/// <summary>
		/// What happens after this stage generates. Can be used to do things like set world variables correctly
		/// </summary>
		readonly Action<Point16> onGenerate;
		/// <summary>
		/// Extra space between this arena and the arena on the right
		/// </summary>
		readonly int marginRight;

		/// <summary>
		/// The world position of this stage's arena.
		/// </summary>
		public Vector2 actualPosition;

		public BossRushStage(string structurePath, int bossType, Vector2 spawnOffset, Action<Vector2> spawnBoss, Action<Point16> onGenerate, int marginRight = 0)
		{
			this.structurePath = structurePath;
			this.bossType = bossType;
			this.spawnOffset = spawnOffset;
			this.spawnBoss = spawnBoss;
			this.onGenerate = onGenerate;
			this.marginRight = marginRight;
		}

		/// <summary>
		/// Generate the arena and run on generate logic for this arena. Moves the ref vector2 to give the next available place for an arena in the world.
		/// </summary>
		/// <param name="pos">Where to generate the arena. Increment this by the size of the arena.</param>
		public void Generate(ref Vector2 pos)
		{
			var dims = new Point16();
			StructureHelper.Generator.GetDimensions(structurePath, StarlightRiver.Instance, ref dims);

			StructureHelper.Generator.GenerateStructure(structurePath, pos.ToPoint16(), StarlightRiver.Instance);
			actualPosition = pos * 16;

			onGenerate(pos.ToPoint16());

			pos.X += dims.X + marginRight;

			if (pos.X > Main.maxTilesX)
			{
				pos.X = 100;
				pos.Y += dims.Y;
			}
		}

		/// <summary>
		/// Teleports the player into the appropriate place in the arena
		/// </summary>
		/// <param name="player">The player to teleport</param>
		public void EnterArena(Player player)
		{
			player.Center = actualPosition + spawnOffset;
		}

		/// <summary>
		/// Starts the fight with this stage's boss
		/// </summary>
		public void BeginFight()
		{
			BossRushSystem.trackedBossType = bossType;
			spawnBoss(actualPosition);
		}

		/// <summary>
		/// Save data to the world about this stage
		/// </summary>
		/// <param name="tag"></param>
		public void Save(TagCompound tag)
		{
			tag["pos"] = actualPosition;
		}

		/// <summary>
		/// Load data for this stage
		/// </summary>
		/// <param name="tag"></param>
		public void Load(TagCompound tag)
		{
			actualPosition = tag.Get<Vector2>("pos");
		}
	}
}