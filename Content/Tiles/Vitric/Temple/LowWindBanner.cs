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
        public LowWindBannerDummy() : base(TileType<LowWindBanner>(), 16, 200) { }

        private RectangularBanner Chain;
        int blow;

		public override int ParentY => (int)(Projectile.position.Y / 16);

		public override bool ValidTile(Tile tile)
		{
			return base.ValidTile(tile);
		}

		public override void SafeSetDefaults()
        {
            Chain = new RectangularBanner(16, false, Projectile.Center - Vector2.UnitY * 90, 8)
            {
                constraintRepetitions = 2,//defaults to 2, raising this lowers stretching at the cost of performance
                drag = 2f,//This number defaults to 1, Is very sensitive
                forceGravity = new Vector2(0f, 1.25f),//gravity x/y
                scale = 15.0f
            };
        }

        public override void Update()
        {
            Chain.UpdateChain(Projectile.Center - Vector2.UnitY * 90);
            Chain.IterateRope(WindForce);

            Projectile.ai[0] += 0.005f;

            if (blow > 0)
                blow--;
            else if (blow < 0)
                blow++;
        }

		public override void Collision(Player Player)
		{
            if (blow == 0)
                blow = (int)Player.velocity.X;
		}

		private void WindForce(int index)//wind
        {
            Vector2 pos = Chain.ropeSegments[index].posNow;

            if (index > 2)
            {
                int offset = (int)(Projectile.position.X / 16 + Projectile.position.Y / 16);

                float sin = (float)System.Math.Sin(StarlightWorld.rottime + offset);
                float sin2 = (float)System.Math.Sin(Main.GameUpdateCount * 0.016f + offset);

                float power = (float)System.Math.Sin(index / 16f * 3.14f) * 2;

                pos = new Vector2(Chain.ropeSegments[index].posNow.X + (0.5f + sin2 * 0.5f) * (sin * 0.15f) * power, Chain.ropeSegments[index].posNow.Y);
                pos.X += blow * 0.2f;
            }

            Color color = new Color(150, 10, 35).MultiplyRGB(Lighting.GetColor((int)pos.X / 16, (int)pos.Y / 16));

            Chain.ropeSegments[index].posNow = pos;
            Chain.ropeSegments[index].color = color;
        }

        public override void Kill(int timeLeft)
        {
            VerletChain.toDraw.Remove(Chain);
        }
    }
}