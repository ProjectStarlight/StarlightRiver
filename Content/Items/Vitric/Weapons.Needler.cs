using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Humanizer.In;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static tModPorter.ProgressUpdate;

namespace StarlightRiver.Content.Items.Vitric
{
	public class Needler : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needler");
			Tooltip.SetDefault("Stick spikes to enemies to build up heat \nOverheated enemies explode, dealing massive damage");
		}

		//TODO: Adjust rarity sellprice and balance
		public override void SetDefaults()
		{
			Item.damage = 8;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Orange;
			Item.shoot = ModContent.ProjectileType<NeedlerHoldout>();
			Item.shootSpeed = 14f;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noUseGraphic = true;

			Item.value = Item.sellPrice(gold: 2, silver: 75);
		}

		//TODO: Add glowmask to weapon
		//TODO: Add holdoffset
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

			return false;
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ModContent.ProjectileType<NeedlerHoldout>()] <= 0;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<SandstoneChunk>(5);
			recipe.AddIngredient<VitricOre>(10);
			recipe.AddIngredient<MagmaCore>(2);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
	
	public class NeedlerHoldout : ModProjectile
	{
		public bool updateVelocity = true;
		public ref float Timer => ref Projectile.ai[0];	
		public ref float UseTime => ref Projectile.ai[1];
		public bool CanHold => Owner.channel && !Owner.CCed && !Owner.noItems;
		public Vector2 ArmPosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true) + new Vector2(16f, -4f * Owner.direction).RotatedBy(Projectile.velocity.ToRotation());
		public Vector2 BarrelPosition => ArmPosition + Projectile.velocity * Projectile.width * 0.5f + new Vector2(-14f * Owner.direction, -5f).RotatedBy(Projectile.rotation);
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.VitricItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needler");

			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.width = 50;
			Projectile.height = 44;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void AI()
		{
			if (!CanHold)
				Projectile.Kill();

			if (Timer == 0f)
				UseTime = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);

			Timer++;

			float spinUpTime = (int)(UseTime * MathHelper.Lerp(2.5f, 1f, Timer < 75f ? Timer / 75f : 1f)); // the time between shots / time between the sprite frame changes is greater when first starting firing

			if (++Projectile.frameCounter % spinUpTime == 0)
			Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];


			if ((int)Timer % spinUpTime == 0)
			{
				Shoot();
			}

			UpdateHeldProjectile();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Timer <= 2)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_Blur").Value;

			Texture2D itemTexGlow = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + "Needler_Glow").Value;
			Texture2D itemTexBlur = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + "Needler_Blur").Value;

			Rectangle frame = tex.Frame(verticalFrames: 3, frameY: Projectile.frame);

			Rectangle glowFrame = texGlow.Frame(verticalFrames: 3, frameY: Projectile.frame);

			SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Vector2 position = Projectile.Center + Vector2.Lerp(Vector2.Zero, new Vector2(-4f * Owner.direction, 0f), EaseBuilder.EaseCircularIn.Ease(Timer < 75f ? Timer / 75f : 1f)).RotatedBy(Projectile.rotation) - Main.screenPosition;

			Main.spriteBatch.Draw(tex, position, frame, lightColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Effect effect = Filters.Scene["DistortSprite"].GetShader().Shader;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.035f);
			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.0035f);
			effect.Parameters["screenPos"].SetValue(Main.screenPosition * new Vector2(0.5f, 0.1f) / new Vector2(Main.screenWidth, Main.screenHeight));

			effect.Parameters["offset"].SetValue(new Vector2(0.001f));
			effect.Parameters["repeats"].SetValue(1);
			effect.Parameters["uImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);
			effect.Parameters["uImage2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.VitricBoss + "LaserBallDistort").Value);

			Color color = new Color(255, 50, 20, 0) * (Timer < 35f ? Timer / 35f : 1f) * 0.55f;

			effect.Parameters["uColor"].SetValue(color.ToVector4());
			effect.Parameters["noiseImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(itemTexGlow, position, null, Color.White, Projectile.rotation, itemTexGlow.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			
			return false;
		}

		/// <summary>
		/// Called when the held projectile should shoot its projectile (in this case, Needle)
		/// </summary>
		private void Shoot()
		{
			Helper.PlayPitched("Guns/SMG2", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), BarrelPosition, Projectile.velocity.RotatedByRandom(0.15f) * Owner.HeldItem.shootSpeed, ModContent.ProjectileType<NeedlerProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

			updateVelocity = true;
		}

		/// <summary>
		/// Updates the basic variables needed for a held projectile
		/// </summary>
		private void UpdateHeldProjectile()
		{
			Owner.ChangeDir(Projectile.direction);
			Owner.heldProj = Projectile.whoAmI;
			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Projectile.timeLeft = 2;
			Projectile.rotation = Utils.ToRotation(Projectile.velocity);
			Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - (Projectile.direction == 1 ? MathHelper.ToRadians(70f) : MathHelper.ToRadians(110f)));

			if (Projectile.spriteDirection == -1)
				Projectile.rotation += 3.1415927f;

			Projectile.position = ArmPosition - Projectile.Size * 0.5f;

			if (Main.myPlayer == Projectile.owner && updateVelocity)
			{
				updateVelocity = false;

				float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

				Vector2 oldVelocity = Projectile.velocity;

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(Main.MouseWorld), interpolant).RotatedByRandom(0.15f);
				if (Projectile.velocity != oldVelocity)
				{
					Projectile.netSpam = 0;
					Projectile.netUpdate = true;
				}
			}

			Projectile.spriteDirection = Projectile.direction;
		}

		public override bool? CanDamage()
		{
			return false;
		}
	}

	public class NeedlerProj : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private int stuckTimer;

		int enemyID;
		bool stuck = false;
		Vector2 offset = Vector2.Zero;

		float needleLerp = 0f;

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 8;
			Projectile.timeLeft = 1200;
		}

		private void findIfHit()
		{
			foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
			{
				OnHitNPC(NPC, new NPC.HitInfo() { Damage = 0 }, 0);
			}
		}

		//TODO: Move methods to top + method breaks
		//TODO: Turn needles into getnset
		public override bool PreAI()
		{
			if (stuck)
			{
				stuckTimer++;

				NPC target = Main.npc[enemyID];
				int needles = target.GetGlobalNPC<NeedlerNPC>().needles;

				if (Main.rand.NextBool(Math.Max((10 - needles) * 30 + 300, 50)))
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.4f, 0.8f));

				if (target.GetGlobalNPC<NeedlerNPC>().needleTimer == 1)
				{
					//Projectile.NewProjectile(Projectile.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), Projectile.damage * 3, Projectile.knockBack, Projectile.owner);
					Projectile.active = false;
				}

				if (needles > needleLerp)
				{
					if (needleLerp < 10)
					{
						needleLerp += 0.2f;

						if (needles > needleLerp + 3)
							needleLerp += 0.4f;
					}
				}
				else
				{
					needleLerp = needles;
				}

				var lightColor = Color.Lerp(Color.Orange, Color.Red, needleLerp / 20f);
				Lighting.AddLight(Projectile.Center, lightColor.R * needleLerp / 2000f, lightColor.G * needleLerp / 2000f, lightColor.B * needleLerp / 2000f);

				if (!target.active)
				{
					if (Projectile.timeLeft > 5)
						Projectile.timeLeft = 5;

					Projectile.velocity = Vector2.Zero;
				}
				else
				{
					Projectile.position = target.position + offset;
				}

				if (Projectile.timeLeft == 2)
					target.GetGlobalNPC<NeedlerNPC>().needles--;

				return false;
			}
			else
			{
				Projectile.rotation = Projectile.velocity.ToRotation();
			}

			return true;
		}

		public override void AI()
		{
			if (Projectile.timeLeft < 1185)
			{
				if (Projectile.velocity.Y < 20f)
				{
					Projectile.velocity.Y += 0.45f;

					if (Projectile.velocity.Y > 0)
					{
						if (Projectile.velocity.Y < 12f)
							Projectile.velocity.Y *= 1.040f;
						else
							Projectile.velocity.Y *= 1.020f;
					}
				}
			}

			if (Main.rand.NextBool(15) && Projectile.timeLeft < 1195)
			{
				Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);

				Dust.NewDustPerfect(pos, ModContent.DustType<Glow>(), Vector2.Zero, 0, new Color(255, 50, 20), 0.5f);
				Dust.NewDustPerfect(pos, ModContent.DustType<Glow>(), Vector2.Zero, 0, new Color(255, 255, 255), 0.15f);
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void PostAI()
		{
			if (Main.myPlayer != Projectile.owner && !stuck)
				findIfHit();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!stuck && target.life > 0)
			{
				Projectile.penetrate++;
				target.GetGlobalNPC<NeedlerNPC>().needles++;
				target.GetGlobalNPC<NeedlerNPC>().needleDamage = Projectile.damage;
				target.GetGlobalNPC<NeedlerNPC>().needlePlayer = Projectile.owner;
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = Projectile.position - target.position;
				offset -= Projectile.velocity;
				Projectile.timeLeft = 400;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (stuckTimer <= 10)
				DrawPrimitives();

			SpriteBatch sb = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			sb.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 20, 0) * 0.55f, 0f, bloomTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);

			return false;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 10; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			cache.Add(Projectile.Center + Projectile.velocity);

			while (cache.Count > 10)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(190), factor => factor * 4.5f, factor =>
			{
				return Color.Lerp(new Color(255, 50, 20), new Color(35, 70, 120), EaseBuilder.EaseQuarticOut.Ease(1f - factor.X)) * (1f - stuckTimer / 10f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(190), factor => factor * 4f, factor =>
			{
				return Color.Lerp(new Color(255, 150, 20), new Color(50, 100, 170), EaseBuilder.EaseQuarticOut.Ease(1f - factor.X)) * (1f - stuckTimer / 10f);
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

				Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
				Matrix view = Main.GameViewMatrix.ZoomMatrix;
				Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

				effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
				effect.Parameters["repeats"].SetValue(1f);
				effect.Parameters["transformMatrix"].SetValue(world * view * projection);
				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value);
				trail?.Render(effect);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

				trail?.Render(effect);
				trail2?.Render(effect);
			});
		}
	}

	public class NeedlerExplosion : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 200;
			Projectile.timeLeft = 20;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			for (int i = 0; i < 2; i++)
				Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + Main.rand.NextVector2Circular(25, 25), Main.rand.NextFloat(3.14f, 6.28f).ToRotationVector2() * 7, Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.4f, 0.8f));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.SetCrit();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 180);

			if (target.GetGlobalNPC<NeedlerNPC>().needles >= 1 && target.GetGlobalNPC<NeedlerNPC>().needleTimer <= 0)
				target.GetGlobalNPC<NeedlerNPC>().needleTimer = 60;
		}
	}

	public class NeedlerEmber : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.aiStyle = 1;
			Projectile.width = Projectile.height = 12;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.extraUpdates = 1;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Projectile.scale *= 0.98f;

			if (Main.rand.NextBool(2))
			{
				var dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<NeedlerDustFastFade>(), Main.rand.NextVector2Circular(1.5f, 1.5f));
				dust.scale = 0.6f * Projectile.scale;
				dust.rotation = Main.rand.NextFloatDirection();
			}
		}
	}

	public class NeedlerNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int needles = 0;
		public int needleTimer = 0;
		public int needleDamage = 0;
		public int needlePlayer = 0;

		public override void ResetEffects(NPC NPC)
		{
			needleTimer--;
			base.ResetEffects(NPC);
		}
		public override void AI(NPC NPC)
		{
			if (needles >= 8 && needleTimer <= 0)
			{
				needles++;
				needleTimer = 60;
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireCast"), NPC.Center);
			}

			if (needleTimer == 1)
			{
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), NPC.Center);

				if (needlePlayer == Main.myPlayer)
				{
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), (int)(needleDamage * Math.Sqrt(needles)), 0, needlePlayer);

					for (int i = 0; i < 5; i++)
					{
						Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, needlePlayer).scale = Main.rand.NextFloat(0.85f, 1.15f);
					}

					CameraSystem.shake = 20;
				}

				for (int i = 0; i < 10; i++)
				{
					var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<NeedlerDust>());
					dust.velocity = Main.rand.NextVector2Circular(10, 10);
					dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
					dust.alpha = 70 + Main.rand.Next(60);
					dust.rotation = Main.rand.NextFloat(6.28f);
				}

				for (int i = 0; i < 10; i++)
				{
					var dust = Dust.NewDustDirect(NPC.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<NeedlerDustSlowFade>());
					dust.velocity = Main.rand.NextVector2Circular(10, 10);
					dust.scale = Main.rand.NextFloat(1.5f, 1.9f);
					dust.alpha = Main.rand.Next(80) + 40;
					dust.rotation = Main.rand.NextFloat(6.28f);

					Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<NeedlerDustGlow>()).scale = 0.9f;
				}

				needles = 0;
			}

			if (needleTimer > 30)
			{
				float angle = Main.rand.NextFloat(6.28f);
				var dust = Dust.NewDustPerfect(NPC.Center - new Vector2(15, 15) - angle.ToRotationVector2() * 70, ModContent.DustType<NeedlerDustGlowGrowing>());
				dust.scale = 0.05f;
				dust.velocity = angle.ToRotationVector2() * 0.2f;
			}

			base.AI(NPC);
		}
	}
}