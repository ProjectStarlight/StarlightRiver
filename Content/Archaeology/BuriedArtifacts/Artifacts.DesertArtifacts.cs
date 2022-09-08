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
    public abstract class DesertArtifact : Artifact
    {
        public override bool CanBeRevealed => true;

        public override string TexturePath => AssetDirectory.Archaeology + Name;

        public override string MapTexturePath => AssetDirectory.Archaeology + "DigMarker";

        public override int SparkleDust => ModContent.DustType<Dusts.ArtifactSparkles.GoldArtifactSparkle>();

        public override int SparkleRate => 40;

        public override Color BeamColor => Color.Gold;

        public override void Draw(SpriteBatch spriteBatch)
        {
            GenericDraw(spriteBatch);
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            return true;
        }
    }

    public class DesertArtifact1 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(36, 62);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact1Item>();
    }

    public class DesertArtifact2 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(30, 30);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact2Item>();
    }

    public class DesertArtifact3 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(30, 28);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact3Item>();
    }

    public class DesertArtifact4 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(24, 18);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact4Item>();
    }

    public class DesertArtifact5 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(28, 20);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact5Item>();
    }

    public class DesertArtifact6 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(38, 26);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact6Item>();
    }

    public class DesertArtifact7 : DesertArtifact
    {
        public override Vector2 Size => new Vector2(30, 28);

        public override float SpawnChance => 1f;

        public override int ItemType => ModContent.ItemType<DesertArtifact7Item>();
    }
}