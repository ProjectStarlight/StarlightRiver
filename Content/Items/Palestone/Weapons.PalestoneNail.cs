using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Palestone
{
	public class PalestoneNail : ModItem
    {
        public override string Texture => AssetDirectory.PalestoneItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Palenail");
            Tooltip.SetDefault("Summons the little man.");
            ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true; // This lets the player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.damage = 7;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 0, 12, 0);
            item.rare = ItemRarityID.White;
            item.UseSound = SoundID.Item44;

            item.noMelee = true;
            item.summon = true;
            item.buffType = BuffType<PalestoneSummonBuff>();
			item.shoot = ProjectileType<PaleKnight>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            player.AddBuff(item.buffType, 2);

            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
            position = Main.MouseWorld;
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UlyssesButterfly, 25);
            recipe.AddTile(TileID.UlyssesButterflyJar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class PaleKnight : ModProjectile
    {
		public override string Texture => AssetDirectory.PalestoneItem + "PalestoneKnight";
		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tiny Killer");
            Main.projFrames[projectile.type] = 20;

			Main.projPet[projectile.type] = true; // Denotes that this projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.Homing[projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

        public sealed override void SetDefaults()
        {
            projectile.width = 28;
            projectile.height = 32;

            projectile.tileCollide = true;

            //projectile.friendly = true;

            projectile.minion = true;

            projectile.minionSlots = 1f;

            projectile.penetrate = -1;
        }

		public override bool? CanCutTiles() => false;
		//public override bool MinionContactDamage() => true;	// This is mandatory if your minion deals contact damage (further related stuff in AI() in the Movement region)

		public const int RunFrames = 8;

		public const int FlyFrames = 6;
		public const int FlyOffset = RunFrames;

		public const int AttackFrames = 6;
		public const int AttackOffset = FlyFrames + RunFrames;

		private const int Walking = 0;
		private const int Flying = 1;
		private const int Attacking = 2;

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Utils.DrawBorderString(spriteBatch, projectile.ai[1].ToString(), projectile.position + new Vector2(0, -60) - Main.screenPosition, Color.LightCoral);
			Utils.DrawBorderString(spriteBatch, projectile.ai[0].ToString(), projectile.position + new Vector2(0, -40) - Main.screenPosition, Color.LightGoldenrodYellow);
			Utils.DrawBorderString(spriteBatch, projectile.velocity.Length().ToString(), projectile.position + new Vector2(0, -20) - Main.screenPosition, Color.LightBlue);
			if(projectile.ai[1] >= 0)
				Utils.DrawLine(spriteBatch, projectile.Center, Main.npc[(int)projectile.ai[1]].Center, Color.LightBlue, Color.IndianRed, 2f);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
			return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => 
			projectile.ai[1] = target.whoAmI;//TODO: possible desync, needs testing

		public const float defaultFrameSpeed = 10f;
		public const float defaultRunSpeed = 8f;
		public const float tiltAmount = 0.02f;

		public const int validZoneWidth = 8;
		public const int validZoneHeight = 24;
		public const int minionSpacing = 24;

		public const int enemyCheckDelay = 30;
		public const int maxMinionChaseRange = 2000;

		public const int MaxVelocity = 7;

		public const int MinionFlybackRange = 450;

		public override void AI()
		{
			Player player = Main.player[projectile.owner];
			PaleKnightPlayer modPlayer = player.GetModPlayer<PaleKnightPlayer>();

			#region Active check
			if (player.dead || !player.active) // This is the "active check", makes sure the minion is alive while the player is alive, and despawns if not
				player.ClearBuff(BuffType<PalestoneSummonBuff>());
			if (player.HasBuff(BuffType<PalestoneSummonBuff>()))
				projectile.timeLeft = 2;
			#endregion

			#region Find target
			// Starting search distance
			Vector2 targetCenter = projectile.Center;
			bool foundTarget = projectile.ai[1] >= 0;

			// This code is required if your minion weapon has the targeting feature
			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < maxMinionChaseRange)
				{
					targetCenter = npc.Center;
					projectile.ai[1] = npc.whoAmI;
					foundTarget = true;
				}
			}
            else if (foundTarget)
            {
				NPC npc = Main.npc[(int)projectile.ai[1]];
				float betweenPlayer = Vector2.Distance(npc.Center, player.Center);
				if (npc.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
				{
					targetCenter = npc.Center;
				}
				else
				{
					projectile.ai[1] = -1;
					foundTarget = false;
				}
			}

            if (!foundTarget)
            {
                if (projectile.ai[1] < -enemyCheckDelay)
                {
                    // This code is required either way, used for finding a target
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        float betweenPlayer = Vector2.Distance(npc.Center, player.Center);
                        if (npc.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
                        {
							float npcBetween = Vector2.Distance(npc.Center, projectile.Center);
							float targetBetween = Vector2.Distance(targetCenter, projectile.Center);
							bool closest = npcBetween < targetBetween;

							//collision check moved into if statement for performance
							if ((closest || !foundTarget) && Collision.CanHitLine(projectile.position, 0, 0, npc.position, 0, 0))
                            {
                                targetCenter = npc.Center;
                                projectile.ai[1] = npc.whoAmI;
                                foundTarget = true;
                            }
                        }
                    }
					if (!foundTarget)
						projectile.ai[1] = -1;

				}
                else
                    projectile.ai[1]--;
            }

            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            //projectile.friendly = foundTarget;
            #endregion

            #region Movement
			Vector2 pos = player.Center + new Vector2((((validZoneWidth + modPlayer.KnightCount * minionSpacing) + 32) * -player.direction), 16);
			Rectangle validZone = new Rectangle((int)(pos.X - validZoneWidth), (int)(pos.Y - validZoneHeight), validZoneWidth * 2, validZoneHeight * 2);


			if ((Vector2.Distance(projectile.position, player.position) > MinionFlybackRange && Vector2.Distance(projectile.Center, validZone.Center()) > MinionFlybackRange) && !foundTarget)
			{
				projectile.tileCollide = false;
				projectile.ai[0] = Flying;
			}
			else if (projectile.ai[0] == Flying && !WorldGen.SolidTileAllowBottomSlope((int)(projectile.Center.X / 16), (int)((projectile.position.Y + projectile.height) / 16)))
			{
				projectile.tileCollide = true;
				projectile.ai[0] = Walking;
			}


			//projectile.ai[0] = 0;//debug

			switch (projectile.ai[0])
            {
				case Walking:
                {
					if (foundTarget)
					{
						projectile.velocity.X += Vector2.Normalize(targetCenter - projectile.Center).X * 0.1f;
					}
					else
					{
						projectile.velocity.X += Vector2.Normalize(validZone.Center() - projectile.Center).X * 0.1f;

						if (validZone.Intersects(projectile.getRect()))
                        {
							projectile.velocity.X *= 0.90f;
							if(projectile.velocity.Length() < 0.2f && (int)projectile.Center.X == (int)validZone.Center().X)
                            {
								projectile.velocity = Vector2.Zero;
								projectile.direction = player.direction;
                            }
						}

                        //!debug
                        //for (int i = 0; i < 4; i++)
                        //    Dust.NewDustPerfect(validZone.TopLeft() + new Vector2(Main.rand.Next(validZone.Width), Main.rand.Next(validZone.Height)), DustType<Dusts.AirDash>(), Vector2.Zero, 0, Color.Blue);
                        //Dust.NewDustPerfect(validZone.Center(), DustType<Dusts.AirDash>(), Vector2.Zero, 0, Color.Red);
                    }
					if (projectile.velocity.Length() < 0.1f)
					{
						projectile.frame = 0;
						projectile.frameCounter = 0;
					}

					projectile.velocity.Y += 0.35f;
				}
					break;
				case Flying:
					projectile.velocity += Vector2.Normalize(player.Center - projectile.Center) * 0.5f;
					break;
				case Attacking:

					break;
			}

			Vector2 cap = new Vector2(MaxVelocity);
			projectile.velocity = Vector2.Clamp(projectile.velocity, -cap, cap);
			#endregion

			#region Animation and visuals

			projectile.spriteDirection = projectile.direction;

			int startOffset = 0;
			int frameCount = FlyFrames;
			float frameSpeed = defaultFrameSpeed;//this value is in fps

			switch (projectile.ai[0])
            {
				case 0:
					{
						projectile.rotation = 0;
						frameCount = RunFrames;
						frameSpeed = Math.Abs(projectile.velocity.X * defaultRunSpeed);
					} break;
				case 1:
                    {
						projectile.rotation = projectile.velocity.X * tiltAmount;
						startOffset = FlyOffset;
					} break;
				case 2:
					startOffset = AttackOffset; break;
			}

			projectile.frameCounter++;
			if ((int)(projectile.frameCounter * frameSpeed) >= 60)
			{
				projectile.frameCounter = 0;
				projectile.frame++;
			}
			if (projectile.frame >= startOffset + frameCount || projectile.frame < startOffset)
				projectile.frame = startOffset;

			//Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.78f);
			projectile.direction = projectile.velocity.X > 0 ? 1 : -1;//this is here since I believe this gets synced, if it doesn't this line can be moved above where the sprite direction is set
			#endregion

			modPlayer.KnightCount++;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (projectile.velocity.Y != oldVelocity.Y) 
			{
				//if(extra tile(s))
				//	jump
				//else if(projectile.velocity.X != oldVelocity.X)...

				if (projectile.velocity.X != oldVelocity.X)//hit wall
				{
					if (projectile.ai[0] == Walking)
					{
						int xpos = (int)((projectile.Center.X + oldVelocity.X) / 16);
						int yposTop = (int)((projectile.position.Y + projectile.height + 12) / 16) - 2;
						//Dust.NewDustPerfect((new Vector2(xpos + projectile.direction, yposTop) * 16) + new Vector2(8, -92), DustType<Dusts.AirDash>(), new Vector2(0, 100), 0, Color.Red);

						if (!(WorldGen.SolidOrSlopedTile(xpos + projectile.direction, yposTop) || WorldGen.SolidOrSlopedTile(xpos + projectile.direction, yposTop - 1)))//its 2 blocks tall so I just do 2 checks here
						{
							int yposFront = (int)((projectile.position.Y + projectile.height) / 16) - 1;
							int yposMid = (int)((projectile.position.Y + projectile.height + 9) / 16) - 1;
							//Dust.NewDustPerfect((new Vector2(xpos + (projectile.spriteDirection), yposFront) * 16) + new Vector2(8, -92), DustType<Dusts.AirDash>(), new Vector2(0, 100), 0, Color.Blue);
							//Dust.NewDustPerfect((new Vector2(xpos, yposMid) * 16) + new Vector2(8, -92), DustType<Dusts.AirDash>(), new Vector2(0, 100), 0, Color.Green);


							Tile frontTile = Framing.GetTileSafely(xpos + projectile.direction, yposFront);// - y - 1);
							Tile midTile = Framing.GetTileSafely(xpos, yposMid);// - y - 1);

							bool frontIsSlab = frontTile.halfBrick();
							bool midIsSlab = midTile.halfBrick();

							if ((frontIsSlab && !midIsSlab) || (!frontIsSlab && midIsSlab))
								projectile.position.Y -= 8;
							else
								projectile.position.Y -= 16;

							projectile.position.X += projectile.direction;
							projectile.velocity.X = oldVelocity.X;
						}
					}
                }
            }
            return false;
		}
	}

	internal class PaleKnightPlayer : ModPlayer
	{
		public int KnightCount = 0;
		public override void ResetEffects() =>
			KnightCount = 0;
	}
}