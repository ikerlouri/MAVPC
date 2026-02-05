// Variables globales
var map;
var allMarkersData = []; // Guardamos los datos puros para buscar

// Grupos de capas (Ahora son Clusters)
var layerCamaras, layerObras, layerNieve, layerIncidencias;

const INITIAL_VIEW = { lat: 43.0, lon: -2.5, zoom: 9 };

document.addEventListener("DOMContentLoaded", function () {

    // 1. CAPAS BASE
    var capaOscura = L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', { maxZoom: 19 });
    var capaSatelite = L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', { maxZoom: 19 });

    // 2. INICIALIZAR MAPA
    map = L.map('map', {
        zoomControl: false,
        attributionControl: false,
        layers: [capaOscura]
    }).setView([INITIAL_VIEW.lat, INITIAL_VIEW.lon], INITIAL_VIEW.zoom);

    // 3. INICIALIZAR GRUPOS CON CLUSTERING (MODIFICADO PARA COLORES)
    
    // Configuración base
    var baseClusterOptions = {
        disableClusteringAtZoom: 15, 
        spiderfyOnMaxZoom: true,     
        showCoverageOnHover: false,  
        maxClusterRadius: 60         
    };

    // Función para generar la bola del color correcto
    function crearIconoCluster(claseCss) {
        return function(cluster) {
            return L.divIcon({
                html: '<div class="cluster-blob ' + claseCss + '">' + cluster.getChildCount() + '</div>',
                className: 'custom-cluster-icon', // Clase vacía para quitar bordes default
                iconSize: L.point(40, 40)
            });
        };
    }

    // AQUI APLICAMOS LA LÓGICA DE COLORES POR GRUPO
    layerCamaras = L.markerClusterGroup({
        ...baseClusterOptions,
        iconCreateFunction: crearIconoCluster('blob-camara') // Azul
    }).addTo(map);

    layerObras = L.markerClusterGroup({
        ...baseClusterOptions,
        iconCreateFunction: crearIconoCluster('blob-obra')   // Naranja
    }).addTo(map);

    layerNieve = L.markerClusterGroup({
        ...baseClusterOptions,
        iconCreateFunction: crearIconoCluster('blob-nieve')  // Blanco
    }).addTo(map);

    layerIncidencias = L.markerClusterGroup({
        ...baseClusterOptions,
        iconCreateFunction: crearIconoCluster('blob-incidencia') // Rojo
    }).addTo(map);


    // --- CONTROLES (ESTO SIGUE IGUAL) ---

    // A. Selector Mapa/Satelite
    L.control.layers({ "MODO OSCURO": capaOscura, "SATÉLITE": capaSatelite }, null, { position: 'bottomright' }).addTo(map);

    // B. Panel Filtros
    addFilterControl();

    // C. Buscador Dinámico
    addSearchControl();

    // D. Barra de Navegación
    addNavigationControl();

    // E. Escala
    L.control.scale({ imperial: false, maxWidth: 100, position: 'bottomleft' }).addTo(map);

    // F. Coordenadas
    addCoordsControl();
});


// --- FUNCIONES DE CREACIÓN DE CONTROLES ---

