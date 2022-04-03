using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
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
            Item.damage = 12;
            Item.summon = true;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 25;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            Item.shootSpeed = 20f;
            Item.knockBack = 1f;
            Item.UseSound = SoundID.Item19;
            Item.shoot = ProjectileType<AuroraDiscProjectile>();
            Item.useAnimation = 10;
            Item.noMelee = true;
        }

        public override bool CanUseItem(Player Player)
        {
            for (int k = 0; k <= Main.maxProjectiles; k++)
            {
                Projectile p = Main.projectile[k];

                if (p.active && p.owner == Player.whoAmI && p.type == ProjectileType<AuroraDiscProjectile>())
                {
                    if (p.ai[0] < 2)
                        p.timeLeft = 31;

                    return false;
                }
            }

            return base.CanUseItem(Player);
        }

        public override bool Shoot(Player Player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                Projectile proj = Main.projectile[k];
                if (proj.active && proj.ModProjectile is DiscHoleDummy && Vector2.Distance(Main.MouseWorld, proj.Center) < 64)
                {
                    int i = Projectile.NewProjectile(Player.Center, Vector2.Zero, type, damage, knockBack, Player.whoAmI, 3, 0);
                    Projectile disc = Main.projectile[i];
                    (disc.ModProjectile as AuroraDiscProjectile).gate = proj.ModProjectile as DiscHoleDummy;
                    (disc.ModProjectile as AuroraDiscProjectile).savedPos = proj.Center;
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

        private ref float State => ref Projectile.ai[0];
        private ref float HitCount => ref Projectile.ai[1];
        private Player Owner => Main.player[Projectile.owner];

        public override string Texture => AssetDirectory.PermafrostItem + Name;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void DrawBehind(int index, List<int> drawCacheProjsBehindNPCsAndTiles, List<int> drawCacheProjsBehindNPCs, List<int> drawCacheProjsBehindProjectiles, List<int> drawCacheProjsOverWiresUI)
        {
            drawCacheProjsBehindNPCsAndTiles.Add(index);
        }

        public override void AI()
        {
            if (State != 1)
                Projectile.rotation += 0.2f;

            if (State == 2)
            {
                Projectile.Center = Vector2.SmoothStep(Owner.Center, savedPos, Projectile.timeLeft / 30f);
                if (Projectile.timeLeft == 20) Projectile.oldPos = new Vector2[ProjectileID.Sets.TrailCacheLength[Projectile.type]]; //reset oldpos array for visual smoothness
            }

            if (State != 2 && Projectile.timeLeft <= 30)
                Explode(State == 1);

            //Special case for when its being used to open a gate
            if (State == 3)
            {
                int timer = 1200 - Projectile.timeLeft;

                if (timer <= 30)
                    Projectile.Center = Vector2.SmoothStep(Owner.Center, savedPos, timer / 30f);

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

                        Dust.NewDustPerfect(Projectile.Center + off, DustType<Dusts.Aurora>(), Vector2.Zero, alpha, color, 1);
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
            var rect = Projectile.Hitbox;
            rect.Inflate(28, 28);
            if (State == 0 && target.CanBeChasedBy() && rect.Intersects(target.Hitbox)) Extend();
            return null;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Extend();
            return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            var tex = Request<Texture2D>(Texture).Value;
            var max = ProjectileID.Sets.TrailCacheLength[Projectile.type];

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
                    var tex2 = Request<Texture2D>("StarlightRiver/Assets/Items/Permafrost/AuroraDiscOver" + k).Value;

                    var progress = (Projectile.timeLeft < 590 - tenk ? 10 : 10 - (Projectile.timeLeft - (590 - tenk))) / 10f;
                    if (progress < 0) progress = 0;

                    spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, color, 0, tex2.Size() / 2, progress, 0, 0);
                    Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.35f * progress);
                }
            }
            else if (State == 0 || (State == 2 && Projectile.timeLeft < 20)) //trail when moving, extra condition to stop a visual bug
            {
                for (int k = 0; k < max; k++)
                    spriteBatch.Draw(tex, Projectile.oldPos[k] + Vector2.One * 2 - Main.screenPosition, null, lighterColor * (1 - k / (float)max), 0, tex.Size() / 2, 1, 0, 0);
            }

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lighterColor, Projectile.rotation, tex.Size() / 2, 1, 0, 0);

            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            if (State == 3)
            {
                float sin0 = 1 + (float)Math.Sin(-StarlightWorld.rottime * 2);
                float cos0 = 1 + (float)Math.Cos(-StarlightWorld.rottime * 2);
                Color color = new Color(0.5f + cos0 * 0.2f, 0.8f, 0.5f + sin0 * 0.2f) * 1.1f;

                int timer = 1200 - Projectile.timeLeft;

                if (timer > 30 && timer < 150)
                {
                    var tex = Request<Texture2D>(Texture).Value;
                    var tex2 = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
                    spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * (timer < 90 ? (timer - 30) / 60f : 1), 0, tex.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, color * (timer < 90 ? (timer - 30) / 60f : 1) * 0.65f, 0, tex2.Size() / 2, 1, 0, 0);

                    if (timer > 120)
                    {
                        var progress = (timer - 120) / 10f;
                        spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, color * ((3 - progress) / 3f), 0, tex2.Size() / 2, 1 + progress, 0, 0);
                    }

                    Lighting.AddLight(Projectile.Center, color.ToVector3() * Math.Min((timer - 30) / 10f, 1));
                }
            }
        }

        private void Extend()
        {
            if (State > 0) return;

            State = 1;
            Projectile.velocity *= 0;
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.position -= Vector2.One * 30;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastHurt, Projectile.Center);
        }

        private void Explode(bool VFX)
        {
            if (State == 1)
            {
                Projectile.width = 4;
                Projectile.height = 4;
                Projectile.position += Vector2.One * 30;
            }

            State = 2;
            savedPos = Projectile.Center;
            Projectile.timeLeft = 30;


            if (!VFX) return;

            Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath, Projectile.Center);

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

                Dust d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20), DustType<Dusts.Crystal>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, color, 0.7f);
                d.fadeIn = Main.rand.NextFloat(-0.05f, 0.05f);

                Dust.NewDustPerfect(Projectile.Center, 264, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, color * 0.5f, 0.75f);
            }
        }
    }

    class AuroraDoT : SmartBuff
    {
        public AuroraDoT() : base("Aurora DoT", "No description", true) { }

        public override bool Autoload(ref string name, ref string texture)
        {
            StarlightNPC.ModifyHitByProjectileEvent += SpecialMinionRecation;
            texture = AssetDirectory.Invisible;
            return true;
        }

        private void SpecialMinionRecation(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (NPC.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT < 10 && Projectile.minion && NPC.HasBuff(Type))
            {
                NPC.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT += 1;
            }
        }

        public override void Update(NPC NPC, ref int buffIndex)
        {
            NPC.GetGlobalNPC<StarlightNPC>().DoT += NPC.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT;
            if (NPC.buffTime[buffIndex] <= 1) NPC.GetGlobalNPC<StarlightNPC>().AuroraDiscDoT = 0;

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

                var pos = NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height));
                Dust d = Dust.NewDustPerfect(pos, DustType<Dusts.Crystal>(), Vector2.UnitY * Main.rand.NextFloat(-1, 0), 0, color * 0.6f, 0.3f);
                d.fadeIn = Main.rand.NextFloat(-0.05f, 0.05f);
            }
        }
    }
}
