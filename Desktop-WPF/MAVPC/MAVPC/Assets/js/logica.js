/**
 * ============================================================
 * MAVPC - MAP ENGINE (WEBVIEW2 CONTEXT)
 * ============================================================
 * Optimizado: 2025
 * Principios: Module Pattern, Event Delegation, DOM Fragments.
 */

"use strict";

// --- GESTIÓN DE ESTADO (State Management) ---
const MapState = {
    map: null,
    data: [], // Source of truth
    layers: {
        camara: null,
        obra: null,
        nieve: null,
        incidencia: null
    },
    config: {
        view: { lat: 43.0, lon: -2.5, zoom: 9 },
        tiles: {
            dark: 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png',
            sat: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}'
        },
        cluster: {
            disableClusteringAtZoom: 15,
            spiderfyOnMaxZoom: true,
            showCoverageOnHover: false,
            maxClusterRadius: 60
        }
    }
};

/**
 * INICIALIZACIÓN
 */
document.addEventListener("DOMContentLoaded", () => {
    initMapBase();
    initClusters();
    initUIControls();
});

// --- 1. CORE DEL MAPA ---

function initMapBase() {
    const darkLayer = L.tileLayer(MapState.config.tiles.dark, { maxZoom: 19 });
    const satLayer = L.tileLayer(MapState.config.tiles.sat, { maxZoom: 19 });

    MapState.map = L.map('map', {
        zoomControl: false,
        attributionControl: false,
        layers: [darkLayer]
    }).setView([MapState.config.view.lat, MapState.config.view.lon], MapState.config.view.zoom);

    L.control.layers({
        "MODO OSCURO": darkLayer,
        "SATÉLITE": satLayer
    }, null, { position: 'bottomright' }).addTo(MapState.map);
}

function initClusters() {
    // Factory para iconos de cluster
    const createClusterIcon = (cssClass) => (cluster) => {
        return L.divIcon({
            html: `<div class="cluster-blob ${cssClass}">${cluster.getChildCount()}</div>`,
            className: 'custom-cluster-icon',
            iconSize: L.point(40, 40)
        });
    };

    // Inicialización de capas
    MapState.layers.camara = L.markerClusterGroup({ ...MapState.config.cluster, iconCreateFunction: createClusterIcon('blob-camara') }).addTo(MapState.map);
    MapState.layers.obra = L.markerClusterGroup({ ...MapState.config.cluster, iconCreateFunction: createClusterIcon('blob-obra') }).addTo(MapState.map);
    MapState.layers.nieve = L.markerClusterGroup({ ...MapState.config.cluster, iconCreateFunction: createClusterIcon('blob-nieve') }).addTo(MapState.map);
    MapState.layers.incidencia = L.markerClusterGroup({ ...MapState.config.cluster, iconCreateFunction: createClusterIcon('blob-incidencia') }).addTo(MapState.map);
}

// --- 2. CONTROLES UI ---

function initUIControls() {
    addFilterControl();
    addSearchControl();
    addNavigationControl();
    addCoordsControl();
    
    L.control.scale({ imperial: false, maxWidth: 100, position: 'bottomleft' }).addTo(MapState.map);
}

// --- Lógica del Buscador (Optimizada) ---
function addSearchControl() {
    const SearchControl = L.Control.extend({
        options: { position: 'topleft' },
        onAdd: function () {
            const container = L.DomUtil.create('div', 'search-wrapper');
            container.innerHTML = `
                <div class="search-container">
                    <i class="fa-solid fa-magnifying-glass search-icon"></i>
                    <input type="text" class="search-input" placeholder="Buscar..." id="txtBuscar" autocomplete="off">
                </div>
                <div class="search-results" id="searchResults" style="display:none;"></div>
            `;
            L.DomEvent.disableClickPropagation(container);
            L.DomEvent.disableScrollPropagation(container);
            return container;
        }
    });
    MapState.map.addControl(new SearchControl());
    
    // Retrasamos binding para asegurar DOM
    setTimeout(bindSearchEvents, 100);
}

