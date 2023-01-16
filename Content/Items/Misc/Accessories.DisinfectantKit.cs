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

        public override void Load()
        {
            StarlightPlayer.OnHitNPCEvent += OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
        }

		public override void Unload()
		{
            StarlightPlayer.OnHitNPCEvent -= OnHitNPCAccessory;
            StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProjAccessory;
        }

		private void OnHit(Player Player, bool crit)
        {
            if (Equipped(Player) && crit)
            {
                if (Main.rand.NextFloat() < 0.1f)
                {
                    DisinfectantWipes.ReduceDebuffDurations(Player);
                }
                if (Main.rand.NextFloat() < 0.25f)
                {
                    SanitizerSpray.TransferRandomDebuffToNearbyEnemies(Player);
                }
            }
        }

        private void OnHitNPCAccessory(Player Player, Item Item, NPC target, int damage, float knockback, bool crit)
            => OnHit(Player, crit);
        private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit)
            => OnHit(Player, crit);

        public override void SafeUpdateEquip(Player Player)
        {
            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (Helper.IsValidDebuff(Player, i))
                {
                    return;
                }
            }
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();

            recipe.AddIngredient(ModContent.ItemType<DisinfectantWipes>());
            recipe.AddIngredient(ModContent.ItemType<SanitizerSpray>());

            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
        }
    }
}
