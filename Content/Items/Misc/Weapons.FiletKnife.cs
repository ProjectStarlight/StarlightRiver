using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    internal class FiletKnife : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Filet Knife");
            Tooltip.SetDefault("egshels update this lol");
        }
        public override void SetDefaults()
        {
            item.damage = 20;
            item.melee = true;
            item.width = 36;
            item.height = 38;
            item.useTime = 25;
            item.useAnimation = 25;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.knockBack = 7.5f;
            item.value = 1000;
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item1;
            item.autoReuse = false;
            item.useTurn = true;
            item.crit = 12;
        }

        public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit)
        {
            if (crit)
            {
                int itemType = -1;
                switch (Main.rand.Next(3))
                {
                    case 0:
                        itemType = ModContent.ItemType<FiletGiblet1>();
                        break;
                    case 1:
                        itemType = ModContent.ItemType<FiletGiblet2>();
                        break;
                    default:
                        itemType = ModContent.ItemType<FiletGiblet3>();
                        break;
                }
                Item.NewItem(target.Center, itemType);

                if (target.GetGlobalNPC<FiletNPC>().DOT < 3)
                    target.GetGlobalNPC<FiletNPC>().DOT += 1;


                Vector2 direction = Vector2.Normalize(target.Center - player.Center);
                for (int j = 0; j < 15; j++)
                {
                    Dust.NewDustPerfect(target.Center, DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0f, 6f), 0, default, 1.5f);
                    Dust.NewDustPerfect(target.Center, DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.NextFloat(0f, 3f), 0, default, 0.8f);
                }
            }
        }
    }
    public class FiletGiblet1 : ModItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Giblet");
            Tooltip.SetDefault("You shouldn't see this");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 24;
            item.maxStack = 1;
        }

        public override bool ItemSpace(Player player) => true;
        public override bool OnPickup(Player player)
        {
            int healAmount = (int)MathHelper.Min(player.statLifeMax2 - player.statLife, 10);
            player.HealEffect(10);
            player.statLife += healAmount;

            player.AddBuff(BuffID.WellFed, 18000);
            Main.PlaySound(SoundID.Grab, (int)player.position.X, (int)player.position.Y);
            return false;
        }
    }

    public class FiletGiblet2 : FiletGiblet1 { }
    public class FiletGiblet3 : FiletGiblet1 { }

    public class FiletNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int DOT = 0;

        public bool hasSword = false;

        public override void SetDefaults(NPC npc)
        {
            if (npc.type == NPCID.BloodZombie && Main.rand.NextBool(30))
                hasSword = true;
            base.SetDefaults(npc);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Color drawColor)
        {     
            if (hasSword)
            {
                Texture2D tex = ModContent.GetTexture(AssetDirectory.MiscItem + "FiletKnifeEmbedded");
                bool facingLeft = npc.direction == -1;

                Vector2 origin = facingLeft ? new Vector2(0, tex.Height) : new Vector2(tex.Width, tex.Height);
                SpriteEffects effects = facingLeft ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
                float rotation = facingLeft ? 0.78f : -0.78f;


                spriteBatch.Draw(tex, npc.Center - Main.screenPosition, null, drawColor, rotation, origin, npc.scale, effects, 0f);
            }
            return base.PreDraw(npc, spriteBatch, drawColor);
        }

        public override void NPCLoot(NPC npc)
        {
            if (hasSword)
                Item.NewItem(npc.Center, ModContent.ItemType<FiletKnife>());
            base.NPCLoot(npc);
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (DOT == 0)
                return;
            if (npc.lifeRegen > 0)
            {
                npc.lifeRegen = 0;
            }
            npc.lifeRegen -= DOT * 3;
            if (damage < DOT)
            {
                damage = DOT;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (DOT != 0)
            {
                if (Main.rand.Next(5) < DOT)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, default, default(Color), 1.25f);
            }

            if (hasSword)
                Dust.NewDustPerfect(npc.Center - new Vector2(npc.spriteDirection * 12, 0), DustID.Blood, Vector2.Zero);
        }
    }
}