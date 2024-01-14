using StarlightRiver.Core.Systems.InstancedBuffSystem;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	internal class HeartStatueSentryItem : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Heart Statue?");
			Tooltip.SetDefault("Summons a heart statue powered by your enemie's souls");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.DamageType = DamageClass.Summon;
			Item.damage = 12;
			Item.UseSound = SoundID.Tink;
			Item.shoot = ModContent.ProjectileType<HeartStatueSentry>();
			Item.shootSpeed = 1;
			Item.sentry = true;
			Item.mana = 20;
			Item.noMelee = true;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = player.GetModPlayer<ControlsPlayer>().mouseWorld;
			velocity = Vector2.Zero;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.altFunctionUse != 2)
			{
				Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
				player.UpdateMaxTurrets();
			}

			return false;
		}
	}

	internal class HeartStatueSentry : ModProjectile
	{
		public const int RADIUS = 300;

		public bool wasFiring;

		public int damageDone;

		public Player Owner => Main.player[Projectile.owner];

		public ref float Timer => ref Projectile.ai[0];
		public ref float PulseTime => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Heart Statue?");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.tileCollide = true;
			Projectile.penetrate = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.sentry = true;
			Projectile.timeLeft = Projectile.SentryLifeTime;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Owner.FindSentryRestingSpot(Projectile.whoAmI, out int worldX, out int worldY, out int pushYUp);
			Projectile.position = new Vector2(worldX, worldY - pushYUp - 8);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override void AI()
		{
			Timer++;

			if (PulseTime > 0)
				PulseTime--;

			if (Timer == 1) // After summoning you get a head-start to spawning a heart
				damageDone = 350;

			wasFiring = false;

			foreach (NPC npc in Main.npc.Where(n => n.active && Vector2.DistanceSquared(n.Center, Projectile.Center) < Math.Pow(RADIUS / 2, 2) && n.CanBeChasedBy(Projectile)))
			{
				BuffInflictor.Inflict<HeartStatueDrain>(npc, 1);
				damageDone += 1;
				wasFiring = true;
			}

			if (wasFiring)
				Lighting.AddLight(Projectile.Center, Color.Red.ToVector3());

			if (damageDone > 500)
			{
				Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Hitbox, ItemID.Heart);
				PulseTime = 30;
				damageDone = 0;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			foreach (NPC npc in Main.npc.Where(n => n.active && Vector2.DistanceSquared(n.Center, Projectile.Center) < Math.Pow(RADIUS / 2, 2) && n.CanBeChasedBy(Projectile)))
			{
				DrawLine(Main.spriteBatch, npc.Center);
			}

			Texture2D statueTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			Texture2D radTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/GlowRing").Value;
			Texture2D tickTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/DirectionalBeam").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;

			int frameX = wasFiring ? 34 : 0;
			int frameY = wasFiring ? (46 * ((int)Timer % 20 / 5)) : 0;
			var source = new Rectangle(frameX, frameY, 34, 46);

			if (PulseTime > 0)
			{
				Color pulseColor = new Color(255, 50, 50) * (PulseTime / 30f);
				pulseColor.A = 0;

				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition + Vector2.UnitY * -16, null, pulseColor, 0, bloomTex.Size() / 2f, (1 - PulseTime / 30f) * 3, 0, 0);
			}

			Main.spriteBatch.Draw(statueTex, Projectile.Center - Main.screenPosition, source, lightColor, 0, new Vector2(17, 23), 1, 0, 0);
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, source, Color.White, 0, new Vector2(17, 23), 1, 0, 0);

			Color radColor = new Color(155, 10, 40) * (0.4f + (float)Math.Sin(Timer / 10f) * 0.1f);
			radColor.A = 0;

			Main.spriteBatch.Draw(radTex, Projectile.Center - Main.screenPosition, null, radColor * 0.6f, 0, radTex.Size() / 2f, RADIUS / (float)radTex.Width, 0, 0);

			for (int k = 0; k < 30; k++)
			{
				float rot = k / 30f * 6.28f + Timer * 0.01f;
				Vector2 pos = Projectile.Center + Vector2.UnitX.RotatedBy(rot) * (RADIUS / 2f - 8);

				Color tickColor = new Color(255, 90, 90) * (0.2f + (float)Math.Sin(Timer / 10f + k / 30f * 6.28f) * 0.5f);
				tickColor.A = 0;

				Main.spriteBatch.Draw(tickTex, pos - Main.screenPosition, null, tickColor * 0.6f, rot + 1.57f, tickTex.Size() / 2f, 0.5f, 0, 0);
			}

			return false;
		}

		public void DrawLine(SpriteBatch spriteBatch, Vector2 endPoint)
		{
			Texture2D texBeam = ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value;

			float rotation = Projectile.Center.DirectionTo(endPoint).ToRotation();
			float distance = Vector2.Distance(Projectile.Center, endPoint);

			int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 20f);

			Color color = new Color(255, 60 + sin, 60 + sin) * (1 - distance / (RADIUS / 2f));
			color.A = 0;

			var origin = new Vector2(0, texBeam.Height / 2);

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/Wiggle").Value;

			effect.Parameters["time"].SetValue(Timer);

			effect.Parameters["freq1"].SetValue(2.0f);
			effect.Parameters["speed1"].SetValue(0.03f);
			effect.Parameters["amp1"].SetValue(0.14f);

			effect.Parameters["freq2"].SetValue(3.5f);
			effect.Parameters["speed2"].SetValue(0.1f);
			effect.Parameters["amp2"].SetValue(0.14f);

			effect.Parameters["colorIn"].SetValue(color.ToVector4());

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

			float height = texBeam.Height / 24f;
			int width = (int)(Projectile.Center - endPoint).Length();

			Vector2 pos = Projectile.Center - Main.screenPosition;

			var target = new Rectangle((int)pos.X, (int)pos.Y, width, (int)height);
			var target2 = new Rectangle((int)pos.X, (int)pos.Y, width, (int)(height * 1.4f));

			var source = new Rectangle((int)(Timer / 40f * texBeam.Width), 0, (int)(distance / texBeam.Width * texBeam.Width * 6), texBeam.Height);
			var source2 = new Rectangle((int)(Timer / 56f * texBeam.Width), 0, (int)(distance / texBeam.Width * texBeam.Width * 6), texBeam.Height);

			spriteBatch.Draw(texBeam, target, source, color, rotation, origin, 0, 0);
			spriteBatch.Draw(texBeam, target2, source2, color, rotation, origin, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			for (int i = 0; i < width; i += 10)
			{
				Lighting.AddLight(pos + Vector2.UnitX.RotatedBy(rotation) * i + Main.screenPosition, color.ToVector3() * height * 0.010f);
			}

			float opacity = height / (texBeam.Height / 2f) * 0.75f;

			Texture2D impactTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "Keys/GlowAlpha").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "GlowTrail").Value;

			spriteBatch.Draw(glowTex, target, source, color * 0.05f, rotation, new Vector2(0, glowTex.Height / 2), 0, 0);

			spriteBatch.Draw(impactTex, endPoint - Main.screenPosition, null, color * (height * 0.012f), 0, impactTex.Size() / 2, 0.8f, 0, 0);
			spriteBatch.Draw(impactTex, pos, null, color * (height * 0.02f), 0, impactTex.Size() / 2, 0.4f, 0, 0);
		}
	}

	internal class HeartStatueDrain : StackableBuff
	{
		public override string Name => "HeartStatueDrain";

		public override string DisplayName => "Heart Siphon";

		public override string Texture => AssetDirectory.Debug;

		public override bool Debuff => true;

		public override BuffStack GenerateDefaultStack(int duration)
		{
			return new BuffStack()
			{
				duration = duration
			};
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.lifeRegen -= 20;
		}
	}
}