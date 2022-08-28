using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using ReLogic.Content;

namespace StarlightRiver.Content.Items.Misc
{
    public class Slitherring : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public Slitherring() : base("Slitherring", "Whips have a chance to shoot out a smaller, snake whip") { }

        public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0,0,40,0);
            Item.rare = ItemRarityID.Orange;
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

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            List<Vector2> points = Projectile.WhipPointsForCollision;
            for (int i = 0; i < points.Count - 1; i++)
            {
                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), points[i], points[i + 1], 8, ref collisionPoint))
                    return true;
            }
            return false;
        }

        public override bool PreAI()
        {
            ProjectileID.Sets.IsAWhip[Projectile.type] = false;
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
            Projectile.WhipPointsForCollision.Clear();
            SetPoints(Projectile.WhipPointsForCollision);
            ArcAI();
            return false;
        }

        public override void PostAI()
        {

            base.PostAI();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            DrawBehindWhip(ref lightColor);
            List<Vector2> points = new List<Vector2>();
            points.Clear();
            SetPoints(points);

            //points = Projectile.WhipPointsForCollision;

            //string
            Vector2 stringPoint = points[0];
            for (int i = 0; i < points.Count - 2; i++)
            {
                Vector2 nextPoint = points[i + 1] - points[i];
                Color color = _stringColor.MultiplyRGBA(Projectile.GetAlpha(Lighting.GetColor(points[i].ToTileCoordinates())));
                Vector2 scale = new Vector2(1f, (nextPoint.Length() + 2f) / (float)TextureAssets.FishingLine.Height());
                Main.EntitySpriteDraw(TextureAssets.FishingLine.Value, stringPoint - Main.screenPosition, null, color, nextPoint.ToRotation() - MathHelper.PiOver2, new Vector2(TextureAssets.FishingLine.Width() * 0.5f, 2f), scale, SpriteEffects.None, 0);
                stringPoint += nextPoint;
            }

            //whip
            Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
            Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
            int height = whipFrame.Height;
            Vector2 firstPoint = points[0];
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 origin = whipFrame.Size() * 0.5f;
                bool draw = true;
                if (i == 0)
                    origin.Y += _handleOffset;
                else if (i == points.Count - 2)
                    whipFrame.Y = height * 4;
                else
                {
                    whipFrame.Y = height * SegmentVariant(i);
                    draw = ShouldDrawSegment(i);
                }

                Vector2 difference = points[i + 1] - points[i];
                if (draw)
                {
                    Color alpha = Projectile.GetAlpha(Lighting.GetColor(points[i].ToTileCoordinates()));
                    float rotation = difference.ToRotation() - MathHelper.PiOver2;
                    Main.EntitySpriteDraw(texture.Value, points[i] - Main.screenPosition, whipFrame, alpha, rotation, origin, Projectile.scale, SpriteEffects.None, 0);
                }
                firstPoint += difference;
            }

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color minLight = lightColor;
            Color minColor = new Color(10, 25, 33);
            if (minLight.R < minColor.R) minLight.R = minColor.R;
            if (minLight.G < minColor.G) minLight.G = minColor.G;
            if (minLight.B < minColor.B) minLight.B = minColor.B;
            return minLight;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.Poisoned, Main.rand.Next(60, 240));
        }
    }
}