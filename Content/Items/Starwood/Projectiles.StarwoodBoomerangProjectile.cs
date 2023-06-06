using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Starwood
{
	public class StarwoodBoomerangProjectile : ModProjectile, IDrawAdditive
	{
		private const int CHARGE_TIME = 50; //how long it takes to charge up

		private float chargeMult; //a multiplier used during charge up, used both in ai and for drawing (goes from 0 to 1)

		//These stats get scaled when empowered
		private int ScaleMult = 2;
		private Vector3 lightColor = new(0.4f, 0.2f, 0.1f);
		private int dustType = DustType<Dusts.Stamina>();
		private bool empowered = false;

		private const int MaxTimeLeft = 1200;
		private const int MaxDistTime = MaxTimeLeft - 30;

		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Boomerang");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.timeLeft = MaxTimeLeft;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.aiStyle = -1;
		}

		public override void AI()
		{
			Player projOwner = Main.player[Projectile.owner];

			Projectile.rotation += 0.3f;

			if (Projectile.timeLeft == MaxTimeLeft)
			{
				StarlightPlayer mp = Main.player[Projectile.owner].GetModPlayer<StarlightPlayer>();

				if (mp.empowered)
				{
					Projectile.frame = 1;
					lightColor = new Vector3(0.1f, 0.2f, 0.4f);
					ScaleMult = 3;
					dustType = DustType<Dusts.BlueStamina>();
					empowered = true;
				}
			}

			Lighting.AddLight(Projectile.Center, lightColor * 0.5f);

			switch (Projectile.ai[0])
			{
				case 0://flying outward
					if (empowered)
					{
						Projectile.velocity += Vector2.Normalize(Main.MouseWorld - Projectile.Center);

						if (Projectile.velocity.Length() > 10)//swap this for shootspeed or something
							Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10; //cap to max speed
					}

					if (Projectile.timeLeft < MaxDistTime)
						NextPhase(0);

					break;

				case 1://has hit something
					if (projOwner.controlUseItem || Projectile.ai[1] >= CHARGE_TIME - 5)
					{
						if (Projectile.ai[1] == 0)
							SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/ImpactHeal"), Projectile.Center);

						chargeMult = Projectile.ai[1] / (CHARGE_TIME + 3);
						Projectile.ai[1]++;
						Projectile.velocity *= 0.75f;
						Lighting.AddLight(Projectile.Center, lightColor * chargeMult);

						if (Projectile.ai[1] >= CHARGE_TIME + 3) //reset stats and start return phase
						{
							Projectile.position = Projectile.Center;
							Projectile.width = 18;
							Projectile.height = 18;
							Projectile.Center = Projectile.position;

							for (int k = 0; k < Projectile.oldPos.Length; k++)
							{
								Projectile.oldPos[k] = Projectile.position;
							}

							NextPhase(1);
						}//ai[]s reset here
						else if (Projectile.ai[1] == CHARGE_TIME) //change hitbox size, stays for 3 frames
						{
							Projectile.position = Projectile.Center;
							Projectile.width = 67 * ScaleMult;
							Projectile.height = 67 * ScaleMult;
							Projectile.Center = Projectile.position;

							for (int k = 0; k < Projectile.oldPos.Length; k++)
							{
								Projectile.oldPos[k] = Projectile.position;
							}
						}
						else if (Projectile.ai[1] == CHARGE_TIME - 5) //sfx
						{
							Helpers.DustHelper.DrawStar(Projectile.Center, dustType, pointAmount: 5, mainSize: 2.25f * ScaleMult, dustDensity: 2, pointDepthMult: 0.3f);
							Lighting.AddLight(Projectile.Center, lightColor * 2);
							SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/MagicAttack"), Projectile.Center);

							for (int k = 0; k < 50; k++)
							{
								Dust.NewDustPerfect(Projectile.Center, dustType, Vector2.One.RotatedByRandom(6.28f) * (Main.rand.NextFloat(0.25f, 1.5f) * ScaleMult), 0, default, 1.5f);
							}
						}
					}
					else
					{
						NextPhase(1); // ai[]s and damage reset here
					}

					break;

				case 2://heading back
					if (Vector2.Distance(projOwner.Center, Projectile.Center) < 24)
						Projectile.Kill();
					else if (Vector2.Distance(projOwner.Center, Projectile.Center) < 200)
						Projectile.velocity += Vector2.Normalize(projOwner.Center - Projectile.Center) * 4;
					else
						Projectile.velocity += Vector2.Normalize(projOwner.Center - Projectile.Center);

					if (Projectile.velocity.Length() > 10)//swap this for shootspeed or something
						Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10;//cap to max speed

					break;
			}

			if (Projectile.ai[0] != 1)
			{
				if (Projectile.timeLeft % 8 == 0)
				{
					SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
					Dust.NewDustPerfect(Projectile.Center, dustType, (Projectile.velocity * 0.5f).RotatedByRandom(0.5f), Scale: Main.rand.NextFloat(0.8f, 1.5f));
				}
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Projectile.ai[0] == 1)
			{
				if (Projectile.ai[1] >= CHARGE_TIME - 3 && Projectile.ai[1] <= CHARGE_TIME + 3)
				{
					modifiers.SourceDamage *= ScaleMult;
					modifiers.Knockback *= ScaleMult;
					modifiers.HitDirectionOverride = target.Center.X > Projectile.Center.X ? 1 : -1;
				}
				else
				{
					modifiers.SourceDamage -= int.MaxValue;
					modifiers.Knockback *= 0.1f;
				}
			}
			else if (empowered)
			{
				modifiers.SourceDamage *= 2f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			NextPhase(0, true);
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			NextPhase(0, true);
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			NextPhase(0, true);
		}

		private Texture2D GlowingTrail => Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodBoomerangGlowTrail").Value;
		private Texture2D GlowingTexture => Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodBoomerangGlow").Value;
		private Texture2D AuraTexture => Request<Texture2D>(AssetDirectory.StarwoodItem + "Glow").Value;

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			var drawOrigin = new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width * 0.5f, Projectile.height * 0.5f);

			if (Projectile.ai[0] != 1)
			{
				for (int k = 0; k < Projectile.oldPos.Length; k++)
				{
					Color color = Projectile.GetAlpha(Color.White) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length * 0.5f);
					float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length;

					spriteBatch.Draw(GlowingTrail,
					Projectile.oldPos[k] + drawOrigin - Main.screenPosition,
					new Rectangle(0, TextureAssets.Projectile[Projectile.type].Value.Height / 2 * Projectile.frame, TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / 2),
					color,
					Projectile.rotation,
					new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / 4),
					scale, default, default);
				}
			}

			spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, TextureAssets.Projectile[Projectile.type].Value.Height / 2 * Projectile.frame, TextureAssets.Projectile[Projectile.type].Value.Width, TextureAssets.Projectile[Projectile.type].Value.Height / 2),
				lightColor,
				Projectile.rotation,
				new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width / 2, TextureAssets.Projectile[Projectile.type].Value.Height / 4),
				1f, default, default);

			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = AuraTexture;

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				if (!(Projectile.ai[0] == 1 && (Projectile.oldPos[k] / 5).ToPoint() == (Projectile.position / 5).ToPoint()))
				{
					Color color = (empowered ? new Color(70, 90, 100) : new Color(100, 90, 60)) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

					if (k <= 4)
						color *= 1.2f;

					float scale = Projectile.scale * (Projectile.oldPos.Length - k) / Projectile.oldPos.Length * 0.8f;

					spriteBatch.Draw(tex, Projectile.oldPos[k] + Projectile.Size / 2 - Main.screenPosition, null, color, 0, tex.Size() * 0.5f, scale * 0.5f, default, default);
				}
			}

			Texture2D tex2 = Request<Texture2D>(AssetDirectory.StarwoodItem + "Glow2").Value;//a
			spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, tex2.Frame(), new Color(255, 255, 200, 75) * (Projectile.ai[1] / CHARGE_TIME), 0, tex2.Size() * 0.5f, (-chargeMult + 1) * 1f, 0, 0);
		}

		public override void PostDraw(Color lightColor)
		{
			Color color = Color.White * (chargeMult + 0.25f);
			var source = new Rectangle(0, GlowingTexture.Height / 2 * Projectile.frame, GlowingTexture.Width, GlowingTexture.Height / 2);

			Main.spriteBatch.Draw(GlowingTexture, Projectile.Center - Main.screenPosition, source, color, Projectile.rotation, new Vector2(GlowingTexture.Width / 2, GlowingTexture.Height / 4), 1f, default, default);
		}

		private void NextPhase(int phase, bool bounce = false)
		{
			if (phase == 0 && Projectile.ai[0] == phase)
			{
				if (bounce)
					Projectile.velocity = -Projectile.velocity;

				Projectile.tileCollide = false;
				Projectile.ignoreWater = true;
				Projectile.ai[0] = 1;
			}
			else if (phase == 1 && Projectile.ai[0] == phase)
			{
				Projectile.velocity.Y += 1f;
				Projectile.ai[0] = 2;
				Projectile.ai[1] = 0;
			}
		}
	}
}