using NetEasy;
using System;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Packets
{
	[Serializable]
	public class SpawnDummy : Module
	{
		private readonly int fromWho;
		private readonly int type;
		private readonly int x;
		private readonly int y;

		public SpawnDummy(int fromWho, int type, int x, int y)
		{
			this.fromWho = fromWho;
			this.type = type;
			this.x = x;
			this.y = y;
		}

		protected override void Receive()
		{
			if (Main.netMode == Terraria.ID.NetmodeID.Server)
			{
				if (DummyTile.DummyExists(x, y, type))
				{
					DummyTile.GetDummy(x, y, type).netUpdate = true; //this case meant that a Player went up to a tile dummy that did not exist for them, but did on server and we want to make sure they receive it
					return;
				}

				var p = new Projectile();
				p.SetDefaults(type);

				Vector2 spawnPos = new Vector2(x, y) * 16 + p.Size / 2;

				int n = Projectile.NewProjectile(new EntitySource_WorldEvent(), spawnPos, Vector2.Zero, type, 0, 0);
				NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile, -1, -1, null, n);

				var key = new Point16(x, y);
				DummyTile.dummies[key] = Main.projectile[n];
			}
		}
	}
}