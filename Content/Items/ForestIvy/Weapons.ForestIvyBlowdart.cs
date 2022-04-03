using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Items.Herbology.Materials;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
            Item.width = 36;
            Item.height = 16;

            Item.useStyle = ItemUseStyleID.HoldingOut;
            Item.autoReuse = true;

            Item.useAnimation = Item.useTime = 30; // 15 less than vanilla blowpipe (1.3), 5 more than vanilla blowpipe (1.4) TODO: maybe change idk
            Item.useAmmo = AmmoID.Dart;
            Item.UseSound = SoundID.Item63;

            Item.shootSpeed = 12.5f; // 1.5 more than vanilla blowpipe
#pragma warning disable ChangeMagicNumberToID
            Item.shoot = 10;
#pragma warning restore ChangeMagicNumberToID

            Item.noMelee = true;
            Item.ranged = true;
            Item.knockBack = 4f; // .5 more than vanilla blowpipe
            Item.damage = 16; // TODO: determine if this is good (same with other stats), I can't balance if my life depended on it
            // (btw 7 more than vanilla blowpipe)

            // TODO: Value
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY,
            ref int type, ref int damage, ref float knockBack)
        {
            // Pos modifications for the Projectile so it's shot near where the blowdart is actually drawn (see: ForestIvyBlowdartPlayer)
            position.X -= 4f * Player.direction;
            position.Y -= 2f * Player.gravDir;

            Projectile proj = Projectile.NewProjectileDirect(position, new Vector2(speedX, speedY), type, damage, knockBack, Player.whoAmI);
            proj.GetGlobalProjectile<ForestIvyBlowdartGlobalProj>().forestIvyPoisonVine = true;

            return false;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
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
            // This code makes the Player hold the blowdart to their mouth instead of the normal useStyle code behavior
            // TODO: Determine if it'd be better to entirely customize useStyle code, not sure because we'd likely have to copy over draw-code which is a pain
            if (Player.inventory[Player.selectedItem].type != ModContent.ItemType<ForestIvyBlowdart>() ||
                Player.ItemTime <= 0)
                return;

            Player.bodyFrame.Y = Player.bodyFrame.Height * 2;
            drawInfo.ItemLocation -= new Vector2(0, 8); // account for added stuff on the blowdart that fricks with the origin
        }
    }

    public class ForestIvyBlowdartGlobalProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public bool forestIvyPoisonVine;

        public override void OnHitNPC(Projectile Projectile, NPC target, int damage, float knockback, bool crit)
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

        public override void AI(NPC NPC)
        {
            if (forestIvyPoisonVineCount <= 0)
                return;

            foreach (NPC otherNPC in Main.npc.Where(n => n.active && n.life > 5 && !n.friendly && n.type != NPCID.TargetDummy))
            {
                if (NPC.Hitbox.Intersects(otherNPC.Hitbox))
                {
                    if (NPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount <=
                        otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount ||
                        ++otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineContact < 60)
                        continue;

                    otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineCount++;
                    otherNPC.GetGlobalNPC<ForestIvyBlowdartGlobalNPC>().forestIvyPoisonVineContact = 0;
                }
            }
        }

        public override void UpdateLifeRegen(NPC NPC, ref int damage)
        {
            /*if (forestIvyPoisonVineCount <= 0) fix this later, its broken AF man >.<
                return;

            if (NPC.lifeRegen > 0)
                NPC.lifeRegen = 0;

            NPC.lifeRegen -= forestIvyPoisonVineCount * 2 * 5;
            damage += forestIvyPoisonVineCount * 5;*/
        }
    }
}