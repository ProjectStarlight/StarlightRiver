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
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;

namespace StarlightRiver.Content.Items.Misc
{
    internal class FiletKnife : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Filet Knife");
            Tooltip.SetDefault("Critical strikes carve chunks of flesh from enemies\n" +
            "Devour chunks to heal and gain Blood Frenzy\n" +
            "Blood Frenzy grants increased attack speed and decreased critical strike chance");
        }
        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.DamageType = DamageClass.Melee;
            Item.width = 36;
            Item.height = 38;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7.5f;
            Item.value = Item.sellPrice(0,1,0,0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.crit = 10;
        }
        public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
        {
            if (player.HasBuff(ModContent.BuffType<FiletFrenzyBuff>()) && Main.rand.NextBool())
                crit = false;
        }
        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (crit)
            {
                Helper.PlayPitched("Impacts/StabTiny", 0.8f, Main.rand.NextFloat(-0.3f, 0.3f), target.Center);

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
                Item.NewItem(player.GetSource_OnHit(target), target.Center, itemType);

                if (target.GetGlobalNPC<FiletNPC>().DOT < 3) //TODO: Port to a proper stacking buff system later
                    target.GetGlobalNPC<FiletNPC>().DOT += 1;

                Projectile.NewProjectile(player.GetSource_ItemUse(Item), target.Center, Vector2.Zero, ModContent.ProjectileType<FiletSlash>(), 0, 0, player.whoAmI, target.whoAmI);

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
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
        }

        public override bool ItemSpace(Player Player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 10);
            player.HealEffect(10);
            player.statLife += healAmount;

            player.AddBuff(BuffID.WellFed, 18000);
            player.AddBuff(ModContent.BuffType<FiletFrenzyBuff>(), 600);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab, player.position);
            return false;
        }
    }

    public class FiletGiblet2 : FiletGiblet1 { }
    public class FiletGiblet3 : FiletGiblet1 { }

    public class FiletNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int DOT = 0;

        public int DOTreductionCounter = 0;

        public bool hasSword = false;

        public override void ResetEffects(NPC npc)
        {
            if (DOT > 0)
            {
                DOTreductionCounter++;
                if (DOTreductionCounter > 330)
                {
                    DOT--;
                    DOTreductionCounter = 0;
                }
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.type == NPCID.BloodZombie && Main.rand.NextBool(100))
                hasSword = true;
            base.OnSpawn(npc, source);
        }

        public override bool PreDraw(NPC NPC, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {     
            if (hasSword)
            {
                Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "FiletKnifeEmbedded").Value;
                bool facingLeft = NPC.direction == -1;

                Vector2 origin = facingLeft ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);
                SpriteEffects effects = facingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float rotation = facingLeft ? 0.78f : -0.78f;


                spriteBatch.Draw(tex, NPC.Center - screenPos, null, drawColor, rotation, origin, NPC.scale, effects, 0f);
            }
            return base.PreDraw(NPC, spriteBatch, screenPos, drawColor);
        }

		public override void OnKill(NPC npc)
        {
            if (hasSword)
                Item.NewItem(npc.GetSource_Loot(), npc.Center, ModContent.ItemType<FiletKnife>());

            base.OnKill(npc);
        }

        public override void UpdateLifeRegen(NPC NPC, ref int damage)
        {
            if (DOT == 0)
                return;
            if (NPC.lifeRegen > 0)
            {
                NPC.lifeRegen = 0;
            }
            NPC.lifeRegen -= DOT * 3;
            if (damage < DOT)
            {
                damage = DOT;
            }
        }

        public override void DrawEffects(NPC NPC, ref Color drawColor)
        {
            if (DOT != 0)
            {
                if (Main.rand.Next(5) < DOT)
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f, default, default(Color), 1.25f);
            }

            if (hasSword)
                Dust.NewDustPerfect(NPC.Center - new Vector2(NPC.spriteDirection * 12, 0), DustID.Blood, Vector2.Zero);
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
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = BASETIMELEFT - 2;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (effect == null)
            {
                effect = new BasicEffect(Main.instance.GraphicsDevice);
                effect.VertexColorEnabled = true;
            }

            NPC target = Main.npc[(int)Projectile.ai[0]];
            Projectile.Center = target.Center;

            if (direction == Vector2.Zero)
                direction = Main.rand.NextFloat(6.28f).ToRotationVector2() * (target.width + target.height) * 0.06f;
            cache = new List<Vector2>();

            float progress = (BASETIMELEFT - Projectile.timeLeft) / (float)BASETIMELEFT;

            int widthExtra = (int)(6 * Math.Sin(progress * 3.14f));

            int min = (BASETIMELEFT - (20 + widthExtra)) - Projectile.timeLeft;
            int max = (BASETIMELEFT + (widthExtra)) - Projectile.timeLeft;

            int average = (min + max) / 2;
            for (int i = min; i < max; i++)
            {
                float offset = (float)Math.Pow(Math.Abs(i - average) / (float)(max - min), 2);
                Vector2 offsetVector = (direction.RotatedBy(1.57f) * offset * 10);

                cache.Add(target.Center + (direction * i));
            }

            trail = new Trail(Main.instance.GraphicsDevice, 20 + (widthExtra * 2), new TriangularTip((int)((target.width + target.height) * 0.6f)), factor => 10 * (1 - Math.Abs((1 - factor) - (Projectile.timeLeft /(float)(BASETIMELEFT + 5)))) * (Projectile.timeLeft / (float)BASETIMELEFT), factor =>
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
        public override string Texture => AssetDirectory.Debug;

		public FiletFrenzyBuff() : base("Blood Frenzy", "Increased melee speed, but decreased crit rate on Filet Knife", false, false) { }

        public override void Update(Player Player, ref int buffIndex)
        {
            Player.GetAttackSpeed(DamageClass.Melee) += 0.3f;
        }
    }
}