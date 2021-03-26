using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    public class Zapper : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;
        public Tile parent;
        private Vector2 dims = new Vector2(32, 0);

        public override void SetDefaults()
        {
            projectile.hostile = true;
            projectile.height = 8;
            projectile.width = 8;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
            projectile.tileCollide = false;
            projectile.ignoreWater = true;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Zapper");
        }

        public override void AI()
        {
            dims.Y = 0;
            for (int k = 0; 1 == 1; k++)
            {
                dims.Y++;
                if (Main.tileSolid[Main.tile[((int)projectile.position.X + 4) / 16, (int)(projectile.position.Y + k) / 16].type] && Main.tile[(int)projectile.position.X / 16, (int)(projectile.position.Y + k) / 16].active()) break;
            }

            foreach (Player player in Main.player.Where(player => player.active))
                if (Collision.CheckAABBvAABBCollision(projectile.position, dims, player.position, player.Hitbox.Size()) && !player.ActiveAbility<Wisp>())
                {
                    player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was zapped to death."), 50, 0);
                    player.velocity.X = player.velocity.Length() <= 8 ? (-Vector2.Normalize(player.velocity) * 8).X : player.velocity.X * -1;
                    player.velocity.Y = 0.1f;

                    Projectile proj = Main.projectile.FirstOrDefault(p => p.owner == player.whoAmI && Main.projHook[p.type]);
                    if (proj != null) proj.timeLeft = 0;

                    player.GetHandler().ActiveAbility?.Deactivate();
                }

            projectile.timeLeft = 2;
            if (!parent.active())
                projectile.timeLeft = 0;

            //Dust
            if (Main.time % 15 == 0)
            {
                Vector2 startpos = projectile.Center + new Vector2(8, -8);
                Vector2[] joints = new Vector2[(int)dims.Y / 20 + 1];
                joints[0] = startpos;
                joints[(int)dims.Y / 20] = startpos + new Vector2(0, dims.Y);

                for (int k = 1; k < joints.Count(); k++)
                {
                    if (k < joints.Count() - 1)
                    {
                        joints[k].X = startpos.X + Main.rand.NextFloat(-16, 16);
                        joints[k].Y = startpos.Y + k * 20 + Main.rand.NextFloat(-5, 5);
                    }
                    for (float k2 = 0; k2 <= 1; k2 += 0.1f)
                        Dust.NewDustPerfect(Vector2.Lerp(joints[k], joints[k - 1], k2), DustType<Dusts.GoldNoMovement>(), null, 0, default, 0.5f);
                }
                for (float k = 0; k <= 3.14f; k += 0.1f)
                    Dust.NewDustPerfect(joints[(int)dims.Y / 20], DustType<Content.Dusts.GoldWithMovement>(), new Vector2(-1, 0).RotatedBy(k) * Main.rand.NextFloat(2), 0, default, 0.6f);
                Main.PlaySound(SoundID.NPCHit53.WithVolume(0.2f), joints[(int)dims.Y / 20]);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = projectile.position - Main.screenPosition + new Vector2(1, -23);
            spriteBatch.Draw(GetTexture(AssetDirectory.OvergrowTile + "ZapperGlow0"), pos, Helper.IndicatorColor);
            spriteBatch.Draw(GetTexture(AssetDirectory.OvergrowTile + "ZapperGlow1"), pos + Vector2.One * 3, Color.White * 0.8f);
        }
    }
}