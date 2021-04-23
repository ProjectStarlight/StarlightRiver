using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Permafrost
{
    class AuroraDisc : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostItem + "AuroraDisc";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH]Aurora Disc");
            Tooltip.SetDefault("Can open gates in the permafrost\n" +
                "Grows frost spikes when striking an enemy or tiles\n" +
                "Enemies struck by these spikes lose health over time if struck by minions\n" +
                "Repeated minion strikes increase the intensity up to 10\n" +
                "Can be re-used to recall the disc");
        }

        public override void SetDefaults()
        {
            item.damage = 12;
            item.summon = true;
            item.width = 32;
            item.height = 32;
            item.useTime = 25;
            item.noUseGraphic = true;
            item.rare = ItemRarityID.Green;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.shootSpeed = 20f;
            item.knockBack = 1f;
            item.UseSound = SoundID.Item19;
            item.shoot = ProjectileType<AuroraDiscProjectile>();
            item.useAnimation = 10;
            item.noMelee = true;
        }

        public override bool CanUseItem(Player player)
        {
            for (int k = 0; k <= Main.maxProjectiles; k++)
            {
                Projectile p = Main.projectile[k];

                if (p.active && p.owner == player.whoAmI && p.type == ProjectileType<AuroraDiscProjectile>())
                {
                    if (p.ai[0] < 2)
                        p.timeLeft = 31;

                    return false;
                }
            }

            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile proj = Main.projectile[k];
                if (proj.active && proj.modProjectile is DiscHoleDummy && Vector2.Distance(Main.MouseWorld, proj.Center) < 64)
                {
                    int i = Projectile.NewProjectile(player.Center, Vector2.Zero, type, damage, knockBack, player.whoAmI, 3, 0);
                    Projectile disc = Main.projectile[i];
                    (disc.modProjectile as AuroraDiscProjectile).gate = proj.modProjectile as DiscHoleDummy;
                    (disc.modProjectile as AuroraDiscProjectile).savedPos = proj.Center;
                    disc.tileCollide = false;
                    disc.hide = false;

                    return false;
                }
            }

            return true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new LearnableRecipe("Aurora Disc");
            recipe.AddIngredient(ItemID.StoneBlock, 10);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    class AuroraDiscProjectile : ModProjectile, IDrawAdditive
    {
        public DiscHoleDummy gate;
        public Vector2 savedPos;

        private ref float State => ref projectile.ai[0];
        private ref float HitCount => ref projectile.ai[1];
        private Player Owner => Main.player[projectile.owner];

        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.width = 4;
            projectile.height = 4;
            projectile.timeLeft = 1200;
            projectile.penetrate = -1;
            projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

        public override void AI()
        {
            if (State != 1)
                projectile.rotation += 0.2f;

            if (State == 2)
            {
                projectile.Center = Vector2.SmoothStep(Owner.Center, savedPos, projectile.timeLeft / 30f);
                if (projectile.timeLeft == 20) projectile.oldPos = new Vector2[ProjectileID.Sets.TrailCacheLength[projectile.type]]; //reset oldpos array for visual smoothness
            }

            if (State != 2 && projectile.timeLeft <= 30)
                Explode(State == 1);

            //Special case for when its being used to open a gate
            if (State == 3)
            {
                int timer = 1200 - projectile.timeLeft;

                if (timer <= 30)
                    projectile.Center = Vector2.SmoothStep(Owner.Center, savedPos, timer / 30f);

                if (timer > 30 && timer < 90) //funny visuals
                {
                    for (int k = 0; k < 10; k++)
                    {
                        float radius = 30 + (float)Math.Sin((timer - 30) / 60f * 6.28f - 1.57f) * 30;
                        float angleOff = timer / 40f;

                        float angle = k / 10f * 6.28f + angleOff;

                        var off = Vector2.One.RotatedBy(angle) * radius;

                        float sin = 1 + (float)Math.Sin(-StarlightWorld.rottime + k / 10f * 6.28f);
                        float cos = 1 + (float)Math.Cos(-StarlightWorld.rottime + k / 10f * 6.28f);
                        Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.1f;

                        int alpha = (int)(255 * (0.5f + radius / 120f * 0.5f));

                        Dust.NewDustPerfect(projectile.Center + off, DustType<Dusts.Aurora>(), Vector2.Zero, alpha, color, 1);
                    }
                }

                if (timer == 120)
                    gate.OpenGate();

                if (timer == 150)
                    Explode(false);
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            switch (State)
            {
                case 1: //extended
                    target.AddBuff(BuffType<AuroraDoT>(), 600);
                    HitCount++;

                    if (HitCount > 3) //return conditions
                        Explode(true);

                    break;
            }
        }

        public override bool? CanHitNPC(NPC target) //handled here in the event of I-frames
        {
            var rect = projectile.Hitbox;
            rect.Inflate(28, 28);
            if (State == 0 && Helper.IsTargetValid(target) && rect.Intersects(target.Hitbox)) Extend();
            return null;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Extend();
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = GetTexture(Texture);
            var max = ProjectileID.Sets.TrailCacheLength[projectile.type];

            float sin0 = 1 + (float)Math.Sin(-StarlightWorld.rottime * 2);
            float cos0 = 1 + (float)Math.Cos(-StarlightWorld.rottime * 2);
            Color lighterColor = Color.Lerp(lightColor, new Color(0.5f + cos0 * 0.2f, 0.8f, 0.5f + sin0 * 0.2f) * 1.1f, 0.5f);

            if (State == 1) //crystals when not
            {
                for (int k = 0; k < 3; k++)
                {
                    float sin = 1 + (float)Math.Sin(-StarlightWorld.rottime + k);
                    float cos = 1 + (float)Math.Cos(-StarlightWorld.rottime + k);
                    Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.1f;

                    var tenk = k * 5;
                    var tex2 = GetTexture("StarlightRiver/Assets/Items/Permafrost/AuroraDiscOver" + k);

                    var progress = (projectile.timeLeft < 590 - tenk ? 10 : 10 - (projectile.timeLeft - (590 - tenk))) / 10f;
                    if (progress < 0) progress = 0;

                    spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, color, 0, tex2.Size() / 2, progress, 0, 0);
                    Lighting.AddLight(projectile.Center, color.ToVector3() * 0.35f * progress);
                }
            }
            else if (State == 0 || (State == 2 && projectile.timeLeft < 20)) //trail when moving, extra condition to stop a visual bug
            {
                for (int k = 0; k < max; k++)
                    spriteBatch.Draw(tex, projectile.oldPos[k] + Vector2.One * 2 - Main.screenPosition, null, lighterColor * (1 - k / (float)max), 0, tex.Size() / 2, 1, 0, 0);
            }

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lighterColor, projectile.rotation, tex.Size() / 2, 1, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (State == 3)
            {
                float sin0 = 1 + (float)Math.Sin(-StarlightWorld.rottime * 2);
                float cos0 = 1 + (float)Math.Cos(-StarlightWorld.rottime * 2);
                Color color = new Color(0.5f + cos0 * 0.2f, 0.8f, 0.5f + sin0 * 0.2f) * 1.1f;

                int timer = 1200 - projectile.timeLeft;

                if (timer > 30 && timer < 150)
                {
                    var tex = GetTexture(Texture);
                    var tex2 = GetTexture("StarlightRiver/Assets/Keys/Glow");
                    spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * (timer < 90 ? (timer - 30) / 60f : 1), 0, tex.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, color * (timer < 90 ? (timer - 30) / 60f : 1) * 0.65f, 0, tex2.Size() / 2, 1, 0, 0);

                    if (timer > 120)
                    {
                        var progress = (timer - 120) / 10f;
                        spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, color * ((3 - progress) / 3f), 0, tex2.Size() / 2, 1 + progress, 0, 0);
                    }

                    Lighting.AddLight(projectile.Center, color.ToVector3() * Math.Min((timer - 30) / 10f, 1));
                }
            }
        }

        private void Extend()
        {
            if (State > 0) return;

            State = 1;
            projectile.velocity *= 0;
            projectile.width = 64;
            projectile.height = 64;
            projectile.position -= Vector2.One * 30;
            projectile.timeLeft = 600;
            projectile.tileCollide = false;

            Main.PlaySound(SoundID.DD2_WitherBeastHurt, projectile.Center);
        }

        private void Explode(bool VFX)
        {
            if (State == 1)
            {
                projectile.width = 4;
                projectile.height = 4;
                projectile.position += Vector2.One * 30;
            }

            State = 2;
            savedPos = projectile.Center;
            projectile.timeLeft = 30;


            if (!VFX) return;

            Main.PlaySound(SoundID.DD2_WitherBeastDeath, projectile.Center);

            for (int k = 0; k < 16; k++)
            {
                Color color = new Color();

                switch (Main.rand.Next(4))
                {
                    case 0: color = new Color(200, 255, 220); break;
                    case 1: color = new Color(200, 240, 255); break;
                    case 2: color = new Color(200, 220, 240); break;
                    case 3: color = new Color(230, 220, 240); break;
                }

                Dust d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), DustType<Dusts.Crystal>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, color, 0.7f);
                d.fadeIn = Main.rand.NextFloat(-0.05f, 0.05f);

                Dust.NewDustPerfect(projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, color * 0.5f, 0.75f);
            }
        }
    }

    class AuroraDoT : SmartBuff
    {
        public AuroraDoT() : base("Aurora DoT", "What the fuck you shouldn't have this its NPC only", true) { }

        public override bool Autoload(ref string name, ref string texture)
        {
            StarlightNPC.ModifyHitByProjectileEvent += SpecialMinionRecation;
            texture = AssetDirectory.Invisible;
            return true;
        }

        private void SpecialMinionRecation(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (npc.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT < 10 && projectile.minion && npc.HasBuff(Type))
            {
                npc.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT += 1;
            }
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<StarlightNPC>().DoT += npc.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT;
            if (npc.buffTime[buffIndex] <= 1) npc.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT = 0;

            if (Main.rand.Next(5) == 0)
            {
                Color color = new Color();

                switch (Main.rand.Next(4))
                {
                    case 0: color = new Color(200, 255, 220); break;
                    case 1: color = new Color(200, 240, 255); break;
                    case 2: color = new Color(200, 220, 240); break;
                    case 3: color = new Color(230, 220, 240); break;
                }

                var pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                Dust d = Dust.NewDustPerfect(pos, DustType<Dusts.Crystal>(), Vector2.UnitY * Main.rand.NextFloat(-1, 0), 0, color * 0.6f, 0.3f);
                d.fadeIn = Main.rand.NextFloat(-0.05f, 0.05f);
            }
        }
    }
}
