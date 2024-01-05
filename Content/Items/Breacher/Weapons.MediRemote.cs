using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Items.SpaceEvent;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Breacher
{
	public class MediRemote : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + "Scrapshot";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("M.E.D.I Remote");
			Tooltip.SetDefault("Summons a Medical Energy Delivery Instrument, to siphon health energy from your enemies\nAfter siphoning enough health energy, it drops a healing beacon\nOnly one can be summoned at a time");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 5;
			Item.knockBack = 0f;
			Item.mana = 30;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<MediRemoteSummonBuff>();
			Item.shoot = ModContent.ProjectileType<MediRemoteProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile proj = Main.projectile.Where(p => p.active && p.owner == player.whoAmI && p.type == type).FirstOrDefault();

			if (proj != default)
				proj.active = false;

			player.AddBuff(Item.buffType, 2);
			Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer).originalDamage = Item.damage;

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<Dusts.GlowFastDecelerate>(),
					Main.rand.NextVector2CircularEdge(3f, 3f), 0, new Color(0, 100, 255), 0.5f);
			}

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.MeteoriteBar, 10)
				.AddIngredient<Astroscrap>(5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class MediRemoteProjectile : ModProjectile
	{
		public const int MAX_CHARGE = 120;

		private List<Vector2> cache;
		private Trail trail;

		public int lifetime;
		public float armRotation;

		public ref float TargetWhoAmI => ref Projectile.ai[0];
		public ref float AttackTimer => ref Projectile.ai[1];
		public ref float CurrentCharge => ref Projectile.ai[2];

		public float TrailOpacity => FoundTarget ? (Projectile.Distance(TargetAttackPosition) > 250f ? 0 : 1f - Projectile.Distance(TargetAttackPosition) / 250f) : 0f;

		public Vector2 ArmPosition => Projectile.Center + new Vector2(0f, 20f).RotatedBy(Projectile.rotation);

		public Vector2 TargetAttackPosition => FoundTarget ? Target.Center + new Vector2(0f, -125f - Target.height) : Vector2.Zero; // the position in which the summon attacks enemies from
		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public bool FoundTarget => Target != null;

		public Player Owner => Main.player[Projectile.owner];

		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}
		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Medical Energy Delivery Instrument");
			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(20);
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.minion = true;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void OnSpawn(IEntitySource source)
		{
			TargetWhoAmI = -1f;
		}

		public override void AI()
		{
			UpdateProjectileLifetime();

			if (MinionTarget != null)
				TargetWhoAmI = MinionTarget.whoAmI;

			bool any = Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<MediRemoteHealthBeacon>() && p.owner == Owner.whoAmI);

			if (CurrentCharge >= MAX_CHARGE && !any)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, -Vector2.UnitY.RotatedByRandom(1f) * 5f, ModContent.ProjectileType<MediRemoteHealthBeacon>(), 0, 0f, Projectile.owner);
				CurrentCharge = 0;
			}

			if (!FoundTarget)
			{
				DoIdleMovement();

				armRotation = MathHelper.Lerp(Projectile.rotation, Projectile.velocity.X, 0.1f) + MathHelper.PiOver2;

				if (TargetWhoAmI < 0)
				{
					NPC target = FindTarget();
					if (target != default)
						TargetWhoAmI = target.whoAmI;
				}
			}
			else
			{
				DoAttackingMovement();

				armRotation = MathHelper.Lerp(Projectile.rotation, Projectile.DirectionTo(Target.Center).ToRotation(), 1f);
				if (!Main.dedServ)
				{
					ManageCaches();
					ManageTrail();
				}

				if (++AttackTimer % 30 == 0 && Projectile.Distance(TargetAttackPosition) < 250f)
				{
					int damage = Target.SimpleStrikeNPC(Projectile.damage, 1, false, 0f, DamageClass.Summon, false, 0f);

					if (CurrentCharge < MAX_CHARGE)
						CurrentCharge += damage;

					if (CurrentCharge > MAX_CHARGE)
						CurrentCharge = MAX_CHARGE;
				}

				if (!Target.active || Target.Distance(Owner.Center) > 1000f)
				{
					TargetWhoAmI = -1;
					AttackTimer = 0;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail(Main.spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_Blur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D armTex = ModContent.Request<Texture2D>(Texture + "_Arm").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.BreacherItem + "SupplyBeaconProj_Star").Value;

			Main.spriteBatch.Draw(armTex, ArmPosition - Main.screenPosition, null, lightColor, armRotation, armTex.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : 0, 0f);

			Effect effect = Filters.Scene["DistortSprite"].GetShader().Shader;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.005f);
			effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.005f);
			effect.Parameters["screenPos"].SetValue(Main.screenPosition * new Vector2(0.5f, 0.1f) / new Vector2(Main.screenWidth, Main.screenHeight));

			effect.Parameters["offset"].SetValue(new Vector2(0.001f));
			effect.Parameters["repeats"].SetValue(2);
			effect.Parameters["uImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);
			effect.Parameters["uImage2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/PerlinNoise").Value);

			Color color = new Color(0, 255, 100, 0) * (CurrentCharge / (float)MAX_CHARGE);

			effect.Parameters["uColor"].SetValue(color.ToVector4());
			effect.Parameters["noiseImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/PerlinNoise").Value);

			effect.CurrentTechnique.Passes[0].Apply();

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, texGlow.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			if (FoundTarget && Projectile.Distance(TargetAttackPosition) < 250f)
			{
				Main.spriteBatch.Draw(bloomTex, ArmPosition - Main.screenPosition, null, new Color(0, 255, 100, 0) * TrailOpacity, 0f, bloomTex.Size() / 2f, TrailOpacity * 0.45f, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, ArmPosition - Main.screenPosition, null, new Color(0, 0, 255, 0) * TrailOpacity, 0f, bloomTex.Size() / 2f, TrailOpacity * 0.35f, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, Target.Center - Main.screenPosition, null, new Color(0, 255, 100 ,0) * TrailOpacity, 0f, bloomTex.Size() / 2f, TrailOpacity * 0.5f, 0f, 0f);

				Main.spriteBatch.Draw(starTex, Target.Center - Main.screenPosition, null, new Color(0, 255, 100, 0) * TrailOpacity, 0f, starTex.Size() / 2f, new Vector2(0.75f, TrailOpacity * 0.75f), 0f, 0f);

				Main.spriteBatch.Draw(starTex, Target.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * TrailOpacity, 0f, starTex.Size() / 2f, new Vector2(0.75f, TrailOpacity * 0.75f), 0f, 0f);
			}

			return false;
		}

		internal void DoIdleMovement()
		{
			float sin = (float)Math.Sin(lifetime * 0.1f);
			if (sin < 0f)	
				sin *= -1f;

			Vector2 idlePos = Owner.Center + new Vector2(50f * Owner.direction, -125f + 25f * EaseBuilder.EaseCircularInOut.Ease(sin));

			float dist = Vector2.Distance(Projectile.Center, idlePos);

			Vector2 toIdlePos = idlePos - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = 20f;
				if (dist < 1000f)
					speed = MathHelper.Lerp(5f, 20f, dist / 1000f);

				if (dist < 100f)
					speed = MathHelper.Lerp(0.1f, 5f, dist / 100f);

				toIdlePos.Normalize();
				toIdlePos *= speed;
			}

			Projectile.velocity = (Projectile.velocity * (5f - 1) + toIdlePos) / 5f;

			if (dist > 2000f)
			{
				Projectile.Center = idlePos;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}

			Projectile.rotation = Projectile.velocity.X * 0.085f;

			if (Projectile.velocity.Length() < 0.1f)
				Projectile.velocity *= 0f;

			Projectile.spriteDirection = Owner.direction;
		}

		internal void DoAttackingMovement()
		{
			float sin = (float)Math.Sin(lifetime * 0.1f);
			if (sin < 0f)
				sin *= -1f;

			Vector2 idlePos = TargetAttackPosition;

			float dist = Vector2.Distance(Projectile.Center, idlePos);

			Vector2 toIdlePos = idlePos - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = 20f;
				if (dist < 1000f)
					speed = MathHelper.Lerp(5f, 20f, dist / 1000f);

				if (dist < 100f)
					speed = MathHelper.Lerp(0.1f, 5f, dist / 100f);

				toIdlePos.Normalize();
				toIdlePos *= speed;
			}
			
			Projectile.velocity = (Projectile.velocity * (25f - 1) + toIdlePos) / 25f;

			if (dist > 2000f)
			{
				Projectile.Center = idlePos;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}

			Projectile.rotation = Projectile.velocity.X * 0.085f;

			if (Projectile.velocity.Length() < 0.1f)
				Projectile.velocity *= 0f;

			Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
		}

		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<MediRemoteSummonBuff>())
				Projectile.timeLeft = 2;

			lifetime++;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		#region PRIMITIVE DRAWING
		private BezierCurve GetBezierCurve()
		{
			float lerper = 1f - Vector2.Distance(Projectile.Center, Target.Center) / 250f;

			Vector2[] curvePoints =
			{
				Vector2.Lerp(Projectile.Center + Projectile.velocity, Target.Center, 0.2f) + new Vector2(0f, -MathHelper.Lerp(30f, 10f, lerper) * (float)Math.Sin(lifetime * 0.05f)).RotatedBy(Projectile.DirectionTo(Target.Center).ToRotation()),
			};

			var curve = new BezierCurve(new Vector2[] {
				ArmPosition + Projectile.velocity,
				curvePoints[0],
				Target.Center
			});

			return curve;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 26; i++)
				{
					cache.Add(Projectile.Center + Projectile.velocity);
				}
			}

			for (int k = 0; k < 26; k++)
			{
				BezierCurve curve = GetBezierCurve();

				int points = 26;
				Vector2[] curvePositions = curve.GetPoints(points).ToArray();
				cache[k] = curvePositions[k];
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 26, new TriangularTip(4), factor => 5f, factor =>
			{
				return new Color(0, 255, 100) * 0.6f * TrailOpacity;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[25];
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			/*effect.Parameters["intensity"].SetValue(new Vector2(0.1f, 0.15f));
			effect.Parameters["sinTime"].SetValue(Lifetime * 0.075f);*/
			effect.Parameters["time"].SetValue(lifetime * -0.02f);
			effect.Parameters["repeats"].SetValue(1);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			if (FoundTarget)
			{
				trail?.Render(effect);
			}

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		#endregion PRIMITIVEDRAWING
	}

	class MediRemoteHealthBeacon: ModProjectile
	{
		public int lifetime;
		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Medical Energy Delivery Instrument Beacon");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 40;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
		}

		public override void AI()
		{
			lifetime++;
			if (lifetime <= 60)
				Projectile.scale = MathHelper.Lerp(0.1f, 1f, lifetime / 60f);

			Projectile.velocity.Y += 0.15f;
			if (Projectile.velocity.Y > 0)
			{
				if (Projectile.velocity.Y < 12f)
					Projectile.velocity.Y *= 1.05f;
				else
					Projectile.velocity.Y *= 1.025f;
			}

			if (Projectile.velocity.Y > 16f)
				Projectile.velocity.Y = 16f;

			Projectile.velocity.X *= 0.925f;

			Projectile.rotation += Projectile.velocity.Length() * 0.015f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, Projectile.direction == 1 ? SpriteEffects.FlipHorizontally : 0, 0f);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity *= 0f;
			Projectile.rotation = 0f;

			return false;
		}
	}
}
