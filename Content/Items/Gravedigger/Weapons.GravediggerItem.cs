using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Gravedigger
{
	public class GravediggerItem : ModItem
	{
		public override string Texture => AssetDirectory.GravediggerItem + "GravediggerItem";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tombsmasher");
			Tooltip.SetDefault("Strikes enemies up into the air when holding W and slams them down with S \nHit enemies in the air for more damage");
		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6.5f;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.crit = 4;
			Item.rare = ItemRarityID.Green;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = ModContent.ProjectileType<GravediggerSwing>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
		}

		public override bool CanUseItem(Player Player)
		{
			return Player.GetModPlayer<GravediggerPlayer>().SwingDelay == 0;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddIngredient(ModContent.ItemType<LivingBlood>(), 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class GravediggerSwing : ModProjectile
	{
		private const int FRAMEWIDTH = 128;
		private const int FRAMEHEIGHT = 138;
		private const int COLUMNONEFRAMES = 4;
		private const int COLUMNTWOFRAMES = 2;
		private const int COLUMNTHREEFRAMES = 3;
		private const int COLUMNFOURFRAMES = 4;

		private bool shouldDoSFX = true;

		private int frameX = 0;
		private int SlashWindow = 0;
		private Vector2 direction = Vector2.Zero;
		private Vector2 directionTwo = Vector2.Zero;
		public override string Texture => AssetDirectory.GravediggerItem + "GravediggerSwing";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grave digger");
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(128, 138);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.ownerHitCheck = true;
		}

		Player Owner => Main.player[Projectile.owner];

		private int SwingFrame
		{
			get => frameX / FRAMEWIDTH;
			set => frameX = value * FRAMEWIDTH;
		}

		private Vector2 SwingOrigin => SwingFrame switch
		{
			0 => new Vector2(FRAMEWIDTH * 0.5f, FRAMEHEIGHT * 0.5f),
			1 => new Vector2(FRAMEWIDTH * 0.33f, FRAMEHEIGHT * 0.4f),
			2 => new Vector2(FRAMEWIDTH * 0.5f, FRAMEHEIGHT * 0.5f),
			3 => new Vector2(FRAMEWIDTH * 0.35f, FRAMEHEIGHT * 0.6f),
			_ => Vector2.Zero,
		};

		public override void OnSpawn(IEntitySource source)
		{
			Reinitialize();
		}

		/// <summary>
		/// resets the projectile to an initial state using the players mouse / controls to determine, and sets a netupdate to be sent
		/// </summary>
		private void Reinitialize()
		{
			directionTwo = Main.MouseWorld - Owner.MountedCenter;
			directionTwo.Normalize();
			Owner.ChangeDir(Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1);
			Projectile.frame = 0;
			Projectile.frameCounter = 0;
			SlashWindow = 30;

			if (Owner.controlUp)
			{
				direction = new Vector2(Owner.direction, -1);
				SwingFrame = 2;
			}
			else if (Owner.controlDown)
			{
				Owner.GetModPlayer<GravediggerPlayer>().Combo = 0;
				direction = new Vector2(Owner.direction, -1);
				SwingFrame = 3;
			}
			else
			{
				Owner.GetModPlayer<GravediggerPlayer>().Combo++;
				SwingFrame = Owner.GetModPlayer<GravediggerPlayer>().SwingFrame == 1 ? 0 : 1;
				if (SwingFrame == 0)
				{
					direction = new Vector2(Owner.direction, 1);
				}
				else
				{
					direction = new Vector2(Owner.direction, -1);
				}
			}

			direction.Normalize();
			Owner.GetModPlayer<GravediggerPlayer>().SwingFrame = SwingFrame;

			shouldDoSFX = true;

			Projectile.netUpdate = true;
		}

		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			if (shouldDoSFX)
			{
				shouldDoSFX = false;

				Helper.PlayPitched("Effects/HeavyWhooshShort", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), Projectile.Center);
			}

			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;
			Owner.GetModPlayer<GravediggerPlayer>().SwingDelay = 2;
			Vector2 frameOrigin;
			if (Owner.direction < 0)
				frameOrigin = new Vector2(FRAMEWIDTH, FRAMEHEIGHT) - SwingOrigin;
			else
				frameOrigin = SwingOrigin;

			Projectile.position = Owner.MountedCenter - frameOrigin;
			if (SwingFrame < 2)
			{
				if (!CheckFrameDeath())
				{
					if (SwingFrame == 0 && Projectile.frame < 3)
						direction = direction.RotatedBy(Owner.direction * 0.3f);
					else if (SwingFrame == 1)
						direction = direction.RotatedBy(Owner.direction * -0.45f);
				}

				Owner.TryGetModPlayer(out ControlsPlayer controlsPlayer);
				controlsPlayer.mouseRotationListener = true;

				Owner.ChangeDir(controlsPlayer.mouseWorld.X > Owner.MountedCenter.X ? 1 : -1);
				Projectile.position += directionTwo * 10;
				Owner.itemRotation = MathHelper.WrapAngle(direction.ToRotation() - ((Owner.direction < 0) ? 0 : MathHelper.Pi));
				Projectile.rotation = MathHelper.WrapAngle(Owner.AngleFrom(Projectile.position + frameOrigin) - ((Owner.direction < 0) ? 0 : MathHelper.Pi));
			}
			else
			{
				if (!CheckFrameDeath())
				{
					if (SwingFrame == 2) //swing UP
						direction = direction.RotatedBy(Owner.direction * -0.3f);
					else if (SwingFrame == 3) //swing DOWN
						direction = direction.RotatedBy(Owner.direction * -0.2f);
				}

				Owner.itemRotation = MathHelper.WrapAngle(direction.ToRotation() - ((Owner.direction < 0) ? 0 : MathHelper.Pi));
				Projectile.rotation = 0;
			}

			#region hardcoding
			if (SwingFrame == 3 && Owner.direction < 0)
				Projectile.position -= new Vector2(5, 20);

			if (SwingFrame == 2)
				Projectile.position.X += 14 * Owner.direction;

			if (SwingFrame == 1 && Owner.direction < 0)
				Projectile.position.Y += 12;
			#endregion

			if (!CheckFrameDeath())
				Projectile.frameCounter++;

			if (Projectile.frameCounter > 4)
			{
				Projectile.frame++;
				Projectile.frameCounter = 0;
			}

			if (CheckFrameDeath())
			{
				SlashWindow--;
				if (Owner == Main.LocalPlayer)
				{
					if (Main.mouseLeft && SlashWindow < 20)
					{
						Reinitialize();
					}
					else if (SlashWindow < 0)
					{
						Owner.GetModPlayer<GravediggerPlayer>().Combo = 0;
						Projectile.active = false;
					}
				}
			}
		}

		private bool CheckFrameDeath()
		{
			int deathFrame = 0;
			switch (SwingFrame)
			{
				case 0:
					deathFrame = COLUMNONEFRAMES;
					break;
				case 1:
					deathFrame = COLUMNTWOFRAMES;
					break;
				case 2:
					deathFrame = COLUMNTHREEFRAMES;
					break;
				case 3:
					deathFrame = COLUMNFOURFRAMES;
					break;
			}

			return Projectile.frame >= deathFrame;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return Color.Lerp(lightColor, Color.White, 0.2f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			var frame = new Rectangle(frameX, Projectile.frame * FRAMEHEIGHT, FRAMEWIDTH, FRAMEHEIGHT);
			SpriteEffects effects;

			Vector2 frameOrigin = SwingOrigin;
			if (Owner.direction < 0)
				frameOrigin.X = FRAMEWIDTH - SwingOrigin.X;

			if (Owner.direction < 0)
				effects = SpriteEffects.None;
			else
				effects = SpriteEffects.FlipHorizontally;
			Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition + frameOrigin, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frameOrigin, Projectile.scale, effects, 0);
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.frame > 0 || target.immune[Projectile.owner] > 0)
				return false;
			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Main.player[Projectile.owner].TryGetModPlayer(out StarlightPlayer sp);
			sp.SetHitPacketStatus(true);

			if (Main.myPlayer == Owner.whoAmI)
				CameraSystem.shake += 3;

			Helper.PlayPitched("Impacts/GoreLight", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f), target.Center);

			if (target.knockBackResist != 0)
			{
				switch (SwingFrame)
				{
					case 0:
						if (!target.noGravity && !target.collideY)
							target.velocity.Y = -3;
						break;
					case 1:
						if (!target.noGravity && !target.collideY)
							target.velocity.Y = -3;
						break;
					case 2:
						if (!target.noGravity)
						{
							target.AddBuff(ModContent.BuffType<ShovelSlowFall>(), 120, true);
							target.velocity.Y = -8f;
						}

						break;
					case 3:
						if (!target.collideY && !target.noGravity)
						{
							target.AddBuff(ModContent.BuffType<ShovelQuickFall>(), 60, true);
							target.GetGlobalNPC<GravediggerNPC>().SlamPlayer = Owner;
						}

						break;
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (target.knockBackResist != 0 && !target.collideY && !target.noGravity && SwingFrame < 2 && target.HasBuff(ModContent.BuffType<ShovelSlowFall>()))
				modifiers.FinalDamage *= 1.5f;

			if (SwingFrame < 2)
			{
				modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Owner.Center.X);
				if (target.HasBuff(ModContent.BuffType<ShovelSlowFall>()))
					modifiers.Knockback *= 0.3f;
			}
			else
			{
				modifiers.HitDirectionOverride = 0;
			}

			if (SwingFrame == 3 && !target.noGravity && target.knockBackResist != 0 && !target.collideY)
			{
				target.velocity.X = Main.player[Projectile.owner].direction * 20;
				modifiers.Knockback *= 0;
			}

			CreateBlood(target, modifiers.HitDirection, modifiers.Knockback.Base);
		}

		private void CreateBlood(NPC target, int hitDirection, float knockback)
		{
			Vector2 direction = Vector2.Zero;
			float variance = 0.5f;

			switch (SwingFrame)
			{
				case 0:
					direction = new Vector2(Math.Sign(hitDirection - 0.5f), 0);
					break;
				case 1:
					direction = new Vector2(Math.Sign(hitDirection - 0.5f), 0);
					break;
				case 2:
					direction = new Vector2(0, -1);
					variance = 0.35f;
					break;
				case 3:
					direction = new Vector2(0, 1);
					break;
			}

			for (int i = 0; i < 15; i++)
			{
				Vector2 dustDirection = direction.RotatedBy(Main.rand.NextFloat(0 - variance, variance));
				dustDirection *= Main.rand.NextFloat(0.5f, 4f + knockback);
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.BloodMetaballDust>(), dustDirection, 0, default, 0.2f);

				Vector2 dustDirection2 = direction.RotatedBy(Main.rand.NextFloat(0 - variance, variance));
				dustDirection2 *= Main.rand.NextFloat(0.5f, 4f + knockback);
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.BloodMetaballDustLight>(), dustDirection2, 0, default, 0.2f);
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteVector2(directionTwo);
			writer.WriteVector2(direction);
			writer.Write(Projectile.frame);
			writer.Write(Projectile.frameCounter);
			writer.Write(SlashWindow);
			writer.Write(SwingFrame);
			writer.Write(shouldDoSFX);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			directionTwo = reader.ReadVector2();
			direction = reader.ReadVector2();
			Projectile.frame = reader.ReadInt32();
			Projectile.frameCounter = reader.ReadInt32();
			SlashWindow = reader.ReadInt32();
			SwingFrame = reader.ReadInt32();
			Owner.GetModPlayer<GravediggerPlayer>().SwingFrame = SwingFrame;
			shouldDoSFX = reader.ReadBoolean();

		}
	}

	public class GravediggerSlam : ModProjectile, IDrawOverTiles
	{
		float counter = 0;

		public override string Texture => AssetDirectory.GravediggerItem + "GraveDiggerSlam";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Grave digger");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(128, 128);
			Projectile.penetrate = -1;
			Projectile.timeLeft = 200;
			Projectile.rotation = Main.rand.NextFloat(6.28f);
		}

		public override void AI()
		{
			if (Projectile.timeLeft < 195)
				Projectile.friendly = false;
			counter += (float)(Math.PI / 2f) / 200;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawOverTiles(SpriteBatch spriteBatch)
		{
			Color color = Color.White;
			color *= (float)Math.Cos(counter);
			spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, TextureAssets.Projectile[Projectile.type].Value.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
		}
	}

	internal class GravediggerPlayer : ModPlayer
	{
		public int SwingDelay = 0;
		public int SwingFrame = 0;
		public int Combo = 0;

		public override void ResetEffects()
		{
			SwingDelay = Math.Max(SwingDelay - 1, 0);
		}
	}

	public class GravediggerNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public Player SlamPlayer;
	}

	class ShovelSlowFall : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public ShovelSlowFall() : base("Slow fall", "Falling Speed Reduced", false) { }

		public override void Update(NPC NPC, ref int buffIndex)
		{
			if (NPC.velocity.Y > 0.5f && !NPC.HasBuff(ModContent.BuffType<ShovelQuickFall>()))
				NPC.velocity.Y -= 0.1f;
		}
	}

	class ShovelQuickFall : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public ShovelQuickFall() : base("Quick fall", "You slammin", false) { }

		public override void Update(NPC npc, ref int buffIndex)
		{
			npc.velocity.X *= 0.85f;
			npc.velocity.Y = 40;

			if (npc.collideY)
			{
				npc.velocity.X = 0;
				Player Player = Main.player[npc.target];
				npc.DelBuff(buffIndex--);
				CameraSystem.shake += 10;

				for (int k = 0; k <= 50; k++)
				{
					Dust.NewDustPerfect(npc.Center, ModContent.DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-10, 10));
				}

				Vector2 pos = npc.Center + new Vector2(0, npc.height / 2);
				int damage = (int)(30 * npc.GetGlobalNPC<GravediggerNPC>().SlamPlayer.GetDamage(DamageClass.Melee).Multiplicative);

				Projectile.NewProjectile(new EntitySource_Buff(npc, Type, buffIndex), pos, Vector2.Zero, ModContent.ProjectileType<GravediggerSlam>(), damage, 0, npc.GetGlobalNPC<GravediggerNPC>().SlamPlayer.whoAmI);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item70, npc.Center);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit42, npc.Center);
			}
		}
	}
}