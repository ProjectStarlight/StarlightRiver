using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Vitric
{
    internal class VKnife
    {
        public Vector2 pos;
        public int index;
        public int max;
        public float rotation = 0;
        public int direction = 1;

        internal VKnife(int index, int max)
        {
            this.index = index;
            this.max = max;
        }

        public void Update(float rotation, Player owner)
        {
            float angle = (float)index / max * MathHelper.TwoPi * direction;
            pos = new Vector2((float)Math.Cos(rotation + angle) * 8, (float)Math.Sin(rotation + angle) * 16);
            this.rotation = (float)Math.Sin(rotation + angle) / 2f;
        }
    }

    public class VitricSummonOrb : ModProjectile
    {
        private float startchase = 0;
        private float reversechase = 0;
        private float moltenglowanim = 0f;

        private float AnimationTimer
        {
            get => projectile.localAI[0];
            set => projectile.localAI[0] = value;
        }
        private float MovementLerp
        {
            get => projectile.localAI[1];
            set => projectile.localAI[1] = value;
        }
        private List<VKnife> knives;
        internal static readonly Vector2[] SwordOff = { new Vector2(4, 4), new Vector2(4, 9), new Vector2(4, 5), new Vector2(4, 38) };
        internal static readonly Vector2[] HoldingWeaponsOff = { new Vector2(-32, -24), new Vector2(10, -6), new Vector2(-32, -16), new Vector2(30, -32) };
        private enum WeaponType
        {
            Hammer, Sword, Knives, Javelin
        }

        //Why can't I just string the enum? Eh whatever, arrayed it
        private string[] WeaponSprite =
        {
            "VitricSummonHammer","VitricSummonSword","VitricSummonKnife", "VitricSummonJavelin"
        };

        private int Weapon
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }
        private int DisabledTime
        {
            get => (int)projectile.ai[1];
            set => projectile.ai[1] = value;
        }

        public VitricSummonOrb()
        {
            knives = new List<VKnife>();
            for (int i = 0; i < 3; i += 1)
                knives.Add(new VKnife(i, 3));
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write((int)startchase);
            writer.Write((int)reversechase);
            writer.Write((int)AnimationTimer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            startchase = reader.ReadInt32();
            reversechase = reader.ReadInt32();
            AnimationTimer = reader.ReadInt32();
        }

        public int NextWeapon
        {
            get
            {
                int projcount = 0;
                List<int> findweapon = new List<int>();
                for (int i = 0; i < 4; i++)
                    findweapon.Add(i);

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile currentProjectile = Main.projectile[i];
                    if (currentProjectile.active
                    && currentProjectile.owner == projectile.owner
                    && currentProjectile.type == projectile.type)
                        if (i == currentProjectile.whoAmI)
                        {
                            projcount += 1;
                            for (int j = 0; j < 20; j++)
                                findweapon.Add((int)currentProjectile.ai[0]);
                        }
                }

                if (projcount < 3)
                    for (int i = 0; i < 4; i++)
                        for (int j = Weapon; j >= 0; j--)
                            findweapon.Add(j);

                //Find least common
                return findweapon.ToArray().GroupBy(i => i).OrderBy(g => g.Count()).Select(g => g.Key).ToList().First();
            }

        }

        public static Rectangle WhiteFrame(Rectangle tex, bool white)
        {
            return new Rectangle(white ? tex.Width / 2 : 0, tex.Y, tex.Width / 2, tex.Height);
        }

        public static Color MoltenGlow(float time)
        {
            Color MoltenGlowc = Color.White;
            if (time > 30 && time < 60)
                MoltenGlowc = Color.Lerp(Color.White, Color.Orange, Math.Min((time - 30f) / 20f, 1f));
            else if (time >= 60)
                MoltenGlowc = Color.Lerp(Color.Orange, Color.Lerp(Color.Red, Color.Transparent, Math.Min((time - 60f) / 50f, 1f)), Math.Min((time - 60f) / 30f, 1f));
            return MoltenGlowc;
        }

        public override bool CanDamage() => false;

        public override bool MinionContactDamage() => false;

        public override string Texture => AssetDirectory.GlassBoss + "CrystalWave";

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
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.minion = true;
            projectile.minionSlots = 1f;
            projectile.penetrate = -1;
            projectile.timeLeft = 60;
        }

        public override void AI()
        {
            //if (projectile.owner == null || projectile.owner < 0)
            //return;

            Player player = projectile.Owner();

            if (player.dead || !player.active)
                player.ClearBuff(ModContent.BuffType<VitricSummonBuff>());

            if (player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
                projectile.timeLeft = 2;

            bool toplayer = true;
            Vector2 gothere = player.Center + new Vector2(player.direction * HoldingWeaponsOff[Weapon].X, HoldingWeaponsOff[Weapon].Y);

            if (DisabledTime > 0)
            {
                projectile.Center = player.Center;
                AnimationTimer = 0f;
                projectile.spriteDirection = player.direction;
                moltenglowanim = 0;
                if (DisabledTime == 1)
                    Weapon = NextWeapon;
            }

            AnimationTimer += 1;
            moltenglowanim += Weapon == (int)WeaponType.Knives ? 2f : 1f;
            DisabledTime -= 1;

            List<NPC> closestnpcs = new List<NPC>();

            for (int i = 0; i < Main.maxNPCs; i += 1)
            {
                bool colcheck = Collision.CheckAABBvLineCollision(Main.npc[i].position, new Vector2(Main.npc[i].width, Main.npc[i].height), Main.npc[i].Center, projectile.Center)
                    && Collision.CanHit(Main.npc[i].Center, 0, 0, projectile.Center, 0, 0);

                if (Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].townNPC && !Main.npc[i].dontTakeDamage && Main.npc[i].CanBeChasedBy() && colcheck
                    && (Main.npc[i].Center - player.Center).Length() < 300)
                    closestnpcs.Add(Main.npc[i]);
            }

            NPC them = closestnpcs.Count < 1 ? null : closestnpcs.ToArray().OrderBy(npc => projectile.Distance(npc.Center)).ToList()[0];
            NPC oldthem = null;

            if (player.HasMinionAttackTargetNPC)
            {
                oldthem = them;
                them = Main.npc[player.MinionAttackTargetNPC];
                gothere = them.Center + new Vector2(them.direction * 120, -64);
            }

            if (them != null && them.active && AnimationTimer > 15)
            {
                toplayer = false;
                if (!player.HasMinionAttackTargetNPC)
                    gothere = them.Center + Vector2.Normalize(projectile.Center - them.Center) * 64f;

                DoAttack((byte)Weapon, them);
            }

            float thisprojectile = 0f; //These names are completely nondescript. Please change them. -IDG-ok
            float maxmatchingprojectiles = 0f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile currentProjectile = Main.projectile[i]; //Smells like there should be a helper method for checking this validity. -IDG-I do have an idea on what do about that later
                if (currentProjectile.active
                && currentProjectile.owner == player.whoAmI
                && currentProjectile.type == projectile.type)
                {
                    if (i == projectile.whoAmI)
                        thisprojectile = maxmatchingprojectiles;
                    maxmatchingprojectiles += 1f;
                }
            }

            float timer = player.GetModPlayer<StarlightPlayer>().Timer * (MathHelper.TwoPi / 180f);
            double angles = thisprojectile / maxmatchingprojectiles * MathHelper.TwoPi - (float)Math.PI / 2f + timer * projectile.spriteDirection; //again, please work in radians.IDG-yes
            float dist = 16f;
            float aval = timer + thisprojectile * MathHelper.Pi * 0.465f * projectile.spriteDirection;
            Vector2 here;

            if (!toplayer)
            {
                here = new Vector2((float)Math.Sin(aval / 60f) * 6f, 20f * (float)Math.Sin(aval / 70f)).RotatedBy((them.Center - gothere).ToRotation());
                projectile.rotation = projectile.rotation.AngleTowards(MovementLerp * projectile.spriteDirection * 0.10f, 0.1f);
            }
            else
            {
                float anglz = (float)Math.Cos(aval) / 4f;
                projectile.rotation = projectile.rotation.AngleTowards(player.direction * 0 + anglz - MovementLerp * projectile.spriteDirection * 0.07f, 0.05f);
                here = new Vector2((float)Math.Cos(angles) / 2f, (float)Math.Sin(angles)) * dist;
            }

            foreach (VKnife knife in knives)
                knife.Update(aval, player);

            Vector2 where = gothere + here;
            Vector2 difference = where - projectile.Center;

            if ((where - projectile.Center).Length() > 0f)
            {
                Vector2 diff = where - projectile.Center;

                if (toplayer)
                {
                    startchase = 0f;
                    reversechase += 1f;
                    projectile.velocity += diff / 5f * Math.Min(reversechase / 405f, 1f);
                    projectile.velocity *= 0.725f * Math.Max(diff.Length() / 100f, 1f);
                    projectile.spriteDirection = player.direction;
                }
                else
                {
                    startchase += 1f;
                    reversechase = 0f;
                    projectile.velocity += diff * 0.005f * Math.Min(startchase / 45f, 1f);
                    projectile.velocity *= 0.925f;
                }
            }

            float maxspeed = Math.Min(projectile.velocity.Length(), 12 + (toplayer ? player.velocity.Length() : 0));
            projectile.velocity.Normalize();
            projectile.velocity *= maxspeed;

            MovementLerp += (projectile.velocity.X - MovementLerp) / 20f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {

            if (Weapon < 0 || DisabledTime > 0)
                return false;

            Texture2D tex = ModContent.GetTexture(AssetDirectory.VitricItem + WeaponSprite[Weapon]);

            float scale = Math.Min(AnimationTimer / 15f, 1f);
            Rectangle Rect = WhiteFrame(tex.Size().ToRectangle(), false);
            Rectangle Rect2 = WhiteFrame(tex.Size().ToRectangle(), true);


            Vector2 drawPos = projectile.Center - Main.screenPosition;
            Color color = lightColor * scale;
            Vector2 drawOrigin;

            //Local variables. Use them. Please. IDG-ok
            if (Weapon == (int)WeaponType.Hammer || Weapon == (int)WeaponType.Javelin)
            {
                drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
                spriteBatch.Draw(tex, drawPos, Rect, color, (projectile.rotation + (Weapon == 3 ? MathHelper.Pi / 2f : 0)) * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(tex, drawPos, Rect2, MoltenGlow(moltenglowanim), (projectile.rotation + (Weapon == 3 ? MathHelper.Pi / 2f : 0)) * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }

            if (Weapon == (int)WeaponType.Sword)
            {
                drawOrigin = new Vector2((projectile.spriteDirection < 0 ? tex.Width - SwordOff[1].X : SwordOff[1].X) / 2f, SwordOff[1].Y);
                spriteBatch.Draw(tex, drawPos, WhiteFrame(new Rectangle(0, tex.Height / 4, tex.Width, tex.Height / 4), false), color, projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                spriteBatch.Draw(tex, drawPos, WhiteFrame(new Rectangle(0, tex.Height / 4, tex.Width, tex.Height / 4), true), MoltenGlow(moltenglowanim), projectile.rotation * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }

            if (Weapon == (int)WeaponType.Knives)
                foreach (VKnife knife in knives)
                {
                    drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
                    float rotoffset = projectile.rotation + MathHelper.Pi / 4f + knife.rotation;
                    spriteBatch.Draw(tex, drawPos + knife.pos * scale, Rect, color, rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                    spriteBatch.Draw(tex, drawPos + knife.pos * scale, Rect2, MoltenGlow(moltenglowanim), rotoffset * projectile.spriteDirection, drawOrigin, projectile.scale * scale, projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                }

            return false;
        }

        public void DoAttack(byte attack, NPC target)
        {
            //something tells me attack should be an enum type here. Or at the very least commented
            //idg-done
            bool didattack = false;
            if (DisabledTime < 1 && AnimationTimer > 15)
            {
                float targetdistance = target.Distance(projectile.Center);
                if (attack == (int)WeaponType.Hammer && targetdistance < 72)
                {
                    Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity / 3f, ModContent.ProjectileType<VitricSummonHammer>(), projectile.damage, projectile.knockBack + 2f, projectile.owner)];
                    proj.ai[0] = projectile.rotation + (projectile.spriteDirection > 0 ? 0 : 1000);
                    proj.ai[1] = target.whoAmI;
                    projectile.ai[1] = 80;
                    (proj.modProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
                    proj.netUpdate = true;
                    didattack = true;
                }

                if (attack == (int)WeaponType.Sword && targetdistance < 80)
                {
                    Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity * 10.50f, ModContent.ProjectileType<VitricSummonSword>(), projectile.damage, projectile.knockBack + 1f, projectile.owner)];
                    proj.ai[0] = projectile.rotation + (projectile.spriteDirection > 0 ? 0 : 1000);
                    proj.ai[1] = target.whoAmI;
                    projectile.ai[1] = 80;
                    (proj.modProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
                    proj.netUpdate = true;
                    didattack = true;
                }

                if (attack == (int)WeaponType.Knives && targetdistance < 300)
                {
                    int index = 0;
                    foreach (VKnife knife in knives)
                    {
                        Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center + knife.pos, projectile.velocity * 2, ModContent.ProjectileType<VitricSummonKnife>(), projectile.damage, projectile.knockBack, projectile.owner)];
                        proj.ai[0] = projectile.rotation + knife.rotation + (projectile.spriteDirection > 0 ? 0 : 1000);
                        proj.ai[1] = target.whoAmI;
                        (proj.modProjectile as VitricSummonKnife).offset = new Vector2(0, -10 + index * 10);
                        projectile.ai[1] = 80;
                        (proj.modProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
                        proj.netUpdate = true;
                        index += 1;
                        didattack = true;
                    }
                }

                if (attack == (int)WeaponType.Javelin && targetdistance < 300)
                {
                    Projectile proj = Main.projectile[Projectile.NewProjectile(projectile.Center, projectile.velocity * -5f, ModContent.ProjectileType<VitricSummonJavelin>(), projectile.damage, projectile.knockBack, projectile.owner)];
                    proj.ai[0] = projectile.rotation + (projectile.spriteDirection > 0 ? 0 : 1000);
                    proj.ai[1] = target.whoAmI;
                    projectile.ai[1] = 80;
                    (proj.modProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
                    proj.netUpdate = true;
                    didattack = true;
                }

                if (didattack)
                    projectile.netUpdate = true;
            }
        }

    }

}

/*Concluding comments:
 * - Please try to minimize the amount of vars you use in projectiles, use the ai fields when possible. You also did not implement any netsync for the ones you did use. that needs to be done.
 * - please use local vars over splitting a call into multiple lines, its very hard to read and looks abhorrently messy.
 * - some of your var names are very nondescriptive, like "us". use VS rename tool to change this to something more appropriate for what it represents
 * - fields => properties => ctor => lambda methods => lambda overrides => overrides and methods. please.
 * - remove redundant parenthesis once you're done thinking through something, and please actually make an attempt to format things consistently. put whitespace lines between methods, before conditional statements, etc.
 * - note that you can also re-type the closing parenthesis on a statement or definition to have VS auto-format the tabulation for you. You had alot of incorrect tabulation.
 * - you have adapted vanilla code where you didnt re-name the local vars or comment that it was adapted vanilla code. dont do that.
 * - each weapon here belongs in it's own file. a 1.1k line long file is not OK.
 */

