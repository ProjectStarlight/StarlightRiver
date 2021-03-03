using System.Linq;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items.Herbology.Materials;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace StarlightRiver.Content.Items.ForestIvy
{
    public class ForestIvyBlowdart : ModItem
    {
        public override string Texture => AssetDirectory.IvyItem + Name;

        public override void SetStaticDefaults()
        {
            // TODO: Better name?
            DisplayName.SetDefault("Forest Ivy Blowpipe");
            Tooltip.SetDefault("On hit, builds up poisonous vines on enemies, dealing contact damage and spreading to other enemies.");
        }

        public override void SetDefaults()
        {
            item.width = 36;
            item.height = 16;

            item.useStyle = ItemUseStyleID.HoldingOut;
            item.autoReuse = true;

            item.useAnimation = item.useTime = 30; // 15 less than vanilla blowpipe (1.3), 5 more than vanilla blowpipe (1.4) TODO: maybe change idk
            item.useAmmo = AmmoID.Dart;
            item.UseSound = SoundID.Item63;

            item.shootSpeed = 12.5f; // 1.5 more than vanilla blowpipe
#pragma warning disable ChangeMagicNumberToID
            item.shoot = 10;
#pragma warning restore ChangeMagicNumberToID

            item.noMelee = true;
            item.ranged = true;
            item.knockBack = 4f; // .5 more than vanilla blowpipe
            item.damage = 16; // TODO: determine if this is good (same with other stats), I can't balance if my life depended on it
            // (btw 7 more than vanilla blowpipe)

            // TODO: Value
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY,
            ref int type, ref int damage, ref float knockBack)
        {
            // Pos modifications for the projectile so it's shot near where the blowdart is actually drawn (see: ForestIvyBlowdartPlayer)
            position.X -= 4f * player.direction;
            position.Y -= 2f * player.gravDir;

            Projectile proj = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI);
            proj.GetGlobalProjectile<ForestIvyBlowdartGlobalProj>().forestIvyPoisonVine = true;

            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Ivy>(), 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    /// <summary>
    /// ModPlayer that handles slight bodyFrame modifications for the Ivy Blowdart.
    /// </summary>
    public class ForestIvyBlowdartPlayer : ModPlayer
    {
        public override void ModifyDrawInfo(ref PlayerDrawInfo drawInfo)
        {
            // Don't know if this is the best hook to put it in, but eh
            // This code makes the player hold the blowdart to their mouth instead of the normal useStyle code behavior
            // TODO: Determine if it'd be better to entirely customize useStyle code, not sure because we'd likely have to copy over draw-code which is a pain
            if (player.inventory[player.selectedItem].type != ModContent.ItemType<ForestIvyBlowdart>() ||
                player.itemTime <= 0)
                return;

            player.bodyFrame.Y = player.bodyFrame.Height * 2;
            drawInfo.itemLocation -= new Vector2(0, 8); // account for added stuff on the blowdart that fricks with the origin
        }
    }

    public class ForestIvyBlowdartGlobalProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public bool forestIvyPoisonVine;

        public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit)
        {
            if (Main.rand.NextBool(2) && target.life > 5 && !target.friendly && target.type != NPCID.TargetDummy)
                target.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount++;
        }
    }

    // TODO: add actual visuals
    public class ForestIvyBlowdartGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        // TODO: probably needs syncing in mp
        public int forestIvyPoisonVineCount;

        public int forestIvyPoisonVineContact;

        public override void AI(NPC npc)
        {
            foreach (NPC otherNPC in Main.npc.Where(n => n.active && n.life > 5 && !n.friendly &&
                                                         n.type != NPCID.TargetDummy))
                if (npc.Hitbox.Intersects(otherNPC.Hitbox))
                {
                    if (npc.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount <=
                        otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount ||
                        ++otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineContact < 60)
                        continue;

                    otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount++;
                    otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineContact = 0;
                }
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            /*if (forestIvyPoisonVineCount <= 0) fix this later, its broken AF man >.<
                return;

            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= forestIvyPoisonVineCount * 2 * 5;
            damage += forestIvyPoisonVineCount * 5;*/
        }
    }
}