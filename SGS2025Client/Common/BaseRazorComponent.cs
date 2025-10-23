using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Common
{
    public class BaseRazorComponent : LayoutComponentBase
    {
        protected bool IsBusy { get; private set; }
        protected string BusyMessage { get; private set; } = "";

        // 🎯 Cấu hình mặc định (có thể override ở page con)
        protected virtual string DefaultBusyMessage => "Vui lòng chờ...";
        protected virtual string DefaultErrorMessage => "Đã xảy ra lỗi!";
        protected virtual ToastDuration DefaultToastDuration => ToastDuration.Short;
        protected virtual double DefaultToastFontSize => 14;

        // 📢 Hàm toast tiện dụng
        protected async Task ShowToastAsync(string message, bool isSuccess = true)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var toast = Toast.Make(message, DefaultToastDuration, DefaultToastFontSize);
            await toast.Show();
        }

        // 🧩 Hàm trung tâm
        protected async Task SafeExecuteAsync(
            Func<Task> action,
            string? successMessage = null,
            string? errorMessage = null,
            Func<Exception, Task>? onError = null,
            Func<Task>? onSuccess = null,
            bool requireConfirm = false,
            string? confirmMessage = null,
            string? busyMessage = null)
        {
            if (requireConfirm)
            {
                // 👉 Nếu bạn có ConfirmDialog riêng, có thể gọi tại đây
                bool confirm = await ConfirmAsync(confirmMessage ?? "Bạn có chắc chắn?");
                if (!confirm)
                    return;
            }

            try
            {
                SetBusy(true, busyMessage ?? DefaultBusyMessage);
                await action();

                if (onSuccess != null)
                    await onSuccess();

                if (!string.IsNullOrWhiteSpace(successMessage))
                    await ShowToastAsync(successMessage, true);
            }
            catch (Exception ex)
            {
                if (onError != null)
                    await onError(ex);

                await ShowToastAsync($"{errorMessage ?? DefaultErrorMessage}\n{ex.Message}", false);
            }
            finally
            {
                SetBusy(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        // 🌀 Gán trạng thái busy để UI có thể binding vào
        protected void SetBusy(bool value, string? message = null)
        {
            IsBusy = value;
            BusyMessage = value ? (message ?? DefaultBusyMessage) : "";
            InvokeAsync(StateHasChanged);
        }

        // ⚡ Overload 1: chỉ action, có toast lỗi mặc định
        protected Task Run(Func<Task> action)
            => SafeExecuteAsync(action, null, null, null, null, false, null, null);

        // ⚡ Overload 2: có busy message
        protected Task RunBusy(Func<Task> action, string? busyMessage = null)
            => SafeExecuteAsync(action, null, null, null, null, false, null, busyMessage);

        // ⚡ Overload 3: có onError và onSuccess
        protected Task RunSafe(
            Func<Task> action,
            Func<Exception, Task>? onError = null,
            Func<Task>? onSuccess = null,
            string? busyMessage = null)
            => SafeExecuteAsync(action, null, null, onError, onSuccess, false, null, busyMessage);
        protected Task RunSafe(Func<Task> action, string busyMessage = "Đang xử lý...")
        {
            return RunSafe(action,
                onError: async ex => await ShowToastAsync($"Lỗi: {ex.Message}"),
                onSuccess: async () => await ShowToastAsync("Thành công!"),
                busyMessage: busyMessage);
        }
        // ⚡ Overload 4: có confirm
        protected Task RunConfirm(Func<Task> action, string confirmMessage, string? busyMessage = null)
            => SafeExecuteAsync(action, null, null, null, null, true, confirmMessage, busyMessage);

        // 💬 Mẫu ConfirmDialog đơn giản (có thể thay bằng popup thật)
        protected virtual Task<bool> ConfirmAsync(string message)
        {
            // ⚠️ Nếu bạn có ConfirmDialog riêng, override hàm này để mở popup.
            Console.WriteLine($"Confirm: {message}");
            return Task.FromResult(true);
        }
    }
}
