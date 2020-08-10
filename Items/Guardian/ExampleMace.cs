using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Guardian
{
    internal class ExampleMace : Mace
    {
        public ExampleMace() : base(10, 6, 48, 4)
        {
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Iron Mace");
        }

        public override void SafeSetDefaults()
        {
            item.damage = 10;
            item.useTime = 15;
            item.useAnimation = 15;
            item.noUseGraphic = true;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.UseSound = SoundID.Item1;
            item.noMelee = true;
        }

        public override bool UseItem(Player player)
        {
            SpawnProjectile(ProjectileType<ExampleMaceProjectile>(), player);
            return true;
        }
    }

    internal class ExampleMaceProjectile : MaceProjectile
    {
        public override void SafeSetDefaults()
        {
            projectile.timeLeft = 10;
            projectile.width = 16;
            projectile.height = 16;
        }
    }
}