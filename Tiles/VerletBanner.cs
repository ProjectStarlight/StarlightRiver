using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Projectiles.Dummies;

namespace StarlightRiver.Tiles
{
	class VerletBanner : DummyTile
	{
        public override int DummyType => ProjectileType<VerletBannerDummy>();

		public override void SetDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;
            Main.tileFrameImportant[Type] = true;
            //drop = ModContent.ItemType< >();
        }

        //These were for if the end of the banner could be pinned to a location
        //public override bool SpawnConditions(int i, int j)
        //{
        //    return true;
        //}
        //public override void PlaceInWorld(int i, int j, Item item)
        //{
        //    Main.tile[i, j].frameX = short.MaxValue / 2;
        //    Main.tile[i, j].frameY = short.MaxValue / 2;
        //}
    }



    internal class VerletBannerDummy : Dummy
    {
        public VerletBannerDummy() : base(TileType<VerletBanner>(), 16, 16) { }

        private VerletChainInstance Chain;

        public override void SafeSetDefaults()
        {
            Chain = new VerletChainInstance
            {
                segmentCount = 16,
                segmentDistance = 16,//if your using a texture to connect all the points, keep this near the texture size
                constraintRepetitions = 2,//defaults to 2, raising this lowers stretching at the cost of performance
                drag = 2f,//This number defaults to 1, Is very sensitive
                forceGravity = new Vector2(0f, 0.25f),//gravity x/y
                gravityStrengthMult = 1f
            };
        }


        public override void Update()
        {
            Chain.UpdateChain(projectile.Center);

            if (Chain.init) Chain.IterateRope(WindForce);
            projectile.ai[0] += 0.005f;
        }

        private void WindForce(int index)//wind
        {
            float sin = (float)System.Math.Sin(StarlightWorld.rottime - index / 3f);

            float cos = (float)System.Math.Cos(projectile.ai[0]);
            float sin2 = (float)System.Math.Sin(StarlightWorld.rottime + cos);

            Vector2 pos = new Vector2(Chain.ropeSegments[index].posNow.X + 1 + sin2 * 1.2f, Chain.ropeSegments[index].posNow.Y + sin * 1.4f);

            Color color = new Color(150, 10, 35).MultiplyRGB(Color.White * (1 - sin * 0.5f));

            Chain.ropeSegments[index].posNow = pos;
            Chain.ropeSegments[index].color = color;
        }


        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Chain.init) Chain.DrawStrip();
        }

        //private void ChainDrawMethod(SpriteBatch spriteBatch, int index, Vector2 position, Vector2 prevPosition, Vector2 nextPosition)
        //{
        //    Texture2D tex = GetTexture("StarlightRiver/Tiles/Decoration/VerletBannerTex");
        //    Texture2D tex2 = GetTexture("StarlightRiver/Tiles/VerletBanner");

        //    //dots between each segment
        //    spriteBatch.Draw(tex2,
        //        position - Main.screenPosition,
        //        new Rectangle(0, 0, tex2.Width, tex2.Height),
        //        Color.White,
        //        0f,
        //        new Vector2(tex2.Width / 2, tex2.Height / 2),
        //        0.50f, default, default);
        //}
    }
}