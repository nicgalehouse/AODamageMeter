namespace AODamageMeter.UI.Helpers
{
    public static class ProfessionExtensions
    {
        public static string GetIconPath(this Profession profession)
            => profession == null ? $"/Icons/NPC.png"
            : $"/Icons/{profession.GetType().Name}.png";

        public static string GetColorHexCode(this Profession profession)
            => profession == null ? GetColorHexCode(Profession.Unknown)
            : $"#{profession.Color.R:X2}{profession.Color.G:X2}{profession.Color.B:X2}";
    }
}
