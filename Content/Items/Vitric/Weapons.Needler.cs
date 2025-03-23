using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Core.Loaders;
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
			Item.damage = 7;
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
			{
				Projectile.Kill();
				return;
			}

			if (Timer == 0f)
				UseTime = CombinedHooks.TotalUseTime(Owner.HeldItem.useTime, Owner, Owner.HeldItem);

			UpdateHeldProjectile();

			Timer++;

			float spinUpTime = (int)(UseTime * MathHelper.Lerp(2.5f, 1f, Timer < 75f ? Timer / 75f : 1f)); // the time between shots / time between the sprite frame changes is greater when first starting firing

			if (++Projectile.frameCounter % (int)Utils.Clamp(spinUpTime - 3, 1, 50) == 0)
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];

			if ((int)Timer % spinUpTime == 0)
			{
				Shoot();
			}
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

			Vector2 position = Projectile.Center + Vector2.Lerp(Vector2.Zero, new Vector2(-4f * Owner.direction, 0f), Eases.EaseCircularIn(Timer < 75f ? Timer / 75f : 1f)).RotatedBy(Projectile.rotation) - Main.screenPosition;

			Main.spriteBatch.Draw(tex, position, frame, lightColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Effect effect = ShaderLoader.GetShader("DistortSprite").Value;

			if (effect != null)
			{
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
			}

			return false;
		}

		/// <summary>
		/// Called when the held projectile should shoot its projectile (in this case, Needle)
		/// </summary>
		private void Shoot()
		{
			SoundHelper.PlayPitched("Guns/SMG2", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), BarrelPosition, Projectile.velocity.RotatedByRandom(0.15f) * Owner.HeldItem.shootSpeed, ModContent.ProjectileType<NeedlerProj>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

			CameraSystem.shake += 1;

			Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 20f, ModContent.DustType<NeedlerMuzzleFlashDust>(), Vector2.Zero, 0, default, Main.rand.NextFloat(0.8f, 1.2f)).rotation = Projectile.velocity.ToRotation();

			Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 20f + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), Projectile.velocity * 0.05f, 50, new Color(255, 130, 0), 0.05f);

			Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 20f + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<CoachGunUpgradeSmokeDust>(), Projectile.velocity * Main.rand.NextFloat(0.8f, 1.5f), Main.rand.Next(100, 175), new Color(255, 130, 0), Main.rand.NextFloat(0.03f, 0.06f)).noGravity = true;

			for (int i = 0; i < 4; i++)
			{
				Dust.NewDustPerfect(BarrelPosition + Projectile.velocity * 10f, ModContent.DustType<PixelatedGlow>(), Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(1.5f, 3f), 0, new Color(255, 100, 20, 0), 0.15f);
			}

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

		public int explodeTimer;

		public int enemyID = -1;
		public bool stuck = false;
		Vector2 offset = Vector2.Zero;

		public int NeedleCount => stuck ? Main.projectile.Count(p => p.active && p.type == Type
									&& (p.ModProjectile as NeedlerProj).enemyID == enemyID) : -1;
		public float NeedleProgress => NeedleCount / 7f;
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
			Projectile.hide = true;
			Projectile.ArmorPenetration = 10;
		}

		//TODO: Move methods to top + method breaks
		//TODO: Turn needles into getnset
		public override bool PreAI()
		{
			if (stuck)
			{
				stuckTimer++;

				if (enemyID > 0)
				{
					if (Main.rand.NextBool(45))
					{
						Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);

						Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedGlow>(),
							Main.rand.NextVector2Circular(3f, 3f), 0, new Color(255, 100, 20, 0), 0.1f);
					}

					NPC target = Main.npc[enemyID];

					if (!target.active)
						Projectile.Kill();
					else
						Projectile.position = target.position + offset;
				}

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

			if (Main.rand.NextBool(5) && Projectile.timeLeft < 1195)
			{
				Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);

				//Dust.NewDustPerfect(pos, ModContent.DustType<PixelatedGlow>(), Projectile.velocity.RotatedByRandom(0.1f) * 0.05f, 0, new Color(255, 50, 20, 0), 0.2f);
				//Dust.NewDustPerfect(pos, ModContent.DustType<PixelatedGlow>(), Projectile.velocity.RotatedByRandom(0.1f) * 0.05f, 0, new Color(255, 255, 255, 0), 0.1f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<PixelatedGlow>(),
					Projectile.velocity.RotatedByRandom(0.1f) * 0.05f, 0, new Color(255, 100, 20, 0), 0.1f);

				Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, Projectile.velocity.RotatedByRandom(0.1f) * 0.05f, 150, default, Main.rand.NextFloat(1f, 2f));
				dust.noGravity = true;
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool ShouldUpdatePosition()
		{
			return !(stuck && enemyID < 0);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!stuck && target.life > 0)
			{
				Projectile.penetrate++;
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				enemyID = target.whoAmI;
				offset = Projectile.position - target.position;
				offset -= Projectile.velocity;
				Projectile.timeLeft = 400;

				if (NeedleProgress >= 1f)
					target.GetGlobalNPC<NeedlerNPC>().ProcExplode(Projectile.Center, Projectile.damage, Projectile.owner);
			}

			if (NPCHelper.IsFleshy(target))
			{
				SoundHelper.PlayPitched("Impacts/StabFleshy", 0.25f, Main.rand.NextFloat(-0.05f, 0.05f), Projectile.Center);
			}
			else
			{
				SoundHelper.PlayPitched("Impacts/StabTiny", 0.25f, Main.rand.NextFloat(-0.05f, 0.05f), Projectile.Center);
			}

			for (int i = 0; i < 2; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(),
					-Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.65f), 0, new Color(255, 100, 20, 0), 0.15f);

				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedImpactLineDustGlow>(),
					-Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.65f), 0, new Color(255, 100, 20, 0), 0.05f);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (!stuck)
			{
				stuck = true;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				Projectile.timeLeft = 20;
				Projectile.position += oldVelocity;

				for (int i = 0; i < 3; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(),
						-Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.65f), 0, new Color(255, 100, 20, 0), 0.15f);

					Vector2 velo = -Projectile.velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.65f);

					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedImpactLineDustGlow>(),
						velo, 0, new Color(255, 100, 20, 0), 0.05f);
				}

				SoundHelper.PlayPitched("Impacts/IceHit", 0.75f, Main.rand.NextFloat(-0.05f, 0.05f), Projectile.Center);
			}

			return false;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (stuckTimer <= 10)
				DrawPrimitives();

			SpriteBatch sb = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_Blur").Value;
			Texture2D texWhite = ModContent.Request<Texture2D>(Texture + "_White").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Masks + "GlowAlpha").Value;

			float fadeOut = 1f;
			if (Projectile.timeLeft < 30f)
				fadeOut = Projectile.timeLeft / 30f;

			sb.Draw(tex, Projectile.Center - Main.screenPosition, null, Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16)) * fadeOut, Projectile.rotation + MathHelper.PiOver2, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			sb.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 120, 20, 0) * fadeOut * 0.55f, 0f, bloomTex.Size() / 2f, 0.25f, SpriteEffects.None, 0f);

			sb.Draw(texGlow, Projectile.Center - Main.screenPosition, null, new Color(255, 90, 20, 0) * fadeOut * NeedleProgress * 0.75f, Projectile.rotation + MathHelper.PiOver2, texGlow.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			if (explodeTimer > 0)
				sb.Draw(texWhite, Projectile.Center - Main.screenPosition, null, Color.White * (1f - explodeTimer / 60f), Projectile.rotation + MathHelper.PiOver2, texWhite.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

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
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(190), factor => factor * 4.5f, factor =>
			Color.Lerp(new Color(255, 100, 20), new Color(35, 70, 120), Eases.EaseQuarticOut(1f - factor.X)) * (1f - stuckTimer / 10f));

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(190), factor => factor * 4f, factor =>
			Color.Lerp(new Color(255, 130, 20), new Color(50, 100, 170), Eases.EaseQuarticOut(1f - factor.X)) * (1f - stuckTimer / 10f));

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					Matrix world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());

					// !!! IMPORTANT WHEN PIXELIZING, MAKE SURE TO USE Main.GameViewMatrix.EffectMatrix IMPORTANT !!!

					Matrix view = Main.GameViewMatrix.EffectMatrix;
					Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
					effect.Parameters["repeats"].SetValue(1f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value);
					trail?.Render(effect);

					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

					trail?.Render(effect);
					trail2?.Render(effect);
				}
			});
		}
	}

	public class NeedlerExplosion : ModProjectile
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;
		public override string Texture => AssetDirectory.Invisible;
		private float Progress => Utils.Clamp(1 - Projectile.timeLeft / 30f, 0f, 1f);

		private float Radius => Projectile.ai[0] * Eases.EaseQuinticOut(Progress);

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Hellfire Explosion");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			for (int k = 0; k < 6; k++)
			{
				float rot = Main.rand.NextFloat(0, 6.28f);

				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, DustID.Torch,
					Vector2.One.RotatedBy(rot) * 0.5f, 0, default, Main.rand.NextFloat(1.5f, 3f)).noGravity = true;

				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, DustID.Torch,
					Vector2.One.RotatedBy(rot) * 0.5f + Main.rand.NextVector2Circular(2f, 2f), 50, default, Main.rand.NextFloat(0.5f, 1f));
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 300);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
			return false;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 40; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			for (int k = 0; k < 40; k++)
			{
				cache[k] = Projectile.Center + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius;
			}

			while (cache.Count > 40)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 40 * (1f - Progress), factor => Color.Lerp(new Color(255, 180, 20), new Color(255, 20, 20), Eases.EaseQuinticInOut(Progress)));

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 30 * (1f - Progress), factor => Color.Lerp(new Color(255, 255, 255), new Color(255, 180, 20), Eases.EaseQuinticInOut(Progress)));

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[39];

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[39];
		}

		public void DrawPrimitives()
		{
			if (Projectile.timeLeft < 2)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					Matrix world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Main.GameViewMatrix.EffectMatrix;
					Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
					effect.Parameters["repeats"].SetValue(5f);
					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

					trail?.Render(effect);
					trail2?.Render(effect);

					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "EnergyTrail").Value);

					trail2?.Render(effect);
				}
			});
		}
	}

	public class NeedlerEmber : ModProjectile
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Needle");

			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.aiStyle = 1;
			Projectile.width = Projectile.height = 12;
			Projectile.extraUpdates = 1;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			Projectile.scale *= 0.995f;

			var dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelSmokeColor>(), Main.rand.NextVector2Circular(.5f, .5f), 120, new Color(255, 120, 20));
			dust.scale = 0.025f * Projectile.scale;
			dust.rotation = Main.rand.NextFloat(6.28f);
			dust.customData = new Color(50, 50, 50);
		}
	}

	public class NeedlerNPC : GlobalNPC
	{
		public int explodeTimer;
		public int explodePlayerID = -1; // simple for projectile ownership
		private int explodeDamage;
		public override bool InstancePerEntity => true;
		public override void AI(NPC npc)
		{
			if (explodeTimer > 0)
			{
				if (explodeTimer == 1)
					Explode(npc);

				foreach (Projectile proj in Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<NeedlerProj>() && (p.ModProjectile as NeedlerProj).enemyID == npc.whoAmI))
					(proj.ModProjectile as NeedlerProj).explodeTimer = explodeTimer;

				if (explodeTimer <= 30)
				{
					float lerper = Eases.EaseQuinticOut(explodeTimer / 30f);

					if (explodeTimer >= 10)
					{
						for (int i = 0; i < 2; i++)
						{
							Vector2 pos = npc.Center + Main.rand.NextVector2Circular(150f, 150f) * lerper;

							Dust.NewDustPerfect(pos, ModContent.DustType<PixelatedImpactLineDust>(), pos.DirectionTo(npc.Center) * 5f, 0, new Color(255, 100, 20, 0), 0.075f);

							pos = npc.Center + Main.rand.NextVector2Circular(150f, 150f) * lerper;

							Dust.NewDustPerfect(pos, ModContent.DustType<PixelatedGlow>(), pos.DirectionTo(npc.Center) * 5f, 0, new Color(255, 100, 20, 0), 0.2f);
						}
					}
				}

				explodeTimer--;
			}
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture_Alt").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Masks + "GlowAlpha").Value;

			if (explodeTimer > 0 && explodeTimer < 20)
			{
				float lerper = explodeTimer / 20f;

				Main.spriteBatch.Draw(starTex, npc.Center - Main.screenPosition, null, new Color(255, 50, 20, 0), 1.7f * Eases.EaseQuadIn(lerper), starTex.Size() / 2f, 3f * lerper, 0f, 0f);

				Main.spriteBatch.Draw(starTex, npc.Center - Main.screenPosition, null, new Color(255, 200, 100, 0), 1.7f * Eases.EaseQuadIn(lerper), starTex.Size() / 2f, 2.8f * lerper, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, npc.Center - Main.screenPosition, null, new Color(255, 50, 20, 0), 1f * lerper, bloomTex.Size() / 2f, 2f * (1f - lerper), 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, npc.Center - Main.screenPosition, null, new Color(255, 200, 100, 0), 1f * lerper, bloomTex.Size() / 2f, 1.8f * (1f - lerper), 0f, 0f);
			}
		}

		/// <summary>
		/// called when an explosion is to be started
		/// </summary>
		/// <param name="playerID">the player starting the explosion</param>
		/// <param name="damage">the damage of the projectile which started the explosion</param>
		public void ProcExplode(Vector2 pos, int damage, int playerID)
		{
			if (explodeTimer > 0)
				return;

			explodeTimer = 60;
			explodeDamage = damage;
			explodePlayerID = playerID;

			SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireCast"), pos);
		}

		private void Explode(NPC npc)
		{
			float mult = 10f + 0.35f * Main.projectile.Count(p => p.active && p.type == ModContent.ProjectileType<NeedlerProj>() && (p.ModProjectile as NeedlerProj).enemyID == npc.whoAmI);

			foreach (Projectile proj in Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<NeedlerProj>() && (p.ModProjectile as NeedlerProj).enemyID == npc.whoAmI))
				proj.active = false;

			SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), npc.Center);

			Projectile.NewProjectile(npc.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<NeedlerExplosion>(), (int)(explodeDamage * mult), 5f, explodePlayerID, 80f);

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(npc.Center, ModContent.DustType<PixelatedImpactLineDustGlowLerp>(), Main.rand.NextVector2CircularEdge(15f, 15f) * Main.rand.NextFloat(0.7f, 1f), 0, new Color(255, 80, 20, 0), 0.15f).customData = new Color(255, 20, 20, 0);
			}

			for (int i = 0; i < 5; i++)
			{
				Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, -Vector2.UnitY.RotatedByRandom(2f) * Main.rand.NextFloat(2f, 4f), ModContent.ProjectileType<NeedlerEmber>(), 0, 0, explodePlayerID).scale = Main.rand.NextFloat(0.85f, 1.15f);
			}

			for (int i = 0; i < 20; i++)
			{
				Dust dust = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(80f, 80f), ModContent.DustType<PixelSmokeColor>(), Main.rand.NextVector2Circular(4f, 4f), Main.rand.Next(50, 125), new Color(255, 80, 0), Main.rand.NextFloat(0.1f, 0.15f));
				dust.rotation = Main.rand.NextFloat(6.28f);

				dust = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(80f, 80f), ModContent.DustType<PixelSmokeColor>(), Main.rand.NextVector2Circular(4.5f, 4.5f) + -Vector2.UnitY * 2f, Main.rand.Next(50, 125), new Color(255, 110, 0), Main.rand.NextFloat(0.05f, 0.1f));
				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(50, 50, 50);

				dust = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(80f, 80f), ModContent.DustType<PixelSmokeColor>(), Main.rand.NextVector2Circular(5.5f, 5.5f), Main.rand.Next(50, 125), new Color(255, 150, 0), Main.rand.NextFloat(0.05f, 0.1f));
				dust.rotation = Main.rand.NextFloat(6.28f);
				dust.customData = new Color(75, 75, 75);
			}

			CameraSystem.shake += 10;

			explodePlayerID = -1;
		}
	}

	public class NeedlerMuzzleFlashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.frame = new Rectangle(0, 0, 4, 4);
		}

		public override bool Update(Dust dust)
		{
			dust.alpha += 20;
			dust.alpha = (int)(dust.alpha * 1.05f);

			if (dust.alpha >= 255)
				dust.active = false;

			Lighting.AddLight(dust.position, new Color(255, 255, 20).ToVector3() * new Vector3(1.5f * 1f - dust.alpha / 255f, 1.5f * 1f - dust.alpha / 255f, 1.5f * 1f - dust.alpha / 255f));

			return false;
		}

		public override bool PreDraw(Dust dust)
		{
			float lerper = 1f - dust.alpha / 255f;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + Name).Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + Name + "_Blur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + Name + "_Glow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Masks + "GlowAlpha").Value;

			Main.spriteBatch.Draw(bloomTex, dust.position - Main.screenPosition, null, new Color(255, 75, 0, 0) * 0.25f * lerper, dust.rotation, bloomTex.Size() / 2f, dust.scale * 1.25f, 0f, 0f);

			Main.spriteBatch.Draw(texGlow, dust.position - Main.screenPosition, null, new Color(255, 75, 0, 0) * lerper, dust.rotation, texGlow.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, dust.position - Main.screenPosition, null, Color.White * lerper, dust.rotation, tex.Size() / 2f, dust.scale, 0f, 0f);

			Main.spriteBatch.Draw(texBlur, dust.position - Main.screenPosition, null, Color.White with { A = 0 } * 0.5f * lerper, dust.rotation, texBlur.Size() / 2f, dust.scale, 0f, 0f);

			return false;
		}
	}
}