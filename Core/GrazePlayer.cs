using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using Terraria.GameContent;
using StarlightRiver.Content.CustomHooks;
using Terraria.ID;
using StarlightRiver.Content.Dusts;
using Terraria.Audio;

namespace StarlightRiver.Core
{
    /// <summary>
    /// A ModPlayer containing all logic for "grazing" effects of enemy projectiles (near misses)
    /// </summary>
    public class GrazePlayer : ModPlayer
    {
        public readonly Color GrazeColor = new Color(135, 235, 255);

        public float grazeRectangleMult;

        public int lastGrazeDamage; // damage of the last projectile the player grazed, before defense etc. 

        private int grazeCooldown;
        public int GrazeCooldownLength = 120; // modify this if you want to change the cooldown

        public bool doGrazeLogic;

        public bool justGrazed;

        public bool inGrazeRect;

        public Projectile grazeProj;

        public override void ResetEffects()
        {
            grazeRectangleMult = 0;

            doGrazeLogic = false;
            grazeProj = null;

            GrazeCooldownLength = 120;

            if (grazeCooldown > 0)
                grazeCooldown --;
        }

        public override void PreUpdate()
        {
            if (!doGrazeLogic)
                return;

            if (grazeCooldown > 0)
            {
                float scale = MathHelper.Lerp(0.55f, 0.15f, 1f - (grazeCooldown / (float)GrazeCooldownLength));
                Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<Glow>(), 0f, 0f, 0, GrazeColor, scale);
                if (grazeCooldown == 1)
                    SoundEngine.PlaySound(SoundID.MaxMana with { Pitch = -0.2f }, Player.position);
            }

            if (grazeCooldown <= 0) // this was changed because before it would scan targets while on cooldown, causing you to immediately graze again after the cooldown is over, even if there isnt a projectile nearby to graze
            {
                Rectangle grazeRect = new Rectangle(Player.Hitbox.X - (int)(12 * (grazeRectangleMult + 1f)), Player.Hitbox.Y - (int)(22 * (grazeRectangleMult + 1f)), (int)(Player.Hitbox.Width * (2f + grazeRectangleMult)), (int)(Player.Hitbox.Height * (2f + grazeRectangleMult)));

                float maxDist = 200f;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    //TODO: use entity sources to make it so that projectiles spawned from npcs that were spawned from statues dont proc grazes. I have no fucking clue how to do this but
                    if (proj.hostile && proj.active && proj.Distance(Player.Center) < maxDist && !proj.GetGlobalProjectile<GrazeProjectile>().hitAndGrazedPlayers.Contains(Player))
                    {
                        float dist = proj.Distance(Player.Center);
                        if (dist < maxDist)
                        {
                            grazeProj = proj;
                            maxDist = dist;
                        }
                    }
                }

                if (grazeProj != null)
                {
                    if (grazeProj.Hitbox.Intersects(Player.Hitbox))
                    {
                        grazeProj.GetGlobalProjectile<GrazeProjectile>().hitAndGrazedPlayers.Add(Player);
                        grazeProj = null;
                        justGrazed = false;
                        inGrazeRect = false;
                        return;
                    }

                    if (grazeProj.Hitbox.Intersects(grazeRect))
                        inGrazeRect = true;

                    if (!grazeProj.Hitbox.Intersects(grazeRect) && inGrazeRect)
                    {
                        grazeProj.GetGlobalProjectile<GrazeProjectile>().hitAndGrazedPlayers.Add(Player);

                        grazeCooldown = GrazeCooldownLength;

                        lastGrazeDamage = grazeProj.damage;

                        justGrazed = true;
                        DoGrazeEffects();
                        inGrazeRect = false;

                    }
                }
            }
        }
        public override void PostUpdate() => justGrazed = false;

        private void DoGrazeEffects()
        {
            Helper.PlayPitched("ProjectileImpact1", 0.8f, Main.rand.NextFloat(-0.05f, 0.05f), Player.position);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, ModContent.DustType<Glow>(), 0f, 0f, 0, GrazeColor, Main.rand.NextFloat(0.3f, 0.45f));
            }
        }
    }

    public class GrazeProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public List<Player> hitAndGrazedPlayers = new List<Player>();
    }
}
