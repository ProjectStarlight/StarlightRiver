using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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

        public ref float Timer => ref projectile.ai[0];
        public ref float State => ref projectile.ai[1];

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
            projectile.scale = Timer < 10 ? (Timer / 10f) : 1;
            projectile.damage = (int)(Timer * 1.2f * player.meleeDamage);

            if (Timer == 100)
            {
                Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f));
            }

            if (State == 0) //charging/holding
            {
                projectile.position = player.Top + (projectile.position - projectile.Bottom);
            }

            if (State == 0 && Timer < 100) //charge up
            {
                Timer++;

                if (Timer == 100) //full charge FX
                {
                    for (int k = 0; k <= 100; k++)
                    {
                        Dust.NewDustPerfect(projectile.Center, DustType<Dusts.GoldWithMovement>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.5f);
                    }
                    Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                }

                projectile.velocity = Vector2.Zero;

                float rot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedBy(rot) * 35, DustType<Dusts.GoldWithMovement>(), -Vector2.One.RotatedBy(rot) * 1.5f, 0, default, Timer / 100f);
            }

            if (!player.channel && Timer > 10 && State == 0) //throw if enough charge
            {
                if(player == Main.LocalPlayer)
                    projectile.velocity = Vector2.Normalize(Main.MouseWorld - player.Center) * Timer * 0.1f;

                projectile.tileCollide = true;
                projectile.friendly = true;
                State = 1;

                projectile.netUpdate = true;
            }

            if (State == 1) //thrown/falling
            {
                projectile.velocity.Y += 0.4f;

                if (projectile.velocity.Y == 0.4f) //when it hits the ground
                {
                    projectile.velocity *= 0;
                    projectile.timeLeft = 120;
                    State = 2;

                    player.GetModPlayer<StarlightPlayer>().Shake += (int)(Timer * 0.2f);
                    for (int k = 0; k <= 100; k++)
                    {
                        Dust.NewDustPerfect(projectile.Center + new Vector2(0, 32), DustType<Dusts.Stone>(), new Vector2(0, 1).RotatedByRandom(1) * Main.rand.NextFloat(-1, 1) * Timer / 10f);
                    }
                    Main.PlaySound(SoundID.Item70, projectile.Center);
                    Main.PlaySound(SoundID.NPCHit42, projectile.Center);
                }
            }

            if (State == 2) //retracting
            {
                projectile.velocity += -Vector2.Normalize(projectile.Center - player.Center) * 0.1f;

                if (projectile.velocity.Length() >= 5) 
                    State = 3;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) 
                    projectile.timeLeft = 0;

                if (projectile.timeLeft == 3) 
                    State = 4;
            }

            if (State == 3) //retracting faster
            {
                projectile.velocity = -Vector2.Normalize(projectile.Center - player.Center) * 5;
                projectile.velocity.Y += 3;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30) 
                    projectile.timeLeft = 0;

                if (projectile.timeLeft == 3)
                    State = 4;
            }

            if (State == 4) //retracting even faster and phasing
            {
                projectile.velocity = -Vector2.Normalize(projectile.Center - player.Center) * 18;
                projectile.tileCollide = false;

                if (Vector2.Distance(projectile.Center, player.Center) <= 30)
                    projectile.timeLeft = 0;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Player player = Main.player[projectile.owner];

            if (State != 0)
            {
                var chainTex = GetTexture("StarlightRiver/Assets/Items/Overgrow/ShakerChain");

                for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(player.Center, projectile.Center) / 16))
                {
                    var pos = (Vector2.Lerp(projectile.Center, player.Center + new Vector2(0, Main.player[projectile.owner].gfxOffY), k) - Main.screenPosition);
                    spriteBatch.Draw(chainTex, pos, null, lightColor, (projectile.Center - player.Center).ToRotation() + 1.58f, chainTex.Size() / 2, 1, 0, 0);
                }
            }

            var ballPos = (projectile.Center - Main.screenPosition);

            if (State == 0)
                ballPos += new Vector2(0, player.gfxOffY);

            spriteBatch.Draw(Main.projectileTexture[projectile.type], ballPos, Main.projectileTexture[projectile.type].Frame(), Color.White, projectile.rotation, projectile.Size / 2, projectile.scale, 0, 0);

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (State == 0)
            {
                float colormult = Timer / 100f * 0.7f;
                float scale = 1.2f - Timer / 100f * 0.5f;
                Texture2D tex = GetTexture("StarlightRiver/Assets/Tiles/Interactive/WispSwitchGlow2");
                Vector2 pos = ((projectile.Center - Main.screenPosition) + new Vector2(0, Main.player[projectile.owner].gfxOffY)).PointAccur();
                spriteBatch.Draw(tex, pos, tex.Frame(), Color.LightYellow * colormult, 0, tex.Size() / 2, scale, 0, 0);
            }

            if (Timer == 100)
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