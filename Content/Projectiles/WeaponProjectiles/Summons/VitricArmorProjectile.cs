using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Buffs.Summon;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Projectiles.WeaponProjectiles.Summons
{

    public class VitricSummonArmor : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Enchanted Vitric Weapons");
            Main.projFrames[projectile.type] = 1;
            ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true;
            Main.projPet[projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[projectile.type] = true;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }
        public sealed override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.tileCollide = false;
            projectile.friendly = false;
            projectile.minion = true;
            projectile.minionSlots = 1f;
            projectile.penetrate = -1;
            projectile.timeLeft = 60;
        }
        public override bool CanDamage() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            //if (projectile.owner == null || projectile.owner < 0)
            //return;


            Player player = Main.player[projectile.owner];
            if (player.dead || !player.active)
            {
                player.ClearBuff(ModContent.BuffType<VitricSummonBuff>());
            }
            if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
            {
                projectile.timeLeft = 2;
            }
            bool toplayer = true;
            Vector2 gothere = player.Center + new Vector2(player.direction * -32, 0);
            projectile.localAI[0] += 1;

            List<NPC> closestnpcs = new List<NPC>();
            for (int i = 0; i < Main.maxNPCs; i += 1)
            {
                bool colcheck = Collision.CheckAABBvLineCollision(Main.npc[i].position, new Vector2(Main.npc[i].width, Main.npc[i].height), Main.npc[i].Center, projectile.Center)
                    && Collision.CanHit(Main.npc[i].Center, 0, 0, projectile.Center, 0, 0);
                if (Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].townNPC && !Main.npc[i].dontTakeDamage && Main.npc[i].CanBeChasedBy() && colcheck
                    && (Main.npc[i].Center - player.Center).Length() < 300)
                    closestnpcs.Add(Main.npc[i]);
            }

            //int it=player.grappling.OrderBy(n => (Main.projectile[n].active ? 0 : 999999) + Main.projectile[n].timeLeft).ToArray()[0];
            NPC them = closestnpcs.Count < 1 ? null : closestnpcs.ToArray().OrderBy(npc => projectile.Distance(npc.Center)).ToList()[0];
            NPC oldthem = null;

            if (player.HasMinionAttackTargetNPC)
            {
                oldthem = them;
                them = Main.npc[player.MinionAttackTargetNPC];
                gothere = them.Center + new Vector2(them.direction * 96, them.direction == 0 ? -96 : 0);
            }

            if (them != null && them.active)
            {
                toplayer = false;
                if (!player.HasMinionAttackTargetNPC)
                    gothere = them.Center + Vector2.Normalize(projectile.Center - them.Center) * 64f;
                //if (projectile.ai[0]==1)
                //gothere = them.Center + new Vector2(them.direction * 64, 0);

            }

            /*int dust = Dust.NewDust(new Vector2(projectile.position.X, projectile.position.Y), projectile.width, projectile.height, ModContent.DustType<Dusts.GlassGravity>());
			Main.dust[dust].scale = 0.7f;
			Main.dust[dust].velocity = projectile.velocity * 0.2f;
			Main.dust[dust].noGravity = true;*/

            float us = 0f;
            float maxus = 0f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile currentProjectile = Main.projectile[i];
                if (currentProjectile.active
                && currentProjectile.owner == Main.myPlayer
                && currentProjectile.type == projectile.type)
                {
                    if (i == projectile.whoAmI)
                        us = maxus;
                    maxus += 1f;
                }
            }
            Vector2 there = player.Center;

            int timer = player.GetModPlayer<StarlightPlayer>().Timer * 2;
            double angles = MathHelper.ToRadians(((float)((us / maxus) * 360.00) - 90f) + timer);
            float dist = 16f;//Main.rand.NextFloat(54f, 96f);
            float aval = (float)timer + (us * 83f);
            Vector2 here;
            if (!toplayer)
            {
                here = (new Vector2((float)Math.Sin(aval / 60f) * 6f, 20f * ((float)Math.Sin(aval / 70f)))).RotatedBy((them.Center - gothere).ToRotation());
                projectile.rotation = projectile.rotation.AngleTowards(0, 0.1f);
            }
            else
            {
                float anglz = (float)(Math.Cos(MathHelper.ToRadians(aval)) * player.direction) / 4f;
                projectile.rotation = projectile.rotation.AngleTowards(((player.direction * 45) + anglz), 0.05f);
                here = new Vector2((float)Math.Cos(angles) / 2f, (float)Math.Sin(angles)) * dist;
            }

            Vector2 where = gothere + here;
            Vector2 difference = where - projectile.Center;

            if ((where - projectile.Center).Length() > 0f)
            {
                if (toplayer)
                {
                    projectile.velocity += (where - projectile.Center) * 0.25f;
                    projectile.velocity *= 0.725f;
                }
                else
                {
                    projectile.velocity += (where - projectile.Center) * 0.005f;
                    projectile.velocity *= 0.925f;
                }
            }

            float maxspeed = Math.Min(projectile.velocity.Length(), 12 + (toplayer ? player.velocity.Length() : 0));
            projectile.velocity.Normalize();
            projectile.velocity *= maxspeed;

            Lighting.AddLight(projectile.Center, Color.Yellow.ToVector3() * 0.78f);

        }

        public override string Texture
        {
            get { return ("StarlightRiver/Bosses/GlassBoss/CrystalWave"); }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {

            if (projectile.localAI[0] < 0)
                return false;
            Texture2D tex = Main.projectileTexture[projectile.type];
            Player player = Main.player[projectile.owner];

            Vector2 drawOrigin = new Vector2(tex.Width, tex.Height) / 2f;
            Vector2 drawPos = ((projectile.Center - Main.screenPosition));
            Color color = Color.Lerp((projectile.GetAlpha(lightColor) * 0.5f), Color.White * (Math.Min(projectile.localAI[0] / 15f, 1f)), 0.5f);
            spriteBatch.Draw(tex, drawPos, null, color, (projectile.velocity.X) * 0.07f + projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

    }

}