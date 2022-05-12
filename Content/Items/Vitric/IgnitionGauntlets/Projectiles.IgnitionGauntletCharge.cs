using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Vitric
{
	public class IgnitionGauntletCharge : ModProjectile
	{
		int charge = 0;
		public override string Texture => AssetDirectory.VitricItem + "IgnitionGauntletLaunch_Star";
		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			Projectile.hide = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
			overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }

        public override bool PreDraw(ref Color lightColor)
        {
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 handOffset = new Vector2(8 * owner.direction, 0);
			Main.spriteBatch.Draw(starTex, (owner.Center + handOffset) - Main.screenPosition, null, new Color(255, 255, 255, 0) * ((float)charge / (float)modPlayer.charge), (float)Main.GameUpdateCount * 0.085f, starTex.Size() / 2, 0.5f + (0.07f * (float)Math.Sin(Main.GameUpdateCount * 0.285f)), SpriteEffects.None, 0f);
			return false;
        }
        public override void AI()
		{
			IgnitionPlayer modPlayer = owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = owner.Center;
			if (Main.mouseRight)
			{
				owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -1.57f * owner.direction);
				if (modPlayer.charge > charge)
				{
					charge += 3;
				}
				if (modPlayer.charge - 50 > charge)
				{
					Dust dust = Dust.NewDustPerfect(owner.Center + new Vector2(Main.rand.Next(-25, 25), 25), ModContent.DustType<IgnitionChargeDust>(), default, default, Color.OrangeRed);
					dust.customData = owner.whoAmI;
				}
				modPlayer.potentialCharge = charge;
			}
			else
			{
				modPlayer.potentialCharge = 0;
				if (!owner.GetModPlayer<IgnitionPlayer>().launching)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), owner.Center, Vector2.Zero, ModContent.ProjectileType<IgnitionGauntletLaunch>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
					//owner.velocity = owner.DirectionTo(Main.MouseWorld) * 20;
				}

				//UNCOMMENT THIS BEFORE RELEASE
				modPlayer.charge -= charge;
				modPlayer.loadedCharge = charge;
				owner.GetModPlayer<IgnitionPlayer>().launching = true;
				Projectile.active = false;
			}
		}
	}
}