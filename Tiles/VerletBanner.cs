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
                constraintRepetitions = 1,//defaults to 2, raising this lowers stretching at the cost of performance
                drag = 1.01f,//This number defaults to 1, Is very sensitive
                forceGravity = new Vector2(0f, 0.25f),//gravity x/y
                gravityStrengthMult = 1f
            };
        }

        public override void Update()
        {
            Chain.UpdateChain(projectile.Center);

            if (Chain.init)
            {
                Chain.IterateRope(TestMethod);//This is basically a for loop

                //this was for if the end of the banner could be pinned to a location
                //    Chain.ropeSegments[Chain.segmentCount - 1] = new VerletChainInstance.RopeSegment(new Vector2(projectile.position.X + 250, projectile.position.Y + 50));
                //    
                //if (Main.rand.Next(25) == 0)//random movements for testing
                //{
                //    int index = Main.rand.Next(Chain.segmentCount);
                //    Vector2 pos = new Vector2(Chain.ropeSegments[index].posNow.X + 20, Chain.ropeSegments[index].posNow.Y);
                //    Chain.ropeSegments[index] = new VerletChainInstance.RopeSegment(pos);
                //}
                //    //}
            }
        }
        private void TestMethod(int index)//wind
        {
            Vector2 pos = new Vector2(Chain.ropeSegments[index].posNow.X + 1, Chain.ropeSegments[index].posNow.Y);
            Chain.ropeSegments[index] = new VerletChainInstance.RopeSegment(pos);
        }


        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (Chain.init)
            {
                Chain.DrawRope(spriteBatch, ChainDrawMethod);
            }
        }
        private void ChainDrawMethod(SpriteBatch spriteBatch, int index, Vector2 position, Vector2 prevPosition, Vector2 nextPosition)
        {
            Texture2D tex = GetTexture("StarlightRiver/Tiles/Decoration/VerletBannerTex");
            Texture2D tex2 = GetTexture("StarlightRiver/Tiles/VerletBanner");

            //dots between each segment
            spriteBatch.Draw(tex2,
                position - Main.screenPosition,
                new Rectangle(0, 0, tex2.Width, tex2.Height),
                Color.White,
                0f,
                new Vector2(tex2.Width / 2, tex2.Height / 2),
                0.50f, default, default);

            if (nextPosition != Vector2.Zero)
            {
                //old version used a int instead of a rect for the last input
                //Helper.DrawLine(spriteBatch, position - Main.screenPosition, nextPosition - Main.screenPosition, Main.blackTileTexture, Color.White, (int)((-((float)i / Chain.segmentCount) + 1) * 20));
                
                //see the boomerang, It has a switch statment that shows having a texture for the ends
                int frameWidth = tex.Width / 3;
                Helper.DrawLine(spriteBatch, position - Main.screenPosition, nextPosition - Main.screenPosition, tex, Color.White, new Rectangle(frameWidth * (index % 3), 0, frameWidth, tex.Height));
            }
        }
    }
}