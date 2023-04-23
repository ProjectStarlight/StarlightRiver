using StarlightRiver.Content.Bosses.GlassMiniboss;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Vitric;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class Coolmite : ModNPC
	{
		bool melting = false;
		int meltingTimer = 0;

		public ref float ActionState => ref NPC.ai[0];
		public ref float ActionTimer => ref NPC.ai[1];
		public ref float GlobalTimer => ref NPC.ai[2];
		public ref float TurnTimer => ref NPC.ai[3];
		public float meltingTransparency => (float)meltingTimer / 150;

		public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/Coolmite";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coolmite");
			Main.npcCatchable[Type] = true;
		}

		public override void SetDefaults()
		{
			NPC.catchItem = ItemType<CoolmiteItem>();
			NPC.width = 24;
			NPC.height = 24;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.lifeMax = 25;
			NPC.aiStyle = -1;
			NPC.lavaImmune = true;
			NPC.HitSound = SoundID.Item27;

			ActionState = -1;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("[PH] Entry")
			});
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WritePackedVector2(NPC.velocity);
			writer.Write(melting);
			writer.Write(meltingTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			NPC.velocity = reader.ReadPackedVector2();
			melting = reader.ReadBoolean();
			meltingTimer = reader.Read();
		}

		public override void AI()
		{
			int x = (int)(NPC.Center.X / 16) + NPC.direction; //check 1 tile infront of la cretura
			int y = (int)((NPC.Center.Y + 8) / 16);
			Tile tile = Framing.GetTileSafely(x, y);
			Tile tileUp = Framing.GetTileSafely(x, y - 1);
			Tile tileClose = Framing.GetTileSafely(x - NPC.direction, y - 1);
			Tile tileFar = Framing.GetTileSafely(x + NPC.direction * 2, y - 1);
			Tile tileUnder = Framing.GetTileSafely(x, y + 1);

			ActionTimer++;
			GlobalTimer++;

			if (melting)
			{
				meltingTimer++;

				if (meltingTimer % 4 == 0)
					Gore.NewGoreDirect(NPC.GetSource_FromAI(), NPC.Center, (Vector2.UnitY * -3).RotatedByRandom(0.2f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));
			}

			if (meltingTimer > 120)
			{
				NPC.active = false;
				NPC magmite = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.position.X, (int)NPC.position.Y, NPCType<MagmitePassive>(), 0, NPC.ai[0], NPC.ai[1], NPC.ai[2], NPC.ai[3], NPC.target);
				magmite.frame = NPC.frame;
				magmite.velocity = NPC.velocity;
				magmite.velocity.Y = -10;

				SoundEngine.PlaySound(SoundID.Item176);

				for (int k = 0; k < 20; k++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.5f), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.5f, 0.8f));

				for (int k = 0; k < 20; k++)
					Dust.NewDustDirect(NPC.Center, 16, 16, DustID.Torch, 0, 0, 0, default, 1.5f).velocity *= 3f;
			}

			if (ActionState == -1)
			{
				if (tile.LiquidAmount > 0)
				{
					NPC.velocity.Y = -4;
				}
				else
				{
					NPC.velocity.X += Main.rand.NextBool() ? 5 : -5;
					NPC.velocity.Y = -10;
					ActionState = 0;
					if (Main.netMode == NetmodeID.Server)
						NPC.netUpdate = true;
				}
			}

			if (ActionState == 0)
			{
				if (NPC.velocity.Y == 0 && NPC.velocity.X == 0 && tile.Slope == SlopeType.Solid && !tile.IsHalfBlock &&
					tile.HasTile && tile.BlockType == BlockType.Solid && Main.tileSolid[tile.TileType] &&
					(!tileUp.HasTile || !Main.tileSolid[tileUp.TileType] && !Main.tileSolidTop[tileUp.TileType]) &&
					(!tileClose.HasTile || !Main.tileSolid[tileClose.TileType] && !Main.tileSolidTop[tileClose.TileType])) //climb up small cliffs
				{
					ActionState = 1;
					NPC.velocity *= 0;
					ActionTimer = 0;
					return;
				}
				else if (NPC.velocity.X == 0 && tile.HasTile && Main.tileSolid[tile.TileType] && (!tileUp.HasTile || !Main.tileSolid[tileUp.TileType] && !Main.tileSolidTop[tileUp.TileType]))
				{
					NPC.velocity.Y -= 2;
				}

				if (NPC.velocity.X == 0)
					TurnTimer++;
				else
					TurnTimer = 0;

				if (TurnTimer > 180)
				{
					NPC.velocity.X = NPC.direction * -1;
					NPC.target = -1;
					TurnTimer = 0;
				}

				if (ActionTimer % 60 == 0)
					NPC.TargetClosest();

				Vector2? lavaPos = FindLava(); // make coolmite target lava if nearby so it can melt back down to a magmite

				if (lavaPos != null)
					NPC.velocity.X += ((Vector2)lavaPos).X == NPC.Center.X ? 0 : 0.05f * (((Vector2)lavaPos).X > NPC.Center.X ? 1 : -1);
				else if (NPC.target >= 0)
					NPC.velocity.X += 0.05f * (Main.player[NPC.target].Center.X > NPC.Center.X ? 1 : -1);

				if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava)
				{
					melting = true;
				}

				NPC.velocity.X = Math.Min(NPC.velocity.X, 1.5f);
				NPC.velocity.X = Math.Max(NPC.velocity.X, -1.5f);

				NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
				NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;

				if (tileFar.HasTile && tileFar.BlockType == BlockType.Solid && Main.tileSolid[tileFar.TileType] && NPC.velocity.Y == 0) //jump up big cliffs
					NPC.velocity.Y -= 8;

				if ((!tileUnder.HasTile || !Main.tileSolid[tileUnder.TileType] && !Main.tileSolidTop[tileUnder.TileType]) && NPC.velocity.Y == 0) //hop off edges
					NPC.velocity.Y -= 4;

				if (NPC.velocity.Y != 0)
				{
					NPC.frame.X = 0;
					NPC.frame.Y = 0;
				}
				else
				{
					NPC.frame.X = 42;
					NPC.frame.Y = (int)(ActionTimer / 5 % 5) * 40;
				}
			}

			if (ActionState == 1)
			{
				if (ActionTimer == 60)
				{
					ActionState = 0;
					ActionTimer = 0;
					NPC.position.Y -= 16;
					NPC.position.X += 26 * NPC.direction;
				}

				NPC.frame.X = 84;
				NPC.frame.Y = (int)(ActionTimer / 60f * 9) * 40;
			}

			NPC.frame.Width = 42;
			NPC.frame.Height = 40;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int k = 0; k < 5; k++)
					Dust.NewDust(NPC.position, 16, 16, DustID.Demonite);

				for (int k = 0; k < 25; k++)
					Dust.NewDust(NPC.position, 16, 16, DustID.Glass);

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 pos = NPC.Center - screenPos + new Vector2(0, -8);

			if (ActionState == 1)
			{
				pos += new Vector2(8 * NPC.spriteDirection, -4);

				if (NPC.spriteDirection == -1)
					pos.X += 4;
			}

			int originX = 18;

			if (NPC.spriteDirection == -1)
				originX = 30;

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos, NPC.frame, drawColor * (1 - meltingTransparency), 0, new Vector2(originX, 20), 1, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		private Vector2? FindLava()
		{
			Vector2? lavaPos = null;
			for (int i = -25; i < 25; i++)
			{
				for (int j = -5; j < 15; j++)
				{
					Tile tileLava = Main.tile[(int)NPC.Center.X / 16 + i, (int)NPC.Center.Y / 16 + j];

					if (tileLava.LiquidAmount > 0 && tileLava.LiquidType == LiquidID.Lava)
					{
						if (lavaPos == null || ((Vector2)lavaPos - NPC.Center).Length() > new Vector2(i, j).Length() * 16)
						{
							Vector2 checkPos = NPC.Center + new Vector2(i, j) * 16;
							if (Collision.CanHitLine(NPC.Center, 1, 1, checkPos, 1, 1) || Collision.CanHitLine(NPC.Center - Vector2.UnitY * 50, 1, 1, checkPos, 1, 1)) // checks if lava can be reached
								lavaPos = checkPos;
						}
					}
				}
			}

			return lavaPos;
		}
	}

	internal class CoolmiteItem : QuickCritterItem
	{
		public CoolmiteItem() : base("Coolmite", "Fragile! Please handle with care.", Item.sellPrice(silver: 15), ItemRarityID.Orange, NPCType<Coolmite>(), AssetDirectory.VitricItem) { }
	}
}