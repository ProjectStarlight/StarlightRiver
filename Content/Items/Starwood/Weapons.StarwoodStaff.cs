using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Starwood
{
    public class StarwoodStaff : StarwoodItem
    {
        public override string Texture => AssetDirectory.StarwoodItem + Name;
        public StarwoodStaff() : base(ModContent.GetTexture(AssetDirectory.StarwoodItem + "StarwoodStaff_Alt")) { }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Starwood Staff");
            Tooltip.SetDefault("Yabba Dabba Doo");
            Item.staff[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.damage = 20;
            item.magic = true;
            item.mana = 10;
            item.width = 18;
            item.height = 34;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.UseSound = SoundID.Item43;
            item.knockBack = 0f;
            item.shoot = ModContent.ProjectileType<StarwoodStaffProjectile>();
            item.shootSpeed = 15f;
            item.noMelee = true;
            item.autoReuse = true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            StarlightPlayer mp = Main.player[player.whoAmI].GetModPlayer<StarlightPlayer>();
            int amount = mp.Empowered ? 4 : 3;
            int projDamage = (int)(damage * (mp.Empowered ? 1.3f : 1f));//TODO: actually change the item itself's damage
            float projSpeedX = speedX * (mp.Empowered ? 1.05f : 1f);
            float projSpeedY = speedY * (mp.Empowered ? 1.05f : 1f);

            Vector2 staffEndPosition = player.Center + Vector2.Normalize(Main.MouseWorld - position) * 45;//this makes it spawn a distance from the player, useful for other stuff

            for (int k = 0; k < amount; k++)
                Projectile.NewProjectile(staffEndPosition, new Vector2(projSpeedX, projSpeedY).RotatedBy(Main.rand.NextFloat(-0.05f, 0.05f) * (k * 0.10f + 1)) * Main.rand.NextFloat(0.9f, 1.1f) * (k * 0.15f + 1), type, projDamage, knockBack, player.whoAmI, Main.rand.NextFloat(-0.025f, 0.025f), Main.rand.Next(50));

            for (int k = 0; k < 10; k++)
                Dust.NewDustPerfect(staffEndPosition + new Vector2(Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-5f, 15f)), mp.Empowered ? ModContent.DustType<Dusts.BlueStamina>() : ModContent.DustType<Dusts.Stamina>(), (new Vector2(projSpeedX, projSpeedY) * Main.rand.NextFloat(0.01f, 0.1f)).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) + player.velocity * 0.5f, 0, default, 1.5f);
            return false;
        }

    }
}