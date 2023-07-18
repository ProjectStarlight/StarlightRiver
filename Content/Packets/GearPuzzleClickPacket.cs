using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Bosses.VitricBoss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using static StarlightRiver.Content.Bosses.VitricBoss.VitricBoss;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class GearPuzzleClickPacket : Module
	{
		readonly int tileX;
		readonly int tileY;
		readonly int dummyType;

		public GearPuzzleClickPacket(int tileX, int tileY, int dummyType)
		{
			this.tileX = tileX;
			this.tileY = tileY;
			this.dummyType = dummyType;
		}

		protected override void Receive()
		{
			var dummy = DummyTile.GetDummy(tileX, tileY, dummyType).ModProjectile as GearTileDummy;

			var entity = TileEntity.ByPosition[new Point16(tileX, tileY)] as GearTileEntity;

			if (entity is null)
			{
				if (Main.netMode == NetmodeID.Server)
					StarlightRiver.Instance.Logger.Error("failed to find gear tile entity.");

				return;
			}
				

			if (dummy is null)
			{
				if (Main.netMode == NetmodeID.Server)
					StarlightRiver.Instance.Logger.Error("failed to find gear dummy.");

				return;
			}

			entity.Disengage();

			dummy.oldSize = dummy.Size;
			dummy.Size++;
			dummy.gearAnimation = 40;

			GearPuzzleHandler.engagedObjectives = 0;

			GearPuzzleHandler.PuzzleOriginEntity?.Engage(2);
			
			if (Main.netMode == NetmodeID.Server)
				Send(-1, Sender, false);
		}
	}
}