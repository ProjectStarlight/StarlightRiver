using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Vitric
{
    class GlassReplica : GlobalItem
    {
        public override bool InstancePerEntity => true;

        public override bool CloneNewInstances => true;

        public override bool NeedsSaving(Item item) => isReplica;

        public bool isReplica;
        private bool firstTime = true;

        public override TagCompound Save(Item item)
        {
            return new TagCompound
            {
                ["isReplica"] = isReplica
            };
        }

        public override void Load(Item item, TagCompound tag)
        {
            isReplica = tag.GetBool("isReplica");
            firstTime = false;
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (isReplica)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, Filters.Scene["VitricReplicaItem"].GetShader().Shader, Main.UIScaleMatrix);
                Filters.Scene["VitricReplicaItem"].GetShader().Shader.Parameters["uTime"].SetValue(StarlightWorld.rottime);
            }

            return true;
        }

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (isReplica)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
            }
        }

        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (isReplica)
            {
                if (firstTime)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                    var tex = ModContent.GetTexture("StarlightRiver/Assets/RiftCrafting/Glow1");
                    float scale1 = Main.itemTexture[item.type].Size().Length() / tex.Size().Length();
                    var color = new Color(180, 240, 255);

                    spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, color * 0.5f, StarlightWorld.rottime, tex.Size() / 2, 2 * scale1, 0, 0);
                    spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, color * 0.3f, -StarlightWorld.rottime, tex.Size() / 2, 2.5f * scale1, 0, 0);
                    spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, color * 0.8f, StarlightWorld.rottime * 2, tex.Size() / 2, 1.2f * scale1, 0, 0);
                }

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, Filters.Scene["VitricReplicaItem"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);
                Filters.Scene["VitricReplicaItem"].GetShader().Shader.Parameters["uTime"].SetValue(StarlightWorld.rottime);
            }

            return true;
        }

        public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (isReplica)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
            }
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (isReplica)
            {
                tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.mod == "Terraria").text = "Replica " + item.Name;
            }
        }

        public override bool OnPickup(Item item, Player player)
        {
            firstTime = false;
            return true;
        }
    }
}
