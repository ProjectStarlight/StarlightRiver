using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Crimson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class TheThinker : ModNPC
	{
		public static readonly List<TheThinker> toRender = new();

		public bool active = false;
		public List<Point16> tilesChanged = new();

		public override string Texture => AssetDirectory.BrainRedux + Name;

		public override void Load()
		{
			GraymatterBiome.onDrawHallucinationMap += DrawAura;
			GraymatterBiome.onDrawOverHallucinationMap += DrawMe;
		}

		public override void SetDefaults()
		{
			NPC.noGravity = true;
			NPC.aiStyle = -1;
			NPC.width = 128;
			NPC.height = 128;
			NPC.immortal = true;
			NPC.dontTakeDamage = true;
			NPC.damage = 10;
			NPC.lifeMax = 1000;

			toRender.Add(this);
		}

		public override void AI()
		{
			GraymatterBiome.forceGrayMatter = true;

			Lighting.AddLight(NPC.Center, new Vector3(1f, 0.2f, 0.2f));

			for(int k = 0; k < Main.maxPlayers; k++)
			{
				var player = Main.player[k];

				if (Vector2.DistanceSquared(player.Center, NPC.Center) <= Math.Pow(256, 2))
					player.AddBuff(ModContent.BuffType<CrimsonHallucination>(), 10);
			}
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override bool NeedSaving()
		{
			return true;
		}

		public void CreateArena()
		{
			for(int x = -60; x <= 60; x++)
			{
				for(int y = -60; y <= 60; y++)
				{
					var off = new Vector2(x, y);
					var dist = off.LengthSquared();

					if (dist <= Math.Pow(50, 2))
					{
						var tile = Main.tile[(int)NPC.Center.X / 16 + x, (int)NPC.Center.Y / 16 + y];

						tile.LiquidAmount = 0;

						if (tile.HasTile && !tile.IsActuated)
						{
							tile.IsActuated = true;							
							tilesChanged.Add(new Point16(x, y));
						}
					}

					if (dist > Math.Pow(50, 2) && dist <= Math.Pow(60, 2))
					{
						var tile = Main.tile[(int)NPC.Center.X / 16 + x, (int)NPC.Center.Y / 16 + y];

						if (!tile.HasTile)
						{
							tile.HasTile = true;
							tile.TileType = (ushort)ModContent.TileType<BrainBlocker>();
							tile.Slope = Terraria.ID.SlopeType.Solid;
							WorldGen.TileFrame((int)NPC.Center.X / 16 + x, (int)NPC.Center.Y / 16 + y);
							tilesChanged.Add(new Point16(x, y));
						}
					}
				}
			}

			active = true;
		}

		public void ResetArena()
		{
			foreach(Point16 point in tilesChanged)
			{
				var tile = Main.tile[(int)NPC.Center.X / 16 + point.X, (int)NPC.Center.Y / 16 + point.Y];

				if (tile.IsActuated)
					tile.IsActuated = false;

				if (tile.TileType == ModContent.TileType<BrainBlocker>())
					tile.HasTile = false;
			}

			tilesChanged.Clear();
			active = false;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			if (tilesChanged.Count == 0)
				CreateArena();
			else
				ResetArena();
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot)
		{
			return Helpers.Helper.CheckCircularCollision(NPC.Center, 64, target.Hitbox);
		}

		public override void SaveData(TagCompound tag)
		{
			tag.Add("active", active);
			tag.Add("tiles", tilesChanged);
		}

		public override void LoadData(TagCompound tag)
		{
			active = tag.GetBool("active");
			tilesChanged = tag.GetList<Point16>("tiles") as List<Point16>;
		}

		private void DrawAura(SpriteBatch sb)
		{
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;
			var color = Color.White;
			color.A = 0;

			foreach (TheThinker thinker in toRender)
			{
				sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 6f, 0, 0);
				sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 6f, 0, 0);
				sb.Draw(glow, thinker.NPC.Center - Main.screenPosition, null, color, 0, glow.Size() / 2f, 6f, 0, 0);
			}

			toRender.RemoveAll(n => n is null || !n.NPC.active);
		}

		private void DrawMe(SpriteBatch sb)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			foreach (TheThinker thinker in toRender)
			{
				sb.Draw(tex, thinker.NPC.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
			}
		}
	}
}
