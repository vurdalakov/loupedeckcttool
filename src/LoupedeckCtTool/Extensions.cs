namespace Vurdalakov.LoupedeckCtTool
{
    using System;

    public static class Extensions
    {
        public static Boolean ContainsNoCase(this String @this, String that)
        {
            return @this.IndexOf(that, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
