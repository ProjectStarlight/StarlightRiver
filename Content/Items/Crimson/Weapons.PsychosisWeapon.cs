using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Metaballs;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	public class PsychosisWeapon : ModItem
	{
		public float shootRotation;
		public int shootDirection;

		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;

			DisplayName.SetDefault("Neurosis & Psychosis");
			Tooltip.SetDefault(
				"<left> to fire bullets infused with {{BUFF:Neurosis}}\n" +
				"<right> to fire bullets infused with {{BUFF:Psychosis}}\n" +
				"Hitting enemies with the opposing effect causes a BRAIN BLAST, scaling with stacks"
				);
		}

		public override void SetDefaults()
		{
			Item.damage = 25;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 21;
			Item.useAnimation = 21;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = 15f;
			Item.useAmmo = AmmoID.Bullet;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TheUndertaker);
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);
			recipe.AddIngredient(ModContent.ItemType<ImaginaryTissue>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override bool CanUseItem(Player Player)
		{
			shootRotation = (Player.Center - Main.MouseWorld).ToRotation();
			shootDirection = (Main.MouseWorld.X < Player.Center.X) ? -1 : 1;

			return base.CanUseItem(Player);
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6, 0);
		}

		public override bool AltFunctionUse(Player Player)
		{
			return true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (player.altFunctionUse == 2)
			{

			}
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			/*Vector2 itemPosition = CommonGunAnimations.SetGunUseStyle(player, Item, shootDirection, -10f, new Vector2(52f, 28f), new Vector2(-40f, 4f));

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;
			 
			if (animProgress >= 0.5f)
			{
				float lerper = (animProgress - 0.5f) / 0.5f;
				Dust.NewDustPerfect(itemPosition + new Vector2(50f, -10f * player.direction).RotatedBy(player.compositeFrontArm.rotation + 1.5707964f * player.gravDir), DustID.Smoke, Vector2.UnitY * -2f, (int)MathHelper.Lerp(210f, 200f, lerper), default, MathHelper.Lerp(0.5f, 1f, lerper)).noGravity = true;
			}*/
		}

		public override void UseItemFrame(Player player)
		{
			//CommonGunAnimations.SetGunUseItemFrame(player, shootDirection, shootRotation, -0.2f, true, new Vector2(0.3f, 0.7f));
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Color c = PsychosisWeaponGlobalProjectile.DrawColor(player.altFunctionUse == 2);

			Vector2 barrelPos = position + new Vector2(60f, -20f * player.direction).RotatedBy(velocity.ToRotation());

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.025f, 50, new Color(255, 0, 0), 0.1f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(255, 0, 0), 0.2f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(100, 100, 100), 0.2f);

			for (int i = 0; i < 4; i++)
			{
				/*Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, default, 1.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelatedGlow>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, c, 0.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, c, 0.2f).customData = -player.direction;*/

				Vector2 vel = velocity.SafeNormalize(Vector2.One);

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.5f) * Main.rand.NextFloat(5f), 50, default, 1.25f).noGravity = true;

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.5f) * Main.rand.NextFloat(5f), 50, default, 1.25f).fadeIn = 1f;

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.75f) * Main.rand.NextFloat(10f), 50, default, 1.5f).noGravity = true;

				Dust.NewDustPerfect(barrelPos, DustID.Blood, vel.RotatedByRandom(0.75f) * Main.rand.NextFloat(10f), 50, default, 1.5f).fadeIn = 1f;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<SmokeDustColor_Alt>(),
					vel.RotatedByRandom(0.25f) * Main.rand.NextFloat(2f), Main.rand.Next(120, 200), new Color(101, 13, 13), Main.rand.NextFloat(0.3f, 0.5f)).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<SmokeDustColor_Alt>(),
					vel.RotatedByRandom(0.25f) * Main.rand.NextFloat(2f), Main.rand.Next(120, 200), new Color(215, 29, 29), Main.rand.NextFloat(0.5f, 0.7f)).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.DustType<SmokeDustColor_Alt>(),
					vel.RotatedByRandom(0.25f) * Main.rand.NextFloat(2f) * Main.rand.NextFloat(), Main.rand.Next(120, 200), new Color(150, 15, 15), Main.rand.NextFloat(0.7f, 0.8f)).noGravity = true;

				Dust.NewDustPerfect(barrelPos, ModContent.DustType<GraveBlood>(), vel.RotatedByRandom(0.75f) * Main.rand.NextFloat(6f), 50, default, 2f).fadeIn = 1f;

				Dust.NewDustPerfect(barrelPos, ModContent.DustType<GraveBlood>(), vel.RotatedByRandom(1.5f) * Main.rand.NextFloat(9f), 50, default, 2f).fadeIn = 1f;
			}

			Vector2 flashPos = barrelPos - new Vector2(5f, 0f).RotatedBy(velocity.ToRotation());

			//Dust.NewDustPerfect(flashPos, ModContent.DustType<FlareBreacherMuzzleFlashDust>(), Vector2.Zero, 0, default, 0.75f).rotation = velocity.ToRotation();

			//SoundHelper.PlayPitched("Guns/FlareFire", 0.6f, Main.rand.NextFloat(-0.1f, 0.1f), position);
			CameraSystem.shake += 2;
			
			Projectile p = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, velocity * 2, type, damage, knockback, player.whoAmI);

			Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), p.Center, Vector2.Zero, ModContent.ProjectileType<PsychosisWeaponDummyProjectile>(), 0, 0f, player.whoAmI, player.altFunctionUse == 2 ? 1 : 0, p.whoAmI);

			PsychosisWeaponGlobalProjectile gp = p.GetGlobalProjectile<PsychosisWeaponGlobalProjectile>();

			if (player.altFunctionUse == 2)
				gp.psychosis = true;
			else
				gp.neurosis = true;

			SoundHelper.PlayPitched("Impacts/GoreLight", 0.05f, 0.2f, player.Center);
			SoundHelper.PlayPitched("Guns/Pistol", 0.1f, -0.2f, player.Center);

			Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, velocity, ModContent.ProjectileType<PsychosisWeaponHeldProjectile>(), damage, knockback, player.whoAmI, 0, 0, player.altFunctionUse == 2 ? 1 : 0);

			return false;
		}
	}

	// just for animation purposes
	class PsychosisWeaponHeldProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + "PsychosisWeapon";

		public bool updateVelocity = true;
		public ref float Timer => ref Projectile.ai[0];
		public ref float MaxTime => ref Projectile.ai[1];
		public bool UseAltTexture => Projectile.ai[2] == 0;
		public float Progress => 1f - Projectile.timeLeft / MaxTime;
		public Vector2 Offset;
		public float Rotation;
		public Vector2 ArmPosition => Owner.RotatedRelativePoint(Owner.MountedCenter, true) + Offset;
		public Player Owner => Main.player[Projectile.owner];

		public override void Load()
		{
			StarlightPlayer.PreDrawEvent += DrawAltGun;
		}

		private void DrawAltGun(Player player, SpriteBatch spriteBatch)
		{
			if (player.ownedProjectileCounts[Type] > 0)
			{
				Projectile p = Main.projectile.FirstOrDefault(p => p.type == Type && p.active && p.owner == player.whoAmI);

				if (p == default)
					return;

				float direction = player.Center.X < Main.MouseWorld.X ? 1 : -1;

				PsychosisWeaponHeldProjectile mp = p.ModProjectile as PsychosisWeaponHeldProjectile;

				Texture2D tex = ModContent.Request<Texture2D>(Texture + (!mp.UseAltTexture ? "_Alt" : "")).Value;
				Texture2D texBlur = ModContent.Request<Texture2D>(Texture + (!mp.UseAltTexture ? "_Alt" : "") + "_Blur").Value;
				Texture2D texBloom = ModContent.Request<Texture2D>(Texture + "_Bloom").Value;
				Texture2D texWhite = ModContent.Request<Texture2D>(Texture + "_White").Value;

				SpriteEffects spriteEffects = direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

				Vector2 position = player.MountedCenter - Main.screenPosition;
				
				float fade = p.timeLeft / mp.MaxTime;

				float rotation = MathHelper.Lerp(1f, 0.7f, Eases.EaseBackOut(1f - fade)) * direction;

				Color c = new Color(150, 150, 150, 255);

				Color glowColor = PsychosisWeaponGlobalProjectile.DrawColor(mp.UseAltTexture);
				
				Main.spriteBatch.Draw(texBloom, position + new Vector2(14f * direction, 9f),
					null, glowColor * fade, rotation, texBloom.Size() / 2f, 0.9f, spriteEffects, 0f);

				Main.spriteBatch.Draw(tex, position + new Vector2(14f * direction, 9f),
					null, c, rotation, tex.Size() / 2f, 0.9f, spriteEffects, 0f);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Psychosis Weapon");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.timeLeft = Owner.itemAnimation + 2;
			MaxTime = Projectile.timeLeft;
		}

		public override void AI()
		{
			/*if (Timer == 0f)
			{
				Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);
				UseTime = CombinedHooks.TotalUseTime(Owner.itemTime, Owner, Owner.HeldItem);
			}*/

			if (Progress < 0.25f)
			{
				float lerper = Progress / 0.25f;

				Offset = new Vector2(MathHelper.Lerp(32f, 8f, Eases.EaseCircularOut(lerper)), -12f * Owner.direction).RotatedBy(Projectile.velocity.ToRotation());

				Rotation = MathHelper.Lerp(0f, -0.3f * Owner.direction, Eases.EaseCircularOut(lerper));
			}
			else
			{
				float lerper = (Progress - 0.25f) / 0.75f;

				Offset = new Vector2(MathHelper.Lerp(8f, 24f, Eases.EaseBackInOut(lerper)), -12f * Owner.direction).RotatedBy(Projectile.velocity.ToRotation());

				Rotation = MathHelper.Lerp(-0.3f * Owner.direction, 0f, Eases.EaseBackInOut(lerper));
			}

			Timer++;

			UpdateHeldProjectile();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Timer <= 2 || Progress >= 1f)
				return false;

			Texture2D tex = ModContent.Request<Texture2D>(Texture + (UseAltTexture ? "_Alt" : "")).Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + (UseAltTexture ? "_Alt" : "") + "_Blur").Value;
			Texture2D texBloom = ModContent.Request<Texture2D>(Texture + "_Bloom").Value;
			Texture2D texWhite = ModContent.Request<Texture2D>(Texture + "_White").Value;

			Color c = PsychosisWeaponGlobalProjectile.DrawColor(!UseAltTexture);
			
			SpriteEffects spriteEffects = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Vector2 position = ArmPosition - Main.screenPosition;

			float rotation = Projectile.rotation + (spriteEffects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0f);

			float fade = Projectile.timeLeft / MaxTime;
			
			Main.spriteBatch.Draw(texBloom, position, null, c * fade, rotation, texBloom.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.Draw(tex, position, null, lightColor, rotation, tex.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			Main.spriteBatch.Draw(texWhite, position, null, c with { A = 255 } * 0.35f * fade, rotation, texWhite.Size() / 2f, Projectile.scale, spriteEffects, 0f);

			return false;
		}

		/// <summary>
		/// Updates the basic variables needed for a held projectile
		/// </summary>
		protected virtual void UpdateHeldProjectile(bool updateTimeleft = true)
		{
			Owner.ChangeDir(Projectile.direction);
			Owner.heldProj = Projectile.whoAmI;

			Projectile.rotation = Projectile.velocity.ToRotation() + Rotation;
			Owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f));

			Projectile.position = ArmPosition - Projectile.Size * 0.5f;

			if (Main.myPlayer == Projectile.owner)
			{
				Vector2 oldVelocity = Projectile.velocity;

				Projectile.velocity = Owner.DirectionTo(Main.MouseWorld);

				if (Projectile.velocity != oldVelocity)
				{
					Projectile.netSpam = 0;
					Projectile.netUpdate = true;
				}
			}
		}

		public override bool? CanDamage()
		{
			return false;
		}
	}

	class PsychosisWeaponDummyProjectile : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		public List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private Vector2 OffsetPosition;

		public int timer;
		public int deathTimer;

		public bool Psychosis => Projectile.ai[0] == 1;
		public int ParentWhoAmI => (int)Projectile.ai[1];

		public Projectile Parent => Main.projectile[ParentWhoAmI];
		internal float FadeIn()
		{
			float fade = 1f;

			if (timer < 25)
				fade = timer / 25f;

			if (deathTimer > 0)
				fade = deathTimer / 10f;

			return fade;
		}

		internal Vector2 TrailOffsetPosition(Vector2 offset)
		{
			return Projectile.Center + offset + Projectile.velocity + Vector2.UnitX.RotatedBy(Projectile.rotation) * MathHelper.Lerp(-4f, 4f, (float)Math.Sin(Projectile.timeLeft * 0.3f));
		}

		public override void AI()
		{
			if (deathTimer > 0)
			{
				deathTimer--;

				if (deathTimer == 1)
				{
					Projectile.Kill();
					return;
				}

				if (Main.netMode != NetmodeID.Server)
				{
					ManageCaches(Projectile.Center);
					ManageTrail(Projectile.Center);
				}

				return;
			}
			else
			{
				if (Parent is null || !Parent.active)
				{
					deathTimer = 10;
					return;
				}

				Projectile.Center = Parent.Center;

				OffsetPosition = TrailOffsetPosition(Main.rand.NextVector2CircularEdge(15f, 15f));

				if (Main.netMode != NetmodeID.Server)
				{
					ManageCaches(OffsetPosition);
					ManageTrail(OffsetPosition);
				}

				timer++;

				Dust.NewDustPerfect(Projectile.Center, DustID.Blood,
						-Projectile.velocity.RotatedByRandom(0.3f) * 0.5f, 50 + Main.rand.Next(100), Color.White, Main.rand.NextFloat(0.5f, 1f));

				Dust.NewDustPerfect(Projectile.Center, DustID.Blood,
						-Projectile.velocity.RotatedByRandom(0.3f) * 0.5f, 50 + Main.rand.Next(100), Color.White, Main.rand.NextFloat(0.5f, 1f)).noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			cache = null;
			trail.Dispose();
			trail2.Dispose();
			trail = null;
			trail2 = null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D tex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowAlpha.Value;

			Main.spriteBatch.Draw(bloomTex, OffsetPosition - Main.screenPosition, null, PsychosisWeaponGlobalProjectile.DrawColor(Psychosis) * FadeIn(), 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

			Main.spriteBatch.Draw(tex, OffsetPosition - Main.screenPosition, null, PsychosisWeaponGlobalProjectile.DrawColor(Psychosis) * FadeIn(), 0f, tex.Size() / 2f, 0.6f, 0f, 0f);
			Main.spriteBatch.Draw(tex, OffsetPosition - Main.screenPosition, null, new Color(255, 255, 255, 0) * FadeIn(), 0f, tex.Size() / 2f, 0.3f, 0f, 0f);

			return false;
		}

		private void ManageCaches(Vector2 pos)
		{
			if (cache is null)
			{
				cache = [];

				for (int i = 0; i < 12; i++)
				{
					cache.Add(pos);
				}
			}

			cache.Add(pos);

			while (cache.Count > 12)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail(Vector2 pos)
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 12, new TriangularTip(190), factor => factor * 5f, factor => Color.White * FadeIn());
			if (trail is null || trail.IsDisposed)
				trail = new Trail(Main.instance.GraphicsDevice, 12, new TriangularTip(190), factor => factor * 5f, factor => Color.White * FadeIn());

			trail.Positions = cache.ToArray();
			trail.NextPosition = pos;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 12, new TriangularTip(190), factor => factor * 16f, factor => PsychosisWeaponGlobalProjectile.DrawColor(Psychosis) * FadeIn());

			if (trail2 is null || trail2.IsDisposed)
				trail2 = new Trail(Main.instance.GraphicsDevice, 12, new TriangularTip(190), factor => factor * 16f, factor => PsychosisWeaponGlobalProjectile.DrawColor(Psychosis) * FadeIn());

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = pos;
		}

		public void DrawPrimitives()
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.05f);
					effect.Parameters["repeats"].SetValue(1);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

					trail2?.Render(effect);
					trail?.Render(effect);
				}
			});
		}
	}

	public class PsychosisWeaponGlobalProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool psychosis = false;
		public bool neurosis = false;

		public static Color DrawColor(bool psych)
		{
			return psych ? Red() : Blue();
		}

		public static Color Blue()
		{
			float r = 0.2f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
			float b = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
			return new Color(r, g, b, 0);
		}

		public static Color Red()
		{
			float r = 0.7f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.03f;
			float g = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 2f) * 0.05f;
			float b = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + 4f) * 0.03f;
			return new Color(r, g, b, 0);
		}

		public bool Active => psychosis || neurosis;

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (psychosis)
			{
				StackableBuff b = InstancedBuffNPC.GetInstance<Neurosis>(target);

				if (b != null && target.HasBuff(b.BackingType))
				{
					Explode(projectile, target, b.stacks.Count(), true);

					Main.NewText("Cleared Neurosis, stack count: " + b.stacks.Count());
					target.DelBuff(target.FindBuffIndex(b.BackingType));
					(b as Neurosis).stacks.Clear();
				}
				else
				{
					BuffInflictor.Inflict<Psychosis>(target, 240);

					StackableBuff psychosis = InstancedBuffNPC.GetInstance<Psychosis>(target);

					float pitch = -0.5f;
					pitch += psychosis.stacks.Count * 0.1f;

					if (pitch > 0.5f)
						pitch = 0.5f;

					SoundHelper.PlayPitched("Magic/LightningShortest2", 1f, pitch, target.Center);
				}

				for (int i = 0; i < 5; i++)
				{
					Color c = DrawColor(psychosis);

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedImpactLineDustGlow>(), Main.rand.NextVector2CircularEdge(7f, 7f), 0, c, 0.1f).noGravity = true;

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, c, 0.2f).customData = Main.rand.NextBool() ? -1 : 1;
				}
			}
			else if (neurosis)
			{
				StackableBuff b = InstancedBuffNPC.GetInstance<Psychosis>(target);

				if (b != null && target.HasBuff(b.BackingType))
				{
					Explode(projectile, target, b.stacks.Count(), false);

					Main.NewText("Cleared Psychosis, stack count: " + b.stacks.Count());
					target.DelBuff(target.FindBuffIndex(b.BackingType));
					(b as Psychosis).stacks.Clear();
				}
				else
				{
					BuffInflictor.Inflict<Neurosis>(target, 240);
					
					StackableBuff neurosis = InstancedBuffNPC.GetInstance<Neurosis>(target);

					float pitch = -0.5f;
					pitch += neurosis.stacks.Count * 0.1f;

					if (pitch > 0.5f)
						pitch = 0.5f;

					SoundHelper.PlayPitched("Magic/LightningShortest1", 1f, pitch, target.Center);
				}

				for (int i = 0; i < 5; i++)
				{
					Color c = DrawColor(psychosis);

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedImpactLineDustGlow>(), Main.rand.NextVector2CircularEdge(7f, 7f), 0, c, 0.1f).noGravity = true;

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, c, 0.2f).customData = Main.rand.NextBool() ? -1 : 1;
				}
			}
		}

		void Explode(Projectile p, NPC npcHit, int count, bool psych)
		{
			// 10 is max stacks
			if (count > 10)
				count = 10;

			int damage = (int)(30 + 100 * (count / 10f));

			Color c = DrawColor(psych);

			if (Main.myPlayer == p.owner)
			{
				Projectile.NewProjectile(p.GetSource_FromThis(), p.Center, Vector2.Zero, 
					ModContent.ProjectileType<BrainBlastProjectile>(), damage, 2f, p.owner, MathHelper.Lerp(25, 100, count / 10f), psych ? 1 : 0);
			}

			NPC[] closestNPCs = [.. Main.npc.Where(n => n.active && n.CanBeChasedBy() && n != npcHit && n.Distance(p.Center) < 500f).OrderBy(n => n.Distance(p.Center))];

			int maxTargets = (int)MathHelper.Lerp(1, 5, count / 10f);

			for (int i = 0; i < (closestNPCs.Length > maxTargets ? maxTargets : closestNPCs.Length); i++)
			{
				if (Main.myPlayer == p.owner)
				{
					Projectile.NewProjectile(p.GetSource_FromThis(), p.Center + Main.rand.NextVector2CircularEdge(12.5f, 12.5f) * count, Vector2.Zero,
						ModContent.ProjectileType<BrainBlastBolt>(), damage / 5, 1f, p.owner, closestNPCs[i].whoAmI);
				}

				for (int j = 0; j < 5; j++)
				{
					Vector2 velocity = p.DirectionTo(closestNPCs[i].Center);

					Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(2f, 2f),
					DustID.Blood, velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 15f), 50, default, 2.25f).noGravity = true;

					Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(2f, 2f),
						ModContent.DustType<SmokeDustColor_Alt>(),
						velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 10f), Main.rand.Next(120, 200), new Color(101, 13, 13), Main.rand.NextFloat(0.3f, 0.5f)).noGravity = true;

					Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(2f, 2f),
						ModContent.DustType<SmokeDustColor_Alt>(),
						velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(2f, 10f), Main.rand.Next(80, 140), new Color(101, 13, 13), Main.rand.NextFloat(0.5f, 1f)).noGravity = true;

					Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(3f, 3f),
						ModContent.DustType<GraveBlood>(), velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 12f), 50, default, 2f).fadeIn = 1f;

					Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(4f, 4f),
						ModContent.DustType<GraveBlood>(), velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 12f), 50, default, 4f).fadeIn = 1f;

					Dust.NewDustPerfect(p.Center, ModContent.DustType<Dusts.BloodMetaballDust>(),
						velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 12f), 0, Color.White, Main.rand.NextFloat(0.2f, 0.4f));

					Dust.NewDustPerfect(p.Center, ModContent.DustType<Dusts.BloodMetaballDustLight>(),
						velocity.RotatedByRandom(0.3f) * Main.rand.NextFloat(5f, 12f), 0, Color.White, Main.rand.NextFloat(0.1f, 0.3f));
				}
			}


			for (int i = 0; i < MathHelper.Lerp(9, 35, count / 10f); i++)
			{
				float velocity = MathHelper.Lerp(1, 5, count / 10f);
				float radius = MathHelper.Lerp(25, 100, count / 10f);
				float scale = MathHelper.Lerp(0.5f, 1.5f, count / 10f);

				Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(radius, radius),
					DustID.Blood, Main.rand.NextVector2Circular(velocity, velocity), 50, default, 2.25f * scale).noGravity = true;

				Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(radius, radius),
					ModContent.DustType<SmokeDustColor_Alt>(),
					Main.rand.NextVector2Circular(velocity, velocity) * 2f, Main.rand.Next(120, 200), new Color(101, 13, 13), Main.rand.NextFloat(0.3f, 0.5f) * scale).noGravity = true;
				
				Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(radius, radius),
					ModContent.DustType<SmokeDustColor_Alt>(),
					Main.rand.NextVector2Circular(velocity, velocity), Main.rand.Next(80, 140), new Color(101, 13, 13), Main.rand.NextFloat(0.5f, 1f) * scale).noGravity = true;

				Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(radius, radius),
					ModContent.DustType<GraveBlood>(), Main.rand.NextVector2Circular(velocity, velocity), 50, default, 2f * scale).fadeIn = 1f;

				Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(radius, radius),
					ModContent.DustType<GraveBlood>(), Main.rand.NextVector2CircularEdge(velocity, velocity) / 2, 50, default, 4f * scale).fadeIn = 1f;

				Dust.NewDustPerfect(p.Center, ModContent.DustType<Dusts.BloodMetaballDust>(), 
					Vector2.UnitY.RotatedByRandom(0.5f) * -Main.rand.NextFloat(2f * velocity), 0, Color.White, Main.rand.NextFloat(0.2f, 0.4f) * scale);

				Dust.NewDustPerfect(p.Center, ModContent.DustType<Dusts.BloodMetaballDustLight>(), 
					Vector2.UnitY.RotatedByRandom(0.8f) * -Main.rand.NextFloat(2.5f * velocity), 0, Color.White, Main.rand.NextFloat(0.1f, 0.3f) * scale);

				Dust.NewDustPerfect(p.Center + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelatedImpactLineDustGlow>(), Main.rand.NextVector2CircularEdge(6f, 6f) * velocity, 0, c, 0.1f).noGravity = true;
			}

			if (Main.rand.NextBool())
			{
				SoundHelper.PlayPitched("NPC/Crimson/GrossquitoBoom1", 1f, 0.5f, p.Center);
			}
			else
			{
				SoundHelper.PlayPitched("NPC/Crimson/GrossquitoBoom2", 1f, 0.5f, p.Center);
			}

			SoundHelper.PlayPitched("Magic/LightningExplode", 1f, -0.25f, p.Center);

			CameraSystem.shake += 2 + count;
		}
	}

	public class BrainBlastBolt : ModProjectile
	{
		public int deathTimer;
		public int flashTimer;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public Vector2 startPos;
		public override string Texture => AssetDirectory.Invisible;
		public int TargetWhoAmI => (int)Projectile.ai[0];
		public float Progress => 1f - Projectile.timeLeft / 30f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Brain Blast Bolt");
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(20);

			Projectile.DamageType = DamageClass.Magic;

			Projectile.hostile = false;
			Projectile.friendly = true;

			Projectile.tileCollide = false;

			// five frame lifetime

			Projectile.timeLeft = 30;
			Projectile.extraUpdates = 6;

			Projectile.penetrate = 2;
		}

		public override bool? CanDamage()
		{
			return Projectile.penetrate > 1;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return target.whoAmI == TargetWhoAmI;
		}

		public override void OnSpawn(IEntitySource source)
		{
			startPos = Projectile.Center;
			flashTimer = 90;
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			if (flashTimer > 0)
				flashTimer--;

			if (deathTimer > 0)
			{
				Projectile.timeLeft = 2;

				deathTimer--;

				if (deathTimer == 1)
					Projectile.Kill();

				return;
			}

			Projectile.Center = Vector2.Lerp(startPos, Main.npc[TargetWhoAmI].Center, Progress) + Main.rand.NextVector2Circular(25f, 25f);

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(), Projectile.position.DirectionTo(Projectile.oldPosition), 0, Color.Lerp(PsychosisWeaponGlobalProjectile.Blue(), new Color(255, 255, 255, 0), Progress), 0.3f);

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(), Projectile.position.DirectionTo(Projectile.oldPosition).RotatedByRandom(1f) * Main.rand.NextFloat(0.8f, 3f), 0, PsychosisWeaponGlobalProjectile.Red(), 0.2f);

			if (Projectile.timeLeft == 1)
			{
				if (Main.rand.NextBool())
				{
					SoundHelper.PlayPitched("Magic/LightningShortest1", 1f, 0f, Projectile.Center);
				}
				else
				{
					SoundHelper.PlayPitched("Magic/LightningShortest2", 1f, 0f, Projectile.Center);
				}

				for (int i = 0; i < 5; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedEmber>(),
						Projectile.Center.DirectionTo(startPos).RotatedByRandom(0.3f) * Main.rand.NextFloat(3f, 10f), 0, PsychosisWeaponGlobalProjectile.Blue(), 0.1f).customData = Main.rand.NextBool() ? -1 : 1;

					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(),
						Projectile.Center.DirectionTo(startPos).RotatedByRandom(0.3f) * Main.rand.NextFloat(3f, 10f), 0, PsychosisWeaponGlobalProjectile.Blue(), 0.35f);

					Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedImpactLineDustGlow>(), Main.rand.NextVector2CircularEdge(8f, 8f), 0, PsychosisWeaponGlobalProjectile.Blue(), 0.1f).noGravity = true;
				}

				deathTimer = 600;
				Projectile.timeLeft = 2;
				Projectile.Center = Main.npc[TargetWhoAmI].Center;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D starTex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowSoftAlpha.Value;
			Texture2D bloomCircle = Assets.Masks.GlowWithRing.Value;

			SpriteBatch sb = Main.spriteBatch;

			Vector2 pos = startPos - Main.screenPosition;

			float flashProg = flashTimer / 90f;

			if (flashProg > 0)
			{
				Vector2 stretch = new Vector2(MathHelper.Lerp(5f, 1f, Eases.EaseQuinticOut(flashProg)), 1f);

				sb.Draw(bloomCircle, pos, null, PsychosisWeaponGlobalProjectile.Red() * flashProg, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.1f, 0.3f, 1f - flashProg), 0f, 0f);
				sb.Draw(bloomCircle, pos, null, new Color(255, 255, 255, 0) * flashProg, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.1f, 0.25f, 1f - flashProg), 0f, 0f);

				sb.Draw(bloomTex, pos, null, PsychosisWeaponGlobalProjectile.Red() * flashProg, 0f, bloomTex.Size() / 2f, 3f, 0f, 0f);
				sb.Draw(bloomTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, bloomTex.Size() / 2f, 2.5f, 0f, 0f);

				sb.Draw(bloomTex, pos, null, PsychosisWeaponGlobalProjectile.Red() * flashProg, 0f, bloomTex.Size() / 2f, stretch, 0f, 0f);
				sb.Draw(bloomTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, bloomTex.Size() / 2f, stretch * 0.75f, 0f, 0f);

				sb.Draw(starTex, pos, null, PsychosisWeaponGlobalProjectile.Red() * flashProg, 0f, starTex.Size() / 2f, stretch, 0f, 0f);
				sb.Draw(starTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, starTex.Size() / 2f, stretch * 0.75f, 0f, 0f);
			}

			return false;
		}

		private void ManageCaches()
		{
			Vector2 offset = new Vector2(Main.rand.NextFloat(-5f, 5f), 0f);

			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 100; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 100)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(190), factor => factor * 2.5f, factor =>
			new Color(100, 100, 255) * TrailFade());

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(190), factor => factor * 8f, factor =>
			Color.Lerp(new Color(50, 50, 255, 0), new Color(220, 65, 65, 0), 1f - deathTimer / 600f * TrailFade()));

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center;
		}

		private float TrailFade()
		{
			if (deathTimer <= 0)
				return 1f;
			else
				return deathTimer / 600f;
		}

		public void DrawPrimitives()
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderTiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.05f);
					effect.Parameters["repeats"].SetValue(1);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

					trail2?.Render(effect);
					trail?.Render(effect);
				}
			});
		}
	}

	public class BrainBlastProjectile : ModProjectile
	{
		// not sure how performance intensive this is

		private List<Vector2> cache;
		private List<Vector2> cache2;

		private Trail trail;
		private Trail trail2;

		private Trail trail3;
		private Trail trail4;
		public override string Texture => AssetDirectory.Invisible;
		private float Progress => Utils.Clamp(1 - Projectile.timeLeft / 40f, 0f, 1f);

		private float Radius => Projectile.ai[0] * Eases.EaseBackOut(Progress);

		private bool Psychosis => Projectile.ai[1] == 1;

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 40;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("BRAIN BLAST!");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			if (Main.rand.NextBool(3))
			{
				for (int k = 0; k < 6; k++)
				{
					float rot = Main.rand.NextFloat(0, 6.28f);

					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, ModContent.DustType<PixelatedGlow>(),
						-Vector2.UnitY * 5f * Progress, 0, new Color(50, 50, 255, 0), Main.rand.NextFloat(0.15f, 0.25f)).noGravity = true;
				}
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();

			Texture2D starTex = Assets.StarTexture_Alt.Value;
			Texture2D bloomTex = Assets.Masks.GlowSoftAlpha.Value;
			Texture2D bloomCircle = Assets.Masks.GlowWithRing.Value;

			SpriteBatch sb = Main.spriteBatch;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.EffectMatrix);

				sb.Draw(bloomCircle, Projectile.Center - Main.screenPosition, null, new Color(50, 50, 255, 0) * (1f - Progress), 0f, bloomCircle.Size() / 2f, (Radius / 60f) * MathHelper.Lerp(0.2f, 0.45f, Eases.EaseCircularOut(Progress)), 0f, 0f);

				Effect effect = ShaderLoader.GetShader("ElectricExplosion").Value;

				if (effect is null)
					return;

				if (Progress >= 1f)
					return;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.AlphaBlend, Main.DefaultSamplerState, default, Main.Rasterizer, effect, Main.GameViewMatrix.EffectMatrix);

				effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.005f);

				effect.Parameters["uImage1"].SetValue(Assets.Noise.ElectricNoise.Value);
				effect.Parameters["uImage2"].SetValue(Assets.Noise.PerlinNoise.Value);
				effect.Parameters["uProgress"].SetValue(Eases.EaseQuinticIn(Progress));
				Color color = Color.Lerp(new Color(200, 60, 60), new Color(50, 50, 255), Eases.EaseQuinticIn(1f - Progress));

				effect.Parameters["uColor"].SetValue(color.ToVector4());
				effect.Parameters["uOpacity"].SetValue(0f);

				effect.CurrentTechnique.Passes[0].Apply();

				sb.Draw(bloomCircle, Projectile.Center - Main.screenPosition, null, Color.White, 0f, bloomCircle.Size() / 2f, (Radius / 60f) * MathHelper.Lerp(0.2f, 0.45f, Eases.EaseCircularOut(Progress)), 0f, 0f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			});

			return false;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = [];

				for (int i = 0; i < 40; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			float strength = MathHelper.Lerp(15f, 5f, Progress);

			for (int k = 0; k < 40; k++)
			{
				cache[k] = Projectile.Center + Main.rand.NextVector2CircularEdge(strength, strength) + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius;
			}

			while (cache.Count > 40)
			{
				cache.RemoveAt(0);
			}

			if (cache2 is null)
			{
				cache2 = [];

				for (int i = 0; i < 40; i++)
				{
					cache2.Add(Projectile.Center);
				}
			}

			strength = MathHelper.Lerp(3f, 1f, Progress);

			for (int k = 0; k < 40; k++)
			{
				cache2[k] = Projectile.Center + Main.rand.NextVector2CircularEdge(strength, strength) + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius * 0.85f;
			}

			while (cache2.Count > 40)
			{
				cache2.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 9 * (1f - Progress),
				factor => new Color(100, 100, 255));

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 18 * (1f - Progress),
				factor => Color.Lerp(new Color(50, 50, 255, 0), new Color(220, 65, 65, 0), Eases.EaseQuinticInOut(1f - Progress)));

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[39];

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[39];

			trail3 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 9 * (1f - Progress),
				factor => Color.White);

			trail4 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 18 * (1f - Progress),
				factor => Color.Lerp(new Color(210, 100, 255, 0), new Color(85, 65, 255, 0), Eases.EaseQuinticInOut(1f - Progress)));

			trail3.Positions = cache2.ToArray();
			trail3.NextPosition = cache2[39];

			trail4.Positions = cache2.ToArray();
			trail4.NextPosition = cache2[39];
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
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
					effect.Parameters["repeats"].SetValue(5f);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

					//trail3?.Render(effect);
					//trail4?.Render(effect);

					trail2?.Render(effect);
					trail?.Render(effect);
				}
			});
		}
	}
}