function addSearchControl() {
    var SearchControl = L.Control.extend({
        options: { position: 'topleft' },
        onAdd: function () {
            var container = L.DomUtil.create('div', 'search-wrapper');
            container.style.position = 'relative';

            var searchBox = L.DomUtil.create('div', 'search-container', container);
            searchBox.innerHTML = `
                <i class="fa-solid fa-magnifying-glass search-icon"></i>
                <input type="text" class="search-input" placeholder="Buscar cámara, obra..." id="txtBuscar" autocomplete="off">
            `;

            var resultsBox = L.DomUtil.create('div', 'search-results', container);
            resultsBox.id = 'searchResults';

            L.DomEvent.disableClickPropagation(container);
            L.DomEvent.disableScrollPropagation(container);

            return container;
        }
    });
    map.addControl(new SearchControl());

    setTimeout(() => {
        var input = document.getElementById('txtBuscar');
        var resultsDiv = document.getElementById('searchResults');

        if (!input) return;

        input.addEventListener('input', function () {
            var texto = this.value.toLowerCase();
            resultsDiv.innerHTML = '';

            if (texto.length < 2) {
                resultsDiv.style.display = 'none';
                return;
            }

            var encontrados = allMarkersData.filter(m => m.Title.toLowerCase().includes(texto)).slice(0, 10);

            if (encontrados.length > 0) {
                resultsDiv.style.display = 'block';
                encontrados.forEach(item => {
                    var div = document.createElement('div');
                    div.className = 'result-item';

                    var icon = 'fa-circle';
                    var color = '#fff';
                    var t = item.Type.toLowerCase();
                    if (t.includes('camara')) { icon = 'fa-video'; color = '#00F0FF'; }
                    else if (t.includes('obra')) { icon = 'fa-person-digging'; color = '#FFA500'; }
                    else if (t.includes('nieve')) { icon = 'fa-snowflake'; color = '#FFF'; }
                    else { icon = 'fa-triangle-exclamation'; color = '#FF003C'; }

                    div.innerHTML = `<i class="fa-solid ${icon}" style="color:${color}"></i> ${item.Title}`;

                    div.onclick = function () {
                        input.value = item.Title;
                        resultsDiv.style.display = 'none';
                        map.flyTo([item.Lat, item.Lon], 16, { duration: 1.5 });
                    };
                    resultsDiv.appendChild(div);
                });
            } else {
                resultsDiv.style.display = 'none';
            }
        });

        document.addEventListener('click', function (e) {
            if (e.target !== input && e.target !== resultsDiv) {
                resultsDiv.style.display = 'none';
            }
        });

    }, 500);
}

function addNavigationControl() {
    var NavControl = L.Control.extend({
        options: { position: 'bottomleft' },
        onAdd: function (map) {
            var container = L.DomUtil.create('div', 'nav-bar');

            var btnIn = L.DomUtil.create('div', 'nav-btn', container);
            btnIn.innerHTML = '<i class="fa-solid fa-plus"></i>';
            btnIn.onclick = function () { map.zoomIn(); };

            var btnOut = L.DomUtil.create('div', 'nav-btn', container);
            btnOut.innerHTML = '<i class="fa-solid fa-minus"></i>';
            btnOut.onclick = function () { map.zoomOut(); };

            var btnReset = L.DomUtil.create('div', 'nav-btn', container);
            btnReset.innerHTML = '<i class="fa-solid fa-crosshairs"></i>';
            btnReset.title = "Centrar Mapa";
            btnReset.onclick = function () {
                map.setView([INITIAL_VIEW.lat, INITIAL_VIEW.lon], INITIAL_VIEW.zoom);
            };

            L.DomEvent.disableClickPropagation(container);
            return container;
        }
    });
    map.addControl(new NavControl());
}

function addCoordsControl() {
    var CoordsControl = L.Control.extend({
        options: { position: 'bottomright' },
        onAdd: function () {
            var div = L.DomUtil.create('div', 'telemetry-bar');
            div.id = 'coords-display';
            div.innerHTML = 'LAT: -- LON: --';
            return div;
        }
    });
    map.addControl(new CoordsControl());

    map.on('mousemove', function (e) {
        var c = document.getElementById('coords-display');
        if (c) c.innerHTML = `LAT: ${e.latlng.lat.toFixed(4)}  LON: ${e.latlng.lng.toFixed(4)}`;
    });
}

