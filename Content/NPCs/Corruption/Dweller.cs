using StarlightRiver.Content.Foregrounds;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Corruption
{
	class Dweller : ModNPC
	{
		public enum States
		{
			Idle,
			Transforming,
			Attacking
		};

		Tile? root = null;

		public ref float State => ref NPC.ai[0];
		public ref float Timer => ref NPC.ai[1];
		public ref float Variant => ref NPC.ai[2];
		public ref float Height => ref NPC.ai[3];

		public Player Target => Main.player[NPC.target];

		public override string Texture => "StarlightRiver/Assets/NPCs/Corruption/Dweller";

		public override void Load()
		{
			for (int j = 1; j <= 7; j++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, "StarlightRiver/Assets/NPCs/Corruption/DwellerGore" + j);
			}
		}

		public override void SetDefaults()
		{
			Variant = Main.rand.Next(3);

			if (Variant == 0)
				NPC.width = 66;

			if (Variant == 1)
				NPC.width = 58;

			if (Variant == 2)
				NPC.width = 64;

			NPC.height = NPC.width;
			NPC.value = 100;
			NPC.lifeMax = 120;
			NPC.defense = 6;
			NPC.knockBackResist = 0;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.behindTiles = true;
			NPC.HitSound = SoundID.NPCHit47;
			NPC.DeathSound = SoundID.NPCDeath49;
			NPC.damage = 20;
		}

		public override void OnSpawn(IEntitySource source)
		{
			NPC.position.Y -= Main.rand.Next(200, 300);
			NPC.damage = 0; //Use this to override bestiary entry
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
				new FlavorTextBestiaryInfoElement("They have evolved to camoflauge themselves as trees, and use this ability to ambush unsuspecting victims in comical fashion. The laugh just sells the point.")
			});
		}

		public override void AI()
		{
			if (root is null) //scan to find a valid root tile under where it spawned
			{
				NPC.Center = NPC.Center - new Vector2(NPC.Center.X % 16, NPC.Center.Y % 16);

				for (int k = 0; root is null; k++)
				{
					Tile tile = Framing.GetTileSafely((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16 + k);

					if (tile.HasTile && tile.BlockType == BlockType.Solid && Main.tileSolid[tile.TileType])
					{
						root = tile;
						Height = k * 16;

						//Main.NewText(tile.type);//debug
					}
				}
			}

			if (!root.Value.HasTile) //should automatically activate if the tile under it is killed
			{
				State = (int)States.Transforming;
				Timer = 0;
			}

			Timer++;

			switch (State)
			{
				case (int)States.Idle:

					//clientside vignette effect
					float distance = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center + Vector2.UnitY * Height);

					if (distance < 500)
					{
						Vignette.visible = true;
						Vignette.offset = Vector2.Zero;
						Vignette.opacityMult = 1 - distance / 500f;
					}

					NPC.TargetClosest();

					float yOff = Target.Center.Y - NPC.Center.Y;

					if (Timer > 60 && Math.Abs(Target.Center.X - NPC.Center.X) < 32 && yOff < (Height - 16) && yOff > 0) //under the tree
					{
						SoundEngine.PlaySound(SoundID.Zombie79, NPC.Center);
						State = (int)States.Transforming;
						Timer = 0;
					}

					break;

				case (int)States.Transforming:

					if (Timer == 100)
					{
						NPC.noGravity = false;

						Terraria.Audio.SoundEngine.PlaySound(SoundID.Grass, NPC.Center);
					}

					if (Timer > 60)
					{
						for (int k = 0; k < 5; k++)
						{
							Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(NPC.width / 2),
								DustType<Dusts.GreyLeaf>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, new Color(210, 200, 255), Main.rand.NextFloat(1.0f, 1.3f));
						}
					}

					if (Timer >= 100)
					{
						if (NPC.velocity.Y == 0)
						{
							State = (int)States.Attacking;
							NPC.knockBackResist = 1.2f;
							NPC.damage = 20;
						}
					}

					break;

				case (int)States.Attacking:

					NPC.TargetClosest();

					NPC.velocity.X += Target.Center.X > NPC.Center.X ? 0.1f : -0.1f;

					if (NPC.velocity.X > 3)
						NPC.velocity.X = 2.9f;

					if (NPC.velocity.X < -3)
						NPC.velocity.X = -2.9f;

					NPC.rotation += NPC.velocity.X / (NPC.width / 2f);

					if (NPC.collideX && NPC.velocity.Y == 0)
						NPC.velocity.Y = -6;
					break;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var rand = new Random(NPC.GetHashCode());
			int frameHeight = Request<Texture2D>(Texture + Variant).Value.Height / 4;
			int frameWidth = Request<Texture2D>(Texture + Variant).Value.Width;

			if (NPC.IsABestiaryIconDummy)
			{
				NPC.frame = new Rectangle(0, 0, frameWidth, frameHeight);
				spriteBatch.Draw(Request<Texture2D>(Texture + Variant).Value, NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, NPC.Size / 2, NPC.scale, 0, 0);
				return false;
			}

			switch (State)
			{
				case (int)States.Transforming: //fall-through moment
				case (int)States.Idle:

					Texture2D barkTex = Request<Texture2D>("Terraria/Images/Tiles_5_0").Value; //corruption tree bark 

					for (int k = 0; k < Height; k += 16)
					{
						Vector2 pos = NPC.Center + Vector2.UnitY * k;
						pos -= new Vector2(pos.X % 16, pos.Y % 16);

						if (State == (int)States.Transforming && Timer > 60)
						{
							pos.Y += (Timer - 60) / 30f * Height;

							if (pos.Y > NPC.Center.Y + Height)
								continue;
						}

						var source = new Rectangle(rand.Next(2) * 22, rand.Next(6) * 22, 20, 16);
						Color color = Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16);

						spriteBatch.Draw(barkTex, pos - screenPos, source, color);

						if (rand.Next(6) == 0 && k > 48 && k < Height - 48)
						{
							Texture2D branchTex = Request<Texture2D>(Texture + "Branches").Value;

							bool right = rand.Next(2) == 0;
							Vector2 branchPos = pos + new Vector2(right ? 16 : -branchTex.Width / 2 + 6, -16);
							var branchSource = new Rectangle(right ? branchTex.Width / 2 : 0, rand.Next(3) * branchTex.Height / 3, 42, 42);
							Color branchColor = Lighting.GetColor((int)branchPos.X / 16, (int)branchPos.Y / 16);

							spriteBatch.Draw(branchTex, branchPos - screenPos, branchSource, branchColor);
						}

						if (k == Height - 16)
						{
							bool rightRoot = rand.Next(2) == 0;
							spriteBatch.Draw(barkTex, pos - screenPos + Vector2.UnitX * (rightRoot ? 14 : -14), new Rectangle(rightRoot ? 22 : 44, 6 * 22 + rand.Next(3) * 22, 22, 22), color);
						}
					}

					Texture2D topperTex = Request<Texture2D>(Texture + "Tops").Value;

					Vector2 topperPos = NPC.Center - new Vector2(NPC.Center.X % 16, NPC.Center.Y % 16) + new Vector2(11, 8);
					var topperSource = new Rectangle((int)Variant * 82, 0, 82, 82);

					if (State == (int)States.Transforming)
						topperSource.Y = (int)(Timer < 30 ? Timer / 30 * 5 : Timer % 10 < 5 ? 3 : 4) * 84;

					spriteBatch.Draw(topperTex, topperPos - screenPos, topperSource, drawColor, 0, new Vector2(41, 41), 1, 0, 0);

					break;

				case (int)States.Attacking:
					NPC.frame = new Rectangle(0, (int)Timer / 7 % 3 * frameHeight, frameWidth, frameHeight);

					spriteBatch.Draw(Request<Texture2D>(Texture + Variant).Value, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.Size / 2, NPC.scale, 0, 0);

					break;
			}

			return false;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return SpawnCondition.Corruption.Chance * 0.27f;
		}

		public override void OnKill()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				for (int j = 1; j <= 7; j++)
					Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), Main.rand.NextVector2Circular(3, 3), Mod.Find<ModGore>("DwellerGore" + j).Type);
			}
		}
	}
}
