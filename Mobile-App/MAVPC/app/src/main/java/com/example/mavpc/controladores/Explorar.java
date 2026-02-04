package com.example.mavpc.controladores;

import android.Manifest;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.Resources;
import android.location.Location;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

// para el formulario de los filtros
import android.widget.ArrayAdapter;
import android.widget.Spinner;
import java.util.ArrayList;
import java.util.List;

// para coger la fecha de hoy
import java.util.Calendar;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.cardview.widget.CardView;
import androidx.constraintlayout.widget.ConstraintLayout;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

// google maps
import com.bumptech.glide.Glide;
import com.example.mavpc.R;
import com.example.mavpc.modelos.Camara;
import com.example.mavpc.modelos.Incidencia;
import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;

// para poder cambiar el color a los marcadores de googlemaps
import com.google.android.gms.maps.model.BitmapDescriptorFactory;

import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.MapStyleOptions;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.tasks.OnSuccessListener;

// navbar
import com.google.android.material.bottomnavigation.BottomNavigationView;

// Imports para Retrofit y JSON
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

// Import para poner marcadores
import com.google.android.gms.maps.model.MarkerOptions;

public class Explorar extends BaseActivity implements OnMapReadyCallback {

    private GoogleMap gMap;
    private Marker marcadorTemporal;
    private BottomNavigationView navbar;
    private Button btnCloseFilter, btnAplicarFiltros, btnCloseMarkerWindow;
    private View darkBackground;
    private CardView cardSearch, btnLayers, btnFiltros, markerWindow, filterWindow;

    // cliente de ubicacion
    private FusedLocationProviderClient fusedLocationClient;

