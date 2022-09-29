//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using StarlightRiver.Content.Buffs.Summon;
//using StarlightRiver.Core;
//using StarlightRiver.Helpers;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace StarlightRiver.Content.Items.Vitric
//{
//	internal class VKnife
//    {
//        public Vector2 pos;
//        public int index;
//        public int max;
//        public float rotation = 0;
//        public int direction = 1;

//        internal VKnife(int index, int max)
//        {
//            this.index = index;
//            this.max = max;
//        }

//        public void Update(float rotation, Player owner)
//        {
//            float angle = (float)index / max * MathHelper.TwoPi * direction;
//            pos = new Vector2((float)Math.Cos(rotation + angle) * 8, (float)Math.Sin(rotation + angle) * 16);
//            this.rotation = (float)Math.Sin(rotation + angle) / 2f;
//        }
//    }

//    public class VitricSummonOrb : ModProjectile
//    {
//        private float startchase = 0;
//        private float reversechase = 0;
//        private float moltenglowanim = 0f;

//        private float AnimationTimer
//        {
//            get => Projectile.localAI[0];
//            set => Projectile.localAI[0] = value;
//        }
//        private float MovementLerp
//        {
//            get => Projectile.localAI[1];
//            set => Projectile.localAI[1] = value;
//        }
//        private List<VKnife> knives;
//        internal static readonly Vector2[] SwordOff = { new Vector2(4, 4), new Vector2(4, 9), new Vector2(4, 5), new Vector2(4, 38) };
//        internal static readonly Vector2[] HoldingWeaponsOff = { new Vector2(-32, -24), new Vector2(10, -6), new Vector2(-32, -16), new Vector2(30, -32) };
//        private enum WeaponType
//        {
//            Hammer, Sword, Knives, Javelin
//        }

//        //Why can't I just string the enum? Eh whatever, arrayed it
//        private string[] WeaponSprite =
//        {
//            "VitricSummonHammer","VitricSummonSword","VitricSummonKnife", "VitricSummonJavelin"
//        };

//        private int Weapon
//        {
//            get => (int)Projectile.ai[0];
//            set => Projectile.ai[0] = value;
//        }
//        private int DisabledTime
//        {
//            get => (int)Projectile.ai[1];
//            set => Projectile.ai[1] = value;
//        }

//        public VitricSummonOrb()
//        {
//            knives = new List<VKnife>();
//            for (int i = 0; i < 3; i += 1)
//                knives.Add(new VKnife(i, 3));
//        }

//        public override void SendExtraAI(BinaryWriter writer)
//        {
//            writer.Write((int)startchase);
//            writer.Write((int)reversechase);
//            writer.Write((int)AnimationTimer);
//        }

//        public override void ReceiveExtraAI(BinaryReader reader)
//        {
//            startchase = reader.ReadInt32();
//            reversechase = reader.ReadInt32();
//            AnimationTimer = reader.ReadInt32();
//        }

//        public int NextWeapon
//        {
//            get
//            {
//                int projcount = 0;
//                List<int> findweapon = new List<int>();
//                for (int i = 0; i < 4; i++)
//                    findweapon.Add(i);

//                for (int i = 0; i < Main.maxProjectiles; i++)
//                {
//                    Projectile currentProjectile = Main.projectile[i];
//                    if (currentProjectile.active
//                    && currentProjectile.owner == Projectile.owner
//                    && currentProjectile.type == Projectile.type)
//                        if (i == currentProjectile.whoAmI)
//                        {
//                            projcount += 1;
//                            for (int j = 0; j < 20; j++)
//                                findweapon.Add((int)currentProjectile.ai[0]);
//                        }
//                }

//                if (projcount < 3)
//                    for (int i = 0; i < 4; i++)
//                        for (int j = Weapon; j >= 0; j--)
//                            findweapon.Add(j);

//                //Find least common
//                return findweapon.ToArray().GroupBy(i => i).OrderBy(g => g.Count()).Select(g => g.Key).ToList().First();
//            }

//        }

//        public static Rectangle WhiteFrame(Rectangle tex, bool white)
//        {
//            return new Rectangle(white ? tex.Width / 2 : 0, tex.Y, tex.Width / 2, tex.Height);
//        }

//        public static Color MoltenGlow(float time)
//        {
//            Color MoltenGlowc = Color.White;
//            if (time > 30 && time < 60)
//                MoltenGlowc = Color.Lerp(Color.White, Color.Orange, Math.Min((time - 30f) / 20f, 1f));
//            else if (time >= 60)
//                MoltenGlowc = Color.Lerp(Color.Orange, Color.Lerp(Color.Red, Color.Transparent, Math.Min((time - 60f) / 50f, 1f)), Math.Min((time - 60f) / 30f, 1f));
//            return MoltenGlowc;
//        }

