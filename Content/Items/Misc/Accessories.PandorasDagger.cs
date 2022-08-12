using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Potions;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using StarlightRiver.Helpers;
using Terraria.Audio;

namespace StarlightRiver.Content.Items.Misc
{
    public class PandorasDagger : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public PandorasDagger() : base("Pandora's Dagger", "Grazes release Discordant Bolts, that inflict stacks of Volatile") { }

        public override void SafeSetDefaults()
        {
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1);
        }

        public override void SafeUpdateEquip(Player Player)
        {
            var gp = Player.GetModPlayer<GrazePlayer>();
            gp.doGrazeLogic = true;

            if (gp.justGrazed && Player.whoAmI == Main.myPlayer)
            {
                for (int i = 0; i < Main.rand.Next(2, 4); i++)
                {
                    Projectile.NewProjectile(Item.GetSource_FromThis(), Player.Center, Vector2.One.RotatedByRandom(6.18f) * Main.rand.NextFloat(5f, 7f), ModContent.ProjectileType<DiscordantBolt>(),
                        (int)Player.GetTotalDamage(DamageClass.Generic).ApplyTo(15), 3.5f, Player.whoAmI);
                }
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
            AddIngredient(ModContent.ItemType<HolyAmulet>()).
            AddIngredient(ModContent.ItemType<BloodAmulet>()).
            AddTile(TileID.Anvils).
            Register();
        }
    }

    class DiscordantBolt : ModProjectile, IDrawPrimitive, IDrawAdditive
    {
        private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Discordant Bolt");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Generic;
            Projectile.friendly = true;

            Projectile.width = Projectile.height = 12;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
        }

        public override void AI()
        {
            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }

            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 10;
                SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            //just basically copied from blood amulet
            NPC target = Main.npc.Where(n => n.CanBeChasedBy(Projectile, false) && Vector2.Distance(n.Center, Projectile.Center) < 950f).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

            if (target != default)
            {
                Vector2 direction = target.Center - Projectile.Center;
                direction.Normalize();
                direction *= 12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.045f);
            }

            if (Main.rand.NextBool(3))
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0f, 0f, 0, new Color(220, 205, 140), 0.35f);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            var globalNPC = target.GetGlobalNPC<VolatileGlobalNPC>();

            globalNPC.VolatileStacks++;
            globalNPC.VolatileTimer = 600;
        }

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0f, 0f, 0, new Color(220, 205, 140), 0.45f);
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.Glow>(), 0f, 0f, 0, new Color(220, 205, 140), 0.4f);
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;
            Color color = new Color(220, 205, 140);
            color.A = 0;
            Main.spriteBatch.Draw(texture, (Projectile.Center - Projectile.velocity) - Main.screenPosition, null, color, 0, texture.Size() / 2f, Projectile.scale - 0.7f, SpriteEffects.None, 0);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 15; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 15)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(2.5f), factor => 4.5f * (factor * 2f), factor =>
            {
                return Color.Lerp(new Color(210, 210, 200), new Color(220, 205, 140), factor.X) * 0.8f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.03f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

            trail?.Render(effect);
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            Texture2D texture = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowSoft").Value;
            Color color = new Color(220, 205, 140);
            sb.Draw(texture, (Projectile.Center - Projectile.velocity) - Main.screenPosition, null, color * 0.6f, Projectile.rotation, texture.Size() / 2f, Projectile.scale - 0.35f, SpriteEffects.None, 0);
            sb.Draw(texture, (Projectile.Center - Projectile.velocity) - Main.screenPosition, null, color, Projectile.rotation, texture.Size() / 2f, Projectile.scale - 0.45f, SpriteEffects.None, 0);
        }
    }

    class VolatileGlobalNPC : GlobalNPC
    { 
        public const int MAXVOLATILESTACKS = 5;

        public int VolatileStacks;

        public int VolatileTimer;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            VolatileStacks = Utils.Clamp(VolatileStacks, 0, MAXVOLATILESTACKS);

            if (VolatileTimer > 0)
                VolatileTimer--;
            else
                VolatileStacks = 0;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {
            if (VolatileStacks > 0)
                damage = (int)(damage * (1f + (0.07f * VolatileStacks)));
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (VolatileStacks > 0)
                damage = (int)(damage * (1f + (0.07f * VolatileStacks)));
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref int damage, ref bool crit)
        {
            if (VolatileStacks > 0)
                damage = (int)(damage * (1f + (0.03f * VolatileStacks)));
        }

        public override void AI(NPC npc)
        {
            if (VolatileStacks > 0)
            {
                if (Main.rand.NextBool(10 - VolatileStacks))
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0f, 0f, 0, new Color(220, 205, 140), 0.35f);
                    if (VolatileStacks == MAXVOLATILESTACKS)
                        Dust.NewDustDirect(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Glow>(), 0f, 0f, 0, new Color(220, 205, 140), 0.45f);
            }
        }
    }
}
