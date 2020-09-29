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
            StarlightPlayer starlightPlayer = player.GetModPlayer<StarlightPlayer>();
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

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {

            if (player.armor[0].type == ItemType<EngineerHead>() && player.armor[1].type == ItemType<EngineerChest>() && player.armor[2].type == ItemType<EngineerLegs>())//Really is there a better way?
            {

                int layerlocation = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("Body")) - 1;
                int layerlocationfront = layers.FindIndex(PlayerLayer => PlayerLayer.Name.Equals("Arms")) + 1;


                //Ugly Layering Ordering Code here!
                //Supports

                Action<PlayerDrawInfo> backTarget = s => DrawEngineerArm(s, 12, new Vector2(0, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                PlayerLayer backLayer = new PlayerLayer("EngineerLayer", "Engineer Arm", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocation, backLayer); //Insert the layer at the appropriate index. 

                Action<PlayerDrawInfo> frontTarget = s => DrawEngineerArm(s, 6, new Vector2(-20, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                PlayerLayer frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocation, frontLayer); //Insert the layer at the appropriate index. 

                frontTarget = s => DrawEngineerArm(s, 4, new Vector2(-20, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocationfront, frontLayer); //Insert the layer at the appropriate index. 

                //GL/Jetpack

                backTarget = s => DrawEngineerArm(s, 3, new Vector2(0, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                backLayer = new PlayerLayer("EngineerLayer", "Engineer Arm GL", backTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocation+2, backLayer); //Insert the layer at the appropriate index. 

                frontTarget = s => DrawEngineerArm(s, 3, new Vector2(-20, -8)); //the Action<T> of our layer. This is the delegate which will actually do the drawing of the layer.
                frontLayer = new PlayerLayer("EngineerLayer", "Engineer Arm GL", frontTarget); //Instantiate a new instance of PlayerLayer to insert into the list
                layers.Insert(layerlocationfront+2, frontLayer); //Insert the layer at the appropriate index. 

                int XOffset(Texture2D tex,int x)
                {
                    if (player.direction < 1)
                        x = tex.Width - x;
                    return x;
                }


                void DrawEngineerArm(PlayerDrawInfo info, int part, Vector2 bodyoffset)
                {
                    SpriteEffects direction = player.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                    Vector2 facingdirection = new Vector2(player.direction,1f);

                    if ((player.bodyFrame.Y+ player.bodyFrame.Height*2) % (player.bodyFrame.Height*6) > player.bodyFrame.Height*3 && player.bodyFrame.Y > player.bodyFrame.Height*6)
                        bodyoffset -= new Vector2(0, 2);
                    
                    if (player.direction < 0)
                    {
                        bodyoffset *= facingdirection;
                        bodyoffset.X -= player.width;
                    }

                    string directory = "StarlightRiver/Items/Armor/Engineer/";
                    Texture2D[] ShoulderMounts = { ModContent.GetTexture(directory + "ShoulderMount1"), ModContent.GetTexture(directory + "ShoulderMount2"), ModContent.GetTexture(directory + "ShoulderLauncher1") };
                    Vector2[] spriteorigins = { new Vector2(ShoulderMounts[0].Width - 4, ShoulderMounts[0].Height - 4),
                        new Vector2(2, ShoulderMounts[1].Height - 2),
                        new Vector2(4, ShoulderMounts[2].Height / 2) };

                    Vector2[] partoffsets = { Vector2.Zero, new Vector2(-(ShoulderMounts[0].Width - 8), -(ShoulderMounts[0].Height - 8)),
                        new Vector2(ShoulderMounts[1].Width - 6,
                        -(ShoulderMounts[1].Height - 4)) };

                    float[] rotationangles = { (float)Math.Sin(Main.GlobalTime*2f)*0.04f, (float)Math.Sin(Main.GlobalTime *3f) * 0.06f, (float)Math.Sin(Main.GlobalTime * 4f) * 0.05f };
                    rotationangles[0] -= (float)Math.Pow(Math.Abs(player.velocity.X / 20), 0.60);
                    rotationangles[1] -= (float)Math.Pow(Math.Abs(player.velocity.X / 16), 0.70);

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
                        Vector2 GLOffset = Vector2.Zero;
                        Vector2 spriteoriginlocal = new Vector2(XOffset(ShoulderMounts[2], (int)spriteorigins[2].X), spriteorigins[2].Y);
                        for (int i = 0; i < partoffsets.Length; i += 1)
                        {
                            GLOffset += (partoffsets[i] * facingdirection).RotatedBy(i < 1 ? 0f : rotationangles[i - 1] * facingdirection.X);
                        }

                        Vector2 drawhere = player.position + info.bodyOrigin + bodyoffset + (GLOffset);
                        DrawData drawGL = new DrawData(ShoulderMounts[2], drawhere - Main.screenPosition, null, info.middleArmorColor, rotationangles[2] * facingdirection.X, spriteoriginlocal, Vector2.One, direction, 0);

                        Main.playerDrawData.Add(drawGL);

                    }
                }



            }
        }
    }

}