function addFilterControl() {
    var FilterControl = L.Control.extend({
        options: { position: 'topright' },
        onAdd: function () {
            var div = L.DomUtil.create('div', 'filter-panel');
            div.innerHTML = `
                <div class="filter-title">VISUALIZACIÓN</div>
                ${crearFiltroHTML('btn-cam', 'Cámaras', 'fa-video', '#00F0FF')}
                ${crearFiltroHTML('btn-obr', 'Obras', 'fa-person-digging', '#FFA500')}
                ${crearFiltroHTML('btn-met', 'Meteo', 'fa-snowflake', '#FFF')}
                ${crearFiltroHTML('btn-inc', 'Incidencias', 'fa-triangle-exclamation', '#FF003C')}
            `;
            L.DomEvent.disableClickPropagation(div);
            return div;
        }
    });
    map.addControl(new FilterControl());
}

// --- LOGICA DE DATOS ---

function CargarMarcadores(jsonItems) {
    if (!map) return;

    allMarkersData = JSON.parse(jsonItems);

    layerCamaras.clearLayers();
    layerObras.clearLayers();
    layerNieve.clearLayers();
    layerIncidencias.clearLayers();

    allMarkersData.forEach(function (item) {
        var style = getIconHtml(item.Type);

        // Icono personalizado
        var myIcon = L.divIcon({
            className: '',
            html: `<div class="custom-pin ${style.css}">${style.html}</div>`,
            iconSize: [30, 30],
            iconAnchor: [15, 15],
            popupAnchor: [0, -20]
        });

        // Marcador
        var marker = L.marker([item.Lat, item.Lon], { icon: myIcon });

        // Tooltip
        marker.bindTooltip(item.Title, { permanent: false, direction: 'top', className: 'custom-tooltip', offset: [0, -20] });

        // Evento Click -> Enviar a C#
        marker.on('click', function () {
            this.closeTooltip();
            setTimeout(function () {
                if (window.chrome && window.chrome.webview) window.chrome.webview.postMessage(JSON.stringify(item));
            }, 100);
        });

        // Añadir al Cluster correspondiente
        if (style.group === 'camara') layerCamaras.addLayer(marker);
        else if (style.group === 'obra') layerObras.addLayer(marker);
        else if (style.group === 'nieve') layerNieve.addLayer(marker);
        else layerIncidencias.addLayer(marker);
    });
}

function crearFiltroHTML(id, texto, iconClass, color) {
    return `<div class="filter-item active" id="${id}" onclick="toggleLayer('${id}')">
        <div class="filter-check"><i class="fa-solid fa-check"></i></div>
        <i class="fa-solid ${iconClass}" style="margin-right:8px; color:${color}; width:15px; text-align:center;"></i>
        <span>${texto}</span>
    </div>`;
}

function toggleLayer(id) {
    var el = document.getElementById(id);
    var layer;
    if (id === 'btn-cam') layer = layerCamaras;
    else if (id === 'btn-obr') layer = layerObras;
    else if (id === 'btn-met') layer = layerNieve;
    else layer = layerIncidencias;

    if (el.classList.contains('active')) {
        el.classList.remove('active');
        map.removeLayer(layer);
    } else {
        el.classList.add('active');
        map.addLayer(layer);
    }
}

function getIconHtml(type) {
    type = (type || "").toLowerCase();
    
    // 1. Cámaras
    if (type.includes('camara')) 
        return { html: '<i class="fa-solid fa-video"></i>', css: 'pin-camara', group: 'camara' };
    
    // 2. Obras
    if (type.includes('obra')) 
        return { html: '<i class="fa-solid fa-person-digging"></i>', css: 'pin-obra', group: 'obra' };
    
    // 3. Meteo (Nieve, Hielo, Invernal, Montaña...)
    // Ahora atrapamos "meteo" (que viene del C#) y también palabras clave por seguridad
    if (type.includes('nieve') || type.includes('hielo') || 
        type.includes('meteo') || type.includes('invernal') || 
        type.includes('montaña') || type.includes('lluvia')) 
        return { html: '<i class="fa-solid fa-snowflake"></i>', css: 'pin-nieve', group: 'nieve' };
    
    // 4. Default -> Incidencias (Rojo)
    return { html: '<i class="fa-solid fa-triangle-exclamation"></i>', css: 'pin-incidencia', group: 'otro' };
}