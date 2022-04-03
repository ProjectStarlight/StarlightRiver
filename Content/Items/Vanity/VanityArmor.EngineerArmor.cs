using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Noise;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Terraria.WorldGen;

namespace StarlightRiver.Content.Items.Vanity
{
	[AutoloadEquip(EquipType.Head)]
    public class EngineerHead : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Engineer Helmet");
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.value = 0;
            Item.vanity = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.UglySweater, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class EngineerChest : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Engineer Chestplate");
        }

        public override void Load()
        {
            StarlightPlayer.PostUpdateEquipsEvent += PostMovementUpdate;
            return true;
        }

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 20;
            Item.value = 0;
            Item.vanity = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<EngineerHead>() && legs.type == ItemType<EngineerLegs>();
        }

        public override void UpdateArmorSet(Player Player)
        {
            Player.setBonus = "Hold space to hover over tiles";
            //starlightPlayer.ivyArmorComplete = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.UglySweater, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }

        private void PostMovementUpdate(StarlightPlayer slp)
        {
            EngineerArmorPlayer starlightPlayer = slp.Player.GetModPlayer<EngineerArmorPlayer>();
            starlightPlayer.HandleEngineerArmor();
        }
    }

    [AutoloadEquip(EquipType.Legs)]
    public class EngineerLegs : ModItem
    {
        public override string Texture => AssetDirectory.VanityItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Engineer Leggings");
            Tooltip.SetDefault("Slightly increased movement speed");
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 20;
            Item.value = 0;
            Item.vanity = true;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.UglySweater, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class EngineerArmorPlayer : ModPlayer
    {
        readonly FastNoise Noise = new FastNoise(0);
        short MaxTransform => 6;
        float EaseXVel = 0f;
        float EaseYVel = 0f;
        bool TransformActive => SLP.EngineerTransform >= MaxTransform;
        StarlightPlayer SLP => Player.GetModPlayer<StarlightPlayer>();

        public override void Initialize()
        {
            Noise.Seed = _genRandSeed;
            Noise.FractalGain = 0.06f;
            Noise.Frequency = 0.016f;
            Noise.FractalLacunarity = 3.0f;
            Noise.FractalOctaves = 2;
            Noise.NoiseType = FastNoise.NoiseTypes.PerlinFractal;
        }

        private bool EngieArmor()
        {
            return Player.armor[0].type == ItemType<EngineerHead>() && Player.armor[1].type == ItemType<EngineerChest>() && Player.armor[2].type == ItemType<EngineerLegs>();//Really is there a better way?
        }

        private void RaycastTile(int z, int zz, ref int highest, ref int middleheight, ref int middletouch, ref int average, Point16 Playerpos, ref Vector2 touchpoint)
        {
            for (int i = 0; i < 16; i += 1)
            {
                int offset = zz * z;
                Tile tile = Framing.GetTileSafely(Playerpos.X + offset, Playerpos.Y + i);
                if (InWorld(Playerpos.X + offset, Playerpos.Y + i))
                    if (tile.HasTile && Main.tileSolid[tile.type] || tile .LiquidAmount >= 32)
                    {
                        if (touchpoint == default)
                        {
                            touchpoint = new Vector2(Playerpos.X, Playerpos.Y + i) * 16;
                            middleheight = i;
                            highest = (int)touchpoint.Y;
                            middletouch = (int)touchpoint.Y;
                        }
                        else
                        {
                            float valuetoadd = (Playerpos.Y + i) * 16;
                            if (valuetoadd < highest)
                                highest = (int)valuetoadd;
                            touchpoint.Y += valuetoadd;
                        }

                        //Dust.NewDustPerfect((new Vector2((Playerpos.X + offset), Playerpos.Y + i) * 16)+new Vector2(Main.rand.Next(0, 16),0), ModContent.DustType<FireDust2>(), Vector2.Zero, 120, Color.Red, 4f);
                        average += 1;
                        break;
                    }
            }
        }

        public void HandleEngineerArmor()
        {
            EaseXVel += (Player.velocity.X - EaseXVel) / 15f;
            EaseYVel += (Player.velocity.Y - EaseYVel) / 12f;
            if (EngieArmor())
                //Do engie things here

                if (Player.controlJump)
                {
                    SLP.EngineerTransform = (short)Math.Min(SLP.EngineerTransform + 1, MaxTransform);
                    if (TransformActive)
                    {
                        Point16 Playerpos = new Point16((int)Player.Center.X / 16, (int)Player.Center.Y / 16);
                        Vector2 touchpoint = default;
                        int middleheight = 0;
                        int average = 0;
                        int middletouch = 0;
                        int highest = 0;
                        for (int z = 0; z <= 2; z += 1)
                            for (int zz = -1; zz <= 1; zz += 2)
                                RaycastTile(z, zz, ref highest, ref middleheight, ref middletouch, ref average, Playerpos, ref touchpoint);

                        if (touchpoint != default)
                        {
                            touchpoint.Y = (touchpoint.Y / average + highest) / 2f;
                            //Dust.NewDustPerfect(touchpoint + new Vector2(Main.rand.Next(0, 16), 0), ModContent.DustType<BioLumen>(), Vector2.Zero, 120, Color.Red, 2f);
                            if (middleheight < 8 && Main.rand.Next(2, 8) > middleheight)
                            {
                                float scale = 8f - middleheight;
                                Dust dust = Dust.NewDustPerfect(new Vector2(touchpoint.X + Main.rand.Next(0, 16), middletouch), DustType<StarlightSmoke>(), new Vector2(Main.rand.NextFloat(-8, 8) * scale - Player.velocity.X, Main.rand.NextFloat(-1, 1)), 120, Color.Gray, scale / 2f);
                                dust.color = new Color(196, 179, 143);
                            }
                            if (middleheight < 7)
                            {
                                float velocityammount = 15f / (touchpoint.Y - Player.Center.Y);
                                Player.velocity.Y -= velocityammount + 0.2f;
                            }

                            if (Player.velocity.Y > 0)
                                Player.velocity.Y /= 1.05f;

                        }

                        Player.fallStart = (int)(Player.position.Y / 16f);
                        Player.maxRunSpeed += 5; //Only a bit faster run speed
                        Player.runAcceleration /= 3f;
                    }
                    return;
                }

            SLP.EngineerTransform = (short)Math.Max(SLP.EngineerTransform - 1, 0);

        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {

            if (EngieArmor())
            {

                int layerlocation = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("Wings"));
                int layerlocationfront = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("Arms")) + 1;

                //Delete Wings
                layers.RemoveAt(layerlocation);


                //Ugly Layering Ordering Code here!

                //Supports
                Action<PlayerDrawInfo> backTarget = s => DrawEngineerArm(s, 12, new Vector2(0, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                PlayerLayer backLayer = new PlayerLayer("EngineerLayer", "Engineer Arm", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocation, backLayer); //Insert the layer at the appropriate index. 

                Action<PlayerDrawInfo> frontTarget = s => DrawEngineerArm(s, 6, new Vector2(-22, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                PlayerLayer frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocation, frontLayer); //Insert the layer at the appropriate index. 

                frontTarget = s => DrawEngineerArm(s, 4, new Vector2(-22, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocationfront, frontLayer); //Insert the layer at the appropriate index. 

                //GL/Jetpack
                backTarget = s => DrawEngineerArm(s, 3, new Vector2(0, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                backLayer = new PlayerLayer("EngineerLayer", "Engineer Arm GL", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocation + 2, backLayer); //Insert the layer at the appropriate index. 

                frontTarget = s => DrawEngineerArm(s, 3, new Vector2(-22, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm GL", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocationfront + 2, frontLayer); //Insert the layer at the appropriate index. 


                void DrawEngineerArm(PlayerDrawInfo info, int part, Vector2 bodyoffset)
                {

                    SpriteEffects direction = Player.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    Vector2 facingdirection = new Vector2(Player.direction, 1f);

                    //Ugly guessing-game-coded bobber effect
                    if ((Player.bodyFrame.Y + Player.bodyFrame.Height * 2) % (Player.bodyFrame.Height * 6) > Player.bodyFrame.Height * 3 && Player.bodyFrame.Y > Player.bodyFrame.Height * 6)
                        bodyoffset -= new Vector2(0, 2);

                    if (Player.direction < 0)
                    {
                        bodyoffset *= facingdirection;
                        bodyoffset.X -= Player.width;
                    }

                    //Alotta predefined stuff for each part
                    Texture2D[] ShoulderMounts = { Request<Texture2D>(AssetDirectory.VanityItem + "ShoulderMount1").Value, Request<Texture2D>(AssetDirectory.VanityItem + "ShoulderMount2").Value, Request<Texture2D>(AssetDirectory.VanityItem + "ShoulderLauncher1").Value, Request<Texture2D>(AssetDirectory.VanityItem + "ShoulderLauncher2").Value };
                    Vector2[] spriteorigins = { new Vector2(ShoulderMounts[0].Width - 4, ShoulderMounts[0].Height - 4),
                        new Vector2(2, ShoulderMounts[1].Height - 2),
                        new Vector2(4, ShoulderMounts[2].Height / 2),
                    new Vector2(ShoulderMounts[3].Width/4f/2, 2) };

                    Vector2[] partoffsets = { Vector2.Zero,
                        new Vector2(-(ShoulderMounts[0].Width - 6), -(ShoulderMounts[0].Height - 8)),
                        new Vector2(ShoulderMounts[1].Width - 8,-(ShoulderMounts[1].Height - 4)),
                        new Vector2(4, ShoulderMounts[2].Height/2),
                    new Vector2(ShoulderMounts[3].Width/4 - 8,-(ShoulderMounts[3].Height - 4)) };

                    //Redefined angles and some gentle idle animations
                    float[] rotationangles = { (float)Math.Sin(Main.GlobalTime*0.75f)*0.04f,
                        (float)Math.Sin(Main.GlobalTime *1f) * 0.06f,
                        (float)Math.Sin(Main.GlobalTime * 1.33f) * 0.05f,
                        (float)Noise.GetNoise(SLP.Timer * (bodyoffset.X<-11 ? 1f : -1f),SLP.Timer)/5f};

                    //Sway backwards as the Player moves
                    rotationangles[0] -= (float)Math.Pow(Math.Abs(EaseXVel / 20), 0.60);
                    rotationangles[1] -= (float)Math.Pow(Math.Abs(EaseXVel / 16), 0.70);

                    //Vertical movement
                    rotationangles[0] += (float)Math.Pow(Math.Abs(EaseYVel / 40), 0.60) * Math.Sign(EaseYVel);
                    rotationangles[1] -= (float)Math.Pow(Math.Abs(EaseYVel / 32), 0.70) * Math.Sign(EaseYVel);

                    //Transformation angles
                    rotationangles[1] += SLP.EngineerTransform / (float)MaxTransform * (MathHelper.Pi / 1.5f);
                    if (TransformActive)
                        rotationangles[3] += (float)Math.Pow(Math.Abs((EaseXVel + Player.velocity.X / 3f) / 18f), 0.60) * Math.Sign(EaseXVel + Player.velocity.X / 3f) * Player.direction;

                    //Support Arms
                    if (part % 2 == 0)
                        for (int i = 0; i < 2; i += 1)
                            if (part % (i + 3) == 0)
                            {
                                Vector2 spriteoriginlocal = new Vector2(XOffset(ShoulderMounts[i], (int)spriteorigins[i].X), spriteorigins[i].Y);
                                Vector2 partoffset = (partoffsets[i] * facingdirection).RotatedBy(i < 1 ? 0f : rotationangles[i - 1] * facingdirection.X);

                                Vector2 drawhere = Player.position + info.bodyOrigin + bodyoffset + partoffset;
                                DrawData drawarm = new DrawData(ShoulderMounts[i], drawhere - Main.screenPosition, null, info.middleArmorColor, rotationangles[i] * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);

                                Main.playerDrawData.Add(drawarm);
                            }

                    //Pods/GL/Jetpack
                    if (part % 2 == 1)
                    {
                        int isjetpack = TransformActive ? 3 : 2;
                        Vector2 GLOffset = Vector2.Zero;
                        Vector2 spriteoriginlocal = new Vector2(XOffset(ShoulderMounts[isjetpack], (int)spriteorigins[isjetpack].X), spriteorigins[isjetpack].Y);
                        for (int i = 0; i < 3; i += 1)
                            GLOffset += (partoffsets[i] * facingdirection).RotatedBy(i < 1 ? 0f : rotationangles[i - 1] * facingdirection.X);

                        Vector2 drawhere = Player.position + info.bodyOrigin + bodyoffset + GLOffset;
                        DrawData drawGL;

                        if (isjetpack == 3)
                        {
                            int maxframes = 4;
                            Texture2D tex = ShoulderMounts[3];
                            int scale = tex.Width / maxframes;
                            Rectangle drawrect = new Rectangle(SLP.Timer / 10 % maxframes * scale, 0, scale, tex.Height);
                            drawGL = new DrawData(tex, drawhere - Main.screenPosition, drawrect, info.middleArmorColor, rotationangles[3] * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);
                        }
                        else
                        {
                            float transformanimation = (float)SLP.EngineerTransform / MaxTransform * (MathHelper.Pi / 2f);
                            drawGL = new DrawData(ShoulderMounts[2], drawhere - Main.screenPosition, null, info.middleArmorColor, (rotationangles[2] + transformanimation) * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);
                        }

                        Main.playerDrawData.Add(drawGL);

                    }

                    int XOffset(Texture2D tex, int x)
                    {
                        int texwidth = tex.Width;
                        if (tex == ShoulderMounts[3])
                            texwidth /= 4;

                        if (Player.direction < 1)
                            x = texwidth - x;
                        return x;
                    }
                }
            }
        }
    }
}