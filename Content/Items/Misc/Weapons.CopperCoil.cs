using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class CopperCoil : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + "CopperCoil";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Copper Coil");
            Tooltip.SetDefault("Strikes nearby enemies with static electricity");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ItemID.Sets.SummonerWeaponThatScalesWithAttackSpeed[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToWhip(ModContent.ProjectileType<CopperCoilWhip>(), 5, 1.2f, 4f, 30);
            Item.SetShopValues(ItemRarityID.White, Item.sellPrice(0, 0, 50));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CopperBar, 7)
                .AddIngredient(ItemID.Cobweb, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class CopperCoilWhip : BaseWhip
    {
        public override string Texture => AssetDirectory.MiscItem + "CopperCoilWhip";

        public CopperCoilWhip() : base("Copper Coil", 15, 0.9f, new Color(121, 90, 73)) { }

        public override int SegmentVariant(ref int segment)
        {
            int variant = 1;
            //vanilla does this!
            switch (segment)
            {
                default:
                    variant = 1;
                    break;
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    variant = 2;
                    break;
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                    variant = 3;
                    break;
            }
            return variant;
        }

        public override bool ShouldDrawSegment(ref int segment) => segment % 2 == 0;

        public override void ArcAI()
        {
            float ai = Projectile.ai[0] * 0.5f; //Extra Update!
            if (ai > 19 && ai < 21)
            {
                Dust d = Dust.NewDustPerfect(EndPoint, DustID.Electric, Vector2.Zero, 0, Color.LightCyan, 0.6f);
                d.noGravity = true;
                d.velocity += Main.rand.NextVector2Circular(2, 1).RotatedBy(Projectile.rotation);
            }
        }
    }
}
