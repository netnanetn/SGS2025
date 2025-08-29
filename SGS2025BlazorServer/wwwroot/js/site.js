function printSilent(base64String) {
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

function saveAsFile(filename, base64String) {
    const link = document.createElement('a');
    link.href = 'data:application/pdf;base64,' + base64String;
    link.download = filename;
    link.click();
}