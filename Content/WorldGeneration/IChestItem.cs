namespace StarlightRiver.Content.WorldGeneration
{
    public interface IChestItem
    {
        int Stack { get; }

        ChestRegionFlags Regions { get; }
    }
}
