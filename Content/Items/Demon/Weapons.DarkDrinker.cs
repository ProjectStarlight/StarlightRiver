using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Demon
{
	public class DarkDrinker : ModItem
	{
		public override string Texture => AssetDirectory.DemonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Darkdrinker");
			Tooltip.SetDefault("I dont get paid to write descriptions");
		}

		public override void SetDefaults()
		{
			item.damage = 20;
			item.melee = true;
			item.width = 36;
			item.height = 44;
			item.useTime = 12;
			item.useAnimation = 12;
			item.reuseDelay = 20;
			item.channel = true;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.knockBack = 6.5f;
			item.value = Item.sellPrice(0, 1, 0, 0);
			item.crit = 4;
			item.rare = 2;
			item.shootSpeed = 0f;
			item.shoot = ModContent.ProjectileType<DarkDrinkerSlash>();
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = true;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			float rot = 0;
			Vector2 frontPosition = player.Center + new Vector2(player.direction * 30, 0);
			var target = Main.npc.Where(x => x.Distance(frontPosition) < 250 && x.active && !x.townNPC && !x.friendly).OrderBy(x => x.Distance(Main.MouseWorld)).FirstOrDefault();
			if (target != default)
			{
				rot = (target.Center - frontPosition).ToRotation();
			}
			else
			{
				rot = (Main.MouseWorld - frontPosition).ToRotation();
			}

			Vector2 pos = frontPosition + ((rot + Main.rand.NextFloat(-0.3f, 0.3f)).ToRotationVector2() * Main.rand.Next(40));
			DarkDrinkerSlash mp = Projectile.NewProjectileDirect(pos, Vector2.Zero, type, damage, knockBack, player.whoAmI).modProjectile as DarkDrinkerSlash;
			mp.frameX = Main.rand.Next(4);
			mp.projectile.rotation = rot + Main.rand.NextFloat(-0.4f, 0.4f);
			return false;
		}
	}

	internal class DarkDrinkerSlash : ModProjectile
	{
		public override string Texture => AssetDirectory.DemonItem + Name;

		const int FRAMEHEIGHT = 76;
		const int FRAMEWIDTH = 76;

		public int frameX;
		public int frameY;

		private int frameCounter;
		private Color drawColor;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Dark Drinker");
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			projectile.friendly = true;
			projectile.melee = true;
			projectile.tileCollide = false;
			projectile.Size = new Vector2(76, 76);
			projectile.penetrate = -1;
			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 16;
			projectile.ownerHitCheck = true;

			Color color1 = new Color(165, 80, 210);
			Color color2 = new Color(250, 60, 80);
			drawColor = Color.Lerp(color1, color2, Main.rand.NextFloat());
		}

        public override void AI()
        {
			frameCounter++;
			if (frameCounter % 5 == 0)
            {
				frameY++;
				if (frameY > 3)
					projectile.active = false;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D tex = Main.projectileTexture[projectile.type];
			Rectangle frame = new Rectangle(frameX * FRAMEWIDTH, frameY * FRAMEHEIGHT, FRAMEWIDTH, FRAMEHEIGHT);

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, frame, drawColor, projectile.rotation, tex.Size() / 8, projectile.scale, SpriteEffects.None, 0); ;
			return false;
        }
    }
}