namespace PopStudio.Avalonia.Drop
{
    internal static class PathExt
    {
        public static readonly string[] Package = { ".dz", ".rsb", ".pak", ".arcv" };

        public static string PackageByIndex(int index) => index switch
        {
            0 => ".dz",
            1 => ".rsb",
            2 => ".pak",
            3 => ".arcv",
            _ => ".dz"
        };

        public static string TextureByIndex(int cmode) => cmode switch
        {
            0 => ".ptx",
            1 => ".cdat",
            2 => ".tex",
            3 => ".txz",
            4 => ".tex",
            5 => ".ptx",
            6 => ".ptx",
            7 => ".ptx",
            8 => ".xnb",
            _ => ".ptx"
        };

        public static string PamByIndex(int mode) => mode switch
        {
            0 => ".pam",
            1 => ".pam.json",
            2 => ".xfl",
            _ => ".pam"
        };

        public static string ReanimByIndex(int mode) => mode switch
        {
            0 => ".reanim.compiled",
            1 => ".reanim.compiled",
            2 => ".reanim.compiled",
            3 => ".xnb",
            4 => ".reanim.compiled",
            5 => ".reanim.compiled",
            6 => ".reanim.json",
            7 => ".reanim",
            8 => ".xfl",
            9 => ".fla",
            10 => ".fla",
            _ => ".reanim"
        };

        public static string ParticlesByIndex(int mode) => mode switch
        {
            0 => ".xml.compiled",
            1 => ".xml.compiled",
            2 => ".xml.compiled",
            3 => ".xnb",
            4 => ".xml.compiled",
            5 => ".xml.compiled",
            6 => ".xml.json",
            7 => ".xml",
            _ => ".xml"
        };

        public static string TrailByIndex(int mode) => mode switch
        {
            0 => ".trail.compiled",
            1 => ".trail.compiled",
            2 => ".trail.compiled",
            3 => ".xnb",
            4 => ".trail.compiled",
            5 => ".trail.compiled",
            6 => ".trail.json",
            7 => ".trail",
            _ => ".trail"
        };
    }
}
