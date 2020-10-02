namespace Vurdalakov.LoupedeckCtTool
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public static class Extensions
    {
        public static Boolean ContainsNoCase(this String @this, String that) => @this.IndexOf(that, StringComparison.OrdinalIgnoreCase) >= 0;

        public static FileVersionInfo GetFileVersionInfo(this Assembly assembly) => FileVersionInfo.GetVersionInfo(assembly.Location);
    }
}
