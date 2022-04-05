using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.AstralMeteor;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Item.damage = 15;
            Item.DamageType = DamageClass.Melee;
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.pick = 75;
            Item.axe = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.value = Item.sellPrice(0, 0, 30, 0);
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item18;
            Item.useTurn = true;
        }

        public override bool? UseItem(Player Player)
        {
            if (Main.rand.Next(10) == 0)
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC target = Main.npc[k];
                    if (target.active && Vector2.Distance(target.Center, Player.Center) < 100)
                    {
                        Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<ReaverLightningNode>(), 20, 0, 0, 2, 100);
                        DrawHelper.DrawElectricity(Player.Center, target.Center, DustType<Dusts.Electric>());
                    }
                }
            return true;
        }

        public override void AddRecipes()
        {
            //Recipe recipe = CreateRecipe();
            //recipe.AddIngredient(ItemType<AluminumBarItem>(), 20);
            //recipe.AddTile(TileID.Anvils);
        }
    }

    internal class ReaverLightningNode : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.timeLeft = 1;
            Projectile.friendly = true;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            //AI Fields:
            //0: jumps remaining
            //1: jump radius

            List<NPC> possibleTargets = new List<NPC>();
            foreach (NPC NPC in Main.npc.Where(NPC => NPC.active && !NPC.immortal && Vector2.Distance(NPC.Center, Projectile.Center) < Projectile.ai[1] && NPC != target))
            {
                possibleTargets.Add(NPC); //This grabs all possible targets, which includes all NPCs in the appropriate raidus which are alive and vulnerable, excluding the hit NPC
            }
            if (possibleTargets.Count == 0) return; //kill if no targets are available
            NPC chosenTarget = possibleTargets[Main.rand.Next(possibleTargets.Count)];

            if (Projectile.ai[0] > 0 && chosenTarget != null) //spawns the next node and VFX if more nodes are available and a target is also available
            {
                Projectile.NewProjectile(chosenTarget.Center, Vector2.Zero, ProjectileType<ReaverLightningNode>(), damage, knockback, Projectile.owner, Projectile.ai[0] - 1, Projectile.ai[1]);
                DrawHelper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Dusts.Electric>());
            }
            Projectile.timeLeft = 0;
        }
    }
}
