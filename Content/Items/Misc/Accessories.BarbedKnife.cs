using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	public class BarbedKnife : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public BarbedKnife() : base("Barbed Knife", "Critical strikes apply a bleeding debuff that stacks up to five times") { }

        public override void Load()
        {
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
            StarlightPlayer.OnHitNPCEvent += OnHitNPC;
        }

		public override void Unload()
		{
            StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProjAccessory;
            StarlightPlayer.OnHitNPCEvent -= OnHitNPC;
        }

		private void OnHit(Player Player, NPC target, bool crit)
        {
            if (Equipped(Player) && crit)
            {
                BleedStack.ApplyBleedStack(target, 300, true);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    Player.GetModPlayer<StarlightPlayer>().shouldSendHitPacket = true;
            }
        }

        private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, int damage, float knockback, bool crit) 
            => OnHit(Player, target, crit);

        private void OnHitNPC(Player Player, Item Item, NPC target, int damage, float knockback, bool crit) 
            => OnHit(Player, target, crit);

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.ShadowScale, 5);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TissueSample, 5);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}