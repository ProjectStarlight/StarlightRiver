//TODO:
//Collision with player
//Gores
//Sound effects
//Animation
//Visuals

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Items.Moonstone;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Moonstone
{
    //bestiary needs to be done but there isnt a moonstone bestiary template thingy
    public class Dreamprism : ModNPC
    {
        private enum Phase
        {
            rising = 0,
            spinning = 1,
            slamming = 2,
            slammed = 3
        }
        public override string Texture => AssetDirectory.MoonstoneNPC + Name;

        private int xFrame = 0;

        private Phase phase = Phase.rising;

        private float bobCounter;
        private Vector2 posAbovePlayer = Vector2.Zero;
        private Vector2 risingPos = Vector2.Zero;

        private int spinTimer = 0;

        private int slamTimer = 0;

        private Player target => Main.player[NPC.target];

        public override void Load()
        {
            /*for (int i = 1; i < 4; i++)
            {
                GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + i);
            }*/
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dreamprism");
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 0;
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 62;
            NPC.knockBackResist = 1f;
            NPC.lifeMax = 150;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.damage = 35;
            NPC.aiStyle = -1;
            NPC.friendly = false;

            NPC.HitSound = new Terraria.Audio.SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact") with {Volume = 0.8f, Pitch = 0.15f };
            NPC.DeathSound = new Terraria.Audio.SoundStyle($"{nameof(StarlightRiver)}/Sounds/VitricBoss/ceramicimpact") with { Volume = 1.1f, Pitch = -0.2f };

            NPC.value = Item.buyPrice(silver: 3, copper: 15);
            NPC.behindTiles = true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            SpriteEffects effects = SpriteEffects.None;
            Vector2 origin = new Vector2(NPC.width / 2, (NPC.height / 2));

            if (NPC.spriteDirection != 1)
                effects = SpriteEffects.FlipHorizontally;

            Vector2 slopeOffset = new Vector2(0, NPC.gfxOffY);
            Main.spriteBatch.Draw(texture, slopeOffset + NPC.Center - screenPos, NPC.frame, Color.White, NPC.rotation, origin, NPC.scale, effects, 0f);
            return false;
        }

        public override void AI()
        {
            Lighting.AddLight(NPC.Center, Color.Purple.ToVector3() * 1.2f);
            NPC.TargetClosest(false);

            switch(phase)
            {
                case Phase.rising:
                    RisingBehavior();
                    break;
                case Phase.spinning:
                    SpinBehavior();
                    break;
                case Phase.slamming:
                    SlamBehavior();
                    break;
                case Phase.slammed:
                    SlammedBehavior();
                    break;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame = new Rectangle(0, xFrame * frameHeight, NPC.width, frameHeight);
        }

        private void RisingBehavior()
        {
            bobCounter += 0.02f;
            if (posAbovePlayer == Vector2.Zero)
            {
                risingPos = NPC.Center;
                posAbovePlayer = target.Center - new Vector2(0, Main.rand.Next(300, 400));
            }

            float distance = posAbovePlayer.Y - risingPos.Y;
            float progress = (NPC.Center.Y - risingPos.Y) / distance;

            posAbovePlayer.X = target.Center.X;

            if (progress < 0)
            {
                progress = MathHelper.Clamp(progress, 0, 1);
                risingPos = NPC.Center;
            }

            Vector2 dir = NPC.DirectionTo(posAbovePlayer);
            NPC.velocity = dir * ((float)Math.Sin(progress * 3.14f) + 0.3f) * 5;
            NPC.velocity.Y += (float)Math.Cos(bobCounter) * 0.15f;

            if ((NPC.Center - posAbovePlayer).Length() < 4)
            {
                NPC.velocity = Vector2.Zero;
                NPC.Center = posAbovePlayer;
                phase = Phase.spinning;
                risingPos = Vector2.Zero;
                posAbovePlayer = Vector2.Zero;
            }
        }

        private void SpinBehavior()
        {
            bobCounter += 0.03f;
            NPC.velocity.Y = (float)Math.Sin(bobCounter) * 0.25f;
            spinTimer++;


            if (spinTimer < 150)
            {
                NPC.velocity.X = (float)MathHelper.Min((float)Math.Pow(Math.Abs(target.Center.X - NPC.Center.X), 0.3f), 5) * Math.Sign(target.Center.X - NPC.Center.X);
            }
            else
                NPC.velocity.X = 0;
            if (spinTimer > 200)
            {
                NPC.velocity.Y = 20;
                spinTimer = 0;
                phase = Phase.slamming;
            }
        }

        private void SlamBehavior()
        {
            NPC.velocity.Y += 0.2f;
            slamTimer++;

            Tile tile = Main.tile[(int)NPC.Center.X / 16, (int)(NPC.Center.Y / 16) + 2];
            if ((tile.HasTile && (Main.tileSolid[tile.TileType])) || slamTimer > 80)
            {
                if (target == Main.LocalPlayer)
                    Core.Systems.CameraSystem.Shake += 8;

                slamTimer = 0;
                phase = Phase.slammed;
                NPC.velocity = Vector2.Zero;
            }
        }

        private void SlammedBehavior()
        {
            slamTimer++;
            if (slamTimer > 90)
            {
                risingPos = NPC.Center;
                posAbovePlayer = target.Center - new Vector2(0, Main.rand.Next(300, 400));
                phase = Phase.rising;
                slamTimer = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return spawnInfo.Player.InModBiome(ModContent.GetInstance<MoonstoneBiome>()) ? 10 : 0;
        }
    }
}
