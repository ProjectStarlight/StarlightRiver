using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.GameContent;
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
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.knockBack = 0.5f;
            Item.value = 10;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ProjectileType<MagmaArrowProj>();
            Item.shootSpeed = 6f;
            Item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            var recipe = CreateRecipe(100);
            recipe.AddIngredient(ItemID.FlamingArrow, 100);
            recipe.AddIngredient(ItemType<MagmaCore>(), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
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
            Projectile.width = 7;
            Projectile.height = 7;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 400;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = 1;
            AIType = ProjectileID.WoodenArrowFriendly;
        }
        public override void AI()
        {
            if (coolness < 1 && moltenCounter <= 0)
                coolness += 0.015f;
            if (moltenCounter > 0)
                moltenCounter -= 0.05f;
            Lighting.AddLight(Projectile.Center, (Color.Orange * heat).ToVector3());
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var spriteBatch = Main.spriteBatch;

            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + "NeedlerBloom").Value;
            Color bloomColor = Color.Orange;
            bloomColor.A = 0;
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY) + ((Projectile.rotation - 1.57f).ToRotationVector2() * 10), new Rectangle(0, 0, tex.Width, tex.Height), //bloom
                bloomColor * heat, Projectile.rotation, new Vector2(tex.Width / 2, tex.Height / 2), Projectile.scale * 0.85f, SpriteEffects.None, 0);

            tex = TextureAssets.Projectile[Projectile.type].Value;

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(tex.Width / 3, 0, tex.Width / 3, tex.Height), //crystal arrow
                lightColor * coolness, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle(0, 0, tex.Width / 3, tex.Height), //magma arrow
                lightColor * heat, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Rectangle((tex.Width / 3) * 2, 0, tex.Width / 3, tex.Height), //white sprite
                Color.Lerp(Color.Orange, Color.White, moltenCounter) * moltenCounter, Projectile.rotation, new Vector2(tex.Width / 6, tex.Height / 2), Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}