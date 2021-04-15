using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.AstralMeteor
{
    internal class AluminumReaver : ModItem
    {
        public override string Texture => AssetDirectory.AluminumItem + "AluminumReaver";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Astral Reaver");
            Tooltip.SetDefault("Occasionally zaps nearby enemies on use");
        }

        public override void SetDefaults()
        {
            item.damage = 15;
            item.melee = true;
            item.width = 38;
            item.height = 38;
            item.useTime = 14;
            item.useAnimation = 14;
            item.pick = 75;
            item.axe = 20;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 5f;
            item.value = Item.sellPrice(0, 0, 30, 0);
            item.rare = ItemRarityID.Blue;
            item.autoReuse = true;
            item.UseSound = SoundID.Item18;
            item.useTurn = true;
        }

        public override bool UseItem(Player player)
        {
            if (Main.rand.Next(10) == 0)
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC target = Main.npc[k];
                    if (target.active && Vector2.Distance(target.Center, player.Center) < 100)
                    {
                        Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<ReaverLightningNode>(), 20, 0, 0, 2, 100);
                        DrawHelper.DrawElectricity(player.Center, target.Center, DustType<Dusts.Electric>());
                    }
                }
            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<AluminumBar>(), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    internal class ReaverLightningNode : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.timeLeft = 1;
            projectile.friendly = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //AI Fields:
            //0: jumps remaining
            //1: jump radius

            List<NPC> possibleTargets = new List<NPC>();
            foreach (NPC npc in Main.npc.Where(npc => npc.active && !npc.immortal && Vector2.Distance(npc.Center, projectile.Center) < projectile.ai[1] && npc != target))
            {
                possibleTargets.Add(npc); //This grabs all possible targets, which includes all NPCs in the appropriate raidus which are alive and vulnerable, excluding the hit NPC
            }
            if (possibleTargets.Count == 0) return; //kill if no targets are available
            NPC chosenTarget = possibleTargets[Main.rand.Next(possibleTargets.Count)];

            if (projectile.ai[0] > 0 && chosenTarget != null) //spawns the next node and VFX if more nodes are available and a target is also available
            {
                Projectile.NewProjectile(chosenTarget.Center, Vector2.Zero, ProjectileType<ReaverLightningNode>(), damage, knockback, projectile.owner, projectile.ai[0] - 1, projectile.ai[1]);
                DrawHelper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Dusts.Electric>());
            }
            projectile.timeLeft = 0;
        }
    }
}
