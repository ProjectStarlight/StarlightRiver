using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Dragons
{
    public enum Roar
    {
        normal = 0,
        scalie = 1
    };

    public enum GrowthStage
    {
        egg = 0,
        baby = 1
    }

    public struct DragonData
    {
        public Color hornColor;
        public Color scaleColor;
        public Color bellyColor;
        public Color eyeColor;

        public string name;

        public Roar roar;

        public GrowthStage stage;
        public Item gear;

        public TagCompound Save()
        {
            return new TagCompound()
            {
                [nameof(hornColor)] = hornColor,
                [nameof(scaleColor)] = scaleColor,
                [nameof(bellyColor)] = bellyColor,
                [nameof(eyeColor)] = eyeColor,

                [nameof(name)] = name,

                [nameof(roar)] = (int)roar,

                [nameof(stage)] = (int)stage,
                [nameof(gear)] = gear
            };
        }

        public void Load(TagCompound tag)
        {
            hornColor = tag.Get<Color>(nameof(hornColor));
            scaleColor = tag.Get<Color>(nameof(scaleColor));
            bellyColor = tag.Get<Color>(nameof(bellyColor));
            eyeColor = tag.Get<Color>(nameof(eyeColor));

            name = tag.GetString(nameof(name));

            roar = (Roar)tag.GetInt(nameof(roar));

            stage = (GrowthStage)tag.GetInt(nameof(stage));
            gear = tag.Get<Item>(nameof(gear));
        }

        public void SetDefault()
        {
            hornColor = new Color(180, 140, 60);
            scaleColor = new Color(255, 50, 50);
            bellyColor = new Color(250, 220, 130);
            eyeColor = new Color(100, 100, 220);

            name = "Draggy the Dragon";

            roar = Roar.normal;

            stage = GrowthStage.egg;
            gear = null;
        }
    }

    public class DragonHandler : ModPlayer
    {
        public DragonData data = new DragonData();
        public bool DragonMounted => player.mount.Type == MountType<YoungDragon>();
        public bool jumpAgainDragon = true;

        public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath)
        {
            if (!mediumcoreDeath)
            {
                Item egg = new Item();
                egg.SetDefaults(ItemType<Items.Dragons.Egg>());
                items.Add(egg);
            }
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                [nameof(data)] = data.Save(),
            };
        }

        public override void Load(TagCompound tag)
        {
            data.Load(tag.GetCompound(nameof(data)));
            if (data.name == null) data.SetDefault(); //safety check
        }
    }
}