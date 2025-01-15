using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Infernal
{
	internal class Lighthunter : ModItem
	{
		public const int MAX_CHARGE = 70;

		public int charge;

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/Lighthunter";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lighthunter");
			Tooltip.SetDefault("Hold to charge a powerful bolt\nThe bolt explodes into arrows on impact\n'A draw weight heavier than your sins.'");
		}

		public override void SetDefaults()
		{
			Item.damage = 60;
			Item.crit = 10;
			Item.rare = ItemRarityID.Orange;
			Item.DamageType = DamageClass.Ranged;
			Item.useAmmo = AmmoID.Arrow;
			Item.channel = true;
			Item.useTime = 5;
			Item.useAnimation = 5;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}

		public override void HoldItem(Player player)
		{
			// Create the held projectile if not present
			if (!Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<LighthunterHeld>() && n.owner == player.whoAmI))
			{
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<LighthunterHeld>(), 0, 0, player.whoAmI);
			}

			if (player.channel && charge < MAX_CHARGE)
				charge++;

			if (!player.channel && charge >= MAX_CHARGE)
			{
				Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center, player.Center.DirectionTo(Main.MouseWorld) * 30, ModContent.ProjectileType<LighthunterBolt>(), Item.damage, Item.knockBack, player.whoAmI, player.ChooseAmmo(Item).shoot, 0);
			}

			if (!player.channel)
				charge = 0;
		}

		public override bool CanUseItem(Player player)
		{
			return !player.channel;
		}

		public override bool? UseItem(Player player)
		{
			return base.UseItem(player);
		}
	}

	internal class LighthunterHeld : ModProjectile
	{
		public override string Texture => "StarlightRiver/Assets/Items/Infernal/LighthunterHeld";

		public Player Owner => Main.player[Projectile.owner];

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			// Stay indefinately so long as the owner is holding the item
			if (Owner.HeldItem.type == ModContent.ItemType<Lighthunter>())
				Projectile.timeLeft = 2;

			// Get the item, we should only continue if we have it
			Lighthunter item = Owner.HeldItem.ModItem as Lighthunter;

			if (item is null)
				return;

			// Adjsut rotation only if this is the owners client
			if (Owner.whoAmI == Main.myPlayer)
			{
				Projectile.rotation = Owner.Center.DirectionTo(Main.MouseWorld).ToRotation();
				Owner.direction = Main.MouseWorld.X > Owner.Center.X ? 1 : -1;
			}

			// Move the players arms
			var stretch = Player.CompositeArmStretchAmount.Full;

			if (item.charge > 0)
				stretch = Player.CompositeArmStretchAmount.ThreeQuarters;

			if (item.charge > 35)
				stretch = Player.CompositeArmStretchAmount.Quarter;

			if (item.charge >= 70)
				stretch = Player.CompositeArmStretchAmount.None;

			Owner.SetCompositeArmFront(true, stretch, Projectile.rotation - 1.57f - 0.2f);
			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);

			// Center and set as held proj
			Projectile.Center = Owner.GetBackHandPosition(Owner.compositeBackArm.stretch, Owner.compositeBackArm.rotation);
			Owner.heldProj = Projectile.whoAmI;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = Assets.Items.Infernal.LighthunterHeld.Value;

			Vector2 direction = Vector2.UnitX.RotatedBy(Projectile.rotation);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation - 1.57f, tex.Size() / 2f + Vector2.UnitY * 2, 1f, direction.X > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			return false;
		}
	}

	internal class LighthunterBolt : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public override string Texture => AssetDirectory.Invisible;

		public ref float NovaType => ref Projectile.ai[0];
		public ref float State => ref Projectile.ai[1];

		public bool Exploded => State == 1;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 2;
			Projectile.friendly = true;
			Projectile.timeLeft = 300;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (State == 0)
			{
				for (int k = 0; k < 2; k++)
				{
					Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Cinder>(), -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 0, new Color(255, 100 + Main.rand.Next(100), 50), Main.rand.NextFloat(0.25f, 0.75f));
				}

				Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.3f, 0.1f));
			}
			else if (State == 1)
			{
				Projectile.extraUpdates = 0;

				if (Projectile.timeLeft > 15)
					Projectile.timeLeft = 15;

				Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.4f, 0.1f) * Projectile.timeLeft / 15f);
			}

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 30; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 30)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 30, new NoTip(), factor => factor * 20, factor =>
				{
					if (factor.X > 0.95f)
						return Color.White * 0;

					float alpha = 1;

					if (Projectile.timeLeft < 15)
						alpha = Projectile.timeLeft / 15f;

					return new Color(255, 100 + (int)(factor.X * 100), (int)(factor.X * 100)) * (float)Math.Sin(factor.X * 3.14f) * alpha;
				});
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		/// <summary>
		/// Fires the nova of arrows for this projectiles impact effects
		/// </summary>
		public void Nova()
		{
			for (int k = 0; k < 8; k++)
			{
				float rot = k / 8f * 6.28f;
				var proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitX.RotatedBy(rot) * 12, (int)NovaType, Projectile.damage, Projectile.knockBack, Projectile.owner);
			}

			// Visual dusts
			Helper.PlayPitched("Magic/FireHit", 0.25f, Main.rand.NextFloat(-0.2f, 0.2f), Projectile.Center);

			var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(255, 200, 100));
			d.customData = 1.8f;

			for (int k = 0; k < 40; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(9, 9), 0, new Color(255, Main.rand.Next(40, 200), 40), Main.rand.NextFloat(1.5f));
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.GetGlobalNPC<TrueImmunityNpc>().immuneTime[Projectile.owner] = 30;

			if (State == 0)
			{			
				Nova();

				Projectile.velocity *= 0;
				State = 1;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (State == 0)
			{
				Nova();

				Projectile.velocity *= 0;
				State = 1;
			}

			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return base.CanHitNPC(target);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var glow = Assets.Keys.GlowAlpha.Value;
			var spike = Assets.Misc.SpikeTell.Value;

			if (State == 0)
			{
				var spikeSource = new Rectangle(spike.Width / 2, 0, spike.Width / 2, spike.Height);

				var glowOrigin = new Vector2(glow.Width, glow.Height / 2f);
				var spikeOrigin = new Vector2(spike.Width / 4f, spike.Height / 8f);

				Color glowColor = new Color(255, 100, 0, 0) * 0.5f;
				Color spikeColor = new Color(255, 200, 40, 0) * 0.8f;
				Color spikeColor2 = new Color(255, 255, 150, 0);

				Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, glowOrigin, new Vector2(2f, 0.4f), 0, 0);
				Main.spriteBatch.Draw(spike, Projectile.Center - Main.screenPosition, spikeSource, spikeColor, Projectile.rotation + 1.57f, spikeOrigin, new Vector2(0.4f, 2f), 0, 0);
				Main.spriteBatch.Draw(spike, Projectile.Center - Main.screenPosition, spikeSource, spikeColor2, Projectile.rotation + 1.57f, spikeOrigin, new Vector2(0.3f, 1.4f), 0, 0);
			}

			if (State == 1 && Projectile.timeLeft <= 15)
			{
				Texture2D tex3 = Assets.StarTexture.Value;
				Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, new Color(255, 100 + (int)(Projectile.timeLeft / 15f * 155), 50, 0) * (Projectile.timeLeft / 15f), 0, glow.Size() / 2, (15 - Projectile.timeLeft) * 0.5f, 0, 0);
				Main.spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, null, new Color(255, 100 + (int)(Projectile.timeLeft / 15f * 155), 50, 0) * (Projectile.timeLeft / 15f) * 1.5f, 0, tex3.Size() / 2, (15 - Projectile.timeLeft) * 0.17f, 0, 0);
				Main.spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, null, new Color(255, 100 + (int)(Projectile.timeLeft / 15f * 155), 50, 0) * (Projectile.timeLeft / 15f) * 1.5f, 1.57f / 2f, tex3.Size() / 2, (15 - Projectile.timeLeft) * 0.17f, 0, 0);

				Main.spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, null, new Color(255, 200 + (int)(Projectile.timeLeft / 15f * 155), 150, 0) * (Projectile.timeLeft / 15f) * 1.5f, 0, tex3.Size() / 2, (15 - Projectile.timeLeft) * 0.1f, 0, 0);
			}

			return false;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.3f);
			effect.Parameters["repeats"].SetValue(5f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(Assets.EnergyTrail.Value);

			trail?.Render(effect);
		}
	}
}