//        public override bool? CanDamage() => false;

//        public override bool MinionContactDamage() => false;

//        public override string Texture => AssetDirectory.VitricBoss + "CrystalWave";

//        public override void SetStaticDefaults()
//        {
//            DisplayName.SetDefault("Enchanted Vitric Weapons");
//            Main.projFrames[Projectile.type] = 1;
//            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
//            Main.projPet[Projectile.type] = true;
//            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
//            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
//        }

//        public sealed override void SetDefaults()
//        {
//            Projectile.width = 16;
//            Projectile.height = 16;
//            Projectile.tileCollide = false;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.minion = true;
//            Projectile.minionSlots = 1f;
//            Projectile.penetrate = -1;
//            Projectile.timeLeft = 60;
//        }

//        public override void AI()
//        {
//            //if (Projectile.owner == null || Projectile.owner < 0)
//            //return;

//            Player Player = Projectile.Owner();

//            if (Player.dead || !Player.active)
//                Player.ClearBuff(ModContent.BuffType<VitricSummonBuff>());

//            if (Player.HasBuff(ModContent.BuffType<VitricSummonBuff>()))
//                Projectile.timeLeft = 2;

//            bool toPlayer = true;
//            Vector2 gothere = Player.Center + new Vector2(Player.direction * HoldingWeaponsOff[Weapon].X, HoldingWeaponsOff[Weapon].Y);

//            if (DisabledTime > 0)
//            {
//                Projectile.Center = Player.Center;
//                AnimationTimer = 0f;
//                Projectile.spriteDirection = Player.direction;
//                moltenglowanim = 0;
//                if (DisabledTime == 1)
//                    Weapon = NextWeapon;
//            }

//            AnimationTimer += 1;
//            moltenglowanim += Weapon == (int)WeaponType.Knives ? 2f : 1f;
//            DisabledTime -= 1;

//            List<NPC> closestNPCs = new List<NPC>();

//            for (int i = 0; i < Main.maxNPCs; i += 1)
//            {
//                bool colcheck = Collision.CheckAABBvLineCollision(Main.npc[i].position, new Vector2(Main.npc[i].width, Main.npc[i].height), Main.npc[i].Center, Projectile.Center)
//                    && Collision.CanHit(Main.npc[i].Center, 0, 0, Projectile.Center, 0, 0);

//                if (Main.npc[i].active && !Main.npc[i].friendly && !Main.npc[i].townNPC && !Main.npc[i].dontTakeDamage && Main.npc[i].CanBeChasedBy() && colcheck
//                    && (Main.npc[i].Center - Player.Center).Length() < 300)
//                    closestNPCs.Add(Main.npc[i]);
//            }

//            NPC them = closestNPCs.Count < 1 ? null : closestNPCs.ToArray().OrderBy(NPC => Projectile.Distance(NPC.Center)).ToList()[0];
//            NPC oldthem = null;

//            if (Player.HasMinionAttackTargetNPC)
//            {
//                oldthem = them;
//                them = Main.npc[Player.MinionAttackTargetNPC];
//                gothere = them.Center + new Vector2(them.direction * 120, -64);
//            }

//            if (them != null && them.active && AnimationTimer > 15)
//            {
//                toPlayer = false;
//                if (!Player.HasMinionAttackTargetNPC)
//                    gothere = them.Center + Vector2.Normalize(Projectile.Center - them.Center) * 64f;

//                DoAttack((byte)Weapon, them);
//            }

//            float thisProjectile = 0f; //These names are completely nondescript. Please change them. -IDG-ok
//            float maxmatchingProjectiles = 0f;

//            for (int i = 0; i < Main.maxProjectiles; i++)
//            {
//                Projectile currentProjectile = Main.projectile[i]; //Smells like there should be a helper method for checking this validity. -IDG-I do have an idea on what do about that later
//                if (currentProjectile.active
//                && currentProjectile.owner == Player.whoAmI
//                && currentProjectile.type == Projectile.type)
//                {
//                    if (i == Projectile.whoAmI)
//                        thisProjectile = maxmatchingProjectiles;
//                    maxmatchingProjectiles += 1f;
//                }
//            }

//            float timer = Player.GetModPlayer<StarlightPlayer>().Timer * (MathHelper.TwoPi / 180f);
//            double angles = thisProjectile / maxmatchingProjectiles * MathHelper.TwoPi - (float)Math.PI / 2f + timer * Projectile.spriteDirection; //again, please work in radians.IDG-yes
//            float dist = 16f;
//            float aval = timer + thisProjectile * MathHelper.Pi * 0.465f * Projectile.spriteDirection;
//            Vector2 here;

