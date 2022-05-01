using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class TinCoil : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + "TinCoil";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tin Coil");
            Tooltip.SetDefault("Strikes nearby enemies with static electricity");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.SummonerWeaponThatScalesWithAttackSpeed[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<TinCoilWhip>(), 5, 1.2f, 5f, 25);
            Item.SetShopValues(ItemRarityID.White, Item.sellPrice(0, 0, 50));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.TinBar, 7)
                .AddIngredient(ItemID.Cobweb, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class TinCoilWhip : BaseWhip
    {
        public override string Texture => AssetDirectory.MiscItem + "TinCoilWhip";

        public TinCoilWhip() : base("Tin Coil", 15, 0.57f, new Color(153, 122, 97)) { }

        public override int SegmentVariant(int segment)
        {
            int variant;
            switch (segment)
            {
                default:
                    variant = 1;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    variant = 2;
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

        public override void ArcAI()
        {
            if (Projectile.ai[0] > MiddleOfArc - 5 && Projectile.ai[0] < MiddleOfArc + 5)
            {
                float scale = Utils.GetLerpValue(MiddleOfArc - 5, MiddleOfArc, Projectile.ai[0], true) * Utils.GetLerpValue(MiddleOfArc + 5, MiddleOfArc, Projectile.ai[0], true);
                Dust d = Dust.NewDustPerfect(EndPoint, DustID.Electric, Vector2.Zero, 0, Color.GhostWhite, scale * 0.5f);
                d.noGravity = true;
                d.position += Main.rand.NextVector2Circular(1, 8).RotatedBy(Projectile.rotation);
                d.velocity += new Vector2(0f, -Main.rand.Next(1, 3)).RotatedBy(Projectile.rotation).RotatedByRandom(0.5f);
            }
            if (Projectile.ai[0] >= MiddleOfArc - 1 && Projectile.ai[0] < MiddleOfArc)
            {
                int id = Projectile.FindTargetWithLineOfSight();
                if (id >= 0)
                {
                    NPC target = Main.npc[id];
                    if (EndPoint.Distance(target.Center) < 300)
                    {
                        Vector2 vel = Main.player[Projectile.owner].DirectionTo(EndPoint) * 5f;
                        Projectile bolt = Projectile.NewProjectileDirect(Projectile.GetItemSource_FromThis(), EndPoint, vel, ModContent.ProjectileType<CoilStaticShock>(), (int)(Projectile.damage * 1.2f), 0.5f, Main.player[Projectile.owner].whoAmI);
                        bolt.ai[0] = id;
                        bolt.localAI[0] = vel.ToRotation();
                    }
                }
            }
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
    }
}
