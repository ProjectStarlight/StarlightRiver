using StarlightRiver.Content.Dusts;
using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Starwood
{
	public class StarwoodSlingshot : StarwoodItem
	{
		private int timesShot = 0;//Test: possible endless ammo

		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodSlingshot() : base(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodSlingshot_Alt").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Slingshot");
			Tooltip.SetDefault("Weaves together fallen stars \nConsumes ammo every 50 shots");
		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Ranged;
			Item.channel = true;
			Item.width = 18;
			Item.height = 34;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4f;
			Item.UseSound = SoundID.Item19;
			Item.shoot = ModContent.ProjectileType<StarwoodSlingshotProj>();
			Item.shootSpeed = 16f;
			Item.noMelee = true;
			Item.useAmmo = ItemID.FallenStar;
			Item.noUseGraphic = true;

			Item.value = Item.sellPrice(silver: 25);
		}

		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			timesShot++;

			if (timesShot >= 50)
			{
				timesShot = 0;
				return true;
			}

			return false;
		}
	}

	public class StarwoodSlingshotProj : ModProjectile
	{
		const int MIN_DAMAGE = 15;
		const int MAX_DAMAGE = 20;
		const int MIN_VELOCITY = 4;
		const int MAX_VELOCITY = 25;
		const float CHARGE_RATE = 0.02f;

		private bool empowered = false;
		private bool released = false;
		private bool fired = false;
		private float charge = 0;
		private Vector2 direction = Vector2.Zero;
		private Vector2 posToBe = Vector2.Zero;
		private Vector3 lightColor = new(0.4f, 0.2f, 0.1f);

		private float flickerTime = 0;

		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public sealed override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.alpha = 255;
			Projectile.timeLeft = 9999;
		}

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, lightColor);
			AdjustDirection();
			Player player = Main.player[Projectile.owner];
			player.TryGetModPlayer(out ControlsPlayer controlsPlayer);
			controlsPlayer.mouseRotationListener = true;
			player.ChangeDir(controlsPlayer.mouseWorld.X > player.position.X ? 1 : -1);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;

			if (Projectile.ai[0] == 0)
			{
				StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();

				if (mp.empowered)
					empowered = true;

				Projectile.ai[0]++;
			}

			posToBe = player.Center + direction * 40;
			Vector2 moveDirection = posToBe - Projectile.Center;

			float speed = (float)Math.Sqrt(moveDirection.Length());

			if (speed > 0.05f)
			{
				moveDirection.Normalize();
				moveDirection *= speed;
				Projectile.velocity = moveDirection;
			}
			else
			{
				Projectile.velocity = Vector2.Zero;
			}

			if (player.channel && !released)
			{
				Projectile.timeLeft = 15;

				if (charge < 1)
				{
					if ((charge + CHARGE_RATE) >= 1)
						Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.25f }, Projectile.Center);

					charge += CHARGE_RATE;
				}
			}
			else
			{
				if (!released)
				{
					player.itemTime = 15;
					player.itemAnimation = 15;
					released = true;
				}

				if (!fired)
				{
					for (int i = 0; i < 3; i++)
					{
						Vector2 dustVel = direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.8f, 2);

						if (empowered)
							Dust.NewDustPerfect(player.Center + direction.RotatedBy(-0.06f) * 25, ModContent.DustType<BlueStamina>(), dustVel);
						else
							Dust.NewDustPerfect(player.Center + direction.RotatedBy(-0.06f) * 25, ModContent.DustType<Stamina>(), dustVel);
					}
				}

				if (Projectile.timeLeft == 8)
				{
					if (Projectile.owner == Main.myPlayer)
					{
						Vector2 velocity = direction * Helper.LerpFloat(MIN_VELOCITY, MAX_VELOCITY, charge);
						int damage = (int)Helper.LerpFloat(MIN_DAMAGE, MAX_DAMAGE, charge);
						StarwoodSlingshotStar.frameToAssign = (int)(charge * 5) - 1;

						if ((int)(charge * 5) == 0)
							StarwoodSlingshotStar.frameToAssign++;

						Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<StarwoodSlingshotStar>(), damage, Projectile.knockBack, Projectile.owner);
					}

					fired = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteEffects spriteEffect = SpriteEffects.None;
			Vector2 offset = Vector2.Zero;

			if (Main.player[Projectile.owner].direction != 1)
				spriteEffect = SpriteEffects.FlipVertically;
			else
				offset = new Vector2(0, -6);

			Color color = lightColor;
			int frameHeight = (charge > 0.5f && !released) ? 30 : 0;

			if (empowered)
				frameHeight += 60;

			var frame = new Rectangle(0, frameHeight, 30, 30);
			Vector2 pos = (Main.player[Projectile.owner].Center - Main.screenPosition + new Vector2(0, Main.player[Projectile.owner].gfxOffY)).PointAccur() + offset;
			Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, pos, frame, color, direction.ToRotation(), new Vector2(8, 10), Projectile.scale, spriteEffect, 0);

			if (!fired)
			{
				for (float i = 0; i < charge; i += 0.2f)
				{
					Vector2 offset2 = (i * 6.28f + direction.ToRotation()).ToRotationVector2();

					if (charge > i + 0.33f || charge >= 1)
						offset2 = Vector2.Zero;
					else
						offset2 *= Helper.LerpFloat(0, 7, (float)Math.Sqrt((0.33f - (charge - i)) * 3));

					Texture2D fragmenttexture = ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodSlingshotParts").Value;
					var frame2 = new Rectangle(0, (int)(i * 5 * 24), 22, 24);

					if (empowered)
						frame2.Y += fragmenttexture.Height / 2;

					Vector2 pos2 = Projectile.Center - Main.screenPosition + offset2;
					var color2 = new Color(255, 255, 255, 1 - (0.33f - (charge - i)) * 5);
					Main.spriteBatch.Draw(fragmenttexture, pos2, frame2, color2, direction.ToRotation() + 1.57f, new Vector2(11, 12), Projectile.scale, SpriteEffects.None, 0);
				}
			}

			if (flickerTime < 16 && charge >= 1 && !fired)
			{
				flickerTime += 0.5f;
				color = Color.White;
				float flickerTime2 = (float)(flickerTime / 20f);
				float alpha = 1.5f - (flickerTime2 * flickerTime2 / 2 + 2f * flickerTime2);

				if (alpha < 0)
					alpha = 0;

				Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodSlingshotStarWhite").Value;
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * alpha, direction.ToRotation() + 1.57f, new Vector2(11, 12), Projectile.scale, SpriteEffects.None, 0);
			}

			return false;
		}

		//helpers
		private void AdjustDirection(float deviation = 0f)
		{
			Player player = Main.player[Projectile.owner];
			player.TryGetModPlayer(out ControlsPlayer controlsPlayer);
			controlsPlayer.mouseRotationListener = true;
			direction = controlsPlayer.mouseWorld - (player.Center - new Vector2(4, 4)) - new Vector2(0, Main.player[Projectile.owner].gfxOffY);
			direction.Normalize();
			direction = direction.RotatedBy(deviation);
			player.itemRotation = direction.ToRotation();

			if (player.direction != 1)
				player.itemRotation -= 3.14f;
		}
	}

	public class StarwoodSlingshotStar : ModProjectile, IDrawAdditive
	{
		const int DAMAGE_INCREASE = 5;

		public static int frameToAssign;

		//These stats get scaled when empowered
		private float ScaleMult = 1.5f;
		private Vector3 lightColor = new(0.4f, 0.2f, 0.1f);
		private int dustType = ModContent.DustType<Stamina>(); //already implemented
		private bool empowered = false;
		private float rotationVar = 0;

		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Shooting Star");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 24;
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.aiStyle = 1;
			Main.projFrames[Projectile.type] = 10;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.frame = frameToAssign;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f + rotationVar;
			rotationVar += 0.4f;
			StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();

			if (!empowered && mp.empowered)
			{
				Projectile.frame += 5;
				lightColor = new Vector3(0.05f, 0.1f, 0.2f);
				ScaleMult = 2f;
				dustType = ModContent.DustType<BlueStamina>();
				Projectile.velocity *= 1.25f;//TODO: This could be on on the Item's side like the staff does, thats generally the better way
				empowered = true;
			}

			if (Projectile.timeLeft % 25 == 0)//delay between star sounds
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);

			Lighting.AddLight(Projectile.Center, lightColor);
			Projectile.velocity.X *= 0.995f;

			if (Projectile.velocity.Y < 50)
				Projectile.velocity.Y += 0.25f;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (empowered)
				modifiers.SourceDamage += DAMAGE_INCREASE;
		}

		public override void Kill(int timeLeft)
		{
			Helpers.DustHelper.DrawStar(Projectile.Center, dustType, pointAmount: 5, mainSize: 1.2f * ScaleMult, dustDensity: 1f, pointDepthMult: 0.3f, rotationAmount: Projectile.rotation);
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

			for (int k = 0; k < 35; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.2f) * ScaleMult), 0, default, 1.5f);
			}

			if (empowered && Projectile.owner == Main.myPlayer)
			{
				for (int k = 0; k < 4; k++)
				{
					Vector2 velocity = -Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.25f, 0.25f)) * Main.rand.NextFloat(0.5f, 0.8f);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position, velocity, ModContent.ProjectileType<StarwoodSlingshotFragment>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, Main.rand.Next(2));
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			var drawOrigin = new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width * 0.5f, Projectile.height * 0.5f);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = Projectile.GetAlpha(Color.White) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length * 0.5f);
				float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length;

				Main.spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodSlingshotGlowTrail").Value,
				Projectile.oldPos[k] + drawOrigin - Main.screenPosition,
				new Rectangle(0, 24 * Projectile.frame, 22, 24),
				color,
				Projectile.oldRot[k],
				new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / 20),
				scale, default, default);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, 24 * Projectile.frame, 22, 24), Color.White, Projectile.rotation, new Vector2(11, 12), Projectile.scale, default, default);
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (Projectile.frame == 4 || Projectile.frame == 9)
			{
				for (int k = 0; k < Projectile.oldPos.Length; k++)
				{
					Color color = (empowered ? new Color(200, 220, 255) * 0.35f : new Color(255, 255, 200) * 0.3f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

					if (k <= 4)
						color *= 1.2f;

					float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.8f;
					Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Starwood/Glow").Value;

					spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() / 2, scale, default, default);
				}
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.frame);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.frame = reader.ReadInt32();
		}
	}

	public class StarwoodSlingshotFragment : ModProjectile
	{
		public override string Texture => AssetDirectory.StarwoodItem + "StarwoodSlingshotFragment";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Star Fragment");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.timeLeft = 9;
			Projectile.width = 12;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.aiStyle = -1;
			Projectile.rotation = Main.rand.NextFloat(4f);
		}

		public override void AI()
		{
			Projectile.rotation += 0.3f;
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 3; k++)
				Dust.NewDustPerfect(Projectile.position, ModContent.DustType<StarFragment>(), Projectile.velocity.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f)) * Main.rand.NextFloat(0.3f, 0.5f), 0, Color.White, 1.5f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.ai[0] > 0 ? 10 : 0, 12, 10), Color.White, Projectile.rotation, new Vector2(6, 5), Projectile.scale, default, default);
			return false;
		}
	}
}