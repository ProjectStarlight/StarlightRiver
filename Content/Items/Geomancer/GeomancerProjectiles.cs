using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Geomancer
{
	public class EmeraldHeart : ModItem
	{
		int timer = 900;

		public override string Texture => AssetDirectory.GeomancerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Emerald Heart");
			Tooltip.SetDefault("You shouldn't see this");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 1;
		}

		public override bool ItemSpace(Player Player)
		{
			return true;
		}

		public override bool OnPickup(Player Player)
		{
			int healAmount = (int)MathHelper.Min(Player.statLifeMax2 - Player.statLife, 5);
			Player.HealEffect(5);
			Player.statLife += healAmount;

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, Player.position);
			return false;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			timer--;
			if (timer <= 0)
				Item.active = false;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(200, 200, 200, 100);
		}
	}

	public class SapphireStar : ModItem
	{
		public override string Texture => AssetDirectory.GeomancerItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sapphire Star");
			Tooltip.SetDefault("You shouldn't see this");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.maxStack = 1;
		}

		public override bool ItemSpace(Player Player)
		{
			return true;
		}

		public override bool OnPickup(Player Player)
		{
			int healAmount = (int)MathHelper.Min(Player.statManaMax2 - Player.statMana, 5);
			Player.ManaEffect(20);
			Player.statMana += healAmount;

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, Player.position);
			return false;
		}

		public override Color? GetAlpha(Color lightColor)
		{
			return new Color(200, 200, 200, 100);
		}
	}

	public class TopazShieldFade : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.GeomancerItem + "TopazShield";

		private float progress => (30 - Projectile.timeLeft) / 30f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Topaz Shield");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = false;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.timeLeft = 30;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];

			Vector2 direction = Main.MouseWorld - Player.Center;
			direction.Normalize();

			Projectile.rotation = direction.ToRotation();

			Projectile.Center = Player.Center + direction * 35;
		}
		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			float transparency = (float)Math.Pow(1 - progress, 2);
			float scale = 1f + progress * 2;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * transparency, Projectile.rotation, tex.Size() / 2, Projectile.scale * scale, SpriteEffects.None, 0f);
		}
	}

	public class TopazShield : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.GeomancerItem + "TopazShield";

		private const int EXPLOSIONTIME = 2;

		private readonly bool initialized = false;

		private Player owner => Main.player[Projectile.owner];

		private BarrierPlayer shieldPlayer => owner.GetModPlayer<BarrierPlayer>();

		public int shieldLife => 100 + (shieldPlayer.barrier - shieldPlayer.maxBarrier);

		public float shieldSpring = 0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Topaz Shield");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = false;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];

			if (shieldLife < 0 && Projectile.timeLeft > EXPLOSIONTIME)
				Projectile.timeLeft = EXPLOSIONTIME;

			Vector2 direction = Main.MouseWorld - Player.Center;
			direction.Normalize();

			Projectile.rotation = direction.ToRotation();

			shieldSpring *= 0.9f;

			Projectile.Center = Player.Center + direction * MathHelper.Lerp(35, 26, shieldSpring);

			if (Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.Topaz || Player.GetModPlayer<GeomancerPlayer>().storedGem == StoredGem.All)
			{
				if (Projectile.timeLeft > EXPLOSIONTIME)
					Projectile.timeLeft = EXPLOSIONTIME + 2;
			}
			else
			{
				Projectile.active = false;
			}

			if (Projectile.timeLeft > EXPLOSIONTIME)
			{
				for (int k = 0; k < Main.maxProjectiles; k++)
				{
					Projectile proj = Main.projectile[k];

					if (proj.active && proj.hostile && proj.damage > 1 && proj.Hitbox.Intersects(Projectile.Hitbox))
					{
						int diff = proj.damage / 2 - shieldLife;

						if (diff <= 0)
						{
							shieldSpring = 1;
							proj.penetrate -= 1;
							proj.friendly = true;
							Player.GetModPlayer<BarrierPlayer>().barrier -= proj.damage / 2;
							CombatText.NewText(Projectile.Hitbox, Color.Yellow, proj.damage / 2);
						}
						else
						{
							CombatText.NewText(Projectile.Hitbox, Color.Yellow, proj.damage / 2);
							proj.damage -= shieldLife;
							shieldPlayer.barrier = shieldPlayer.maxBarrier - 100;
							Projectile.timeLeft = EXPLOSIONTIME;
							return;
						}
					}
				}
			}
			else
			{
				/*float progress = EXPLOSIONTIME - Projectile.timeLeft;

                float deviation = (float)Math.Sqrt(progress) * 0.08f;
                Projectile.rotation += Main.rand.NextFloat(-deviation,deviation);

                Vector2 dustDir = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Dust.NewDustPerfect(Projectile.Center - (dustDir * 50), DustID.TopazBolt, dustDir * 10, 0, default, (float)Math.Sqrt(progress) * 0.3f).noGravity = true;*/
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (Projectile.timeLeft <= EXPLOSIONTIME)
				return false;
			return base.CanHitNPC(target);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = Math.Sign(Projectile.Center.X - Main.player[Projectile.owner].Center.X);
			Player Player = Main.player[Projectile.owner];

			Player.GetModPlayer<BarrierPlayer>().barrier -= target.damage;
			shieldSpring = 1;
			CombatText.NewText(Projectile.Hitbox, Color.Yellow, target.damage);

			if (shieldLife <= 0)
			{
				shieldPlayer.barrier = shieldPlayer.maxBarrier - 100;
				Projectile.timeLeft = EXPLOSIONTIME;
			}
		}

		public override void Kill(int timeLeft)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TopazShieldFade>(), 0, 0, Projectile.owner);
			CameraSystem.shake += 4;
			var direction = Vector2.Normalize(Main.MouseWorld - Main.player[Projectile.owner].Center);
			for (int i = 0; i < 4; i++)
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(0.6f, 1f) * 15, ModContent.ProjectileType<TopazShard>(), Projectile.damage * 2, Projectile.knockBack, Projectile.owner);
		}
	}

	public class TopazShard : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.GeomancerItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Topaz Shard");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(8, 8);
			Projectile.penetrate = 1;
			Projectile.hide = true;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			NPC target = Main.npc.Where(x => x.active && !x.friendly && !x.townNPC && Projectile.Distance(x.Center) < 150).OrderBy(x => Projectile.Distance(x.Center)).FirstOrDefault();
			if (target != default)
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * 30, 0.05f);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 3; i++)
			{
				float rand = Main.rand.NextFloat(0.3f);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz, Projectile.velocity.X * rand, Projectile.velocity.Y * rand);
			}
		}
		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
		}
	}

	public class AmethystShard : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoAmethyst";

		private bool initialized = false;
		private Vector2 offset = Vector2.Zero;

		private int fadeIn;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Amethyst Shard");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(16, 16);
			Projectile.penetrate = 1;
			Projectile.hide = true;
			Projectile.timeLeft = 3;
		}

		public override void AI()
		{
			NPC target = Main.npc[(int)Projectile.ai[1]];
			if (!initialized)
			{
				initialized = true;
				offset = Projectile.Center - target.Center;
			}

			if (fadeIn < 15)
				fadeIn++;

			Projectile.scale = fadeIn / 15f;

			if (!target.active || target.life <= 0 || Projectile.ai[0] >= target.GetGlobalNPC<GeoNPC>().amethystDebuff)
				return;
			Projectile.timeLeft = 2;

			Projectile.Center = target.Center + offset;

			Vector2 direction = target.Center - Projectile.Center;
			Projectile.rotation = direction.ToRotation();
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			for (int k = Projectile.oldPos.Length - 1; k > 0; k--) //TODO: Clean this shit up
			{
				Vector2 drawPos = Projectile.oldPos[k] + new Vector2(Projectile.width, Projectile.height) / 2;
				Color color = Color.White * (float)((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
			}

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * (fadeIn / 15f), Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 4; i++)
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemAmethyst).velocity *= 1.4f;
		}
	}

	public class RubyDagger : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

		Vector2 direction = Vector2.Zero;

		private readonly List<float> oldRotation = new();

		private bool launched = false;
		private bool lockedOn = false;
		private int launchCounter = 15;
		private float rotation = 0f;

		private float radiansToSpin = 0f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ruby Dagger");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(16, 16);
			Projectile.penetrate = 1;
			radiansToSpin = 6.28f * Main.rand.Next(2, 5) * Math.Sign(Main.rand.Next(-100, 100));
			Projectile.hide = true;

		}

		public override void AI()
		{

			oldRotation.Add(Projectile.rotation);
			while (oldRotation.Count > Projectile.oldPos.Length)
			{
				oldRotation.RemoveAt(0);
			}

			NPC target = Main.npc[(int)Projectile.ai[0]];

			if (!lockedOn)
			{
				if (target.active && target.life > 0)
				{
					direction = Vector2.Normalize(target.Center - Projectile.Center);
				}
				else
				{
					NPC temptarget = Main.npc.Where(x => x.active && !x.townNPC && !x.immortal && !x.dontTakeDamage && !x.friendly).OrderBy(x => x.Distance(Projectile.Center)).FirstOrDefault();
					if (temptarget != default)
					{
						Projectile.ai[0] = temptarget.whoAmI;
						target = temptarget;
					}
				}
			}

			if (!launched)
			{
				Projectile.timeLeft = 50;
				Projectile.velocity *= 0.96f;
				rotation = MathHelper.Lerp(rotation, direction.ToRotation() + radiansToSpin, 0.02f);
				Projectile.rotation = rotation + 1.57f; //using a variable here cause I think Projectile.rotation automatically caps at 2 PI?

				float difference = Math.Abs(rotation - (direction.ToRotation() + radiansToSpin));
				if (difference < 0.7f || lockedOn)
				{
					Projectile.velocity *= 0.8f;
					if (difference > 0.2f)
					{
						rotation += 0.1f * Math.Sign(direction.ToRotation() + radiansToSpin - rotation);
					}
					else
					{
						rotation = direction.ToRotation() + radiansToSpin;
					}

					lockedOn = true;
					launchCounter--;
					Projectile.Center -= direction * 3;
					if (launchCounter <= 0)
						launched = true;
				}
				else
				{
					rotation += 0.15f * Math.Sign(direction.ToRotation() + radiansToSpin - rotation);
				}
			}
			else
			{
				if (target.active && target.life > 0)
					direction = Vector2.Normalize(target.Center - Projectile.Center);
				Projectile.velocity = direction * 30;
				Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			var origin = new Vector2(tex.Width / 2, tex.Height);

			SpriteEffects effects = Math.Sign(radiansToSpin) == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			for (int k = Projectile.oldPos.Length - 1; k > 0; k--) //TODO: Clean this shit up
			{
				Vector2 drawPos = Projectile.oldPos[k] + new Vector2(Projectile.width, Projectile.height) / 2;
				Color color = Color.White * (float)((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				if (k > 0 && k < oldRotation.Count)
					spriteBatch.Draw(tex, drawPos - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, Projectile.scale, effects, 0f);
			}

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2, Projectile.scale, effects, 0f);
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 0; i < 10; i++)
				Dust.NewDustPerfect(Projectile.Center, DustID.GemRuby, Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f)) * Main.rand.NextFloat(0.4f), 0, default, 1.25f).noGravity = true;
			;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}
}