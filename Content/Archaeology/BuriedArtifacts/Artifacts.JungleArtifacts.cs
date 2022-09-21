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
    public class WindTotemArtifact : JungleArtifact
    {
        public override Vector2 Size => new Vector2(34, 32);

        public override float SpawnChance => 0.5f;

        public override int ItemType => ModContent.ItemType<WindTotemArtifactItem>();

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.LimeArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.LimeGreen;
    }

    public class RainTotemArtifact : JungleArtifact
    {
        public override Vector2 Size => new Vector2(28, 32);

        public override float SpawnChance => 0.5f;

        public override int ItemType => ModContent.ItemType<RainTotemArtifactItem>();

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.LimeArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.LimeGreen;
    }
}