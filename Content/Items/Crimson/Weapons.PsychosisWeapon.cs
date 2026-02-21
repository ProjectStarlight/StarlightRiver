using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
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
			Vector2 itemPosition = CommonGunAnimations.SetGunUseStyle(player, Item, shootDirection, -10f, new Vector2(52f, 28f), new Vector2(-40f, 4f));

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;

			if (animProgress >= 0.5f)
			{
				float lerper = (animProgress - 0.5f) / 0.5f;
				Dust.NewDustPerfect(itemPosition + new Vector2(50f, -10f * player.direction).RotatedBy(player.compositeFrontArm.rotation + 1.5707964f * player.gravDir), DustID.Smoke, Vector2.UnitY * -2f, (int)MathHelper.Lerp(210f, 200f, lerper), default, MathHelper.Lerp(0.5f, 1f, lerper)).noGravity = true;
			}
		}

		public override void UseItemFrame(Player player)
		{
			CommonGunAnimations.SetGunUseItemFrame(player, shootDirection, shootRotation, -0.2f, true, new Vector2(0.3f, 0.7f));
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Color c = player.altFunctionUse == 2 ? Red() : Blue();

			Vector2 barrelPos = position + new Vector2(60f, -20f * player.direction).RotatedBy(velocity.ToRotation());

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.025f, 50, new Color(255, 0, 0), 0.1f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(255, 0, 0), 0.2f);

			Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), ModContent.DustType<FlareBreacherSmokeDust>(), velocity * 0.05f, 150, new Color(100, 100, 100), 0.2f);

			for (int i = 0; i < 4; i++)
			{
				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Torch, velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.25f), 0, default, 1.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelatedGlow>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, c, 0.2f).noGravity = true;

				Dust.NewDustPerfect(barrelPos + Main.rand.NextVector2Circular(5f, 5f),
					ModContent.DustType<PixelatedEmber>(), velocity.RotatedByRandom(0.5f).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f), 0, c, 0.2f).customData = -player.direction;

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
			Item.noUseGraphic = true;

			Projectile p = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, velocity * 2, type, damage, knockback, player.whoAmI);

			PsychosisWeaponGlobalProjectile gp = p.GetGlobalProjectile<PsychosisWeaponGlobalProjectile>();

			if (player.altFunctionUse == 2)
				gp.psychosis = true;
			else
				gp.neurosis = true;

			return false;
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
	}

	public class PsychosisWeaponGlobalProjectile : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool psychosis = false;
		public bool neurosis = false;

		int timer;

		static Color DrawColor(bool psych)
		{
			return psych ? PsychosisWeapon.Red() : PsychosisWeapon.Blue();
		}

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
				}

				for (int i = 0; i < 5; i++)
				{
					Color c = DrawColor(psychosis);

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedGlow>(), Main.rand.NextVector2Circular(2f, 2f), 0, c, 0.2f).noGravity = true;

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
				}

				for (int i = 0; i < 5; i++)
				{
					Color c = DrawColor(psychosis);

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedGlow>(), Main.rand.NextVector2Circular(2f, 2f), 0, c, 0.2f).noGravity = true;

					Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(5f, 5f),
						ModContent.DustType<PixelatedEmber>(), Main.rand.NextVector2CircularEdge(2f, 2f), 0, c, 0.2f).customData = Main.rand.NextBool() ? -1 : 1;
				}
			}
		}

		public override void AI(Projectile projectile)
		{
			if (psychosis || neurosis)
			{
				timer++;
				if (timer % 2 == 0)
				{
					Dust.NewDustPerfect(projectile.Center + new Vector2(0f, 1.5f * (float)Math.Sin(timer)).RotatedBy(projectile.velocity.ToRotation()) * 9f, ModContent.DustType<Dusts.PixelatedEmber>(),
						projectile.velocity * 0.05f, 0, DrawColor(psychosis), 0.2f * (1 + (float)Math.Sin(timer / 2)));
				}
			}
		}

		void Explode(Projectile p, NPC npcHit, int count, bool psych)
		{
			int damage = (int)(30 + 100 * (count / 10f));

			Color c = DrawColor(psych);

			if (Main.myPlayer == p.owner)
			{
				Projectile.NewProjectile(p.GetSource_FromThis(), p.Center, Vector2.Zero, 
					ModContent.ProjectileType<BrainBlastProjectile>(), damage, 2f, p.owner, 50, psych ? 1 : 0);
			}

			NPC[] closestNPCs = [.. Main.npc.Where(n => n.active && n.CanBeChasedBy() && n != npcHit && n.Distance(p.Center) < 500f).OrderBy(n => n.Distance(p.Center))];

			for (int i = 0; i < (closestNPCs.Length > 5 ? 5 : closestNPCs.Length); i++)
			{
				Projectile.NewProjectile(p.GetSource_FromThis(), p.Center + Main.rand.NextVector2CircularEdge(20f, 20f), Vector2.Zero,
					ModContent.ProjectileType<BrainBlastBolt>(), damage / 5, 1f, p.owner, closestNPCs[i].whoAmI);
			}
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

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(), Projectile.position.DirectionTo(Projectile.oldPosition), 0, Color.Lerp(new Color(20, 20, 255, 0), new Color(255, 255, 255, 0), Progress), 0.3f);

			Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(), Projectile.position.DirectionTo(Projectile.oldPosition).RotatedByRandom(1f) * Main.rand.NextFloat(0.8f, 3f), 0, new Color(200, 50, 50, 0), 0.2f);

			if (Projectile.timeLeft == 1)
			{
				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedEmber>(),
						-Vector2.UnitY.RotatedByRandom(1.5f) * Main.rand.NextFloat(0.8f, 2f), 0, new Color(Main.rand.Next(150, 220), 50, 255, 0), 0.1f).customData = Main.rand.NextBool() ? -1 : 1;
				}

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<PixelatedGlow>(),
						-Vector2.UnitY.RotatedByRandom(1.5f) * Main.rand.NextFloat(4f, 14f), 0, new Color(Main.rand.Next(150, 220), 50, 255, 0), 0.35f);
				}

				CameraSystem.shake += 4;

				deathTimer = 600;
				Projectile.timeLeft = 2;
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

				sb.Draw(bloomCircle, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.1f, 0.3f, 1f - flashProg), 0f, 0f);
				sb.Draw(bloomCircle, pos, null, new Color(255, 255, 255, 0) * flashProg, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.1f, 0.25f, 1f - flashProg), 0f, 0f);

				sb.Draw(bloomTex, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, bloomTex.Size() / 2f, 3f, 0f, 0f);
				sb.Draw(bloomTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, bloomTex.Size() / 2f, 2.5f, 0f, 0f);

				sb.Draw(bloomTex, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, bloomTex.Size() / 2f, stretch, 0f, 0f);
				sb.Draw(bloomTex, pos, null, new Color(255, 255, 255, 0) * flashProg * 0.8f, 0f, bloomTex.Size() / 2f, stretch * 0.75f, 0f, 0f);

				sb.Draw(starTex, pos, null, new Color(200, 80, 255, 0) * flashProg, 0f, starTex.Size() / 2f, stretch, 0f, 0f);
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
			trail ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(190), factor => factor * 4f, factor =>
			new Color(100, 100, 255) * TrailFade());

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 100, new TriangularTip(190), factor => factor * 12f, factor =>
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
		private float Progress => Utils.Clamp(1 - Projectile.timeLeft / 30f, 0f, 1f);

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
			Projectile.timeLeft = 30;

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

				sb.Draw(bloomCircle, Projectile.Center - Main.screenPosition, null, new Color(50, 50, 255, 0) * (1f - Progress), 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.2f, 0.45f, Eases.EaseCircularOut(Progress)), 0f, 0f);

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

				sb.Draw(bloomCircle, Projectile.Center - Main.screenPosition, null, Color.White, 0f, bloomCircle.Size() / 2f, MathHelper.Lerp(0.2f, 0.45f, Eases.EaseCircularOut(Progress)), 0f, 0f);

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