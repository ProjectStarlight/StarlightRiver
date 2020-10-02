using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.DataStructures;
using static Terraria.WorldGen;
using StarlightRiver.Noise;
using StarlightRiver.Dusts;

namespace StarlightRiver.Items.Armor.Engineer
{
    [AutoloadEquip(EquipType.Head)]
    public class EngineerHead : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Engineer Helmet");
            Tooltip.SetDefault("2% increased ranged critial strike change");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.value = 8000;
            item.defense = 2;
        }
        public override void UpdateEquip(Player player)
        {
            player.rangedCrit += 2;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UglySweater, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    [AutoloadEquip(EquipType.Body)]
    public class EngineerChest : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Engineer Chestplate");
            Tooltip.SetDefault("2% increased ranged critial strike change");
        }
        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PostUpdateEquipsEvent += PostMovementUpdate;
            return true;
        }

        public override void SetDefaults()
        {
            item.width = 34;
            item.height = 20;
            item.value = 6000;
            item.defense = 4;
        }
        public override void UpdateEquip(Player player)
        {
            player.rangedCrit += 2;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return head.type == ItemType<EngineerHead>() && legs.type == ItemType<EngineerLegs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "After five (5) seconds of not taking damage, your next attack will ensnare and cause bleeding.";
            //starlightPlayer.ivyArmorComplete = true;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UglySweater, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        private void PostMovementUpdate(StarlightPlayer slp)
        {
            EngineerArmorPlayer starlightPlayer = slp.player.GetModPlayer<EngineerArmorPlayer>();
            starlightPlayer.HandleEngineerArmor();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class EngineerLegs : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Engineer Leggings");
            Tooltip.SetDefault("Slightly increased movement speed");
        }

        public override void SetDefaults()
        {
            item.width = 30;
            item.height = 20;
            item.value = 4000;
            item.defense = 2;
        }

        public override void UpdateEquip(Player player)
        {
            player.moveSpeed += 0.2f;
        }
        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.UglySweater, 8);
            recipe.AddTile(TileID.Anvils);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }

    public class EngineerArmorPlayer : ModPlayer
    {
        FastNoise Noise = new FastNoise(0);
        short MaxTransform => 6;
        float EaseXVel = 0f;
        float EaseYVel = 0f;
        bool TransformActive => (SLP.EngineerTransform >= MaxTransform);
        StarlightPlayer SLP => player.GetModPlayer<StarlightPlayer>();

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
            return (player.armor[0].type == ItemType<EngineerHead>() && player.armor[1].type == ItemType<EngineerChest>() && player.armor[2].type == ItemType<EngineerLegs>());//Really is there a better way?
        }
        public void HandleEngineerArmor()
        {
            EaseXVel += (player.velocity.X - EaseXVel) / 15f;
            EaseYVel += (player.velocity.Y - EaseYVel) / 12f;
            if (EngieArmor())
            {
                //Do engie things here

                if (player.controlJump)
                {
                    SLP.EngineerTransform = (short)Math.Min(SLP.EngineerTransform + 1, MaxTransform);
                    if (TransformActive)
                    {
                        Point16 playerpos = new Point16((int)player.Center.X / 16, (int)player.Center.Y / 16);
                        Point16 touchpoint = default;
                        int i;
                        for (i = 0; i < 12; i += 1)
                        {
                            Tile tile = Framing.GetTileSafely(playerpos.X, playerpos.Y + i);
                            if (WorldGen.InWorld(playerpos.X, playerpos.Y + i))
                            {
                                if (tile.active() && Main.tileSolid[tile.type])
                                {
                                    touchpoint = new Point16(playerpos.X, playerpos.Y + i);
                                    break;
                                }
                            }
                        }

                        if (touchpoint != default)
                        {
                            if (i < 9)
                            {
                                Dust dust = Dust.NewDustPerfect((touchpoint.ToVector2() * 16f) + new Vector2(Main.rand.Next(0, 16), 0), ModContent.DustType<Starlight>(), new Vector2(Main.rand.NextFloat(-16, 16) - player.velocity.X, Main.rand.NextFloat(-1, 1)), 150, Color.Gray, (10f - i) / 3f);
                                dust.color = Color.Gray;
                            }
                            if (i < 7)
                            {
                                float velocityammount = 15f / (((float)touchpoint.Y * 16f) - ((float)player.Center.Y));
                                player.velocity.Y -= (velocityammount + 0.2f);
                            }
                            //if (player.velocity.Y<0)
                            if (player.velocity.Y>0)
                            player.velocity.Y /= 1.05f;

                        }

                            player.fallStart = (int)(player.position.Y / 16f);
                        player.maxFallSpeed /= 2f;
                        player.runAcceleration /= 3f;
                    }
                    return;
                }

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
                layers.Insert(layerlocation+2, backLayer); //Insert the layer at the appropriate index. 

                frontTarget = s => DrawEngineerArm(s, 3, new Vector2(-22, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm GL", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocationfront+2, frontLayer); //Insert the layer at the appropriate index. 


                void DrawEngineerArm(PlayerDrawInfo info, int part, Vector2 bodyoffset)
                {

                    SpriteEffects direction = player.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    Vector2 facingdirection = new Vector2(player.direction,1f);

                    //Ugly guessing-game-coded bobber effect
                    if ((player.bodyFrame.Y+ player.bodyFrame.Height*2) % (player.bodyFrame.Height*6) > player.bodyFrame.Height*3 && player.bodyFrame.Y > player.bodyFrame.Height*6)
                        bodyoffset -= new Vector2(0, 2);
                    
                    if (player.direction < 0)
                    {
                        bodyoffset *= facingdirection;
                        bodyoffset.X -= player.width;
                    }

                    //Alotta predefined stuff for each part
                    string directory = "StarlightRiver/Items/Armor/Engineer/";
                    Texture2D[] ShoulderMounts = { ModContent.GetTexture(directory + "ShoulderMount1"), ModContent.GetTexture(directory + "ShoulderMount2"), ModContent.GetTexture(directory + "ShoulderLauncher1"), ModContent.GetTexture(directory + "ShoulderLauncher2") };
                    Vector2[] spriteorigins = { new Vector2(ShoulderMounts[0].Width - 4, ShoulderMounts[0].Height - 4),
                        new Vector2(2, ShoulderMounts[1].Height - 2),
                        new Vector2(4, ShoulderMounts[2].Height / 2),
                    new Vector2((ShoulderMounts[3].Width/4f)/2, 2) };

                Vector2[] partoffsets = { Vector2.Zero,
                        new Vector2(-(ShoulderMounts[0].Width - 6), -(ShoulderMounts[0].Height - 8)),
                        new Vector2(ShoulderMounts[1].Width - 8,-(ShoulderMounts[1].Height - 4)),
                        new Vector2(4, (ShoulderMounts[2].Height/2)),
                    new Vector2((ShoulderMounts[3].Width/4) - 8,-(ShoulderMounts[3].Height - 4)) };

                //Redefined angles and some gentle idle animations
                float[] rotationangles = { (float)Math.Sin(Main.GlobalTime*0.75f)*0.04f, 
                        (float)Math.Sin(Main.GlobalTime *1f) * 0.06f,
                        (float)Math.Sin(Main.GlobalTime * 1.33f) * 0.05f,
                        (float)Noise.GetNoise(SLP.Timer * (bodyoffset.X<-11 ? 1f : -1f),SLP.Timer)/5f};

                    //Sway backwards as the player moves
                    rotationangles[0] -= (float)Math.Pow(Math.Abs(EaseXVel / 20), 0.60);
                    rotationangles[1] -= (float)Math.Pow(Math.Abs(EaseXVel / 16), 0.70);

                    //Vertical movement

                    rotationangles[0] += (float)Math.Pow(Math.Abs(EaseYVel / 40), 0.60) * Math.Sign(EaseYVel);
                    rotationangles[1] -= (float)Math.Pow(Math.Abs(EaseYVel / 32), 0.70) * Math.Sign(EaseYVel);

                    //Transformation angles
                    rotationangles[1] += ((float)SLP.EngineerTransform / (float)MaxTransform) * (MathHelper.Pi / 1.5f);
                    if (TransformActive)
                        rotationangles[3] += (float)Math.Pow(Math.Abs((EaseXVel + (player.velocity.X/ 3f)) / 18f), 0.60) * Math.Sign(EaseXVel+(player.velocity.X / 3f)) *player.direction;

                    //Support Arms
                    if (part %2 == 0)
                    {
                        for (int i = 0; i < 2; i += 1)
                        {
                            if (part % (i + 3) == 0)
                            {
                                Vector2 spriteoriginlocal = new Vector2(XOffset(ShoulderMounts[i], (int)spriteorigins[i].X), spriteorigins[i].Y);
                                Vector2 partoffset = ((partoffsets[i] * facingdirection).RotatedBy(i < 1 ? 0f : rotationangles[i - 1] * facingdirection.X));

                                Vector2 drawhere = player.position + info.bodyOrigin + bodyoffset + partoffset;
                                DrawData drawarm = new DrawData(ShoulderMounts[i], drawhere - Main.screenPosition, null, info.middleArmorColor, rotationangles[i] * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);

                                Main.playerDrawData.Add(drawarm);
                            }
                        }
                    }

                    //Pods/GL/Jetpack
                    if (part % 2 == 1)
                    {
                        int isjetpack = TransformActive ? 3 : 2;
                        Vector2 GLOffset = Vector2.Zero;
                        Vector2 spriteoriginlocal = new Vector2(XOffset(ShoulderMounts[isjetpack], (int)spriteorigins[isjetpack].X), spriteorigins[isjetpack].Y);
                        for (int i = 0; i < 3; i += 1)
                        {
                            //if (i == 2 && isjetpack == 3)
                                //i = 3;

                            GLOffset += (partoffsets[i] * facingdirection).RotatedBy(i < 1 ? 0f : rotationangles[i - 1] * facingdirection.X);
                        }
                        Vector2 drawhere = player.position + info.bodyOrigin + bodyoffset + (GLOffset);
                        DrawData drawGL;
                        if (isjetpack == 3)
                        {
                            int maxframes = 4;
                            Texture2D tex = ShoulderMounts[3];
                            int scale = (tex.Width / maxframes);
                            Rectangle drawrect = new Rectangle(((int)(SLP.Timer / 10) % maxframes) * scale, 0, scale, tex.Height);
                            drawGL = new DrawData(tex, drawhere - Main.screenPosition, drawrect, info.middleArmorColor, rotationangles[3] * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);
                        }
                        else
                        {
                            float transformanimation = ((float)SLP.EngineerTransform / MaxTransform) * (MathHelper.Pi/2f);
                            drawGL = new DrawData(ShoulderMounts[2], drawhere - Main.screenPosition, null, info.middleArmorColor, (rotationangles[2]+transformanimation) * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);
                        }

                        Main.playerDrawData.Add(drawGL);

                    }

                    int XOffset(Texture2D tex, int x)
                    {
                        int texwidth = tex.Width;
                        if (tex == ShoulderMounts[3])
                            texwidth /= 4;

                        if (player.direction < 1)
                            x = texwidth - x;
                        return x;
                    }

                }

            }
        }
    }

}