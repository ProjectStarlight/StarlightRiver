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
using Terraria.GameContent;

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
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6.5f;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.crit = 4;
			Item.rare = 2;
			Item.shootSpeed = 0f;
			Item.shoot = ModContent.ProjectileType<DarkDrinkerSlash>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			float rot = 0;
			Vector2 frontPosition = Player.Center + new Vector2(Player.direction * 30, 0);
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
			DarkDrinkerSlash mp = Projectile.NewProjectileDirect(pos, Vector2.Zero, type, damage, knockBack, Player.whoAmI).ModProjectile as DarkDrinkerSlash;
			mp.TileFrameX = Main.rand.Next(4);
			mp.Projectile.rotation = rot + Main.rand.NextFloat(-0.4f, 0.4f);
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
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(76, 76);
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 16;
			Projectile.ownerHitCheck = true;

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
					Projectile.active = false;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = new Rectangle(frameX * FRAMEWIDTH, frameY * FRAMEHEIGHT, FRAMEWIDTH, FRAMEHEIGHT);

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, drawColor, Projectile.rotation, tex.Size() / 8, Projectile.scale, SpriteEffects.None, 0); ;
			return false;
        }
    }
}