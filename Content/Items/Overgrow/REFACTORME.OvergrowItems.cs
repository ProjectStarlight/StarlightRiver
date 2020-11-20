using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Overgrow
{
    public class FaeShard : QuickMaterial { public FaeShard() : base("Fae Shard", "Tooltip Placeholder", 999, 100, 1) { } }

    public class MossyBone : QuickMaterial { public MossyBone() : base("Mossy Bone", "Tooltip Placeholder", 999, 100, 1) { } }

    public class Volatiles : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Diffuse volatiles");
            Tooltip.SetDefault("Tooltip Placeholder");
            ItemID.Sets.ItemNoGravity[item.type] = true;
        }

        public override void SetDefaults()
        {
            item.width = 16;
            item.height = 16;
            item.maxStack = 999;
            item.value = 100;
            item.rare = ItemRarityID.Blue;
        }

        public override void PostUpdate()
        {
            Lighting.AddLight(item.Center, .45f, .45f, .25f);
            item.position.Y += (float)Math.Sin(StarlightWorld.rottime) / 3;
        }
    }
}