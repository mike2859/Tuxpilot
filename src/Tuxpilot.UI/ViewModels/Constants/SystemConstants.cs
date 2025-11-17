using System;

namespace Tuxpilot.UI.ViewModels.Constants;

public static class SystemConstants
{
    public static class HealthThresholds
    {
        public const double Critical = 85.0;
        public const double Warning = 70.0;
    }
    
    public static class Colors
    {
        public const string Success = "#10B981";  // Vert
        public const string Warning = "#F59E0B";  // Orange
        public const string Critical = "#EF4444"; // Rouge
        public const string Info = "#3B82F6";     // Bleu
    }
    
    public static class RefreshIntervals
    {
        public static readonly TimeSpan AutoRefresh = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan UiUpdate = TimeSpan.FromSeconds(1);
    }
}