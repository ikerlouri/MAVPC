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
import android.widget.Toast;

// para el formulario de los filtros
import android.widget.ArrayAdapter;
import android.widget.Spinner;
import java.util.ArrayList;
import java.util.List;

// para coger la fecha de hoy
import java.util.Calendar;

import androidx.annotation.NonNull;
import androidx.cardview.widget.CardView;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

// google maps
import com.example.mavpc.R;
import com.example.mavpc.modelos.Camara;
import com.example.mavpc.modelos.Incidencia;
import com.google.android.gms.location.FusedLocationProviderClient;
import com.google.android.gms.location.LocationServices;
import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.MapStyleOptions;
import com.google.android.gms.tasks.OnSuccessListener;

// para cambiar el color a los marcadores
import com.google.android.gms.maps.model.BitmapDescriptorFactory;

// navbar
import com.google.android.material.bottomnavigation.BottomNavigationView;

// Imports para Retrofit y JSON
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import retrofit2.http.GET;

// Import para poner marcadores
import com.google.android.gms.maps.model.MarkerOptions;

public class Explorar extends BaseActivity implements OnMapReadyCallback {

    private GoogleMap gMap;
    private BottomNavigationView navbar;
    private CardView cardSearch, btnLayers, btnFiltros;

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

        CardView filterWindow = findViewById(R.id.filterWindow);
        filterWindow.setOnClickListener(v -> {
            // Consumir click para no cerrar
        });

        View filterBackground = findViewById(R.id.filterBackground);
        filterBackground.setOnClickListener(v -> {
            filterWindow.setVisibility(View.GONE);
            filterBackground.setVisibility(View.GONE);
        });

        btnFiltros = findViewById(R.id.btnFiltros);
        btnFiltros.setOnClickListener(v -> {
            filterWindow.setVisibility(View.VISIBLE);
            filterBackground.setVisibility(View.VISIBLE);
        });

        Button btnCloseFilter = findViewById(R.id.btnCloseFilter);
        btnCloseFilter.setOnClickListener(v -> {
            filterWindow.setVisibility(View.GONE);
            filterBackground.setVisibility(View.GONE);
        });

        Button btnAplicarFiltros = findViewById(R.id.btnAplicarFiltros);
        btnAplicarFiltros.setOnClickListener(v -> {
            filterWindow.setVisibility(View.GONE);
            filterBackground.setVisibility(View.GONE);
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

    @Override
    public void onMapReady(@NonNull GoogleMap googleMap) {
        gMap = googleMap;

        // ESTILO DEL MAPA (Dark Mode)
        try {
            boolean success = googleMap.setMapStyle(
                    MapStyleOptions.loadRawResourceStyle(this, R.raw.map_style_dark));
            if (!success) {
                Log.e("Explorar", "Error al cargar el estilo del mapa");
            }
        } catch (Resources.NotFoundException e) {
            Log.e("Explorar", "no se encuentra el archivo map_style_dark.json", e);
        }

        // UI SETTINGS
        gMap.getUiSettings().setZoomControlsEnabled(false);
        gMap.getUiSettings().setCompassEnabled(true);
        gMap.getUiSettings().setMyLocationButtonEnabled(false);

        // Ubicacion
        obtenerUbicacionActual();

        //incdencias y camaras
        obtenerIncidenciasMapa();
        obtenerCamarasMapa();
    }

    // --- MÉTODOS PARA LA UBICACIÓN ---

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

    private void obtenerIncidenciasMapa() {
        // 1. CONFIGURAR RETROFIT
        // OJO: Si usas emulador usa "http://10.0.2.2:PUERTO/"
        // Si usas móvil físico usa la IP de tu PC "http://192.168.1.X:PUERTO/"
        String BASE_URL = "http://10.10.16.93:8080/api/";

        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();

        ApiService service = retrofit.create(ApiService.class);

        // 2. HACER LA LLAMADA
        Call<List<Incidencia>> call = service.obtenerIncidencias();

        call.enqueue(new Callback<List<Incidencia>>() {
            @Override
            public void onResponse(Call<List<Incidencia>> call, Response<List<Incidencia>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    List<Incidencia> lista = response.body();

                    // Limpiamos mapa por si acaso (opcional)
                    // gMap.clear();

                    for (Incidencia i : lista) {
                        try {
                            // 3. CONVERTIR STRING A DOUBLE
                            // Tu clase Incidencia tiene Strings, el mapa necesita doubles
                            if (i.getLatitude() != null && i.getLongitude() != null) {
                                double lat = Double.parseDouble(i.getLatitude());
                                double lng = Double.parseDouble(i.getLongitude());

                                LatLng posicion = new LatLng(lat, lng);

                                // 4. AÑADIR MARCADOR AL MAPA
                                gMap.addMarker(new MarkerOptions()
                                        .position(posicion)
                                        .title(i.getType()) // Titulo al pulsar: "Accidente", "Obras"...
                                        .snippet(i.getCityTown())); // Subtítulo: Ciudad
                            }
                        } catch (NumberFormatException e) {
                            Log.e("API", "Error al convertir coordenadas: " + e.getMessage());
                        }
                    }
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

    private void obtenerCamarasMapa() {
        String BASE_URL = "http://10.10.16.93:8080/api/"; // Tu URL base

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

                                // AÑADIR MARCADOR AZUL
                                gMap.addMarker(new MarkerOptions()
                                        .position(posicion)
                                        .title(c.getName()) // Ejemplo: "Cámara A-8"
                                        .snippet("Ver imagen") // Texto secundario
                                        // CAMBIAR COLOR A AZUL (HUE_AZURE, HUE_BLUE, HUE_CYAN...)
                                        .icon(BitmapDescriptorFactory.defaultMarker(BitmapDescriptorFactory.HUE_AZURE)));
                            }
                        } catch (NumberFormatException e) {
                            Log.e("API", "Error coordenadas camara: " + e.getMessage());
                        }
                    }
                }
            }

            @Override
            public void onFailure(Call<List<Camara>> call, Throwable t) {
                Log.e("API", "Fallo camaras: " + t.getMessage());
            }
        });
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