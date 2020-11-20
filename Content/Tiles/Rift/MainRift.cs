/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.RiftCrafting;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Tiles.Rift
{
    class MainRift : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<RiftEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("");
            AddMapEntry(new Color(100, 0, 120), name);
            dustType = ModContent.DustType<Dusts.Darkness>();
            disableSmartCursor = true;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D riftTex = ModContent.GetTexture("StarlightRiver/Tiles/Rift/Rift");
            Color color = new Color(50, 10, 120);

            spriteBatch.Draw(riftTex, (new Vector2(i, j) + Helper.TileAdj) * 16 + Vector2.One * 4 - Main.screenPosition, riftTex.Frame(), color * 0.4f, 0, riftTex.Size() / 2,
                0.7f + (float)Math.Sin(LegendWorld.rottime + 2) * 0.04f, 0, 0);

            int index = ModContent.GetInstance<RiftEntity>().Find(i, j);
            if (index == -1) return true;
            RiftEntity entity = (RiftEntity)TileEntity.ByID[index];

            for (int k = 0; k < entity.inventory.Count; k++)
            {
                int maxItems = entity.inventory.Count;
                Item item = entity.inventory[k];
                Texture2D tex = (item.modItem != null) ? ModContent.GetTexture(item.modItem.Texture) : ModContent.GetTexture("Terraria/Item_" + item.type);
                Vector2 pos = new Vector2(i, j) * 16 + Vector2.One * 8 + new Vector2(0, -128).RotatedBy(k / (float)maxItems * 6.28f) - Main.screenPosition + Helper.TileAdj * 16;
                Rectangle frame = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(tex) : tex.Frame();

                spriteBatch.Draw(tex, new Rectangle((int)pos.X, (int)pos.Y, frame.Width, frame.Height), frame, Color.White * 0.6f, (float)Math.Sin(LegendWorld.rottime) * 0.15f, frame.Size() / 2, 0, 0);
            }

            if (entity.timer > 0)
            {
                Utils.DrawBorderString(spriteBatch, (entity.timer / 60).ToString(),
                    new Vector2(i, j) * 16 - new Vector2(Main.fontMouseText.MeasureString((entity.timer / 60).ToString()).X / 2, 0) - Main.screenPosition + Helper.TileAdj * 16, Color.White);
            }

            return true;
        }
    }

    public class RiftEntity : ModTileEntity
    {
        public List<Item> inventory = new List<Item>();
        public RiftRecipe activeCraft;
        public int difficulty = 0;
        public int timer = 0;
        private int spawnCooldown = 0;

        public override bool ValidTile(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.active() && tile.type == ModContent.TileType<MainRift>();
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction)
        {
            if (Main.netMode == 1)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 3);
                NetMessage.SendData(87, -1, -1, null, i, j, Type, 0f, 0, 0, 0);
                return -1;
            }
            return Place(i, j);
        }

        public override void Update()
        {
            if (timer >= 1) timer--;
            Rectangle hitbox = new Rectangle(Position.X * 16 - 24, Position.Y * 16 - 56, 64, 128);
            Vector2 pos = Position.ToVector2() * 16;

            float rot = Main.rand.NextFloat(6.28f);
            Vector2 off = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot) * 3.5f);

            Dust.NewDustPerfect(hitbox.Center.ToVector2() + (off * 30) - Vector2.One * 4, ModContent.DustType<Dusts.VitricBossTell>(),
                Vector2.Normalize(off).RotatedBy(1.58f) * -5, 0, new Color(100, 30, 175) * 0.9f, 1.2f);

            Dust.NewDustPerfect(hitbox.Center.ToVector2() + (off * 30) - Vector2.One * 4, ModContent.DustType<Dusts.VitricBossTell>(),
                Vector2.Normalize(off).RotatedBy(1.58f) * -5, 0, new Color(1, 1, 1), 0.8f);

            if (activeCraft == null)
            {
                //Adds the items to the inventory of the rift
                foreach (Item item in Main.item.Where(item => item.active && item.Hitbox.Intersects(hitbox) && item.stack == 1 &&
                item.GetGlobalItem<RiftItem>().crafted == false && item.GetGlobalItem<RiftItem>().tier == 0 && !item.GetGlobalItem<RiftItem>().starless))
                {
                    Shake(10, Position.ToVector2() * 16);
                    inventory.Add(item.Clone());
                    item.TurnToAir();
                    item.active = false;
                }

                if (Main.item.Any(item => item.active && item.GetGlobalItem<RiftItem>().tier > 0 && item.Hitbox.Intersects(hitbox))) //Catalyst check
                {
                    Item item = Main.item.FirstOrDefault(i => i.active && i.GetGlobalItem<RiftItem>().tier > 0 && i.Hitbox.Intersects(hitbox)); //The Catalyst

                    foreach (RiftRecipe recipe in (mod as StarlightRiver).RiftRecipes) //Checks all the recipies for a valid one
                    {
                        if (recipe.CheckRecipe(inventory) && item.GetGlobalItem<RiftItem>().tier >= recipe.Tier) //Activates that recipe
                        {
                            Shake(30, Position.ToVector2() * 16);
                            activeCraft = recipe;
                            difficulty = recipe.Tier;
                            timer = recipe.Tier * 1800;

                            CleanupInventory(inventory, recipe, Position.ToVector2() * 16);
                            item.TurnToAir();
                        }
                    }
                }
            }
            else
            {
                if (spawnCooldown > 0) spawnCooldown--;
                if (spawnCooldown == 0)
                {
                    int i = NPC.NewNPC((int)pos.X + Main.rand.Next(-400, 400), (int)pos.Y + Main.rand.Next(-400, 400), activeCraft.SpawnPool[Main.rand.Next(0, activeCraft.SpawnPool.Count)]);
                    Main.npc[i].GetGlobalNPC<RiftNPC>().parent = this;
                    for (int k = 0; k <= 50; k++)
                    {
                        Dust.NewDustPerfect(Main.npc[i].Center, ModContent.DustType<Dusts.Darkness>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10));
                    }

                    spawnCooldown = 180 - activeCraft.Tier * 15;
                }
            }

            if (timer == 1)
            {
                inventory.Clear();
                Shake(20, Position.ToVector2() * 16);

                int i = Item.NewItem(Position.ToVector2() * 16, activeCraft.Result);
                Main.item[i].GetGlobalItem<RiftItem>().crafted = true;
                activeCraft = null;
            }

            if (!Main.player.Any(player => Vector2.Distance(player.Center, Position.ToVector2() * 16) < 1000))
            {
                activeCraft = null;
                timer = 0;
                foreach (Item item in inventory.Where(item => item.maxStack == 1))
                {
                    int i = Item.NewItem(pos, item.type);
                    Main.item[i].GetGlobalItem<RiftItem>().starless = true;
                }
                inventory.Clear();
            }
        }
        private void CleanupInventory(List<Item> inventory, RiftRecipe recipe, Vector2 position)
        {
            List<Item> removals = new List<Item>();
            foreach (Item item in inventory.Where(item => recipe.Ingredients.Count(i => i.type == item.type) == 0))
            {
                Item.NewItem(position, item.type);
                removals.Add(item);
            }

            for (int k = 0; k < removals.Count; k++)
            {
                inventory.Remove(inventory.FirstOrDefault(item => item == removals[k]));
            }

            foreach (RiftIngredient ingredient in recipe.Ingredients)
            {
                if (inventory.Any(i => i.type == ingredient.type))
                {
                    int total = inventory.Count(i => i.type == ingredient.type);
                    for (int k = total; k > ingredient.count; k--)
                    {
                        Item.NewItem(position, ingredient.type);
                        inventory.Remove(inventory.FirstOrDefault(i => i.type == ingredient.type));
                    }
                }
            }
        }

        private void Shake(int amount, Vector2 pos)
        {
            foreach (Player player in Main.player.Where(player => Vector2.Distance(player.Center, pos) < 1200))
            {
                player.GetModPlayer<StarlightPlayer>().Shake += amount;
            }
        }
    }

    public class RiftItem : GlobalItem
    {
        public bool crafted = false;
        public bool starless = false;
        public int tier = 0;

        public override bool InstancePerEntity => true;
        public override bool CloneNewInstances => true;
        public override bool OnPickup(Item item, Player player)
        {
            crafted = false;
            return base.OnPickup(item, player);
        }
        public override void PostUpdate(Item item)
        {
            Color color = new Color(220 + (int)(Math.Sin(LegendWorld.rottime * 3) * 25), 140, 255) * 0.5f;
            if (crafted)
            {
                Dust.NewDustPerfect(item.Center, ModContent.DustType<Dusts.VitricBossTell>(), Vector2.One.RotatedByRandom(6.28f) * 20, 0, color);
                Lighting.AddLight(item.Center, color.ToVector3());
            }

            if (starless)
            {
                item.color = new Color(50, 50, 50);
            }
            base.PostUpdate(item);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (starless)
            {
                TooltipLine tip = new TooltipLine(mod, "StarlessState", "Starless\nInfuse with celestial energy to restore");
                tip.overrideColor = new Color(50, 50, 50);
                tooltips.Add(tip);
            }
        }

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (crafted)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive);

                Texture2D tex = ModContent.GetTexture("StarlightRiver/RiftCrafting/Glow0");
                Texture2D tex2 = ModContent.GetTexture("StarlightRiver/RiftCrafting/Glow1");
                Color color = new Color(220 + (int)(Math.Sin(LegendWorld.rottime * 3) * 25), 140, 255) * 0.5f;
                float baseScale = item.Hitbox.Size().Length() / tex.Frame().Size().Length() * 3;

                spriteBatch.Draw(tex, item.Center - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, baseScale + (float)Math.Sin(LegendWorld.rottime * 2) * 0.1f, 0, 0);
                spriteBatch.Draw(tex2, item.Center - Main.screenPosition, tex2.Frame(), color, LegendWorld.rottime, tex2.Size() / 2, baseScale + (float)Math.Cos(LegendWorld.rottime * 2) * 0.1f, 0, 0);
                spriteBatch.Draw(tex2, item.Center - Main.screenPosition, tex2.Frame(), color, -LegendWorld.rottime, tex2.Size() / 2, baseScale + (float)Math.Cos(LegendWorld.rottime * 2) * 0.1f, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            }
            return true;
        }
    }

    public class RiftNPC : GlobalNPC
    {
        public RiftEntity parent = null;

        public override bool InstancePerEntity => true;

        public override void SetDefaults(NPC npc)
        {
            base.SetDefaults(npc);
            if (parent != null)
            {
                npc.lifeMax *= (int)(parent.activeCraft.Tier / 5f * 2f);
                npc.damage *= (int)(parent.activeCraft.Tier / 5f * 1.5f);
            }
        }
        public override bool CheckDead(NPC npc)
        {
            if (parent != null)
            {
                if (parent.timer > 130)
                {
                    parent.timer -= 120;

                    Vector2 pos = parent.Position.ToVector2() * 16;
                    CombatText.NewText(new Rectangle((int)pos.X, (int)pos.Y, 1, 1), new Color(172, 172, 172), -2);
                }
            }
            return base.CheckDead(npc);
        }

        public override bool PreAI(NPC npc)
        {
            if (parent != null && parent.timer == 0)
            {
                Helper.Kill(npc);
            }
            return base.PreAI(npc);
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (parent != null)
            {
                drawColor = new Color(100, 0, 200) * 0.5f;
                Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Darkness>());
            }
            base.DrawEffects(npc, ref drawColor);
        }

        public override bool PreNPCLoot(NPC npc)
        {
            if (parent != null)
            {
                return false;
            }
            return base.PreNPCLoot(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            if (parent != null)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                //GameShaders.Misc["StarlightRiver:Distort"].Apply(null);
            }
            return base.PreDraw(npc, spriteBatch, drawColor);
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {
            if (parent != null)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.Transform);
            }
            base.PostDraw(npc, spriteBatch, drawColor);
        }
    }
}*/