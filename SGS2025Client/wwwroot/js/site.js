function printSilent2(base64String) {
    const iframe = document.createElement('iframe');
    iframe.style.display = 'none';
    iframe.src = 'data:application/pdf;base64,' + base64String;
    document.body.appendChild(iframe);

    iframe.onload = function () {
        try {
            iframe.contentWindow.print();
        } catch (e) {
            console.error('Lỗi khi in ngầm:', e);
            window.print();
        }
        setTimeout(() => document.body.removeChild(iframe), 1000);
    };
}
function printSilent(base64String) {
    const byteCharacters = atob(base64String);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: "application/pdf" });
    const blobUrl = URL.createObjectURL(blob);

    const iframe = document.createElement("iframe");
    iframe.style.display = "none";
    iframe.src = blobUrl;
    document.body.appendChild(iframe);

    iframe.onload = function () {
        try {
            iframe.contentWindow.focus();
            iframe.contentWindow.print();
        } catch (e) {
            console.error("Lỗi khi in:", e);
        }
        setTimeout(() => {
            URL.revokeObjectURL(blobUrl); // giải phóng memory
            document.body.removeChild(iframe);
        }, 2000);
    };
}
function saveAsFile(filename, base64String) {
    const link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64String;
    link.download = filename;
    link.click();
}

window.drawFrameOnCanvas = (canvasId, base64) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    const img = new Image();
    img.onload = () => {
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
    };
    img.src =  base64;
};
window.drawFrameOnCanvasByte = async (canvasId, bytes) => {
    try {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext("2d");

        // Chuyển byte[] từ .NET thành Uint8Array
        const uint8Array = new Uint8Array(bytes);

        // Tạo Blob hình ảnh (JPEG/PNG tuỳ bạn nén từ server)
        const blob = new Blob([uint8Array], { type: "image/jpeg" });

        // Giải mã hình ảnh bằng GPU thread riêng
        const bitmap = await createImageBitmap(blob);

        // Vẽ lên canvas
        ctx.drawImage(bitmap, 0, 0, canvas.width, canvas.height);

        // Giải phóng bộ nhớ GPU
        bitmap.close();
    } catch (err) {
        console.error("drawFrameOnCanvas error:", err);
    }
};