// Variables globales
var map;
var markersLayer;

// Inicialización del mapa
document.addEventListener("DOMContentLoaded", function () {

    // Configuración del mapa (Centro Euskadi)
    map = L.map('map', { zoomControl: false }).setView([43.0, -2.5], 9);

    // Capa base oscura (CartoDB)
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '© OpenStreetMap, © CARTO',
        maxZoom: 19
    }).addTo(map);

    // Capa para los marcadores
    markersLayer = L.layerGroup().addTo(map);
});

// Función para obtener el HTML del icono según el tipo
function getIconHtml(type) {
    type = (type || "").toLowerCase();

    if (type.includes('camara'))
        return { html: '<i class="fa-solid fa-video"></i>', css: 'pin-camara' };

    if (type.includes('obra'))
        return { html: '<i class="fa-solid fa-person-digging"></i>', css: 'pin-obra' };

    if (type.includes('nieve') || type.includes('hielo'))
        return { html: '<i class="fa-solid fa-snowflake"></i>', css: 'pin-nieve' };

    // Default: Incidencia
    return { html: '<i class="fa-solid fa-triangle-exclamation"></i>', css: 'pin-incidencia' };
}

// Función PRINCIPAL llamada desde C#
function CargarMarcadores(jsonItems) {
    if (!map) return;

    markersLayer.clearLayers();
    var items = JSON.parse(jsonItems);

    items.forEach(function (item) {
        var style = getIconHtml(item.Type);

        var myIcon = L.divIcon({
            className: '', // Dejamos esto vacío para que no meta estilos de leaflet por defecto
            html: `<div class="custom-pin ${style.css}">${style.html}</div>`,
            iconSize: [30, 30],
            iconAnchor: [15, 15],
            popupAnchor: [0, -20]
        });

        var marker = L.marker([item.Lat, item.Lon], { icon: myIcon });

        // --- CAMBIO: USAR TOOLTIP (Hover) EN LUGAR DE POPUP (Click) ---
        marker.bindTooltip(item.Title, {
            permanent: false,      // Solo visible al pasar el ratón
            direction: 'top',      // Arriba del icono
            className: 'custom-tooltip', // Clase definida en el CSS nuevo
            offset: [0, -20],      // Un poco separado hacia arriba
            opacity: 1
        });

        // --- EVENTO CLICK ---
        // Al hacer click, SOLO avisamos a C#
        marker.on('click', function () {
            this.closeTooltip();
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage(JSON.stringify(item));
            }
        });

        markersLayer.addLayer(marker);
    });
}