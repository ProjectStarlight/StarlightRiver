using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Buffs;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    internal class FiletKnife : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Filet Knife");
            Tooltip.SetDefault("egshels update this lol");
        }
        public override void SetDefaults()
        {
            item.damage = 20;
            item.melee = true;
            item.width = 36;
            item.height = 38;
            item.useTime = 25;
            item.useAnimation = 25;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item1;
            item.autoReuse = false;
            item.useTurn = true;
            item.crit = 12;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (player.HasBuff(ModContent.BuffType<FiletFrenzyBuff>()))
                crit = false;
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (crit)
            {
                int itemType = -1;
                switch (Main.rand.Next(3))
                {
                    case 0:
                        itemType = ModContent.ItemType<FiletGiblet1>();
                        break;
                    case 1:
                        itemType = ModContent.ItemType<FiletGiblet2>();
                        break;
                    default:
                        itemType = ModContent.ItemType<FiletGiblet3>();
                        break;
                }
                Item.NewItem(target.Center, itemType);

                if (target.GetGlobalNPC<FiletNPC>().DOT < 3)
                    target.GetGlobalNPC<FiletNPC>().DOT += 1;


                Projectile.NewProjectile(target.Center, Vector2.Zero, ModContent.ProjectileType<FiletSlash>(), 0, 0, player.whoAmI, target.whoAmI);

                Vector2 direction = Vector2.Normalize(target.Center - player.Center);
                for (int j = 0; j < 15; j++)
                {
                    Dust.NewDustPerfect(target.Center, DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0f, 6f), 0, default, 1.5f);
                    Dust.NewDustPerfect(target.Center, DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.NextFloat(0f, 3f), 0, default, 0.8f);
                }
            }
        }
    }
    public class FiletGiblet1 : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Giblet");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 10);
            player.HealEffect(10);
            player.statLife += healAmount;

            player.AddBuff(BuffID.WellFed, 18000);
            player.AddBuff(ModContent.BuffType<FiletFrenzyBuff>(), 600);
            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }
    }

    public class FiletGiblet2 : FiletGiblet1 { }
    public class FiletGiblet3 : FiletGiblet1 { }

    public class FiletNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int DOT = 0;

        public bool hasSword = false;

        public override void SetDefaults(NPC npc)
        {
            if (npc.type == NPCID.BloodZombie && Main.rand.NextBool(50))
                hasSword = true;
            base.SetDefaults(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {     
            if (hasSword)
            {
                Texture2D tex = ModContent.GetTexture(AssetDirectory.MiscItem + "FiletKnifeEmbedded");
                bool facingLeft = npc.direction == -1;

                Vector2 origin = facingLeft ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);
                SpriteEffects effects = facingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float rotation = facingLeft ? 0.78f : -0.78f;


                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, drawColor, rotation, origin, npc.scale, effects, 0f);
            }
            return base.PreDraw(npc, spriteBatch, drawColor);
        }

        public override void NPCLoot(NPC npc)
        {
            if (hasSword && Main.rand.NextBool(3))
                Item.NewItem(npc.Center, ModContent.ItemType<FiletKnife>());
            base.NPCLoot(npc);
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (DOT == 0)
                return;
            if (npc.lifeRegen > 0)
            {
                npc.lifeRegen = 0;
            }
            npc.lifeRegen -= DOT * 3;
            if (damage < DOT)
            {
                damage = DOT;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (DOT != 0)
            {
                if (Main.rand.Next(5) < DOT)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, default, default(Color), 1.25f);
            }

            if (hasSword)
                Dust.NewDustPerfect(npc.Center - new Vector2(npc.spriteDirection * 12, 0), DustID.Blood, Vector2.Zero);
        }
    }
    public class FiletSlash : ModProjectile, IDrawPrimitive
    {
        public override string Texture => AssetDirectory.MiscItem + "FiletKnife";

        private readonly int BASETIMELEFT = 12;

        BasicEffect effect;

        private List<Vector2> cache;
        private Trail trail;

        private Vector2 direction = Vector2.Zero;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Slash");

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.aiStyle = -1;
            projectile.friendly = false;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = BASETIMELEFT - 2;
            projectile.ignoreWater = true;
            projectile.alpha = 255;
        }

        public override void AI()
        {
            if (effect == null)
            {
                effect = new BasicEffect(Main.instance.GraphicsDevice);
                effect.VertexColorEnabled = true;
            }

            NPC target = Main.npc[(int)projectile.ai[0]];
            projectile.Center = target.Center;

            if (direction == Vector2.Zero)
                direction = Main.rand.NextFloat(6.28f).ToRotationVector2() * (target.width + target.height) * 0.06f;
            cache = new List<Vector2>();

            float progress = (BASETIMELEFT - projectile.timeLeft) / (float)BASETIMELEFT;

            int widthExtra = (int)(6 * Math.Sin(progress * 3.14f));

            int min = (BASETIMELEFT - (20 + widthExtra)) - projectile.timeLeft;
            int max = (BASETIMELEFT + (widthExtra)) - projectile.timeLeft;

            int average = (min + max) / 2;
            for (int i = min; i < max; i++)
            {
                float offset = (float)Math.Pow(Math.Abs(i - average) / (float)(max - min), 2);
                Vector2 offsetVector = (direction.RotatedBy(1.57f) * offset * 10);

                cache.Add(target.Center + (direction * i));
            }

            trail = new Trail(Main.instance.GraphicsDevice, 20 + (widthExtra * 2), new TriangularTip((int)((target.width + target.height) * 0.6f)), factor => 10 * (1 - Math.Abs((1 - factor) - (projectile.timeLeft /(float)(BASETIMELEFT + 5)))) * (projectile.timeLeft / (float)BASETIMELEFT), factor =>
            {
                return Color.Lerp(Color.Red,Color.DarkRed,factor.X) * 0.8f;
            });

            trail.Positions = cache.ToArray();

            float offset2 = (float)Math.Pow(Math.Abs((max + 1) - average) / (float)(max - min), 2);

            Vector2 offsetVector2 = (direction.RotatedBy(1.57f) * offset2 * 10);
            trail.NextPosition = target.Center + (direction * (max + 1));
        }

        public void DrawPrimitives()
        {
            if (effect == null)
                return;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            trail?.Render(effect);
        }
    }

    public class FiletFrenzyBuff : SmartBuff
    {
        public FiletFrenzyBuff() : base("Frenzy", "Increased melee speed, but Filet Knife can no longer crit", false, false) { }

        public override void Update(Player player, ref int buffIndex)
        {
            player.meleeSpeed += 0.3f;
        }
    }
}