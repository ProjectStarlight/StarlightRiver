using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Archaeology;
using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
    public class ExoticGeodeArtifact : UndergroundArtifact
    { 
        public override Vector2 Size => new Vector2(32, 32);

        public override float SpawnChance => 1f;

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GeodeArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.Purple;

        public override int ItemType => ModContent.ItemType<ExoticGeodeArtifactItem>();
    }

    public class PerfectlyGenericArtifact : UndergroundArtifact
    {
        public override Vector2 Size => new Vector2(32, 32);

        public override float SpawnChance => 0.01f;

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.WhiteArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.White;

        public override int ItemType => ModContent.ItemType<PerfectlyGenericArtifactItem>();
    }
}