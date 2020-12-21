using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
//Things left to do for L-clck
//Make it held out, with dust properly coming off
//-Add prims (or dust) to projectile while it's moving
//-Make it consume flares
//-Touch up flare explosion
//-Add sound effects
//-Make flares embed in ground
//-Add casing gores?
//-glowmasks
using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Misc
{
    public class FlareBreacher : ModItem
    {
        public override string Texture => Directory.MiscItem + Name;
        public override void SetDefaults()
        {
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 18;
            item.useTime = 18;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item11;
            item.width = 24;
            item.height = 28;
            item.damage = 28;
            item.rare = ItemRarityID.Red;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = false;
            // item.useAmmo = AmmoID.Flare;
            item.ranged = true;
            item.shoot = ModContent.ProjectileType<ExplosiveFlare>();
            item.shootSpeed = 17;
            //item.holdStyle = 1;
        }

        //public override void HoldItem(Player player)
        //{
        //unused
        //}
        //public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        //{
        //unused
        //}

        public override Vector2? HoldoutOffset() => new Vector2(0, 0);
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Flare Breacher");
            Tooltip.SetDefault("Left click to launch explosive flares \nRight click to launch a target flare");
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            position.Y -= 4;
            return base.Shoot(player, ref position, ref speedX, ref speedY, ref type, ref damage, ref knockBack);
        }
    }
}