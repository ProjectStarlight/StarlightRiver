using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	public class Zapper : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;
        public Tile parent;
        private Vector2 dims = new Vector2(32, 0);

        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.height = 8;
            Projectile.width = 8;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
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
                if (Main.tileSolid[Main.tile[((int)Projectile.position.X + 4) / 16, (int)(Projectile.position.Y + k) / 16].type] && Main.tile[(int)Projectile.position.X / 16, (int)(Projectile.position.Y + k) / 16].active()) break;
            }

            foreach (Player Player in Main.player.Where(Player => Player.active))
                if (Collision.CheckAABBvAABBCollision(Projectile.position, dims, Player.position, Player.Hitbox.Size()) && !Player.ActiveAbility<Whip>())
                {
                    Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " was zapped to death."), 50, 0);
                    Player.velocity.X = Player.velocity.Length() <= 8 ? (-Vector2.Normalize(Player.velocity) * 8).X : Player.velocity.X * -1;
                    Player.velocity.Y = 0.1f;

                    Projectile proj = Main.projectile.FirstOrDefault(p => p.owner == Player.whoAmI && Main.projHook[p.type]);
                    if (proj != null) proj.timeLeft = 0;

                    Player.GetHandler().ActiveAbility?.Deactivate();
                }

            Projectile.timeLeft = 2;
            if (!parent.active())
                Projectile.timeLeft = 0;

            //Dust
            if (Main.time % 15 == 0)
            {
                Vector2 startpos = Projectile.Center + new Vector2(8, -8);
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
                Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit53.WithVolume(0.2f), joints[(int)dims.Y / 20]);
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Vector2 pos = Projectile.position - Main.screenPosition + new Vector2(1, -23);
            spriteBatch.Draw(Request<Texture2D>(AssetDirectory.OvergrowTile + "ZapperGlow0").Value, pos, Helper.IndicatorColor);
            spriteBatch.Draw(Request<Texture2D>(AssetDirectory.OvergrowTile + "ZapperGlow1").Value, pos + Vector2.One * 3, Color.White * 0.8f);
        }
    }
}