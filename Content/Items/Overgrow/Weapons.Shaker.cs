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

namespace StarlightRiver.Content.Items.Overgrow
{
     internal class Shaker : ModItem
    {
        public override string Texture => AssetDirectory.OvergrowItem + "Shaker";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("The Shaker");
        }

        public override void SetDefaults()
        {
            item.damage = 100;
            item.melee = true;
            item.width = 40;
            item.height = 20;
            item.useTime = 60;
            item.useAnimation = 1;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.knockBack = 4;
            item.rare = ItemRarityID.Orange;
            item.channel = true;
            item.noUseGraphic = true;
        }

        public override bool CanUseItem(Player player) => !Main.projectile.Any(n => n.active && Main.player[n.owner] == player && n.type == ProjectileType<ShakerBall>());

        public override bool UseItem(Player player)
        {
            int proj = Projectile.NewProjectile(player.position + new Vector2(0, -32), Vector2.Zero, ProjectileType<ShakerBall>(), item.damage, item.knockBack);
            Main.projectile[proj].owner = player.whoAmI;
            return true;
        }

        public override void HoldItem(Player player)
        {
            if (player.channel)
            {
                player.velocity.X *= 0.95f;
                player.jump = -1;
                player.GetModPlayer<AnimationHandler>().Lifting = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FirstOrDefault(tooltip => tooltip.Name == "Speed" && tooltip.mod == "Terraria").text = "Snail Speed";
        }
    }

     public class AnimationHandler : ModPlayer
     {
         public bool Lifting = false;

         public override void PostUpdate()
         {
             if (Lifting) 
                 player.bodyFrame = new Rectangle(0, 56 * 5, 40, 56);
         }
 
         public override void ResetEffects()
         {
             Lifting = false;
         }
     }
     internal class ShakerBall : ModProjectile
     {
        public override string Texture => AssetDirectory.OvergrowItem + Name;
        public override void SetDefaults()
        {
            projectile.friendly = false;
            projectile.width = 64;
            projectile.height = 64;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
            projectile.melee = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Shaker");
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (projectile.timeLeft < 2) projectile.timeLeft = 2;
            projectile.scale = projectile.ai[0] < 10 ? (projectile.ai[0] / 10f) : 1;
            projectile.damage = (int)(projectile.ai[0] * 1.2f * player.meleeDamage);

            if (projectile.ai[0] == 100)
            {
                Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
            }

            if (projectile.ai[1] == 0)
            {
                projectile.position = player.Top + (projectile.position - projectile.Bottom);
            }

            if (projectile.ai[1] == 0 && projectile.ai[0] < 100)
            {
                projectile.ai[0]++;
                if (projectile.ai[0] == 100)
                {
                    for (int k = 0; k <= 100; k++)
                    {
                        Dust.NewDustPerfect(projectile.Center, DustType<Content.Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                    }
                    Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                }
                projectile.velocity = Vector2.Zero;
                float rot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Content.Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, projectile.ai[0] / 100f);
            }

            if (!player.channel && projectile.ai[0] > 10 && projectile.ai[1] == 0)
            {
                projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * projectile.ai[0] * 0.1f;
                projectile.tileCollide = true;
                projectile.friendly = true;
                projectile.ai[1] = 1;
            }

            if (projectile.ai[1] == 1)
            {
                projectile.velocity.Y += 0.4f;

                if (projectile.velocity.Y == 0.4f)
                {
                    projectile.velocity *= 0;
                    projectile.timeLeft = 120;
                    projectile.ai[1] = 2;

                    player.GetModPlayer<StarlightPlayer>().Shake += (int)(projectile.ai[0] * 0.2f);
                    for (int k = 0; k <= 100; k++)
                    {
                        Dust.NewDustPerfect(projectile.Center + new Vector2(0, 32), DustType<Content.Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * projectile.ai[0] / 10f);
                    }
                    Main.PlaySound(SoundID.Item70, projectile.Center);
                    Main.PlaySound(SoundID.NPCHit42, projectile.Center);
                }
            }

            if (projectile.ai[1] == 2)
            {
                projectile.velocity += -Vector2.Normalize(projectile.Center - player.Center) * 0.1f;
                if (projectile.velocity.Length() >= 5) projectile.ai[1] = 3;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) projectile.timeLeft = 0;
                if (projectile.timeLeft == 3) projectile.ai[1] = 4;
            }

            if (projectile.ai[1] == 3)
            {
                projectile.velocity = -Vector2.Normalize(projectile.Center - player.Center) * 5;
                projectile.velocity.Y += 3;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) projectile.timeLeft = 0;
                if (projectile.timeLeft == 3) projectile.ai[1] = 4;
            }

            if (projectile.ai[1] == 4)
            {
                projectile.velocity = -Vector2.Normalize(projectile.Center - player.Center) * 18;
                projectile.tileCollide = false;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) projectile.timeLeft = 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] != 0)
            {
                Player player = Main.player[projectile.owner];
                for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(player.Center, projectile.Center) / 16))
                {
                    spriteBatch.Draw(GetTexture("StarlightRiver/Assets/Items/Overgrow/ShakerChain"), (Vector2.Lerp(projectile.Center, player.Center + new Vector2(0, Main.player[projectile.owner].gfxOffY), k) - Main.screenPosition),
                        new Rectangle(0, 0, 8, 16), lightColor, (projectile.Center - player.Center).ToRotation() + 1.58f, new Vector2(4, 8), 1, 0, 0);
                }
            }

            spriteBatch.Draw(Main.projectileTexture[projectile.type], ((projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur(), Main.projectileTexture[projectile.type].Frame(), Color.White, projectile.rotation, projectile.Size / 2, projectile.scale, 0, 0);

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (projectile.ai[1] == 0)
            {
                float colormult = projectile.ai[0] / 100f * 0.7f;
                float scale = 1.2f - projectile.ai[0] / 100f * 0.5f;
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2");
                Vector2 pos = ((projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur();
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * colormult, 0, tex.Size() / 2, scale, 0, 0);
            }

            if (projectile.ai[0] == 100)
            {
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2");
                Vector2 pos = ((projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur();
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - StarlightWorld.rottime) * 0.2f, 0, tex.Size() / 2, StarlightWorld.rottime * 0.17f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime + 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime + 3.14f) * 0.17f, 0, 0);
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * (6.28f - (StarlightWorld.rottime - 3.14f)) * 0.2f, 0, tex.Size() / 2, (StarlightWorld.rottime - 3.14f) * 0.17f, 0, 0);
            }
        }
    }
}