//            if (!toPlayer)
//            {
//                here = new Vector2((float)Math.Sin(aval / 60f) * 6f, 20f * (float)Math.Sin(aval / 70f)).RotatedBy((them.Center - gothere).ToRotation());
//                Projectile.rotation = Projectile.rotation.AngleTowards(MovementLerp * Projectile.spriteDirection * 0.10f, 0.1f);
//            }
//            else
//            {
//                float anglz = (float)Math.Cos(aval) / 4f;
//                Projectile.rotation = Projectile.rotation.AngleTowards(Player.direction * 0 + anglz - MovementLerp * Projectile.spriteDirection * 0.07f, 0.05f);
//                here = new Vector2((float)Math.Cos(angles) / 2f, (float)Math.Sin(angles)) * dist;
//            }

//            foreach (VKnife knife in knives)
//                knife.Update(aval, Player);

//            Vector2 where = gothere + here;
//            Vector2 difference = where - Projectile.Center;

//            if ((where - Projectile.Center).Length() > 0f)
//            {
//                Vector2 diff = where - Projectile.Center;

//                if (toPlayer)
//                {
//                    startchase = 0f;
//                    reversechase += 1f;
//                    Projectile.velocity += diff / 5f * Math.Min(reversechase / 405f, 1f);
//                    Projectile.velocity *= 0.725f * Math.Max(diff.Length() / 100f, 1f);
//                    Projectile.spriteDirection = Player.direction;
//                }
//                else
//                {
//                    startchase += 1f;
//                    reversechase = 0f;
//                    Projectile.velocity += diff * 0.005f * Math.Min(startchase / 45f, 1f);
//                    Projectile.velocity *= 0.925f;
//                }
//            }

//            float maxspeed = Math.Min(Projectile.velocity.Length(), 12 + (toPlayer ? Player.velocity.Length() : 0));
//            Projectile.velocity.Normalize();
//            Projectile.velocity *= maxspeed;

//            MovementLerp += (Projectile.velocity.X - MovementLerp) / 20f;
//        }

//        public override bool PreDraw(ref Color lightColor)
//        {

//            if (Weapon < 0 || DisabledTime > 0)
//                return false;

//            Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + WeaponSprite[Weapon]).Value;

//            float scale = Math.Min(AnimationTimer / 15f, 1f);
//            Rectangle Rect = WhiteFrame(tex.Size().ToRectangle(), false);
//            Rectangle Rect2 = WhiteFrame(tex.Size().ToRectangle(), true);
//            SpriteBatch spriteBatch = Main.spriteBatch;

//            Vector2 drawPos = Projectile.Center - Main.screenPosition;
//            Color color = lightColor * scale;
//            Vector2 drawOrigin;

//            //Local variables. Use them. Please. IDG-ok
//            if (Weapon == (int)WeaponType.Hammer || Weapon == (int)WeaponType.Javelin)
//            {
//                drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
//                spriteBatch.Draw(tex, drawPos, Rect, color, (Projectile.rotation + (Weapon == 3 ? MathHelper.Pi / 2f : 0)) * Projectile.spriteDirection, drawOrigin, Projectile.scale * scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//                spriteBatch.Draw(tex, drawPos, Rect2, MoltenGlow(moltenglowanim), (Projectile.rotation + (Weapon == 3 ? MathHelper.Pi / 2f : 0)) * Projectile.spriteDirection, drawOrigin, Projectile.scale * scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//            }

//            if (Weapon == (int)WeaponType.Sword)
//            {
//                drawOrigin = new Vector2((Projectile.spriteDirection < 0 ? tex.Width - SwordOff[1].X : SwordOff[1].X) / 2f, SwordOff[1].Y);
//                spriteBatch.Draw(tex, drawPos, WhiteFrame(new Rectangle(0, tex.Height / 4, tex.Width, tex.Height / 4), false), color, Projectile.rotation * Projectile.spriteDirection, drawOrigin, Projectile.scale * scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//                spriteBatch.Draw(tex, drawPos, WhiteFrame(new Rectangle(0, tex.Height / 4, tex.Width, tex.Height / 4), true), MoltenGlow(moltenglowanim), Projectile.rotation * Projectile.spriteDirection, drawOrigin, Projectile.scale * scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//            }

