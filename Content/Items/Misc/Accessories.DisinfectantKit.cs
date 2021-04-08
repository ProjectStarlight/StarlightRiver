using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class DisinfectantKit : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public DisinfectantKit() : base("Disinfectant Kit", "Combined effects of the Disinfectant Wipes and Sanitizer Spray\n10% increased critical strike chance when a debuff is active") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;

            return true;
        }

        private void OnHit(Player player, bool crit)
        {
            if (Equipped(player) && crit)
            {
                if (Main.rand.NextFloat() < 0.1f)
                {
                    DisinfectantWipes.ReduceDebuffDurations(player);
                }
                if (Main.rand.NextFloat() < 0.25f)
                {
                    SanitizerSpray.TransferRandomDebuffToNearbyEnemies(player);
                }
            }
        }

        private void OnHitNPCAccessory(Player player, Item item, NPC target, int damage, float knockback, bool crit)
            => OnHit(player, crit);
        private void OnHitNPCWithProjAccessory(Player player, Projectile proj, NPC target, int damage, float knockback, bool crit)
            => OnHit(player, crit);

        public override void SafeUpdateEquip(Player player)
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Helper.IsValidDebuff(player, i))
                {
                    player.BoostAllDamage(0, 10);

                    return;
                }
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ModContent.ItemType<DisinfectantWipes>());
            recipe.AddIngredient(ModContent.ItemType<SanitizerSpray>());

            recipe.AddTile(TileID.TinkerersWorkbench);

            recipe.SetResult(this);

            recipe.AddRecipe();
        }
    }
}
