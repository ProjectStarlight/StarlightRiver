using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
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
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.damage = 7;
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
            Item.DamageType = DamageClass.Magic;
            Item.buffType = BuffType<PalestoneSummonBuff>();
			Item.shoot = ProjectileType<PaleKnight>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
            Player.AddBuff(Item.buffType, 2);

            // Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
            position = Main.MouseWorld;
            return true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.UlyssesButterfly, 25);
            recipe.AddTile(TileID.UlyssesButterflyJar);
        }
    }

    public class PaleKnight : ModProjectile
    {
		public override string Texture => AssetDirectory.PalestoneItem + "PalestoneKnight";
		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tiny Killer");
            Main.projFrames[Projectile.type] = 20;

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.Homing[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

        public sealed override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 32;

            Projectile.tileCollide = true;

            //Projectile.friendly = true;

            Projectile.minion = true;

            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;
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
			Utils.DrawBorderString(spriteBatch, Projectile.ai[1].ToString(), Projectile.position + new Vector2(0, -60) - Main.screenPosition, Color.LightCoral);
			Utils.DrawBorderString(spriteBatch, Projectile.ai[0].ToString(), Projectile.position + new Vector2(0, -40) - Main.screenPosition, Color.LightGoldenrodYellow);
			Utils.DrawBorderString(spriteBatch, Projectile.velocity.Length().ToString(), Projectile.position + new Vector2(0, -20) - Main.screenPosition, Color.LightBlue);
			if(Projectile.ai[1] >= 0)
				Utils.DrawLine(spriteBatch, Projectile.Center, Main.npc[(int)Projectile.ai[1]].Center, Color.LightBlue, Color.IndianRed, 2f);
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough)
        {
			return true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) => 
			Projectile.ai[1] = target.whoAmI;//TODO: possible desync, needs testing

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
			bool foundTarget = Projectile.ai[1] >= 0;

			// This code is required if your minion weapon has the targeting feature
			if (Player.HasMinionAttackTargetNPC)
			{
				NPC NPC = Main.npc[Player.MinionAttackTargetNPC];
				float between = Vector2.Distance(NPC.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
					Projectile.ai[1] = NPC.whoAmI;
					foundTarget = true;
				}
			}
            else if (foundTarget)
            {
				NPC NPC = Main.npc[(int)Projectile.ai[1]];
				float betweenPlayer = Vector2.Distance(NPC.Center, Player.Center);
				if (NPC.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
				}
				else
				{
					Projectile.ai[1] = -1;
					foundTarget = false;
				}
			}

            if (!foundTarget)
            {
                if (Projectile.ai[1] < -enemyCheckDelay)
                {
                    // This code is required either way, used for finding a target
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC NPC = Main.npc[i];
                        float betweenPlayer = Vector2.Distance(NPC.Center, Player.Center);
                        if (NPC.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
                        {
							float NPCBetween = Vector2.Distance(NPC.Center, Projectile.Center);
							float targetBetween = Vector2.Distance(targetCenter, Projectile.Center);
							bool closest = NPCBetween < targetBetween;

							//collision check moved into if statement for performance
							if ((closest || !foundTarget) && Collision.CanHitLine(Projectile.position, 0, 0, NPC.position, 0, 0))
                            {
                                targetCenter = NPC.Center;
                                Projectile.ai[1] = NPC.whoAmI;
                                foundTarget = true;
                            }
                        }
                    }
					if (!foundTarget)
						Projectile.ai[1] = -1;

				}
                else
                    Projectile.ai[1]--;
            }

            // friendly needs to be set to true so the minion can deal contact damage
            // friendly needs to be set to false so it doesn't damage things like target dummies while idling
            // Both things depend on if it has a target or not, so it's just one assignment here
            // You don't need this assignment if your minion is shooting things instead of dealing contact damage
            //Projectile.friendly = foundTarget;
            #endregion

            #region Movement
			Vector2 pos = Player.Center + new Vector2((((validZoneWidth + modPlayer.KnightCount * minionSpacing) + 32) * -Player.direction), 16);
			Rectangle validZone = new Rectangle((int)(pos.X - validZoneWidth), (int)(pos.Y - validZoneHeight), validZoneWidth * 2, validZoneHeight * 2);


			if ((Vector2.Distance(Projectile.position, Player.position) > MinionFlybackRange && Vector2.Distance(Projectile.Center, validZone.Center()) > MinionFlybackRange) && !foundTarget)
			{
				Projectile.tileCollide = false;
				Projectile.ai[0] = Flying;
			}
			else if (Projectile.ai[0] == Flying && !WorldGen.SolidTileAllowBottomSlope((int)(Projectile.Center.X / 16), (int)((Projectile.position.Y + Projectile.height) / 16)))
			{
				Projectile.tileCollide = true;
				Projectile.ai[0] = Walking;
			}


			//Projectile.ai[0] = 0;//debug

			switch (Projectile.ai[0])
            {
				case Walking:
                {
					if (foundTarget)
					{
						Projectile.velocity.X += Vector2.Normalize(targetCenter - Projectile.Center).X * 0.1f;
					}
					else
					{
						Projectile.velocity.X += Vector2.Normalize(validZone.Center() - Projectile.Center).X * 0.1f;

						if (validZone.Intersects(Projectile.getRect()))
                        {
							Projectile.velocity.X *= 0.90f;
							if(Projectile.velocity.Length() < 0.2f && (int)Projectile.Center.X == (int)validZone.Center().X)
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
					Projectile.velocity += Vector2.Normalize(Player.Center - Projectile.Center) * 0.5f;
					break;
				case Attacking:

					break;
			}

			Vector2 cap = new Vector2(MaxVelocity);
			Projectile.velocity = Vector2.Clamp(Projectile.velocity, -cap, cap);
			#endregion

			#region Animation and visuals

			Projectile.spriteDirection = Projectile.direction;

			int startOffset = 0;
			int frameCount = FlyFrames;
			float frameSpeed = defaultFrameSpeed;//this value is in fps

			switch (Projectile.ai[0])
            {
				case 0:
					{
						Projectile.rotation = 0;
						frameCount = RunFrames;
						frameSpeed = Math.Abs(Projectile.velocity.X * defaultRunSpeed);
					} break;
				case 1:
                    {
						Projectile.rotation = Projectile.velocity.X * tiltAmount;
						startOffset = FlyOffset;
					} break;
				case 2:
					startOffset = AttackOffset; break;
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

			modPlayer.KnightCount++;
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
					if (Projectile.ai[0] == Walking)
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

							if ((frontIsSlab && !midIsSlab) || (!frontIsSlab && midIsSlab))
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

	internal class PaleKnightPlayer : ModPlayer
	{
		public int KnightCount = 0;
		public override void ResetEffects() =>
			KnightCount = 0;
	}
}