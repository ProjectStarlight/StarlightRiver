using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using Terraria;
using System;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace StarlightRiver.Content.Items.Misc
{
    public class ElectroArrow : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro Arrow");
            Tooltip.SetDefault("Chains to nearby enemies\nInflicts overcharge, greatly lowering enemy defense");
        }

        public override void SetDefaults()
        {
            item.damage = 1;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;
            item.crit = -4;
            item.knockBack = 0f;
            item.value = 10;
            item.rare = ItemRarityID.Blue;
            item.shoot = ProjectileType<ElectroArrowProjectile>();
            item.shootSpeed = 1f;
            item.ammo = AmmoID.Arrow;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FirstOrDefault(n => n.Name == "Damage").text = "Deals 25% bow damage";
            tooltips.FirstOrDefault(n => n.Name == "CritChance").text = "Cannot critically strike";
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.DirtBlock, 4); //TODO: real recipie
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 50);
            recipe.AddRecipe();
        }
    }

    internal class ElectroArrowProjectile : ModProjectile, IDrawAdditive
    {
        Vector2 savedPos = Vector2.Zero;
        int blacklistNPC = -1;
        int VFXSeed = 0;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 3;
            projectile.timeLeft = 180;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;

            projectile.extraUpdates = 6;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro Arrow");
        }

        public override void AI()
        {
            if (projectile.extraUpdates != 0)
                projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            else
                projectile.Opacity = projectile.timeLeft > 8 ? 1 : projectile.timeLeft / 7f;

            if (projectile.timeLeft == 180)
            {
                savedPos = projectile.Center;
                Main.PlaySound(SoundID.DD2_LightningBugZap, projectile.Center);
            }

            if (Main.GameUpdateCount % 3 == 0)
                VFXSeed = Main.rand.Next(int.MaxValue);

            if (projectile.timeLeft == 1)
                PreKill(projectile.timeLeft);
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            var point1 = savedPos;
            var point2 = projectile.Center;
            var armLength = 30;
            var rand = new Random(VFXSeed);

            if (point1 == Vector2.Zero || point2 == Vector2.Zero)
                return;

            var tex = GetTexture("StarlightRiver/Assets/GlowTrail");

            int nodeCount = (int)Vector2.Distance(point1, point2) / armLength;
            Vector2[] nodes = new Vector2[nodeCount + 1];

            nodes[nodeCount] = point2; //adds the end as the last point

            for (int k = 1; k < nodes.Length; k++)
            {
                nodes[k] = Vector2.Lerp(point1, point2, k / (float)nodeCount) +
                    (k == nodes.Length - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * (float)(rand.NextDouble() * 2 - 1) * armLength / 3);

                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];

                var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos), 10);
                var origin = new Vector2(0, tex.Height / 2);
                var rot = (nodes[k] - prevPos).ToRotation();
                var color = new Color(200, 230, 255) * (projectile.extraUpdates == 0 ? projectile.timeLeft / 15f : 1);

                sb.Draw(tex, target, null, color, rot, origin, 0, 0);
            }
        }

        public override bool? CanHitNPC(NPC target) => target.whoAmI != blacklistNPC;

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 4;
            crit = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffType<Overcharge>(), 300);

            if (projectile.penetrate <= 1)
                return;

            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC npc = Main.npc[k];
                if (npc.active && npc.chaseable && !npc.HasBuff(BuffType<Overcharge>()) && Vector2.Distance(npc.Center, target.Center) < 500)
                {
                    var proj = Projectile.NewProjectileDirect(target.Center, Vector2.Normalize(target.Center - npc.Center) * -6, ProjectileType<ElectroArrowProjectile>(), 20, 0, projectile.owner, 2, 100);
                    proj.penetrate = projectile.penetrate - 1;
                    proj.tileCollide = false;
                    (proj.modProjectile as ElectroArrowProjectile).blacklistNPC = target.whoAmI;
                    break;
                }
            }

            PreKill(projectile.timeLeft);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.extraUpdates == 0)
                return true;

            projectile.velocity *= 0;
            projectile.friendly = false;
            projectile.timeLeft = 15;
            projectile.extraUpdates = 0;

            return false;
        }

        public override bool PreKill(int timeLeft)
        {
            if (projectile.extraUpdates == 0)
                return true;

            projectile.velocity *= 0;
            projectile.friendly = false;
            projectile.timeLeft = 15;
            projectile.extraUpdates = 0;

            return false;
        }
    }
}
