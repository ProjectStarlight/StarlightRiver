using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class AztecDeathSaxophone : ModItem
	{
		public const float MAX_CHARGE = 60f;

		public float charge;

		public bool flashed;

		public int flashTimer;

		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void Load()
		{
			StarlightPlayer.OnHitByNPCEvent += StarlightPlayer_OnHitByNPCEvent;
			StarlightPlayer.OnHitByProjectileEvent += StarlightPlayer_OnHitByProjectileEvent;

			StarlightPlayer.OnHitNPCEvent += StarlightPlayer_OnHitNPCEvent;
			StarlightPlayer.OnHitNPCWithProjEvent += StarlightPlayer_OnHitNPCWithProjEvent;
		}

		private void StarlightPlayer_OnHitNPCWithProjEvent(Player player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (player.HasItem(Type))
				HitEffects(player, target);
		}

		private void StarlightPlayer_OnHitNPCEvent(Player player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (player.HasItem(Type))
				HitEffects(player, target);
		}

		private void StarlightPlayer_OnHitByProjectileEvent(Player player, Projectile projectile, Player.HurtInfo info)
		{
			if (player.HasItem(Type))
				HurtEffects(player, info.Damage, projectile);
		}

		private void StarlightPlayer_OnHitByNPCEvent(Player player, NPC npc, Player.HurtInfo info)
		{
			if (player.HasItem(Type))
				HurtEffects(player, info.Damage, npc);
		}

		public void HitEffects(Player Player, NPC target)
		{
			if (target.life <= 0)
			{
				IncreaseCharge(Player, 0.75f);

				for (int i = 0; i < 15; i++)
				{
					Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(Player.Center).RotatedByRandom(0.5f) * Main.rand.NextFloat(5f, 15f), 0, new Color(255, 0, 0), 0.65f);
				}
			}
		}

		public void HurtEffects(Player Player, int damage, Entity attacker)
		{
			bool valid = damage >= 10;

			if (valid)
			{
				IncreaseCharge(Player);

				for (int i = 0; i < 15; i++)
				{
					Dust.NewDustPerfect(Player.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Player.Center.DirectionTo(attacker.Center).RotatedByRandom(0.35f) * Main.rand.NextFloat(2f, 10f), 0, new Color(255, 0, 0), 0.45f);
				}
			}
		}

		public void IncreaseCharge(Player Player, float increase = 2)
		{
			for (int i = 0; i < Player.inventory.Length; i++)
			{
				Item item = Player.inventory[i];

				if (item.ModItem is AztecDeathSaxophone sax && sax.charge < MAX_CHARGE)
				{
					sax.charge += increase;
					if (sax.charge > MAX_CHARGE)
						sax.charge = MAX_CHARGE;
				}
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Death Saxophone"); //TODO: better name?
			Tooltip.SetDefault("Take damage and kill foes with other weapons to charge the saxophone\nOnce charged, use it to unleash a high-damage roar of primal death"); //TODO: make this not a skippzz tooltip :)
		}

		public override void SetDefaults()
		{
			Item.damage = 30;
			Item.DamageType = DamageClass.Magic;
			Item.knockBack = 3f;
			Item.useTime = 75;
			Item.useAnimation = 75;

			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Green;

			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.width = 32;
			Item.height = 32;

			Item.shoot = ModContent.ProjectileType<AztecDeathSaxophoneHoldout>();
			Item.shootSpeed = 1f;
		}

		public override void UpdateInventory(Player player)
		{
			if (flashTimer > 0)
				flashTimer--;

			if (charge == MAX_CHARGE && !flashed)
			{
				flashTimer = 45;
				flashed = true;
			}
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[ModContent.ProjectileType<AztecDeathSaxophoneHoldout>()] <= 0 && charge >= MAX_CHARGE;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			charge = 0;
			flashed = false;
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			spriteBatch.Draw(tex, position, frame, drawColor, 0f, origin, scale, 0f, 0f);

			Color color = new Color(200, 0, 0, 0) * MathHelper.Lerp(0f, 1f, charge / MAX_CHARGE);

			if (flashTimer > 0)
				color = Color.Lerp(new Color(255, 255, 255, 0), new Color(200, 0, 0, 0), 1f - flashTimer / 45f);

			spriteBatch.Draw(texGlow, position + new Vector2(-1), null, color, 0f, origin, scale, 0f, 0f);

			spriteBatch.Draw(glowTex, position + new Vector2(-65, -75), null, color, 0f, origin, 1f, 0f, 0f);

			return false;
		}
	}

	class AztecDeathSaxophoneHoldout : ModProjectile
	{
		public int originalTimeleft;

		private Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Aztec Death Saxophone");
		}

		public override void SetDefaults()
		{
			Projectile.width = 36;
			Projectile.height = 32;

			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Magic;
		}

		public override bool? CanDamage()
		{
			return false;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.timeLeft = Owner.itemTime;
			originalTimeleft = Projectile.timeLeft;
		}

		public override void AI()
		{
			Vector2 position = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
			position += Vector2.Normalize(Owner.direction * Vector2.UnitX) * 15f;

			Projectile.position = position - Projectile.Size * 0.5f;

			Projectile.spriteDirection = Owner.direction;

			if (Main.myPlayer == Owner.whoAmI)
				Owner.ChangeDir(Main.MouseWorld.X < Owner.Center.X ? -1 : 1);

			Owner.heldProj = Projectile.whoAmI;

			Owner.itemTime = 2;
			Owner.itemAnimation = 2;

			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (MathHelper.PiOver2 + MathHelper.PiOver4) * -Owner.direction);

			if (Projectile.timeLeft > (int)(originalTimeleft * 0.65f))
			{
				float lerper = MathHelper.Lerp(50f, 1f, 1f - (Projectile.timeLeft - (int)(originalTimeleft * 0.65f)) / (float)(originalTimeleft * 0.35f));
				Vector2 offset = Main.rand.NextVector2CircularEdge(lerper, lerper);
				Vector2 pos = Owner.Center + new Vector2(30f * Owner.direction, 10f) + (Owner.direction == -1 ? Vector2.UnitX * 3f : Vector2.Zero);
				Dust.NewDustPerfect(pos + offset, ModContent.DustType<Dusts.GlowFastDecelerate>(), (pos + offset).DirectionTo(pos), 0, new Color(255, 0, 0), 0.75f);

				Dust.NewDustPerfect(pos + offset, ModContent.DustType<Dusts.GlowLineFast>(), (pos + offset).DirectionTo(pos) * 3f, 0, new Color(255, 0, 0), 1f);
			}
			else
			{
				if (Projectile.timeLeft == (int)(originalTimeleft * 0.65f))
				{
					SoundStyle style = SoundID.Roar;
					style.MaxInstances = 0;

					SoundEngine.PlaySound(style with { Pitch = -0.5f }, Owner.Center);
					SoundEngine.PlaySound(style with { Pitch = -0.15f }, Owner.Center);
					SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = -1f }, Owner.Center);
				}

				if (Projectile.timeLeft % ((int)(originalTimeleft * 0.65f) / 5) == 0)
				{
					CameraSystem.shake += 10;

					Vector2 pos = Owner.Center + new Vector2(30f * Owner.direction, 10f) + (Owner.direction == -1 ? Vector2.UnitX * 3f : Vector2.Zero);

					Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<AztecDeathSaxophoneSoundwave>(), Projectile.damage, Projectile.knockBack, Projectile.owner).rotation = Main.rand.NextFloat(6.28f);

					for (int i = 0; i < 30; i++)
					{
						Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2CircularEdge(10f, 10f), 0, new Color(255, 0, 0), 0.95f);

						Dust.NewDustPerfect(pos + new Vector2(0, 32f), ModContent.DustType<AztecDeathSaxophoneVelocityLine>(), Main.rand.NextVector2CircularEdge(40f, 40f) * Main.rand.NextFloat(0.5f, 1f), 0, new Color(255, 0, 0), 1.5f);
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			SpriteEffects flip = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : 0f;

			spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, new Color(255, 0, 0, 0), 0f, texGlow.Size() / 2f, Projectile.scale, flip, 0f);

			spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(255, 0, 0, 0), 0f, glowTex.Size() / 2f, 0.65f, 0f, 0f);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, tex.Size() / 2f, Projectile.scale, flip, 0f);

			return false;
		}
	}

	class AztecDeathSaxophoneSoundwave : ModProjectile
	{
		public float Radius => 50 * Projectile.scale;

		public float RadiusIncrease => 0.15f + 0.35f * EaseBuilder.EaseCircularInOut.Ease(Projectile.timeLeft / 45f);
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;

			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.tileCollide = false;

			Projectile.timeLeft = 45;

			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI()
		{
			Projectile.scale += RadiusIncrease;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			return Helper.CheckCircularCollision(Projectile.Center, (int)Radius, targetHitbox);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_Blurred").Value;

			var color = new Color(100, 0, 0, 0);
			if (Projectile.timeLeft < 10)
				color *= Projectile.timeLeft / 10f;

			Main.spriteBatch.Draw(texBlur, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, texBlur.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}

	class AztecDeathSaxophoneVelocityLine : ModDust
	{
		public override string Texture => AssetDirectory.VitricBoss + "RoarLine";

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			float curveOut = Curve(1 - dust.fadeIn / 30f);
			var color = Color.Lerp(dust.color, new Color(255, 0, 0), dust.fadeIn / 45f);
			dust.color = color * (curveOut + 0.4f);
			return dust.color;
		}

		float Curve(float input) //shrug it works, just a cubic regression for a nice looking curve
		{
			return -2.65f + 19.196f * input - 32.143f * input * input + 15.625f * input * input * input;
		}

		public override void OnSpawn(Dust dust)
		{
			dust.color = Color.Transparent;
			dust.fadeIn = 0;
			dust.noLight = false;
			dust.frame = new Rectangle(0, 0, 8, 128);

			dust.shader = new Terraria.Graphics.Shaders.ArmorShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value), "GlowingDustPass");
		}

		public override bool Update(Dust dust)
		{
			if (dust.color == Color.Transparent)
				dust.position -= new Vector2(4, 32) * dust.scale;

			dust.rotation = dust.velocity.ToRotation() + 1.57f;
			dust.position += dust.velocity;
			dust.shader.UseColor(dust.color);
			dust.fadeIn++;

			Lighting.AddLight(dust.position, dust.color.ToVector3() * 0.6f);

			if (dust.fadeIn > 45)
				dust.active = false;

			dust.velocity *= 0.95f;
			return false;
		}
	}
}