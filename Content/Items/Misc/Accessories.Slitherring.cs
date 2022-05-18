using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Items.Misc
{
    public class Slitherring : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Slitherring() : base("Slitherring", "Whips have a chance to shoot out a smaller, snake whip") { }

        public override void SafeSetDefaults()
        {
            Item.value = 1;
            Item.rare = ItemRarityID.LightRed;
        }

        public override void SafeUpdateEquip(Player Player)
        {
            Player.GetModPlayer<SlitherringPlayer>().equipped = true;
        }
    }

    public class SlitherringPlayer : ModPlayer
    {
        public bool equipped = false;

        public override void ResetEffects()
        {
            equipped = false;
        }
    }

    public class SlitherringGItem : GlobalItem
    {

        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.GetModPlayer<SlitherringPlayer>().equipped && ProjectileID.Sets.IsAWhip[type] && Main.rand.NextBool())
            {
                Projectile proj = Projectile.NewProjectileDirect(source, position, velocity * 0.75f, ModContent.ProjectileType<SlitherringWhip>(), (int)(damage * 0.5f), knockback, player.whoAmI, -1);
                proj.originalDamage = damage / 2;
            }    
            return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
        }
    }
    public class SlitherringWhip : BaseWhip
    {
        public override string Texture => AssetDirectory.MiscItem + "SlitherringWhip";

        public SlitherringWhip() : base("Slither Whip", 15, 0.75f, Color.DarkGreen) { }

        public override int SegmentVariant(int segment)
        {
            int variant;
            switch (segment)
            {
                default:
                    variant = 2;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    variant = 1;
                    break;
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    variant = 3;
                    break;
            }
            return variant;
        }

        public override bool ShouldDrawSegment(int segment) => true;// segment % 2 == 0;

        public override bool PreAI()
        {

            Player player = Main.player[Projectile.owner];
            _flyTime = player.itemAnimationMax * Projectile.MaxUpdates * 2;
            if (Projectile.ai[0] == -1)
                Projectile.ai[0] = _flyTime;
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.ai[0]--;
            Projectile.Center = Main.GetPlayerArmPosition(Projectile) + Projectile.velocity * (Projectile.ai[0] - 1f);
            Projectile.spriteDirection = ((!(Vector2.Dot(Projectile.velocity, Vector2.UnitX) < 0f)) ? 1 : -1);
            if (Projectile.ai[0] <= 0 || player.itemAnimation == 0)
            {
                Projectile.Kill();
                return false;
            }

            if (Projectile.ai[0] == (int)(_flyTime / 2f))
            {
                Projectile.WhipPointsForCollision.Clear();
                SetPoints(Projectile.WhipPointsForCollision);
            }
            if (Utils.GetLerpValue(0.1f, 0.7f, Projectile.ai[0] / _flyTime, true) * Utils.GetLerpValue(0.9f, 0.7f, Projectile.ai[0] / _flyTime, true) > 0.5f)
            {
                Projectile.WhipPointsForCollision.Clear();
                SetPoints(Projectile.WhipPointsForCollision);
            }
            ArcAI();
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color minLight = lightColor;
            Color minColor = new Color(10, 25, 33);
            if (minLight.R < minColor.R)
                minLight.R = minColor.R;
            if (minLight.G < minColor.G)
                minLight.G = minColor.G;
            if (minLight.B < minColor.B)
                minLight.B = minColor.B;
            return minLight;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, Main.rand.Next(60, 240));
        }
    }
}