    // codigo para identificar la petición de permisos
    private static final int PERMISSION_REQUEST_CODE = 44;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.explorar);

        // inicializar cliente de ubicacion
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this);

        navbar = findViewById(R.id.bottomNav);
        setupBottomNav();

        cardSearch = findViewById(R.id.cardSearch);

        // configuracion botones y overlays
        btnLayers = findViewById(R.id.btnMapLayers);
        btnLayers.setOnClickListener(v -> {
            if (gMap != null) {
                int tipoActual = gMap.getMapType();
                if (tipoActual == GoogleMap.MAP_TYPE_NORMAL) {
                    gMap.setMapType(GoogleMap.MAP_TYPE_HYBRID);
                } else {
                    gMap.setMapType(GoogleMap.MAP_TYPE_NORMAL);
                }
            }
        });

        markerWindow = findViewById(R.id.markerWindow);
        markerWindow.setOnClickListener(v -> {
            // consumir click para no cerrar
        });

        filterWindow = findViewById(R.id.filterWindow);
        filterWindow.setOnClickListener(v -> {
            // consumir click para no cerrar
        });

        darkBackground = findViewById(R.id.darkBackground);
        darkBackground.setOnClickListener(v -> {
            filterWindow.setVisibility(View.GONE);
            markerWindow.setVisibility(View.GONE);
            darkBackground.setVisibility(View.GONE);
        });

        btnFiltros = findViewById(R.id.btnFiltros);
        btnFiltros.setOnClickListener(v -> {
            filterWindow.setVisibility(View.VISIBLE);
            darkBackground.setVisibility(View.VISIBLE);
        });

        btnCloseFilter = findViewById(R.id.btnCloseFilter);
        btnCloseFilter.setOnClickListener(v -> {
            filterWindow.setVisibility(View.GONE);
            darkBackground.setVisibility(View.GONE);
        });

        btnAplicarFiltros = findViewById(R.id.btnAplicarFiltros);
        btnAplicarFiltros.setOnClickListener(v -> {
            filterWindow.setVisibility(View.GONE);
            darkBackground.setVisibility(View.GONE);
        });

        btnCloseMarkerWindow = findViewById(R.id.btnCerrarDetalles);
        btnCloseMarkerWindow.setOnClickListener(v -> {
            markerWindow.setVisibility(View.GONE);
            darkBackground.setVisibility(View.GONE);
        });

        // inicializar mapa
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map);
        if (mapFragment != null) {
            mapFragment.getMapAsync(this);
        }

        // inicializar spinners de fecha
        inicializarSpinnersFecha();

        // TIENE QUE SER EL FINAL DEL onCreate!! fuerza al Navbar a no tener relleno inferior
        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
    }

    private void mostrarDialogoCrearIncidencia(LatLng latLng) {
        new AlertDialog.Builder(Explorar.this)
                .setTitle("Nueva Incidencia")
                .setMessage("Crear incidencia en este punto")
                .setPositiveButton("Crear", (dialog, which) -> {
                    // se pulsa si -> formulario de creación
                    Intent intent = new Intent(Explorar.this, Reportar.class);

                    // Pasamos las coordenadas
                    intent.putExtra("LATITUD", latLng.latitude);
                    intent.putExtra("LONGITUD", latLng.longitude);

                    intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                    startActivity(intent);
                    overridePendingTransition(0, 0);

                    // borrar el marcador temporal al irnos
                    if (marcadorTemporal != null) {
                        marcadorTemporal.remove();
                        marcadorTemporal = null;
                    }
                })
                .setNegativeButton("Cancelar", (dialog, which) -> {
                    // se pulsa no -> borrar marcador temporal
                    if (marcadorTemporal != null) {
                        marcadorTemporal.remove();
                        marcadorTemporal = null;
                    }
                })
                .show();
    }

    @Override
    public void onMapReady(@NonNull GoogleMap googleMap) {
        gMap = googleMap;

        // estilo oscuro
        try {
            boolean success = googleMap.setMapStyle(
                    MapStyleOptions.loadRawResourceStyle(this, R.raw.map_style_dark));
            if (!success) {
                Log.e("Explorar", "Error al cargar el estilo del mapa");
            }
        } catch (Resources.NotFoundException e) {
            Log.e("Explorar", "No se encuentra el archivo map_style_dark.json", e);
        }

        // interfaz
        gMap.getUiSettings().setZoomControlsEnabled(false);
        gMap.getUiSettings().setCompassEnabled(true);
        gMap.getUiSettings().setMyLocationButtonEnabled(false);

        obtenerUbicacionActual();

        marcarIncidenciasMapa();
        marcarCamarasMapa();

        // mantener pulsado para añadir marcador
        gMap.setOnMapLongClickListener(latLng -> {
            // 1. Si ya había un marcador de selección previo, lo borramos
            if (marcadorTemporal != null) {
                marcadorTemporal.remove();
            }

            // 2. Añadimos un marcador visual donde el usuario pulsó
            marcadorTemporal = gMap.addMarker(new MarkerOptions()
                    .position(latLng)
                    .title("Nueva ubicación")
                    .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_VIOLET))); // Color distinto

            // 3. Mostramos el diálogo de confirmación
            mostrarDialogoCrearIncidencia(latLng);
        });
    }

    private void obtenerUbicacionActual() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.ACCESS_FINE_LOCATION)
                == PackageManager.PERMISSION_GRANTED) {

            gMap.setMyLocationEnabled(true);

            fusedLocationClient.getLastLocation()
                    .addOnSuccessListener(this, new OnSuccessListener<Location>() {
                        @Override
                        public void onSuccess(Location location) {
                            if (location != null) {
                                LatLng ubicacionActual = new LatLng(location.getLatitude(), location.getLongitude());
                                gMap.moveCamera(CameraUpdateFactory.newLatLngZoom(ubicacionActual, 16.5f));
                            } else {
                                LatLng ayuntaIrun = new LatLng(43.338140946267735, -1.7889326356543134);
                                gMap.moveCamera(CameraUpdateFactory.newLatLngZoom(ayuntaIrun, 16.5f));
                            }
                        }
                    });

        } else {
            ActivityCompat.requestPermissions(this,
                    new String[]{Manifest.permission.ACCESS_FINE_LOCATION},
                    PERMISSION_REQUEST_CODE);
        }
    }

    private void marcarIncidenciasMapa() {
        // conf retrofit
        String BASE_URL = "https://mavpc.up.railway.app/api/";

        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();

        ApiService service = retrofit.create(ApiService.class);

        // llamada a la api
        Call<List<Incidencia>> call = service.obtenerIncidenciasHoy();

        call.enqueue(new Callback<List<Incidencia>>() {
            @Override
            public void onResponse(Call<List<Incidencia>> call, Response<List<Incidencia>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    List<Incidencia> lista = response.body();

                    for (Incidencia i : lista) {
                        try {
                            if (i.getLatitude() != null && i.getLongitude() != null) {
                                double lat = Double.parseDouble(i.getLatitude());
                                double lng = Double.parseDouble(i.getLongitude());

                                LatLng posicion = new LatLng(lat, lng);

                                // rojo por defecto y naranja si es una obra o mantenimiento
                                float markerColor = BitmapDescriptorFactory.HUE_RED;

                                if (i.getType() != null) {
                                    String tipoLower = i.getType().toLowerCase();
                                    if (tipoLower.contains("obra") || tipoLower.contains("mantenimiento")) {
                                        // #eb8934 es un naranja. HUE_ORANGE es 30.0f.
                                        markerColor = BitmapDescriptorFactory.HUE_ORANGE;
                                    }
                                }

                                // colocar marcador
                                Marker marker = gMap.addMarker(new MarkerOptions()
                                        .position(posicion)
                                        .title(i.getType())
                                        .snippet(i.getCityTown())
                                        .icon(BitmapDescriptorFactory.defaultMarker(markerColor)));

                                // guardamos la incidencia dentro del marcador para enseñar su informacion
                                if (marker != null) {
                                    marker.setTag(i);
                                }
                            }
                        } catch (NumberFormatException e) {
                            Log.e("API", "Error al convertir coordenadas: " + e.getMessage());
                        }
                    }

                    configurarClickMarcadores();

                    Log.d("API", "Cargadas " + lista.size() + " incidencias.");
                } else {
                    Log.e("API", "Error en respuesta: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<List<Incidencia>> call, Throwable t) {
                Log.e("API", "Fallo de conexión: " + t.getMessage());
                Toast.makeText(Explorar.this, "Error cargando datos", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void configurarClickMarcadores() {
        gMap.setOnMarkerClickListener(marker -> {
            Object tag = marker.getTag();

            if (tag != null) {
                // comprueba si es marcador de incidencia
                if (tag instanceof Incidencia) {
                    Incidencia incidencia = (Incidencia) tag;
                    prepararDetallesIncidencia(incidencia);
                }
                // comrpueba si es marcador de camara
                else if (tag instanceof Camara) {
                    Camara camara = (Camara) tag;
                    prepararDetallesCamara(camara);
                }
            }

            return false; // False = comportamiento por defecto (centrar y abrir globito)
        });
    }

    private void prepararDetallesIncidencia(Incidencia i) {
        // titulo
        String titulo;
        if(i.getType() == null || i.getType().toLowerCase().trim().contains("otro")){
            titulo = "Otro";
        } else{
            titulo = i.getType();
        }

        String info = "";
        String gravedad = i.getLevel();
        if (gravedad != null){
            info += "Gravedad: " + gravedad + "\n" + "\n";
        }
        String causa = i.getCause();
        if (causa != null){
            info += "Causa: " + causa + "\n" + "\n";
        }
        String ciudad = i.getCityTown();
        if (ciudad != null){
            info += "Ciudad: " + ciudad + "\n" + "\n";
        }
        String carretera = i.getRoad();
        if (carretera != null){
            info += "Carretera: " + carretera + "\n" + "\n";
        }
        String direccion = i.getDirection();
        if (direccion != null){
            info += "Dirección: " + direccion + "\n" + "\n";
        }
        String latitud = i.getLatitude();
        if (latitud != null){
            info += "Latitud: " + latitud + "\n" + "\n";
        }
        String longitud = i.getLongitude();
        if (longitud != null){
            info += "Longitud: " + longitud;
        }

        mostrarDetallesGenerico(titulo, info, null);
    }

    private void marcarCamarasMapa() {
        // url base
        String BASE_URL = "https://mavpc.up.railway.app/api/";

        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();

        ApiService service = retrofit.create(ApiService.class);

        Call<List<Camara>> call = service.obtenerCamaras();

        call.enqueue(new Callback<List<Camara>>() {
            @Override
            public void onResponse(Call<List<Camara>> call, Response<List<Camara>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    List<Camara> lista = response.body();

                    for (Camara c : lista) {
                        try {
                            if (c.getLatitude() != null && c.getLongitude() != null) {
                                double lat = Double.parseDouble(c.getLatitude());
                                double lng = Double.parseDouble(c.getLongitude());

                                LatLng posicion = new LatLng(lat, lng);

                                // colocar marcador azul
                                Marker marker = gMap.addMarker(new MarkerOptions()
                                        .position(posicion)
                                        .title(c.getName())
                                        .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_AZURE)));

                                // guardamos la camara dentro del marcador para enseñar su informacion
                                if (marker != null) {
                                    marker.setTag(c);
                                }
                            }
                        } catch (NumberFormatException e) {
                            Log.e("API", "Error con las coordenadas de la cámara: " + e.getMessage());
                        }
                    }

                    configurarClickMarcadores();
                }
            }

            @Override
            public void onFailure(Call<List<Camara>> call, Throwable t) {
                Log.e("API", "Error al cargar las cámaras: " + t.getMessage());
            }
        });
    }

    private void prepararDetallesCamara(Camara c) {
        // Título
        String titulo = c.getName();

        String info = "";

        String carretera = c.getRoad();
        if (carretera != null){
            info += "Carretera: " + carretera + "\n" + "\n";
        }

        String direccion = c.getDirection();
        if (direccion != null){
            info += "Dirección: " + direccion + "\n" + "\n";
        }

        String km = c.getKm();
        if (km != null){
            info += "Kilómetro: " + km + "\n" + "\n";
        }

        String latitude = c.getLatitude();
        if (latitude != null){
            info += "Latitud: " + latitude + "\n" + "\n";
        }

        String longitud = c.getLongitude();
        if (longitud != null){
            info += "Longitud: " + longitud;
        }

        String imagenCamara = c.getUrlImage();

        mostrarDetallesGenerico(titulo, info, imagenCamara);
    }

    private void mostrarDetallesGenerico(String titulo, String descripcion, String urlImagen) {
        TextView tvTituloDetalles = findViewById(R.id.tvTituloDetalles);
        ImageView ivCamara = findViewById(R.id.ivCamara);
        TextView tvInfoDetalles = findViewById(R.id.tvInfoDetalles);

        tvTituloDetalles.setText(titulo);
        tvInfoDetalles.setText(descripcion);

        // obtener los parámetros del layout para modificar el porcentaje
        // casteamos a ConstraintLayout.LayoutParams porque el padre es un ConstraintLayout
        ConstraintLayout.LayoutParams params = (ConstraintLayout.LayoutParams) markerWindow.getLayoutParams();

        if (urlImagen != null && !urlImagen.isEmpty()) {
            // si es una camara
            ivCamara.setVisibility(View.VISIBLE);

            // cargar imagen
            Glide.with(this)
                    .load(urlImagen)
                    .placeholder(R.drawable.ic_launcher_foreground)
                    .into(ivCamara);

            // Cambiar altura al 70%
            params.matchConstraintPercentHeight = 0.6f;

        } else {
            // si es una incidencia
            ivCamara.setVisibility(View.GONE);

            // cambiar altura al 50%
            params.matchConstraintPercentHeight = 0.4f;
        }

        // aplicar los cambios de tamaño al cardview
        markerWindow.setLayoutParams(params);

        // mostrar la ventana y el fondo oscuro
        markerWindow.setVisibility(View.VISIBLE);
        darkBackground.setVisibility(View.VISIBLE);
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == PERMISSION_REQUEST_CODE) {
            if (grantResults.length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                obtenerUbicacionActual();
            } else {
                Toast.makeText(this, "Se necesita permiso para mostrar tu ubicación", Toast.LENGTH_SHORT).show();
            }
        }
    }

    private void setupBottomNav() {
        navbar.setSelectedItemId(R.id.nav_explorar);

        navbar.setOnItemSelectedListener(item -> {
            int id = item.getItemId();

            if (id == R.id.nav_explorar) return true;
            if (id == R.id.nav_favoritos) {
                Intent intent = new Intent(Explorar.this, Favoritos.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_reportar) {
                Intent intent = new Intent(Explorar.this, Reportar.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_perfil) {
                Intent intent = new Intent(Explorar.this, Perfil.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            return false;
        });
    }

    private void inicializarSpinnersFecha() {
        Spinner spinDia = findViewById(R.id.spinDia);
        Spinner spinMes = findViewById(R.id.spinMes);
        Spinner spinAno = findViewById(R.id.spinAno);

        // obtencion de fecha actual
        Calendar calendar = Calendar.getInstance();
        int diaHoy = calendar.get(Calendar.DAY_OF_MONTH);
        int mesHoy = calendar.get(Calendar.MONTH) + 1; // enero es 0, así que sumamos 1
        int anoHoy = calendar.get(Calendar.YEAR);

        // dia (1-31)
        List<String> dias = new ArrayList<>();
        for(int i=1; i<=31; i++) dias.add(String.valueOf(i));

        // mes (1-12)
        List<String> meses = new ArrayList<>();
        for(int i=1; i<=12; i++) meses.add(String.valueOf(i));

        // año (2008-2026)
        List<String> anos = new ArrayList<>();
        for(int i=2008; i<=2026; i++) anos.add(String.valueOf(i));

        // adaptadores
        ArrayAdapter<String> adapterDia = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, dias);
        adapterDia.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinDia.setAdapter(adapterDia);

        ArrayAdapter<String> adapterMes = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, meses);
        adapterMes.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinMes.setAdapter(adapterMes);

        ArrayAdapter<String> adapterAno = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, anos);
        adapterAno.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinAno.setAdapter(adapterAno);

        // seleccion de la fecha actual como default
        if (diaHoy <= 31) spinDia.setSelection(diaHoy - 1);

        if (mesHoy <= 12) spinMes.setSelection(mesHoy - 1);

        int indiceAno = anoHoy - 2008;

        // verificamos que el año actual esté dentro del rango para que no falle
        if (indiceAno > 0 && indiceAno < anos.size()) {
            spinAno.setSelection(indiceAno);
        } else{
            spinAno.setSelection(anos.size() - 1);
        }
    }
}