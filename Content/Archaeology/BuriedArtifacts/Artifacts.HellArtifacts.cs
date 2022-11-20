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
using StarlightRiver.Content.Items.Hell;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Archaeology.BuriedArtifacts
{
    public class CharonsObolArtifact : HellArtifact
    {
        public override string TexturePath => AssetDirectory.HellItem + "CharonsObol";

        public override Vector2 Size => new Vector2(32, 28);

        public override float SpawnChance => 1f;

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.Gold;

        public override int ItemType => ModContent.ItemType<CharonsObol>();
    }
}