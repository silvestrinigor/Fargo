window.fargoBarcode = {
    render: function (canvasId, bcid, text) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !window.bwipjs) return false;

        try {
            bwipjs.toCanvas(canvas, {
                bcid: bcid,
                text: text,
                scale: 3,
                height: 10,
                includetext: true,
                textxalign: 'center'
            });
            return true;
        } catch (e) {
            console.error('barcode render error for ' + canvasId + ':', e);
            return false;
        }
    }
};