function bindSearchEvents() {
    const input = document.getElementById('txtBuscar');
    const resultsDiv = document.getElementById('searchResults');
    if (!input || !resultsDiv) return;

    let debounceTimer;

    input.addEventListener('input', function () {
        clearTimeout(debounceTimer);
        const term = this.value.toLowerCase();

        // DEBOUNCE: Espera 300ms antes de ejecutar la búsqueda
        debounceTimer = setTimeout(() => {
            resultsDiv.innerHTML = '';
            
            if (term.length < 2) {
                resultsDiv.style.display = 'none';
                return;
            }

            const matches = MapState.data.filter(m => m.Title && m.Title.toLowerCase().includes(term)).slice(0, 10);

            if (matches.length > 0) {
                resultsDiv.style.display = 'block';
                const fragment = document.createDocumentFragment(); // Optimización DOM

                matches.forEach(item => {
                    const div = document.createElement('div');
                    div.className = 'result-item';
                    const info = getIconMetadata(item.Type);
                    div.innerHTML = `<i class="fa-solid ${info.icon}" style="color:${info.color}"></i> ${item.Title}`;
                    
                    div.onclick = () => {
                        input.value = item.Title;
                        resultsDiv.style.display = 'none';
                        MapState.map.flyTo([item.Lat, item.Lon], 16, { duration: 1.5 });
                    };
                    fragment.appendChild(div);
                });
                resultsDiv.appendChild(fragment);
            } else {
                resultsDiv.style.display = 'none';
            }
        }, 300);
    });

    document.addEventListener('click', (e) => {
        if (e.target !== input) resultsDiv.style.display = 'none';
    });
}

// --- Lógica de Filtros ---
function addFilterControl() {
    const FilterControl = L.Control.extend({
        options: { position: 'topright' },
        onAdd: function () {
            const div = L.DomUtil.create('div', 'filter-panel');
            div.innerHTML = `
                <div class="filter-title">VISUALIZACIÓN</div>
                ${generateFilterHtml('camara', 'Cámaras', 'fa-video', '#00F0FF')}
                ${generateFilterHtml('obra', 'Obras', 'fa-person-digging', '#FFA500')}
                ${generateFilterHtml('nieve', 'Meteo', 'fa-snowflake', '#FFF')}
                ${generateFilterHtml('incidencia', 'Incidencias', 'fa-triangle-exclamation', '#FF003C')}
            `;
            L.DomEvent.disableClickPropagation(div);
            return div;
        }
    });
    MapState.map.addControl(new FilterControl());
}

function generateFilterHtml(type, label, icon, color) {
    // Usamos onclick inline apuntando a la función global expuesta abajo
    return `
        <div class="filter-item active" id="btn-${type}" onclick="toggleLayer('${type}')">
            <div class="filter-check"><i class="fa-solid fa-check"></i></div>
            <i class="fa-solid ${icon}" style="margin-right:8px; color:${color}; width:15px; text-align:center;"></i>
            <span>${label}</span>
        </div>`;
}

// --- Controles Standard ---
function addNavigationControl() {
    const NavControl = L.Control.extend({
        options: { position: 'bottomleft' },
        onAdd: () => {
            const container = L.DomUtil.create('div', 'nav-bar');
            container.innerHTML = `
                <div class="nav-btn" id="zoomIn"><i class="fa-solid fa-plus"></i></div>
                <div class="nav-btn" id="zoomOut"><i class="fa-solid fa-minus"></i></div>
                <div class="nav-btn" id="resetMap"><i class="fa-solid fa-crosshairs"></i></div>
            `;
            L.DomEvent.disableClickPropagation(container);

            // Bind events asynchronously
            setTimeout(() => {
                document.getElementById('zoomIn').onclick = () => MapState.map.zoomIn();
                document.getElementById('zoomOut').onclick = () => MapState.map.zoomOut();
                document.getElementById('resetMap').onclick = () => MapState.map.setView(
                    [MapState.config.view.lat, MapState.config.view.lon], MapState.config.view.zoom
                );
            }, 0);
            return container;
        }
    });
    MapState.map.addControl(new NavControl());
}

