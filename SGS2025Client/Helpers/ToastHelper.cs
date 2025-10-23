using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Helpers
{
    public static class ToastHelper
    {
        public static async Task ShowAsync(string message, ToastDuration duration = ToastDuration.Short, double fontSize = 14)
        {
            var toast = Toast.Make(message, duration, fontSize);
            await toast.Show();
        }

        public static async Task ShowSuccessAsync(string message)
        {
            await ShowAsync($"✅ {message}");
        }

        public static async Task ShowErrorAsync(string message)
        {
            await ShowAsync($"❌ {message}", ToastDuration.Long);
        }
    }
}
