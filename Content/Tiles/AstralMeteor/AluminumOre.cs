using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.AstralMeteor
{
	public class AluminumOre : ModTile
    {
        public override string Texture => AssetDirectory.AluminumTile + Name;

        public override void SetDefaults() =>
            this.QuickSet(0, DustType<Dusts.Electric>(), SoundID.Tink, new Color(156, 172, 177), ItemType<AluminumOreItem>(), true, true, "Aluminum Ore");

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Lighting.AddLight(new Vector2(i + 0.5f, j + 0.5f) * 16, new Vector3(0.1f, 0.32f, 0.5f) * 0.35f);

            if (Main.rand.Next(40) == 0)
                Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, DustType<Dusts.Starlight>(), new Vector2(0, -Main.rand.NextFloat(4, 6)), 0, default, Main.rand.NextFloat(0.4f, 0.7f));

            if (Main.rand.Next(300) == 0)
            {
                Vector2 pos = new Vector2(i + 0.5f, j + 0.5f) * 16;
                DrawHelper.DrawElectricity(pos, pos + Vector2.One.RotatedByRandom(6.28f) * Main.rand.Next(15, 35), DustType<Dusts.Electric2>(), 0.4f, 9);
            }
        }

        public override void FloorVisuals(Player Player) => 
            Player.AddBuff(BuffType<Buffs.Overcharge>(), 120);
    }

    public class AluminumOreItem : QuickTileItem
    {
        public AluminumOreItem() : base("Astral Aluminum", "", "AluminumOre", ItemRarityID.Blue, AssetDirectory.AluminumTile) { }

        public override void SafeSetDefaults() => Item.value = Item.sellPrice(0, 0, 2, 0);
    }
}