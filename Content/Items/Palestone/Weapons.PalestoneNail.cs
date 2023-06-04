using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Tiles.Palestone;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Palestone
{
	public class PalestoneNail : ModItem
	{
		public override string Texture => AssetDirectory.PalestoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Palenail");
			Tooltip.SetDefault("Summons the Pale Knight");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(0, 0, 12, 0);
			Item.rare = ItemRarityID.White;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = BuffType<PalestoneSummonBuff>();
			Item.shoot = ProjectileType<PaleKnight>();
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);
			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
				
			recipe.AddIngredient(ItemID.SilverBar, 12);
			recipe.AddIngredient<PalestoneItem>(25);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TungstenBar, 12);
			recipe.AddIngredient<PalestoneItem>(25);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class PaleKnight : ModProjectile
	{
		public const int RunFrames = 8;

		public const int FlyFrames = 6;
		public const int FlyOffset = RunFrames;

		public const int AttackFrames = 6;
		public const int AttackOffset = FlyFrames + RunFrames;

		public const int PogoFrames = 5;
		public const int PogoOffset = FlyFrames + RunFrames + AttackFrames;

		private const int Walking = 0;
		private const int Flying = 1;
		private const int Attacking = 2;

		public const float defaultFrameSpeed = 10f;
		public const float defaultRunSpeed = 8f;
		public const float tiltAmount = 0.02f;

		public const int validZoneWidth = 8;
		public const int validZoneHeight = 24;
		public const int minionSpacing = 24;

		public const int maxMinionChaseRange = 2000;

		public const int MaxVelocity = 7;

		public const int MinionFlybackRange = 450;

		public int PogoAnimTimer;
		public int AttackAnimTimer;

		public int JumpDelay;
		public int AttackDelay;
		public bool JustPogod;

		public ref float AttackState => ref Projectile.ai[0];
		public ref float EnemyWhoAmI => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.PalestoneItem + "PalestoneKnight";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pale Knight");
			Main.projFrames[Projectile.type] = 25;

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void SetDefaults()
		{
			Projectile.width = 28;
			Projectile.height = 32;

			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;

			Projectile.minion = true;

			Projectile.minionSlots = 1f;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			EnemyWhoAmI = target.whoAmI; //TODO: possible desync, needs testing
		}

		public override void AI()
		{
			if (PogoAnimTimer > 0)
				PogoAnimTimer--;

			if (AttackAnimTimer > 0)
				AttackAnimTimer--;

			if (JumpDelay > 0)
				JumpDelay--;

			if (AttackDelay > 0)
				AttackDelay--;

			Player Player = Main.player[Projectile.owner];
			PaleKnightPlayer modPlayer = Player.GetModPlayer<PaleKnightPlayer>();

			#region Active check
			if (Player.dead || !Player.active) // This is the "active check", makes sure the minion is alive while the Player is alive, and despawns if not
				Player.ClearBuff(BuffType<PalestoneSummonBuff>());

			if (Player.HasBuff(BuffType<PalestoneSummonBuff>()))
				Projectile.timeLeft = 2;
			#endregion

			#region Find target
			// Starting search distance
			Vector2 targetCenter = Projectile.Center;
			bool foundTarget = EnemyWhoAmI >= 0;

			// This code is required if your minion weapon has the targeting feature
			if (Player.HasMinionAttackTargetNPC)
			{
				NPC NPC = Main.npc[Player.MinionAttackTargetNPC];
				float between = Vector2.Distance(NPC.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
					EnemyWhoAmI = NPC.whoAmI;
					foundTarget = true;
				}
			}
			else if (foundTarget)
			{
				NPC NPC = Main.npc[(int)EnemyWhoAmI];
				float betweenPlayer = Vector2.Distance(NPC.Center, Player.Center);

				if (NPC.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
				}
				else
				{
					EnemyWhoAmI = -1;
					foundTarget = false;
				}
			}

			if (!Player.HasMinionAttackTargetNPC)
			{
				NPC target = Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Player.Center) < maxMinionChaseRange && Collision.CanHitLine(Projectile.position, 0, 0, n.position, 0, 0)).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();
				if (target != default)
				{
					targetCenter = target.Center;
					EnemyWhoAmI = target.whoAmI;
					foundTarget = true;
				}
				else
				{
					EnemyWhoAmI = 0;
					foundTarget = false;
				}
			}

			#endregion

			#region Movement
			Vector2 pos = Player.Center + new Vector2((validZoneWidth + modPlayer.KnightCount * minionSpacing + 32) * -Player.direction, 16);
			var validZone = new Rectangle((int)(pos.X - validZoneWidth), (int)(pos.Y - validZoneHeight), validZoneWidth * 2, validZoneHeight * 2);
			if (foundTarget)
			{
				Projectile.tileCollide = true;
				AttackState = Attacking;
			}
			else if (AttackState != Flying)
			{
				Projectile.tileCollide = true;
				AttackState = Walking;
			}

			bool farAway = Vector2.Distance(Projectile.position, Player.position) > MinionFlybackRange && Vector2.Distance(Projectile.Center, validZone.Center()) > MinionFlybackRange;

			bool stuck = Vector2.Distance(Projectile.Center, Player.Center) > 200f && !Collision.CanHitLine(Projectile.Center, 1, 1, Player.Center, 1, 1);

			if ((farAway || stuck) && !foundTarget)
			{
				Projectile.tileCollide = false;
				AttackState = Flying;
			}
			/*else if (AttackState == Flying && WorldGen.SolidTileAllowBottomSlope((int)(Projectile.Center.X / 16), (int)(Projectile.Bottom.Y / 16)))
			{
				Projectile.tileCollide = true;
				AttackState = Walking;
			}*/

			//AttackState = 0;//debug
			float stepSpeed = 1f;
			switch (AttackState)
			{
				case Walking:
					{
						if (!foundTarget)
						{
							Projectile.velocity.X += Vector2.Normalize(validZone.Center() - Projectile.Center).X * 0.1f;

							if (Projectile.Center.Y > Player.Bottom.Y && WorldGen.SolidTileAllowBottomSlope((int)(Projectile.Center.X / 16), (int)((Projectile.position.Y + Projectile.height) / 16)))
								Projectile.velocity.Y -= (Projectile.Center.Y - Player.Center.Y) * 0.15f;

							if (validZone.Intersects(Projectile.getRect()))
							{
								Projectile.velocity.X *= 0.90f;

								if (Projectile.velocity.Length() < 0.2f && (int)Projectile.Center.X == (int)validZone.Center().X)
								{
									Projectile.velocity = Vector2.Zero;
									Projectile.direction = Player.direction;
								}
							}

							//!debug
							//for (int i = 0; i < 4; i++)
							//    Dust.NewDustPerfect(validZone.TopLeft() + new Vector2(Main.rand.Next(validZone.Width), Main.rand.Next(validZone.Height)), DustType<Dusts.AirDash>(), Vector2.Zero, 0, Color.Blue);
							//Dust.NewDustPerfect(validZone.Center(), DustType<Dusts.AirDash>(), Vector2.Zero, 0, Color.Red);
						}

						if (Projectile.velocity.Length() < 0.1f)
						{
							Projectile.frame = 0;
							Projectile.frameCounter = 0;
						}

						Projectile.velocity.Y += 0.35f;
					}

					break;
				case Flying:

					var idlePos = new Vector2(Player.Center.X, Player.Center.Y - 70);

					Vector2 toIdlePos = idlePos - Projectile.Center;
					if (toIdlePos.Length() < 0.0001f)
					{
						toIdlePos = Vector2.Zero;
					}
					else
					{
						float speed = Vector2.Distance(idlePos, Projectile.Center) * 0.2f;
						speed = Utils.Clamp(speed, 1f, 25f);
						toIdlePos.Normalize();
						toIdlePos *= speed;
					}

					Projectile.velocity = (Projectile.velocity * (45f - 1) + toIdlePos) / 45f;

					if (Vector2.Distance(Projectile.Center, idlePos) < 50f)
					{ 
						AttackState = Walking;
						Projectile.velocity *= 0.25f;
					}

					break;
				case Attacking:
					if (Projectile.Center.Y < targetCenter.Y)
						Projectile.shouldFallThrough = true;

					NPC target = Main.npc[(int)EnemyWhoAmI];


					if (!Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1))
					{
						AttackState = Flying;
						Projectile.tileCollide = false;
					}

					if (Projectile.Center.Distance(targetCenter) < 250f && JumpDelay <= 0 && AttackDelay <= 0 && WorldGen.SolidTileAllowBottomSlope((int)(Projectile.Center.X / 16), (int)(Projectile.Bottom.Y / 16)))
					{
						Projectile.velocity.Y -= 5f + (Projectile.Center.Y - target.Top.Y) * 0.1f;
						JumpDelay = 120;
					}
					else
					{
						Projectile.velocity.X += Vector2.Normalize(targetCenter - Projectile.Center).X * 0.15f;

						if (Projectile.Center.Y > targetCenter.Y && WorldGen.SolidTileAllowBottomSlope((int)(Projectile.Bottom.X / 16), (int)(Projectile.Bottom.Y / 16)))
							Projectile.velocity.Y -= Math.Abs(Projectile.Center.Y - targetCenter.Y) * 0.15f;

						if (JustPogod)
						{
							Vector2 pogoPos = Main.npc[(int)EnemyWhoAmI].Top;

							Vector2 toPogoPos = pogoPos - Projectile.Center;
							toPogoPos.Normalize();
							toPogoPos *= 10f;

							Projectile.velocity = (Projectile.velocity * 14f + toPogoPos) / 15f;
						}
						else if (Math.Abs(Projectile.Center.X - targetCenter.X) < 30f)
						{
							if (Projectile.Center.Y < targetCenter.Y)
								Projectile.velocity.X *= 0.9f;
							else
								Projectile.velocity.X *= 0.95f;

							if (Projectile.Center.Y < targetCenter.Y && Math.Abs(Projectile.Center.Y - targetCenter.Y) > 100f)
								Projectile.velocity.Y += 0.35f;
						}
					}

					if (WorldGen.SolidTileAllowBottomSlope((int)(Projectile.Center.X / 16 + 16), (int)(Projectile.Bottom.Y / 16)) || Projectile.Center.Y > Main.npc[(int)EnemyWhoAmI].Top.Y)
						JustPogod = false;

					if (Projectile.velocity.Length() < 0.1f)
					{
						Projectile.frame = 0;
						Projectile.frameCounter = 0;
					}

					Projectile.velocity.Y += 0.35f;

					if (Projectile.velocity.HasNaNs())
						foundTarget = false;

					if (AttackDelay <= 0)
					{
						if (Projectile.Distance(target.Top) < 50f && Projectile.Center.Y < target.Top.Y)
						{
							var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Bottom, Vector2.UnitY * 15f, ModContent.ProjectileType<PalestoneSlash>(), Projectile.damage, 1f, Projectile.owner, 0, 1f);

							PalestoneSlash slash = proj.ModProjectile as PalestoneSlash;


							slash.parent = Projectile;
							slash.knockbackVelo = new Vector2(0f, 25f);
							proj.rotation = MathHelper.ToRadians(90);

							AttackDelay = 24;
							Projectile.velocity *= 0.25f;
							JustPogod = true;
							PogoAnimTimer = 20;
							Terraria.Audio.SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

							HitNPC(new Vector2(0, 25f), true, target);
						}
						else if (Projectile.Distance(target.Left) < 35f && Projectile.Center.X < target.Left.X)
						{
							var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(10f, 5f), ModContent.ProjectileType<PalestoneSlash>(), Projectile.damage, 1f, Projectile.owner, -1).ModProjectile as PalestoneSlash;
							proj.parent = Projectile;
							AttackDelay = 44;
							Projectile.velocity *= 0.25f;
							AttackAnimTimer = 20;
							Terraria.Audio.SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

							HitNPC(new Vector2(6.5f, 0), false, target);
						}
						else if (Projectile.Distance(target.Right) < 35f && Projectile.Center.X > target.Right.X)
						{
							var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, new Vector2(-5f, 5f), ModContent.ProjectileType<PalestoneSlash>(), Projectile.damage, 1f, Projectile.owner, 1).ModProjectile as PalestoneSlash;
							proj.parent = Projectile;
							AttackDelay = 44;
							Projectile.velocity *= 0.25f;
							AttackAnimTimer = 20;
							Terraria.Audio.SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

							HitNPC(new Vector2(-6.5f, 0), false, target);
						}
					}

					break;
			}

			if (Projectile.Distance(Player.Center) > 1500f)
			{
				Projectile.velocity += Main.rand.NextVector2Circular(2.5f, 2.5f);

				for (int i = 0; i < 5; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2.5f, 2.5f), 0, Color.White, 0.55f);
				}

				Projectile.Center = Player.Center;

				for (int i = 0; i < 5; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2.5f, 2.5f), 0, Color.White, 0.55f);
				}
			}

			Projectile.velocity.X = Vector2.Clamp(Projectile.velocity, -new Vector2(8f), new Vector2(8f)).X;

			Projectile.velocity.Y = Vector2.Clamp(Projectile.velocity, -new Vector2(18f), new Vector2(18f)).Y;

			#endregion

			#region Animation and visuals

			Projectile.spriteDirection = Projectile.direction;

			int startOffset = 0;
			int frameCount = FlyFrames;
			float frameSpeed = defaultFrameSpeed; //this value is in fps

			switch (AttackState)
			{
				case 0:

					Projectile.rotation = 0;
					frameCount = RunFrames;
					frameSpeed = Math.Abs(Projectile.velocity.X * defaultRunSpeed);
					break;

				case 1:

					Projectile.rotation = Projectile.velocity.X * tiltAmount;
					startOffset = FlyOffset;
					break;

				case 2:
					frameCount = AttackFrames;

					Projectile.rotation = 0;
					if (PogoAnimTimer > 0)
					{
						frameSpeed = 15f;
						frameCount = PogoFrames;
						startOffset = PogoOffset;
					}
					else if (AttackAnimTimer > 0)
					{
						frameSpeed = 15f;
						startOffset = AttackOffset;
					}

					break;
			}

			Projectile.frameCounter++;

			if ((int)(Projectile.frameCounter * frameSpeed) >= 60)
			{
				Projectile.frameCounter = 0;
				Projectile.frame++;
			}

			if (Projectile.frame >= startOffset + frameCount || Projectile.frame < startOffset)
				Projectile.frame = startOffset;

			//Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
			Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;//this is here since I believe this gets synced, if it doesn't this line can be moved above where the sprite direction is set
			#endregion

			if (!foundTarget && JustPogod)
			{
				JustPogod = false;
				Projectile.velocity.Y *= 0.35f;
			}

			modPlayer.KnightCount++;
		}

		private void HitNPC(Vector2 knockbackVelo, bool Down, NPC target) // rather do this than an actual proj, less jank, hits consistently. Projectile is just for visuals (can be moved to drawcode in future).
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<PalestoneDummy>(), Projectile.damage, 0f, Projectile.owner); // yes this sucks i know ill fix it later

			Projectile.velocity -= knockbackVelo;
			for (int i = 0; i < 5; i++)
			{
				float mult = Main.rand.NextFloat(0.75f, 1.5f);
				if (Down)
					mult = Main.rand.NextFloat(0.1f, 0.35f);

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), -knockbackVelo.RotatedByRandom(0.35f) * mult, 0, Color.White, 0.55f);
			}
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			fallThrough = Projectile.shouldFallThrough;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				//if(extra tile(s))
				//	jump
				//else if(Projectile.velocity.X != oldVelocity.X)...

				if (Projectile.velocity.X != oldVelocity.X)//hit wall
				{
					if (AttackState == Walking || AttackState == Attacking)
					{
						int xpos = (int)((Projectile.Center.X + oldVelocity.X) / 16);
						int yposTop = (int)((Projectile.position.Y + Projectile.height + 12) / 16) - 2;
						//Dust.NewDustPerfect((new Vector2(xpos + Projectile.direction, yposTop) * 16) + new Vector2(8, -92), DustType<Dusts.AirDash>(), new Vector2(0, 100), 0, Color.Red);

						if (!(WorldGen.SolidOrSlopedTile(xpos + Projectile.direction, yposTop) || WorldGen.SolidOrSlopedTile(xpos + Projectile.direction, yposTop - 1)))//its 2 blocks tall so I just do 2 checks here
						{
							int yposFront = (int)((Projectile.position.Y + Projectile.height) / 16) - 1;
							int yposMid = (int)((Projectile.position.Y + Projectile.height + 9) / 16) - 1;
							//Dust.NewDustPerfect((new Vector2(xpos + (Projectile.spriteDirection), yposFront) * 16) + new Vector2(8, -92), DustType<Dusts.AirDash>(), new Vector2(0, 100), 0, Color.Blue);
							//Dust.NewDustPerfect((new Vector2(xpos, yposMid) * 16) + new Vector2(8, -92), DustType<Dusts.AirDash>(), new Vector2(0, 100), 0, Color.Green);

							Tile frontTile = Framing.GetTileSafely(xpos + Projectile.direction, yposFront);// - y - 1);
							Tile midTile = Framing.GetTileSafely(xpos, yposMid);// - y - 1);

							bool frontIsSlab = frontTile.IsHalfBlock;
							bool midIsSlab = midTile.IsHalfBlock;

							if (frontIsSlab && !midIsSlab || !frontIsSlab && midIsSlab)
								Projectile.position.Y -= 8;
							else
								Projectile.position.Y -= 16;

							Projectile.position.X += Projectile.direction;
							Projectile.velocity.X = oldVelocity.X;
						}
					}
				}
			}

			return false;
		}
	}

	public class PalestoneDummy : ModProjectile // stupid fucking bandaid fix but damage was janky before
	{
		public override string Texture => AssetDirectory.PalestoneItem + "KnightSlash";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pale Slash");
		}

		public override void SetDefaults()
		{
			Projectile.width = 50;
			Projectile.height = 50;

			Projectile.tileCollide = false;

			Projectile.friendly = true;
			Projectile.alpha = 255;

			Projectile.penetrate = 1;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;

			Projectile.timeLeft = 5;
			Projectile.DamageType = DamageClass.Summon;
		}
	}

	public class PalestoneSlash : ModProjectile
	{
		public Vector2 knockbackVelo;

		public bool hasHit;
		public float SwingDirection => Projectile.ai[0];
		public bool Down => Projectile.ai[1] == 1f;

		public Projectile parent;
		public override string Texture => AssetDirectory.PalestoneItem + "KnightSlash";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pale Slash");
			Main.projFrames[Projectile.type] = 6;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.tileCollide = false;

			Projectile.friendly = false;

			Projectile.penetrate = -1;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;

			Projectile.timeLeft = 12;
			Projectile.scale = 1.5f;
		}

		public override void AI()
		{
			if (Down)
				Projectile.rotation = MathHelper.ToRadians(90);

			if (parent != null)
			{
				Projectile.position = parent.position + Projectile.velocity + Vector2.UnitX * 12f;
				if (parent.direction == -1)
					Projectile.position = parent.position + Projectile.velocity + Vector2.UnitX * 7f;

				if (SwingDirection == 1 && !Down)
					Projectile.spriteDirection = -1;
			}
			else
			{
				Projectile.Kill();
			}

			if (++Projectile.frameCounter % 2 == 0)
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			SpriteEffects flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0f;
			if (Down && parent != null)
				flip = parent.spriteDirection == -1 ? SpriteEffects.FlipVertically : 0f;

			Main.spriteBatch.Draw(texGlow, Projectile.Center + knockbackVelo * 0.5f - Main.screenPosition, null, Color.Lerp(new Color(255, 255, 255, 0) * 0.25f, Color.Transparent, 1f - Projectile.timeLeft / 18f), Projectile.rotation, texGlow.Size() / 2f, 1.625f, flip, 0f);

			Rectangle frame = tex.Frame(verticalFrames: 6, frameY: Projectile.frame);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2f, Projectile.scale, flip, 0f);
			return false;
		}
	}

	internal class PaleKnightPlayer : ModPlayer
	{
		public int KnightCount = 0;

		public override void ResetEffects()
		{
			KnightCount = 0;
		}
	}
}