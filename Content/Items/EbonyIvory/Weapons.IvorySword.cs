using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.EbonyIvory
{
    public class IvorySword : ModItem
    {
        private int combostate = 0;

        public override void SetDefaults()
        {
            item.width = 44;
            item.height = 44;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.useAnimation = 16;
            item.useTime = 16;
            item.knockBack = 2f;
            item.damage = 46;
            item.shoot = ProjectileType<Projectiles.WeaponProjectiles.IvorySwordProjectile>();
            item.rare = ItemRarityID.LightRed;
            item.autoReuse = false;
            item.useTurn = true;
            item.noMelee = true;
            item.melee = true;
            item.noUseGraphic = true;
        }

        public override bool CloneNewInstances => true;

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Vector2 aim = Vector2.Normalize(Main.MouseWorld - player.Center);
            int proj = Projectile.NewProjectile(player.Center + aim * 18, aim * 0.1f, type, damage, knockBack, player.whoAmI);
            Main.projectile[proj].localAI[1] = combostate;

            if (combostate < 2) { Main.PlaySound(SoundID.Item65, player.Center); }
            else { Main.PlaySound(SoundID.Item64, player.Center); }

            combostate++;
            if (combostate > 2) { combostate = 0; }
            return false;
        }

        public override bool UseItem(Player player)
        {
            return true;
        }

        public override void HoldItem(Player player)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ivory Rapier");
            Tooltip.SetDefault("Huzzah!");
        }
    }
}