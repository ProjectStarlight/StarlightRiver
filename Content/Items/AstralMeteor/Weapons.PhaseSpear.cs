using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Projectiles;
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
	internal abstract class Phasespear : ModItem
    {
        private readonly Color glowColor;
        private readonly int ProjectileType;
        private readonly int gemID;

        public Phasespear(Color color, int projType, int gem)
        {
            glowColor = color;
            ProjectileType = projType;
            gemID = gem;
        }

        public override string Texture => AssetDirectory.AluminumItem + "Phasespear";

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Zaps your foes with colored lightning!");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.damage = 16;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.reuseDelay = 40;
            Item.knockBack = 4;
            Item.shoot = ProjectileType;
            Item.shootSpeed = 1;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.melee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item15;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 54, 0);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<AluminumBarItem>(), 15);
            recipe.AddIngredient(gemID, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.AluminumItem + "PhasespearGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.AluminumItem + "PhasespearGlow2").Value;

            spriteBatch.Draw(tex2, position, frame, Color.White, 0, origin, scale, 0, 0);
            spriteBatch.Draw(tex, position, frame, glowColor, 0, origin, scale, 0, 0);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.AluminumItem + "PhasespearGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.AluminumItem + "PhasespearGlow2").Value;

            spriteBatch.Draw(tex2, Item.Center + Vector2.UnitY * -7 - Main.screenPosition, tex.Frame(), Color.White, rotation, tex2.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(tex, Item.Center + Vector2.UnitY * -7 - Main.screenPosition, tex.Frame(), glowColor, rotation, tex.Size() / 2, 1, 0, 0);
        }
    }

    internal abstract class PhasespearProjectile : SpearProjectile
    {
        public override string Texture => AssetDirectory.AluminumItem + "PhasespearProjectile";

        private readonly Color glowColor;
        public PhasespearProjectile(Color color) : base(30, 40, 120) => glowColor = color;

        public override void SafeAI() => Lighting.AddLight(Projectile.Center, glowColor.ToVector3() * 0.5f);

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D tex = Request<Texture2D>(AssetDirectory.AluminumItem + "PhasespearProjectileGlow").Value;
            Texture2D tex2 = Request<Texture2D>(AssetDirectory.AluminumItem + "PhasespearProjectileGlow2").Value;

            spriteBatch.Draw(tex2, (Projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY), tex2.Frame(), Color.White, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);
            spriteBatch.Draw(tex, (Projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[Projectile.owner].gfxOffY), tex.Frame(), glowColor, Projectile.rotation, Vector2.Zero, Projectile.scale, 0, 0);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Projectile.ai[0] == 0)
            {
                int i = Projectile.NewProjectile(target.Center, Vector2.Zero, ProjectileType<PhasespearNode>(), Projectile.damage / 2, 0, Projectile.owner, 3, 250);
                Projectile proj = Main.projectile[i];
                if (proj.ModProjectile is PhasespearNode) (proj.ModProjectile as PhasespearNode).color = glowColor;

                Projectile.ai[0] = 1;
            }
        }
    }

    internal class PhasespearNode : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;
        public Color color;

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
                possibleTargets.Add(NPC); //This grabs all possible targets, which includes all NPCs in the appropriate raidus which are alive and vulnerable, excluding the hit NPC
            if (possibleTargets.Count == 0) return; //kill if no targets are available
            NPC chosenTarget = possibleTargets[Main.rand.Next(possibleTargets.Count)];

            if (Projectile.ai[0] > 0 && chosenTarget != null) //spawns the next node and VFX if more nodes are available and a target is also available
            {
                int i = Projectile.NewProjectile(chosenTarget.Center, Vector2.Zero, ProjectileType<PhasespearNode>(), damage, knockback, Projectile.owner, Projectile.ai[0] - 1, Projectile.ai[1]);
                Projectile proj = Main.projectile[i];
                if (proj.ModProjectile is PhasespearNode) (proj.ModProjectile as PhasespearNode).color = color;

                DrawHelper.DrawElectricity(target.Center, chosenTarget.Center, DustType<Content.Dusts.ElectricColor>(), 1, 20, color);
            }
            Projectile.timeLeft = 0;
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
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemType<AluminumBarItem>(), 15);
            //recipe.AddIngredient(ItemType<Debug.DebugPotion>());
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    internal class ScaliesPhasespearProjectile : PhasespearProjectile
    { public ScaliesPhasespearProjectile() : base(new Color(50, 255, 180)) { } }


}
