using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	public class MagmaArrow : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Magma Arrow");
            Tooltip.SetDefault("Becomes more cooled as it travels");
        }

        public override void SetDefaults()
        {
            item.damage = 8;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;
            item.knockBack = 0.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<MagmaArrowProj>();
            item.shootSpeed = 6f;
            item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            var r = new ModRecipe(mod);
            r.AddIngredient(ItemID.FlamingArrow, 100);
            r.AddIngredient(ItemType<MagmaCore>(), 1);
            r.SetResult(this, 100);
            r.AddRecipe();
        }
    }

    internal class MagmaArrowProj : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        private float coolness = 0;
        private float moltenCounter = 1;
        private float heat
        {
            get
            {
                return 1 - coolness;
            }
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Magma Arrow");
        }
        public override void SetDefaults()
        {
            projectile.width = 7;
            projectile.height = 7;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 400;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.aiStyle = 1;
            aiType = ProjectileID.WoodenArrowFriendly;
        }
        public override void AI()
        {
            if (coolness < 1 && moltenCounter <= 0)
                coolness += 0.015f;
            if (moltenCounter > 0)
                moltenCounter -= 0.05f;
            Lighting.AddLight(projectile.Center, (Color.Orange * heat).ToVector3());
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = ModContent.GetTexture(AssetDirectory.VitricItem + "NeedlerBloom");
            Color bloomColor = Color.Orange;
            bloomColor.A = 0;
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY) + ((projectile.rotation - 1.57f).ToRotationVector2() * 10), new Rectangle(0, 0, tex.Width, tex.Height), //bloom
                bloomColor * heat, projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), projectile.scale * 0.85f, SpriteEffects.None, 0);

            tex = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), new Rectangle(tex.Width / 3, 0, tex.Width / 3, tex.Height), //crystal arrow
                lightColor * coolness, projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), new Rectangle(0, 0, tex.Width / 3, tex.Height), //magma arrow
                lightColor * heat, projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), projectile.scale, SpriteEffects.None, 0);

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition + new Vector2(0, projectile.gfxOffY), new Rectangle((tex.Width / 3) * 2, 0, tex.Width / 3, tex.Height), //white sprite
                Color.Lerp(Color.Orange, Color.White, moltenCounter) * moltenCounter, projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}