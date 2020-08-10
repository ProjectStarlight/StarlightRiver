using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.Ultrashark
{
    public class Ultrashark : ModItem, IGlowingItem
    {
        #region values
        public float spinup;

        public bool turretDeployed = false; //true after the player pressed RMB
        public bool turretSetup = false; //true after the turret setup animation is complete, the player can now shoot

        public int turretDirection = 0; //set to player direction upon pressing RMB
        public float sharkRotation = 0; //cached rotation, thats about it
        public int sharkFrame = 1; //self explanitory
        public int sharkFrameCount = 10;

        public int standFrame = 1;
        public int standFrameCount = 9;
        #endregion

        private void SpawnCasing(Vector2 velocity, Vector2 position) //pos infront of player pretty much
        {
            Gore.NewGore(position, (-velocity + new Vector2(0, -1) + new Vector2(Main.rand.NextFloat(3f) - 1.5f, -2)) * 0.25f, mod.GetGoreSlot("Gores/UltrasharkCasing"));
        }

        private Vector2 GetSharkPos(Player player) => player.Center + new Vector2(turretDirection * player.width, -7);

        public Vector2 GetStandPos(Player player) => player.Center + new Vector2(turretDirection * player.width, 7);

        public float GetSharkRotation(Player player) //used to set sharkRotation
        {
            float rotation = (Main.MouseWorld - GetSharkPos(player)).ToRotation();
            float anglediff = ((turretDirection == 1 ? 0 : 3.14f) - rotation + 9.42f) % 6.28f - 3.14f;
            float f = 1.256f;

            if (anglediff <= f && anglediff >= -f) return rotation;
            else return sharkRotation;
        }

        #region drawing
        public void DrawStand(PlayerDrawInfo info) //should all be obivous
        {
            Texture2D standTexture = mod.GetTexture("Items/Ultrashark/StandDelpoyAnimation");
            int frameHeight = standTexture.Height / standFrameCount;
            int frame = frameHeight * standFrame;
            Player player = info.drawPlayer;
            Vector2 standPos = GetStandPos(player) - Main.screenPosition + new Vector2(0f, player.gfxOffY);
            Main.playerDrawData.Add(new DrawData(
                standTexture,
                standPos,  //position
                new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, frame, standTexture.Width, frameHeight)), //source
                Lighting.GetColor((int)GetStandPos(player).X / 16, (int)GetStandPos(player).Y / 16), //color
                0, //rotation
                new Vector2(standTexture.Width / 2f, frameHeight / 2f), //origin
                1f, //scale
                turretDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0));
        }
        public void DrawGun(PlayerDrawInfo info)
        {
            Texture2D sharkTexture = mod.GetTexture("Items/Ultrashark/ShootingAnimation");
            int frameHeight = sharkTexture.Height / sharkFrameCount;
            int frame = frameHeight * sharkFrame;
            Player player = info.drawPlayer;
            Vector2 sharkPos = GetSharkPos(player) - Main.screenPosition + new Vector2(0f, player.gfxOffY);
            sharkRotation = GetSharkRotation(player);
            Main.playerDrawData.Add(new DrawData(
                sharkTexture,
                sharkPos,  //position
                new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, frame, sharkTexture.Width, frameHeight)), //source
                Lighting.GetColor((int)GetSharkPos(player).X / 16, (int)GetSharkPos(player).Y / 16), //color
                sharkRotation, //rotation
                new Vector2(sharkTexture.Width / 2f, frameHeight / 2f), //origin
                1f, //scale
                turretDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0));
        }
        public void DrawGlowmask(PlayerDrawInfo info)
        {
            if (turretDeployed)
            {
                DrawStand(info);
                DrawGun(info);
            }
        }
        #endregion

        #region item
        public override void SetDefaults()
        {
            item.useStyle = 5;
            item.useAnimation = 24;
            item.useTime = 24;
            item.shootSpeed = 20f;
            item.knockBack = 2f;
            item.UseSound = SoundID.Item11;
            item.width = 64;
            item.height = 24;
            item.damage = 28;
            item.rare = ItemRarityID.Red;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.autoReuse = true;
            item.useTurn = false;
            item.useAmmo = AmmoID.Bullet;
            item.ranged = true;
            item.shoot = ProjectileID.Bullet;
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ultrashark");
            Tooltip.SetDefault("");
        }
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (turretDeployed && player.altFunctionUse == 2) return false;

            if (turretDeployed && !turretSetup) return false;

            if (player.altFunctionUse == 2 || turretDeployed) item.noUseGraphic = true;
            else item.noUseGraphic = false;

            return false;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override float UseTimeMultiplier(Player player) => 1 + spinup;

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (spinup < (turretDeployed ? 4f : 3f)) spinup += (turretDeployed ? 0.5f : 0.2f);

            if (turretDeployed) //shoot as stand
            {
                sharkFrame++;
                if (sharkFrame >= sharkFrameCount) sharkFrame = 0;

                Vector2 velocity = (GetSharkRotation(player) + Main.rand.NextFloat(-0.1f, 0.1f)).ToRotationVector2() * 10;
                position = GetSharkPos(player);
                SpawnCasing(velocity, position);
                return true;
            }
            else if (player.altFunctionUse == 2) //summon stand
            {
                if (player.velocity.X >= -0.2f || player.velocity.X <= 0.2f || player.velocity.Y >= -0.2f || player.velocity.Y >= 0.2f)
                {
                    turretDeployed = true;
                    turretDirection = player.direction;
                }
                return false;
            }

            Vector2 velocity2 = (GetSharkRotation(player) + Main.rand.NextFloat(-0.2f, 0.2f)).ToRotationVector2() * 12;
            SpawnCasing(velocity2, position);
            return true;
        }
        #endregion
    }
    public class UltrasharkHandler : ModPlayer
    {
        public override void PostUpdate()
        {
            if (player.HeldItem.type == ItemType<Ultrashark>())
            {
                Ultrashark item = (Ultrashark)player.HeldItem.modItem;

                if (item.turretDeployed && item.turretDirection != 0) player.direction = item.turretDirection;
            }
        }
        public override void PreUpdate()
        {
            if (player.HeldItem.type == ItemType<Ultrashark>())
            {
                Ultrashark item = (Ultrashark)player.HeldItem.modItem;

                if (player.releaseUseItem) item.spinup = 0;

                if (item.turretDeployed)
                {
                    if (player.velocity.X <= -0.2f || player.velocity.X >= 0.2f || player.velocity.Y <= -0.2f || player.velocity.Y >= 0.2f) //cancel if moving
                    {
                        item.turretDeployed = false;
                        item.turretSetup = false;
                        item.standFrame = 0;
                        item.sharkFrame = 0;
                        item.spinup = 0;
                    }
                }

                StarlightPlayer sPlayer = player.GetModPlayer<StarlightPlayer>();

                if (item.turretDeployed)
                {
                    if (sPlayer.Timer % 6 == 0) //animate stand
                    {
                        if (item.standFrame < item.standFrameCount - 1) item.standFrame++;
                        else if (!item.turretSetup) item.turretSetup = true;
                    }

                    if (sPlayer.Timer - item.spinup * 6f == 0) //animate gun
                    {
                        if (item.sharkFrame < item.sharkFrameCount - 1) item.sharkFrame++;
                    }
                }
            }
        }
    }
}