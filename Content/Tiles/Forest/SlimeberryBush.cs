using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Forest
{
	public class SlimeberryBush : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			var anchor = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
			int[] valid = new int[] { TileID.Grass };

			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.DrawYOffset = 2;
			QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Grass, SoundID.Dig, false, new Color(200, 255, 220), false, false, "", anchor, default, valid);

			HitSound = SoundID.Grass;
		}

		public override void RandomUpdate(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			var data = TileObjectData.GetTileData(tile);
			int fullFrameWidth = data.Width * (data.CoordinateWidth + data.CoordinatePadding);

			if (tile.TileFrameX == 0 && tile.TileFrameY % 36 == 0)
			{
				if (Main.rand.NextBool(1) && tile.TileFrameX == 0)
				{
					for (int x = 0; x < data.Width; x++)
					{
						for (int y = 0; y < data.Height; y++)
						{
							Tile targetTile = Main.tile[i + x, j + y];
							targetTile.TileFrameX += (short)fullFrameWidth;
						}
					}
				}
			}
		}

		public override bool RightClick(int i, int j)
		{
			if (Main.tile[i, j].TileFrameX > 35)
			{
				Tile tile = Main.tile[i, j]; //Selects current tile

				int newX = i;
				int newY = j;

				if (tile.TileFrameX % 36 == 18)
					newX = i - 1;

				if (tile.TileFrameY % 36 == 18)
					newY = j - 1;

				for (int k = 0; k < 2; k++)
				{
					for (int l = 0; l < 2; ++l)
					{
						Main.tile[newX + k, newY + l].TileFrameX -= 36; //Changes frames to berry-less
					}
				}

				int rand = Main.rand.Next(3, 5);

				for (int k = 0; k < rand; k++)
				{
					int index = NPC.NewNPC(new EntitySource_TileInteraction(null, i, j), i * 16 + Main.rand.Next(32), j * 16 + Main.rand.Next(32), NPCType<BerrySlime>());
					Main.npc[index].velocity = Vector2.UnitY.RotatedByRandom(0.6f) * -8;
				}
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Grass, new Vector2(i, j) * 16);
			}

			return true;
		}

		public override void MouseOver(int i, int j)
		{
			if (Main.tile[i, j].TileFrameX >= 32)
			{
				Player Player = Main.LocalPlayer;
				Player.cursorItemIconID = ItemType<Slimeberry>();
				Player.noThrow = 2;
				Player.cursorItemIconEnabled = true;
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i * 16, j * 16), ItemType<SlimeberryBushItem>()); //drop a bush Item

			if (frameX > 35)
				Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<Slimeberry>()); //Drops berries if harvestable
		}
	}

	public class Slimeberry : QuickMaterial
	{
		public Slimeberry() : base("Slimeberry", "Ew.", 99, 1, ItemRarityID.Blue, AssetDirectory.ForestTile) { }
	}

	public class SlimeberryBushItem : QuickTileItem
	{
		public SlimeberryBushItem() : base("Slimeberry Bush", "Places a slimeberry bush", "SlimeberryBush", ItemRarityID.Blue, AssetDirectory.ForestTile) { }
	}

	public class BerrySlime : ModNPC
	{
		public ref float PhaseTimer => ref NPC.ai[0];
		public ref float GlobalTimer => ref NPC.ai[1];

		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slimeberry");
			Main.npcFrameCount[NPC.type] = 2;
		}

		public override void SetDefaults()
		{
			NPC.width = 18;
			NPC.height = 14;
			NPC.aiStyle = -1;
			NPC.lifeMax = 5;
			NPC.catchItem = (short)ItemType<Slimeberry>();
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.color = new Color(30, Main.rand.Next(170, 250), Main.rand.Next(220, 255));
		}

		public override void AI()
		{
			GlobalTimer++;

			if (GlobalTimer > 300) //die after 5 seconds
			{
				if (NPC.velocity.Y == 0)
				{
					NPC.velocity *= 0;
					NPC.frame = new Rectangle(0, NPC.height, NPC.width, NPC.height);

					NPC.alpha += 5;
					NPC.position.Y += 0.1f;
					NPC.noGravity = true;
					NPC.noTileCollide = true;
					NPC.knockBackResist = 0;

					if (NPC.alpha >= 255)
						NPC.active = false;
				}

				return;
			}

			if (NPC.velocity.Y == 0) //else jump around like a lunatic
			{
				NPC.velocity *= 0;
				NPC.frame = new Rectangle(0, NPC.height, NPC.width, NPC.height);

				PhaseTimer++;
				if (PhaseTimer >= 4 && Main.rand.NextBool(4))
				{
					NPC.direction = Main.rand.NextBool() ? 1 : -1;
					NPC.velocity += new Vector2(NPC.direction * Main.rand.NextFloat(2, 4), -6);
					NPC.netUpdate = true;

					Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.5f, PitchRange = (0.5f, 1.0f) }, NPC.Center);
				}
			}
			else
			{
				NPC.frame = new Rectangle(0, 0, NPC.width, NPC.height);
				PhaseTimer = 0;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.position - screenPos, NPC.frame, drawColor.MultiplyRGBA(NPC.color * ((255 - NPC.alpha) / 255f)), NPC.rotation, Vector2.Zero, NPC.scale, 0, 0);
			spriteBatch.Draw(Request<Texture2D>(Texture + "Shine").Value, NPC.position - screenPos, NPC.frame, drawColor * (0.6f * (255 - NPC.alpha) / 255f), NPC.rotation, Vector2.Zero, NPC.scale, 0, 0);
			return false;
		}

		public override void OnCaughtBy(Player player, Item item, bool failed)
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDust(NPC.position, 16, 16, DustID.t_Slime, 0, 0, 200, NPC.color, 0.5f);
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 1, 2));
		}

		/*public override void NPCLoot()
		{
            int i = Item.NewItem(NPC.Center, ItemID.Gel);
            Main.item[i].color = NPC.color;

            for (int k = 0; k < 20; k++)
                Dust.NewDust(NPC.position, 16, 16, DustID.t_Slime, 0, 0, 200, NPC.color, 0.5f);
		}*/
	}
}