function addCoordsControl() {
    const CoordsControl = L.Control.extend({
        options: { position: 'bottomright' },
        onAdd: () => {
            const div = L.DomUtil.create('div', 'telemetry-bar');
            div.id = 'coords-display';
            div.textContent = 'LAT: -- LON: --';
            return div;
        }
    });
    MapState.map.addControl(new CoordsControl());

    MapState.map.on('mousemove', (e) => {
        const display = document.getElementById('coords-display');
        if (display) display.textContent = `LAT: ${e.latlng.lat.toFixed(4)}  LON: ${e.latlng.lng.toFixed(4)}`;
    });
}

// --- 3. GESTIÓN DE DATOS (API EXTERNA) ---

/**
 * Función expuesta globalmente para ser llamada desde C# (WebView2)
 * @param {string} jsonItems - Cadena JSON serializada
 */
window.CargarMarcadores = function(jsonItems) {
    if (!MapState.map) return;

    try {
        MapState.data = JSON.parse(jsonItems);

        // Limpieza de capas
        Object.values(MapState.layers).forEach(layer => layer.clearLayers());

        // Procesamiento
        MapState.data.forEach(item => {
            const style = getIconMetadata(item.Type);

            const icon = L.divIcon({
                className: '',
                html: `<div class="custom-pin ${style.css}">${style.html}</div>`,
                iconSize: [30, 30],
                iconAnchor: [15, 15],
                popupAnchor: [0, -20]
            });

            const marker = L.marker([item.Lat, item.Lon], { icon: icon });
            marker.bindTooltip(item.Title, { direction: 'top', className: 'custom-tooltip', offset: [0, -20] });

            marker.on('click', () => {
                marker.closeTooltip();
                // Comunicación asíncrona segura con C#
                if (window.chrome?.webview) {
                    setTimeout(() => window.chrome.webview.postMessage(JSON.stringify(item)), 50);
                }
            });

            // Distribución a capas
            const targetLayer = MapState.layers[style.group] || MapState.layers.incidencia;
            targetLayer.addLayer(marker);
        });

    } catch (error) {
        console.error("MAVPC Error: Fallo al procesar marcadores.", error);
    }
};

/**
 * Función global para los botones de filtro
 */
window.toggleLayer = function(type) {
    const btn = document.getElementById(`btn-${type}`);
    const layer = MapState.layers[type];
    if (!layer || !btn) return;

    if (btn.classList.contains('active')) {
        btn.classList.remove('active');
        MapState.map.removeLayer(layer);
    } else {
        btn.classList.add('active');
        MapState.map.addLayer(layer);
    }
};

// --- HELPERS ---

function getIconMetadata(type) {
    const t = (type || "").toLowerCase();
    
    if (t.includes('camara')) 
        return { html: '<i class="fa-solid fa-video"></i>', icon: 'fa-video', color: '#00F0FF', css: 'pin-camara', group: 'camara' };
    
    if (t.includes('obra')) 
        return { html: '<i class="fa-solid fa-person-digging"></i>', icon: 'fa-person-digging', color: '#FFA500', css: 'pin-obra', group: 'obra' };
    
    if (t.match(/nieve|meteo|hielo|lluvia/)) 
        return { html: '<i class="fa-solid fa-snowflake"></i>', icon: 'fa-snowflake', color: '#FFF', css: 'pin-nieve', group: 'nieve' };
    
    return { html: '<i class="fa-solid fa-triangle-exclamation"></i>', icon: 'fa-triangle-exclamation', color: '#FF003C', css: 'pin-incidencia', group: 'incidencia' };
}