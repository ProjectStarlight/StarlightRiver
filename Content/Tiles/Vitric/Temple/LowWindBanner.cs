using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Core.VerletGenerators;
using StarlightRiver.Physics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class LowWindBanner : DummyTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.Invisible;
            return base.Autoload(ref name, ref texture);
        }

        public override int DummyType => ProjectileType<LowWindBannerDummy>();

        public override void SetDefaults()
        {
            this.QuickSetFurniture(1, 1, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(180, 100, 100));
        }
    }

    class LowWindBannerItem : QuickTileItem
    {
        public LowWindBannerItem() : base("Rectangular Flowing Banner", "", TileType<LowWindBanner>(), 1, AssetDirectory.VitricTile, false) { }
    }

    internal class LowWindBannerDummy : Dummy
    {
        public LowWindBannerDummy() : base(TileType<LowWindBanner>(), 32, 32) { }

        private RectangularBanner Chain;

        public override void SafeSetDefaults()
        {
            Chain = new RectangularBanner(16, false, projectile.Center, 16)
            {
                constraintRepetitions = 2,//defaults to 2, raising this lowers stretching at the cost of performance
                drag = 2f,//This number defaults to 1, Is very sensitive
                forceGravity = new Vector2(0f, 0.25f),//gravity x/y
                scale = 0.6f
            };
        }

        public override void Update()
        {
            Chain.UpdateChain(projectile.Center);
            Chain.IterateRope(WindForce);

            projectile.ai[0] += 0.005f;
        }

        private void WindForce(int index)//wind
        {
            int offset = (int)(projectile.position.X / 16 + projectile.position.Y / 16);

            float sin = (float)System.Math.Sin(StarlightWorld.rottime + offset - index / 3f);

            float cos = (float)System.Math.Cos(projectile.ai[0]);
            float sin2 = (float)System.Math.Sin(StarlightWorld.rottime + offset + cos);

            Vector2 pos = new Vector2(Chain.ropeSegments[index].posNow.X + 1 + sin2 * 0.2f, Chain.ropeSegments[index].posNow.Y + sin * 0.4f);

            Color color = new Color(150, 10, 35).MultiplyRGB(Color.White * (1 - sin * 0.2f)).MultiplyRGB(Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16));

            Chain.ropeSegments[index].posNow = pos;
            Chain.ropeSegments[index].color = color;
        }

        public override void Kill(int timeLeft)
        {
            VerletChain.toDraw.Remove(Chain);
        }
    }
}