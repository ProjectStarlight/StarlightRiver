using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class ContagiousIce : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public ContagiousIce() : base("Contagious Ice", "Enemies are encased in solid ice upon death which shatters into deadly shards") { }

        public static readonly int ShardDamage = 6;

        public override void SafeSetDefaults()
        {
            Item.value = Item.sellPrice(0, 2, 0, 0);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void SafeUpdateEquip(Player player)
        {
            player.GetModPlayer<ContagiousIcePlayer>().Active = true;
        }

        public override void AddRecipes()
        {

        }
    }

    internal class ContagiousIceProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.MiscItem + "BizarreIce";

        public override void SetStaticDefaults()
        {           
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            

            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 90;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                Projectile.frame = Main.rand.Next(3);
                Projectile.netUpdate = true;
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation()+MathHelper.PiOver2;

            Projectile.velocity.Y += 0.05f;

            Projectile.velocity.X *= 0.99f;   
        }

    }

    internal class ContagiousIcePlayer : ModPlayer
    {
        public bool Active = false;
        public override void ResetEffects()
        {
            Active = false;
        }
    }

    internal class ContagiousIceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;


        public static readonly List<int> DoesNotFreeze = new List<int> { };

        public int FreezePlayer = -1;

        public int FreezeState = 0;

        public int FreezeTimer = 420;

        const int MaxFreeze = 420;

        private bool HasIceAccessory(Player player) => player.GetModPlayer<ContagiousIcePlayer>().Active;

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit)
        {

            FreezePlayer = HasIceAccessory((Player)player)&&!npc.boss&&!DoesNotFreeze.Contains(npc.type) ? player.whoAmI : -1;

        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            FreezePlayer = HasIceAccessory(Main.player[projectile.owner])&&!npc.boss && !DoesNotFreeze.Contains(npc.type) ? projectile.owner : -1;
        }

        public override bool PreAI(NPC npc)
        {
            if (FreezeState==2)
            {
                //npc.noGravity = true;
                npc.velocity = Vector2.Zero;
                npc.noGravity = true;
                npc.noTileCollide = true;
                npc.frameCounter = 0;

                //npc.fram
                --FreezeTimer;

                if (FreezeTimer <= 0)
                {
                    npc.life = 0;
                    DestroyIceTiles(npc);
                    npc.checkDead();
                }
                return false;
            }else if (FreezeState == 1)
            {

                --FreezeTimer;
                npc.noTileCollide = false;
                npc.noGravity = false;
                npc.frameCounter = 0;

                if (npc.velocity.Y == 0)
                {
                    npc.velocity.X *= 0.96f;
                }

                if (npc.velocity.Length() < 1||FreezeTimer<=0)
                {
                    npc.noGravity = true;
                    npc.velocity = Vector2.Zero;
                    npc.noTileCollide = true;
                    FreezeTimer = MaxFreeze;
                    FreezeState = 2;
                    PlaceIceTiles(npc);
                }

                return false;
            }
            
            
            
            return base.PreAI(npc);
        }

        public override void HitEffect(NPC npc, int hitDirection, double damage)
        {
            

            if (FreezePlayer>=0)
            {
                return;
            }
            base.HitEffect(npc, hitDirection, damage);
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {

            if (FreezeState>0)
            {
                return;
            }
            
            base.FindFrame(npc, frameHeight);
        }

        public override bool CheckDead(NPC npc)
        {
            if (FreezeState>0&&FreezeTimer>0)
            {
                return false;
            }else if(FreezeState>1&& FreezeTimer <= 0)
            {
                return base.CheckDead(npc);
            }
            
            else if (FreezePlayer>=0&&FreezeState==0)
            {
                npc.life = 1;
                npc.knockBackResist = 0f;
                npc.dontTakeDamage = true;
                npc.noGravity = false;
                FreezeTimer = MaxFreeze/7;
                FreezeState = 1;
                return false;
            }
            
            return base.CheckDead(npc);
        }

        public void PlaceIceTiles(NPC npc)
        {        
            Point tileGridPoint = new Point((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));

            int tileGridWidth = (int)(npc.width * npc.scale / 16f)+2;

            int tileGridHeight = (int)(npc.height * npc.scale / 16f)+2;

            for(int j = -tileGridHeight/2; j <= tileGridHeight/2; j++)
            {
                for(int i = -tileGridWidth/2; i <= tileGridWidth/2; i++)
                {
                    Point currentTile = new Point(tileGridPoint.X + i, tileGridPoint.Y + j);

                    if (!Framing.GetTileSafely(currentTile).HasTile)
                    {
                        WorldGen.PlaceTile(currentTile.X, currentTile.Y, ModContent.TileType<ContagiousIceTile>(), false, true, -1);             
                    }


                }

            }


        }

        public void DestroyIceTiles(NPC npc)
        {

            Point tileGridPoint = new Point((int)(npc.Center.X / 16f), (int)(npc.Center.Y / 16f));

            int tileGridWidth = (int)(npc.width * npc.scale / 16f) + 2;

            int tileGridHeight = (int)(npc.height * npc.scale / 16f) + 2;

            for (int j = -tileGridHeight / 2; j <= tileGridHeight / 2; j++)
            {
                for (int i = -tileGridWidth / 2; i <= tileGridWidth / 2; i++)
                {
                    Point currentTile = new Point(tileGridPoint.X + i, tileGridPoint.Y + j);

                    if (Main.netMode != 1)
                    {
                        Projectile.NewProjectile(npc.GetSource_Death(), new Vector2(currentTile.X + 0.5f, currentTile.Y + 0.5f) * 16f, Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 12, ModContent.ProjectileType<ContagiousIceProjectile>(), ContagiousIce.ShardDamage, 0, FreezePlayer);
                    }

                    if (Framing.GetTileSafely(currentTile).HasTile&&Framing.GetTileSafely(currentTile).TileType==ModContent.TileType<ContagiousIceTile>())
                    {

                        WorldGen.KillTile(currentTile.X, currentTile.Y, false, false,true);

                    }

                }

            }

        }

        public override bool PreKill(NPC npc)
        {
            if (FreezePlayer>=0 && FreezeState==0)
            {
                return false;
            }


            return base.PreKill(npc);
        }

        public void DrawIceCube(SpriteBatch spriteBatch, NPC npc)
        {
            Texture2D iceTex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "ContagiousIceCube", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

            int tileGridWidth = (int)(npc.width * npc.scale / 16f) + 1;

            int tileGridHeight = (int)(npc.height * npc.scale / 16f) + 1;

            Rectangle drawFrame;

            for (int j = 0; j <= tileGridHeight; j++)
            {
                for (int i = 0; i <= tileGridWidth; i++)
                {
                    bool leftEdge = i == 0;
                    bool rightEdge = i == tileGridWidth;
                    bool topEdge = j == 0;
                    bool bottomEdge = j == tileGridHeight;

                    Vector2 drawPos = npc.Center - (new Vector2(i - (int)(tileGridWidth / 2), j - (int)(tileGridHeight / 2)) * 16f);

                    float rotation = 0;

                    if (leftEdge && topEdge)
                    {
                        rotation = 0;
                        drawFrame = new Rectangle(0, 0, 16, 16);
                    }
                    else if (rightEdge && topEdge)
                    {
                        rotation = MathHelper.PiOver2;
                        drawFrame = new Rectangle(0, 0, 16, 16);
                    }
                    else if (rightEdge && bottomEdge)
                    {
                        rotation = MathHelper.Pi;
                        drawFrame = new Rectangle(0, 0, 16, 16);
                    }
                    else if (leftEdge && bottomEdge)
                    {
                        rotation = MathHelper.PiOver2 * 3f;
                        drawFrame = new Rectangle(0, 0, 16, 16);
                    }
                    else if (leftEdge || rightEdge || topEdge || bottomEdge)
                    {
                        drawFrame = new Rectangle(16, 0, 16, 16);

                        if (topEdge)
                        {
                            rotation = 0;
                        }
                        else if (rightEdge)
                        {
                            rotation = MathHelper.PiOver2;
                        }
                        else if (bottomEdge)
                        {
                            rotation = MathHelper.Pi;
                        }
                        else if (leftEdge)
                        {
                            rotation = MathHelper.PiOver2 * 3;
                        }

                    }
                    else
                    {
                        drawFrame = new Rectangle(32, 0, 16, 16);
                    }

                    spriteBatch.Draw(iceTex, drawPos - Main.screenPosition, drawFrame, Color.LightSkyBlue * 0.5f, rotation + MathHelper.Pi, Vector2.One * 8, 1f, SpriteEffects.None, 0);



                }

            }



        }

        public override Color? GetAlpha(NPC npc, Color drawColor)
        {
            if (FreezeState > 0)
            {
                return Color.CornflowerBlue;
            }



            return base.GetAlpha(npc, drawColor);
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);

            if (FreezeState == 2)
            {
                DrawIceCube(spriteBatch, npc);
            }
        }

    }

    internal class ContagiousIceTile : ModTile
    {
        public override string Texture => AssetDirectory.MiscTile + Name;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;
            //Main.tileStone[Type] = true;
            HitSound = Terraria.ID.SoundID.Shatter;

            DustType = Terraria.ID.DustID.Ice;
            //ItemDrop = ItemType<PalestoneItem>();

           // AddMapEntry(new Color(167, 180, 191));
        }
    }
}