//            if (Weapon == (int)WeaponType.Knives)
//                foreach (VKnife knife in knives)
//                {
//                    drawOrigin = new Vector2(tex.Width / 2f, tex.Height) / 2f;
//                    float rotoffset = Projectile.rotation + MathHelper.Pi / 4f + knife.rotation;
//                    spriteBatch.Draw(tex, drawPos + knife.pos * scale, Rect, color, rotoffset * Projectile.spriteDirection, drawOrigin, Projectile.scale * scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//                    spriteBatch.Draw(tex, drawPos + knife.pos * scale, Rect2, MoltenGlow(moltenglowanim), rotoffset * Projectile.spriteDirection, drawOrigin, Projectile.scale * scale, Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
//                }

//            return false;
//        }

//        public void DoAttack(byte attack, NPC target)
//        {
//            //something tells me attack should be an enum type here. Or at the very least commented
//            //idg-done
//            bool didattack = false;
//            if (DisabledTime < 1 && AnimationTimer > 15)
//            {
//                float targetdistance = target.Distance(Projectile.Center);
//                if (attack == (int)WeaponType.Hammer && targetdistance < 72)
//                {
//                    Projectile proj = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity / 3f, ModContent.ProjectileType<VitricSummonHammer>(), Projectile.damage, Projectile.knockBack + 2f, Projectile.owner)];
//                    proj.ai[0] = Projectile.rotation + (Projectile.spriteDirection > 0 ? 0 : 1000);
//                    proj.ai[1] = target.whoAmI;
//                    Projectile.ai[1] = 80;
//                    (proj.ModProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
//                    proj.netUpdate = true;
//                    didattack = true;
//                }

//                if (attack == (int)WeaponType.Sword && targetdistance < 80)
//                {
//                    Projectile proj = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 10.50f, ModContent.ProjectileType<VitricSummonSword>(), Projectile.damage, Projectile.knockBack + 1f, Projectile.owner)];
//                    proj.ai[0] = Projectile.rotation + (Projectile.spriteDirection > 0 ? 0 : 1000);
//                    proj.ai[1] = target.whoAmI;
//                    Projectile.ai[1] = 80;
//                    (proj.ModProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
//                    proj.netUpdate = true;
//                    didattack = true;
//                }

//                if (attack == (int)WeaponType.Knives && targetdistance < 300)
//                {
//                    int index = 0;
//                    foreach (VKnife knife in knives)
//                    {
//                        Projectile proj = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + knife.pos, Projectile.velocity * 2, ModContent.ProjectileType<VitricSummonKnife>(), Projectile.damage, Projectile.knockBack, Projectile.owner)];
//                        proj.ai[0] = Projectile.rotation + knife.rotation + (Projectile.spriteDirection > 0 ? 0 : 1000);
//                        proj.ai[1] = target.whoAmI;
//                        (proj.ModProjectile as VitricSummonKnife).offset = new Vector2(0, -10 + index * 10);
//                        Projectile.ai[1] = 80;
//                        (proj.ModProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
//                        proj.netUpdate = true;
//                        index += 1;
//                        didattack = true;
//                    }
//                }

//                if (attack == (int)WeaponType.Javelin && targetdistance < 300)
//                {
//                    Projectile proj = Main.projectile[Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * -5f, ModContent.ProjectileType<VitricSummonJavelin>(), Projectile.damage, Projectile.knockBack, Projectile.owner)];
//                    proj.ai[0] = Projectile.rotation + (Projectile.spriteDirection > 0 ? 0 : 1000);
//                    proj.ai[1] = target.whoAmI;
//                    Projectile.ai[1] = 80;
//                    (proj.ModProjectile as VitricSummonHammer).animationProgress = moltenglowanim;
//                    proj.netUpdate = true;
//                    didattack = true;
//                }

//                if (didattack)
//                    Projectile.netUpdate = true;
//            }
//        }

//    }

//}

///*Concluding comments:
// * - Please try to minimize the amount of vars you use in Projectiles, use the ai fields when possible. You also did not implement any netsync for the ones you did use. that needs to be done.
// * - please use local vars over splitting a call into multiple lines, its very hard to read and looks abhorrently messy.
// * - some of your var names are very nondescriptive, like "us". use VS rename tool to change this to something more appropriate for what it represents
// * - fields => properties => ctor => lambda methods => lambda overrides => overrides and methods. please.
// * - remove redundant parenthesis once you're done thinking through something, and please actually make an attempt to format things consistently. put whitespace lines between methods, before conditional statements, etc.
// * - note that you can also re-type the closing parenthesis on a statement or definition to have VS auto-format the tabulation for you. You had alot of incorrect tabulation.
// * - you have adapted vanilla code where you didnt re-name the local vars or comment that it was adapted vanilla code. dont do that.
// * - each weapon here belongs in it's own file. a 1.1k line long file is not OK.
// */

