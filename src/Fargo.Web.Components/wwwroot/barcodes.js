window.fargoBarcode = {
    render: function (canvasId, bcid, text) {
        var canvas = document.getElementById(canvasId);
        if (!canvas) return;
        try {
            bwipjs.toCanvas(canvas, {
                bcid: bcid,
                text: text,
                scale: 3,
                height: 10,
                includetext: true,
                textxalign: 'center'
            });
        } catch (e) {
            console.error('barcode render error for ' + canvasId + ':', e);
        }
    }
};
