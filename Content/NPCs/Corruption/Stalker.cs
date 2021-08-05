using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Corruption
{
	class Stalker : ModNPC
	{
		public override string Texture => AssetDirectory.Assets + "NPCs/Corruption/" + Name;

		public ref float Timer => ref npc.ai[0];
		public ref float State => ref npc.ai[1];

		enum States
		{
			Waiting,
			Fleeing
		}

		public override void SetDefaults()
		{
			npc.noGravity = true;
			npc.width = 36;
			npc.height = 28;
			npc.immortal = true;
			npc.lifeMax = 10000;
			npc.alpha = 255;
		}

		public override void AI()
		{
			Timer++;

			if(Timer % 240 <= 30)
				npc.frame = new Rectangle(0, npc.height * (int)(Timer % 240 / 30f * 8), npc.width, npc.height);
			else
				npc.frame = new Rectangle(0, 0, npc.width, npc.height);

			if (State == 0)
			{
				npc.TargetClosest();
				if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) < 360 ||
					Main.projectile.Any(n => n.active && n.friendly && Vector2.Distance(n.Center, npc.Center) < 360) ||
					NearLight()
					)

					State = (int)States.Fleeing;
			}

			if (State == 1)
			{
				npc.alpha -= 25;
				if (npc.alpha < 50)
					npc.active = false;
			}
		}

		public bool NearLight()
		{
			for(int x = -5; x < 5; x++)
				for(int y =  -5; y < 5; y++)
				{
					int xPos = (int)(npc.Center.X / 16) + x;
					int yPos = (int)(npc.Center.Y / 16) + y;

					if (Lighting.Brightness(xPos, yPos) > 0.15f)
						return true;
				}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			var tile = Framing.GetTileSafely(spawnInfo.spawnTileX, spawnInfo.spawnTileY);
			var spawnPoint = new Vector2(spawnInfo.spawnTileX, spawnInfo.spawnTileY) * 16;

			return spawnInfo.player.ZoneCorrupt && 
				tile.wall == WallID.EbonstoneUnsafe && 
				tile.liquid == 0 &&
				Lighting.Brightness(spawnInfo.spawnTileX, spawnInfo.spawnTileY) <= 0.1f &&
				!Main.npc.Any(n => n.type == NPCType<Stalker>() && Vector2.Distance(n.Center, spawnPoint) < 320) ? 1 : 0;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
		{
			var tex = GetTexture(Texture);
			spriteBatch.Draw(tex, npc.Center - Main.screenPosition, npc.frame, Color.White * (npc.alpha / 255f), 0, npc.Size / 2, 1, 0, 0);

			return false;
		}
	}
}
