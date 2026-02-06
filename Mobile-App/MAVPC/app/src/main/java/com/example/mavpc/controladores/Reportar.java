package com.example.mavpc.controladores;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.ScrollView;
import android.widget.Spinner;
import android.widget.Toast;

import com.example.mavpc.R;
import com.example.mavpc.data.api.ApiService;
import com.example.mavpc.model.Incidencia;
import com.google.android.material.bottomnavigation.BottomNavigationView;

import java.util.ArrayList;

// para coger la fecha actual
import java.util.Calendar;

import java.util.List;

// para coger la hora actual

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

// Controlador de la ventana "reportar", la del mapa
public class Reportar extends BaseActivity {

    private BottomNavigationView navbar;
    private ScrollView sv;
    private EditText etLatitud, etLongitud, etCausa, etCiudad, etCarretera, etDireccion;
    private Spinner spinDia, spinMes, spinAno, spinHora, spinMin, spinSeg;
    private RadioGroup rgTipo, rgGravedad, rgProvincia;
    private RadioButton rbObra, rbIncidencia, rbGrave, rbMedio, rbLeve, rbAlava, rbVizcaya, rbGuipuzkoa;
    private Button btnEnviar;

    // configuracion al crearse la ventana
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.reportar);

        sv = findViewById(R.id.sv);

        spinDia = findViewById(R.id.spinDia);
        spinMes = findViewById(R.id.spinMes);
        spinAno = findViewById(R.id.spinAno);
        inicializarSpinnersFecha();

        spinHora = findViewById(R.id.spinHora);
        spinMin = findViewById(R.id.spinMin);
        spinSeg = findViewById(R.id.spinSeg);
        inicializarSpinnersHora();

        rgTipo = findViewById(R.id.rgTipo);
        rbObra = findViewById(R.id.rbObra);
        rbIncidencia = findViewById(R.id.rbIncidencia);

        rgGravedad = findViewById(R.id.rgGravedad);
        rbGrave = findViewById(R.id.rbGrave);
        rbMedio = findViewById(R.id.rbMedio);
        rbLeve = findViewById(R.id.rbLeve);

        etCausa = findViewById(R.id.etCausa);

        rgProvincia = findViewById(R.id.rgProvincia);
        rbAlava = findViewById(R.id.rbAlava);
        rbVizcaya = findViewById(R.id.rbVizcaya);
        rbGuipuzkoa = findViewById(R.id.rbGuipuzkoa);

        etCiudad = findViewById(R.id.etCiudad);

        etLatitud = findViewById(R.id.etLatitud);
        etLongitud = findViewById(R.id.etLongitud);
        verificarCoordenadasIntent(); // por si viene del mapa con unas coordenadas

        etCarretera = findViewById(R.id.etCarretera);
        etDireccion = findViewById(R.id.etDireccion);

        btnEnviar = findViewById(R.id.btnEnviar);
        btnEnviar.setOnClickListener(v -> reportarIncidencia());

        navbar = findViewById(R.id.bottomNav);
        setupBottomNav();

        // TIENE QUE SER EL FINAL DEL onCreate!! fuerza al Navbar a no tener relleno inferior
        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
    }

    // recoge los datos de la incidencia y la inserta
    private void reportarIncidencia() {
        if (!validarDatos()) return; // si no son datos válidos nada

        // Obtener valores de los RadioButtons seleccionados
        RadioButton rbTipoSel = findViewById(rgTipo.getCheckedRadioButtonId());
        String tipo = rbTipoSel.getText().toString();

        RadioButton rbGravSel = findViewById(rgGravedad.getCheckedRadioButtonId());
        String gravedad = rbGravSel.getText().toString();

        RadioButton rbProvSel = findViewById(rgProvincia.getCheckedRadioButtonId());
        String provincia = rbProvSel.getText().toString();

        // Construir la fecha y hora (Formato ISO 8601: yyyy-MM-ddTHH:mm:ss)
        String fecha = spinAno.getSelectedItem().toString() + "-" +
                spinMes.getSelectedItem().toString() + "-" +
                spinDia.getSelectedItem().toString();

        String hora = spinHora.getSelectedItem().toString() + ":" +
                spinMin.getSelectedItem().toString() + ":" +
                spinSeg.getSelectedItem().toString();

        String fechaCompleta = fecha + "T" + hora;

        // Crear el objeto Incidencia
        Incidencia nuevaIncidencia = new Incidencia();
        nuevaIncidencia.setType(tipo);
        nuevaIncidencia.setLevel(gravedad);
        nuevaIncidencia.setProvince(provincia);
        nuevaIncidencia.setCause(etCausa.getText().toString().trim());
        nuevaIncidencia.setCityTown(etCiudad.getText().toString().trim());
        nuevaIncidencia.setLatitude(etLatitud.getText().toString());
        nuevaIncidencia.setLongitude(etLongitud.getText().toString());
        nuevaIncidencia.setRoad(etCarretera.getText().toString().trim());
        nuevaIncidencia.setDirection(etDireccion.getText().toString().trim());
        nuevaIncidencia.setStartDate(fechaCompleta); // String o Date según tu API

        String BASE_URL = "https://mavpc.up.railway.app/api/";
        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        ApiService service = retrofit.create(ApiService.class);

        Call<Void> call = service.crearIncidencia(nuevaIncidencia);
        call.enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(Reportar.this, "Incidencia reportada con éxito", Toast.LENGTH_LONG).show();
                    limpiarFormulario();

                    Intent intent = new Intent(Reportar.this, Explorar.class);

                    try {
                        double lat = Double.parseDouble(nuevaIncidencia.getLatitude());
                        double lon = Double.parseDouble(nuevaIncidencia.getLongitude());

                        intent.putExtra("LAT_DESTINO", lat);
                        intent.putExtra("LON_DESTINO", lon);
                        intent.putExtra("ACTUALIZAR_MAPA", true);

                        intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                        startActivity(intent);
                        overridePendingTransition(0, 0);
                        finish();
                    } catch (NumberFormatException | NullPointerException e) {
                        e.printStackTrace();
                    }
                } else {
                    Log.e("API_ERROR", String.valueOf(response.code()));
                    Toast.makeText(Reportar.this, "Error en el servidor.", Toast.LENGTH_LONG).show();
                }
            }
            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Log.e("API_FAILURE", "Error en el servidor: " + t.getMessage());
                Toast.makeText(Reportar.this, "Error de conexión: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    // vacia el formulario
    private void limpiarFormulario() {
        // Limpiar EditText
        etCausa.setText("");
        etCiudad.setText("");
        etLatitud.setText("");
        etLongitud.setText("");
        etCarretera.setText("");
        etDireccion.setText("");

        // Desmarcar RadioGroup
        rgTipo.clearCheck();
        rgGravedad.clearCheck();
        rgProvincia.clearCheck();

        // Resetear selectores de fecha y hora
        marcarFechaActualSpinners();
        marcarHoraActualSpinners();
    }

    // validacion de datos de incidencia
    private boolean validarDatos() {
        // Validar RadioGroups usando su ID de selección (-1 significa nada seleccionado)
        if (rgTipo.getCheckedRadioButtonId() == -1) {
            Toast.makeText(this, "Por favor introduce el tipo", Toast.LENGTH_SHORT).show();
            return false;
        }
        if (rgGravedad.getCheckedRadioButtonId() == -1) {
            Toast.makeText(this, "Por favor introduce la gravedad", Toast.LENGTH_SHORT).show();
            return false;
        }
        if (rgProvincia.getCheckedRadioButtonId() == -1) {
            Toast.makeText(this, "Por favor introduce la provincia", Toast.LENGTH_SHORT).show();
            return false;
        }

        // Validar Coordenadas (No solo vacío, sino que sean números válidos)
        String latStr = etLatitud.getText().toString().trim();
        String lonStr = etLongitud.getText().toString().trim();

        if (latStr.isEmpty() || lonStr.isEmpty()) {
            Toast.makeText(this, "Por favor introduce las coordenadas", Toast.LENGTH_SHORT).show();
            return false;
        }
        try {
            Double.parseDouble(latStr);
            Double.parseDouble(lonStr);
        } catch (NumberFormatException e) {
            Toast.makeText(this, "Por favor introduce unas coordenadas válidas", Toast.LENGTH_SHORT).show();
            return false;
        }

        return true;
    }

    // verificar si llega a esta ventana con unas coordenadas
    private void verificarCoordenadasIntent() {
        // si no existen devuelve 0.0
        double lat = getIntent().getDoubleExtra("LATITUD", 0.0);
        double lng = getIntent().getDoubleExtra("LONGITUD", 0.0);

        // Si son distintos de 0, es que venimos del Mapa
        if (lat != 0.0 && lng != 0.0) {
            etLatitud.setText(String.valueOf(lat));
            etLongitud.setText(String.valueOf(lng));
        }
    }

    // poner como default la fecha actual
    private void marcarFechaActualSpinners(){
        // obtencion de fecha actual
        Calendar calendar = Calendar.getInstance();
        int diaHoy = calendar.get(Calendar.DAY_OF_MONTH);
        int mesHoy = calendar.get(Calendar.MONTH) + 1; // enero es 0, así que sumamos 1
        int anoHoy = calendar.get(Calendar.YEAR);

        // seleccion de la fecha actual como default
        if (diaHoy <= 31) spinDia.setSelection(diaHoy - 1);

        if (mesHoy <= 12) spinMes.setSelection(mesHoy - 1);

        int indiceAno = anoHoy - 2008;
        spinAno.setSelection(indiceAno);

        /*
        // verificamos que el año actual esté dentro del rango para que no falle
        if (indiceAno > 0 && indiceAno < anos.size()) {
            spinAno.setSelection(indiceAno);
        } else{
            spinAno.setSelection(anos.size() - 1);
        }
        */
    }

    private void marcarHoraActualSpinners(){
        // la hora actual
        Calendar calendar = Calendar.getInstance();
        int horaActual = calendar.get(Calendar.HOUR_OF_DAY);
        int minutoActual = calendar.get(Calendar.MINUTE);
        int segundoActual = calendar.get(Calendar.SECOND);

        // seleccion de hora actual como default
        spinHora.setSelection(horaActual);
        spinMin.setSelection(minutoActual);
        spinSeg.setSelection(segundoActual);
    }

    private void inicializarSpinnersFecha() {
        Spinner spinDia = findViewById(R.id.spinDia);
        Spinner spinMes = findViewById(R.id.spinMes);
        Spinner spinAno = findViewById(R.id.spinAno);

        // dia (1-31)
        List<String> dias = new ArrayList<>();
        for(int i=1; i<=31; i++) dias.add(String.format("%02d", i));

        // mes (1-12)
        List<String> meses = new ArrayList<>();
        for(int i=1; i<=12; i++) meses.add(String.format("%02d", i));

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

        marcarFechaActualSpinners();
    }

    private void inicializarSpinnersHora() {
        // hora (00-23)
        List<String> horas = new ArrayList<>();
        for(int i=0; i<24; i++) {
            // String.format("%02d", i) convierte el 5 en "05", el 10 en "10"
            horas.add(String.format("%02d", i));
        }

        // minuto (00-59)
        List<String> minutos = new ArrayList<>();
        for(int i=0; i<60; i++) {
            minutos.add(String.format("%02d", i));
        }

        // segundo (00-59)
        List<String> segundos = new ArrayList<>();
        for(int i=0; i<60; i++) {
            segundos.add(String.format("%02d", i));
        }

        // adaptadores
        ArrayAdapter<String> adapterHora = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, horas);
        adapterHora.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinHora.setAdapter(adapterHora);

        ArrayAdapter<String> adapterMinuto = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, minutos);
        adapterMinuto.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinMin.setAdapter(adapterMinuto);

        ArrayAdapter<String> adapterSegundo = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, segundos);
        adapterSegundo.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinSeg.setAdapter(adapterSegundo);

        marcarHoraActualSpinners();
    }

    // configuracion del navbar, comun en todas las ventanas
    private void setupBottomNav() {
        navbar.setSelectedItemId(R.id.nav_reportar);

        navbar.setOnItemSelectedListener(item -> {
            int id = item.getItemId();

            if (id == R.id.nav_explorar) {
                Intent intent = new Intent(Reportar.this, Explorar.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
                this.finish();
            }
            if (id == R.id.nav_favoritos) {
                Intent intent = new Intent(Reportar.this, Favoritos.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
                this.finish();
            }
            if (id == R.id.nav_reportar) return true;
            if (id == R.id.nav_perfil) {
                Intent intent = new Intent(Reportar.this, Perfil.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
                this.finish();
            }
            return false;
        });
    }
}