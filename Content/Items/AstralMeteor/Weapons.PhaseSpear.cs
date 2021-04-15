using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Projectiles;

namespace StarlightRiver.Content.Items.AstralMeteor
{
    internal abstract class Phasespear : ModItem
    {
        private readonly Color glowColor;
        private readonly int projectileType;
        private readonly int gemID;

        public Phasespear(Color color, int projType, int gem)
        {
            glowColor = color;
            projectileType = projType;
            gemID = gem;
        }

        public override string Texture => AssetDirectory.AluminumItem + "Phasespear";

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Zaps your foes with colored lightning!");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.damage = 16;
            item.useTime = 30;
            item.useAnimation = 30;
            item.reuseDelay = 40;
            item.knockBack = 4;
            item.shoot = projectileType;
            item.shootSpeed = 1;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.melee = true;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.UseSound = SoundID.Item15;
            item.rare = ItemRarityID.Blue;
            item.value = Item.sellPrice(0, 0, 54, 0);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<AluminumBar>(), 15);
            recipe.AddIngredient(gemID, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Texture2D tex = GetTexture(AssetDirectory.AluminumItem + "PhasespearGlow");
            Texture2D tex2 = GetTexture(AssetDirectory.AluminumItem + "PhasespearGlow2");

            spriteBatch.Draw(tex2, position, frame, Color.White, 0, origin, scale, 0, 0);
            spriteBatch.Draw(tex, position, frame, glowColor, 0, origin, scale, 0, 0);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = GetTexture(AssetDirectory.AluminumItem + "PhasespearGlow");
            Texture2D tex2 = GetTexture(AssetDirectory.AluminumItem + "PhasespearGlow2");

            spriteBatch.Draw(tex2, item.Center + Vector2.UnitY * -7 - Main.screenPosition, tex.Frame(), Color.White, rotation, tex2.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(tex, item.Center + Vector2.UnitY * -7 - Main.screenPosition, tex.Frame(), glowColor, rotation, tex.Size() / 2, 1, 0, 0);
        }
    }

    internal abstract class PhasespearProjectile : SpearProjectile
    {
        public override string Texture => AssetDirectory.AluminumItem + "PhasespearProjectile";

        private readonly Color glowColor;
        public PhasespearProjectile(Color color) : base(30, 40, 120) => glowColor = color;

        public override void SafeAI() => Lighting.AddLight(projectile.Center, glowColor.ToVector3() * 0.5f);

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = GetTexture(AssetDirectory.AluminumItem + "PhasespearProjectileGlow");
            Texture2D tex2 = GetTexture(AssetDirectory.AluminumItem + "PhasespearProjectileGlow2");

            spriteBatch.Draw(tex2, (projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY), tex2.Frame(), Color.White, projectile.rotation, Vector2.Zero, projectile.scale, 0, 0);
            spriteBatch.Draw(tex, (projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY), tex.Frame(), glowColor, projectile.rotation, Vector2.Zero, projectile.scale, 0, 0);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (projectile.ai[0] == 0)
            {
                int i = Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<PhasespearNode>(), projectile.damage / 2, 0, projectile.owner, 3, 250);
                Projectile proj = Main.projectile[i];
                if (proj.modProjectile is PhasespearNode) (proj.modProjectile as PhasespearNode).color = glowColor;

                projectile.ai[0] = 1;
            }
        }
    }

    internal class PhasespearNode : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;
        public Color color;

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
                possibleTargets.Add(npc); //This grabs all possible targets, which includes all NPCs in the appropriate raidus which are alive and vulnerable, excluding the hit NPC
            if (possibleTargets.Count == 0) return; //kill if no targets are available
            NPC chosenTarget = possibleTargets[Main.rand.Next(possibleTargets.Count)];

            if (projectile.ai[0] > 0 && chosenTarget != null) //spawns the next node and VFX if more nodes are available and a target is also available
            {
                int i = Projectile.NewProjectile(chosenTarget.Center, Vector2.Zero, ProjectileType<PhasespearNode>(), damage, knockback, projectile.owner, projectile.ai[0] - 1, projectile.ai[1]);
                Projectile proj = Main.projectile[i];
                if (proj.modProjectile is PhasespearNode) (proj.modProjectile as PhasespearNode).color = color;

                DrawHelper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Content.Dusts.ElectricColor>(), 1, 20, color);
            }
            projectile.timeLeft = 0;
        }
    }

    internal class RedPhasespear : Phasespear
    { public RedPhasespear() : base(Color.Red, ProjectileType<RedPhasespearProjectile>(), ItemID.Ruby) { } }

    internal class RedPhasespearProjectile : PhasespearProjectile
    { public RedPhasespearProjectile() : base(Color.Red) { } }


    internal class BluePhasespear : Phasespear
    { public BluePhasespear() : base(new Color(0, 50, 255), ProjectileType<BluePhasespearProjectile>(), ItemID.Sapphire) { } }

    internal class BluePhasespearProjectile : PhasespearProjectile
    { public BluePhasespearProjectile() : base(new Color(0, 50, 255)) { } }


    internal class GreenPhasespear : Phasespear
    { public GreenPhasespear() : base(new Color(30, 180, 0), ProjectileType<GreenPhasespearProjectile>(), ItemID.Emerald) { } }

    internal class GreenPhasespearProjectile : PhasespearProjectile
    { public GreenPhasespearProjectile() : base(new Color(30, 180, 0)) { } }


    internal class YellowPhasespear : Phasespear
    { public YellowPhasespear() : base(new Color(255, 160, 0), ProjectileType<YellowPhasespearProjectile>(), ItemID.Topaz) { } }

    internal class YellowPhasespearProjectile : PhasespearProjectile
    { public YellowPhasespearProjectile() : base(new Color(255, 160, 0)) { } }


    internal class PurplePhasespear : Phasespear
    { public PurplePhasespear() : base(new Color(160, 0, 255), ProjectileType<PurplePhasespearProjectile>(), ItemID.Amethyst) { } }

    internal class PurplePhasespearProjectile : PhasespearProjectile
    { public PurplePhasespearProjectile() : base(new Color(160, 0, 255)) { } }


    internal class WhitePhasespear : Phasespear
    { public WhitePhasespear() : base(new Color(180, 160, 180), ProjectileType<WhitePhasespearProjectile>(), ItemID.Diamond) { } }

    internal class WhitePhasespearProjectile : PhasespearProjectile
    { public WhitePhasespearProjectile() : base(new Color(180, 160, 180)) { } }


    internal class ScaliesPhasespear : Phasespear
    {
        public ScaliesPhasespear() : base(new Color(50, 255, 180), ProjectileType<ScaliesPhasespearProjectile>(), ItemID.Diamond) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<AluminumBar>(), 15);
            //recipe.AddIngredient(ItemType<Debug.DebugPotion>());
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    internal class ScaliesPhasespearProjectile : PhasespearProjectile
    { public ScaliesPhasespearProjectile() : base(new Color(50, 255, 180)) { } }


}
