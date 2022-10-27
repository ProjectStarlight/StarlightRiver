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
    public class LavaCharmArtifact : LavaArtifact
    {
        public override string TexturePath => "Terraria/Images/Item_906";

        public override Vector2 Size => new Vector2(30, 32);

        public override float SpawnChance => 1f;

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.RedArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.Red;

        public override int ItemType => ItemID.LavaCharm;
    }
}