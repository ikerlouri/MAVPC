package com.example.mavpc.controladores;

import android.content.Intent;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.Spinner;

import com.example.mavpc.R;
import com.google.android.material.bottomnavigation.BottomNavigationView;

import java.util.ArrayList;

// para coger la fecha actual
import java.util.Calendar;

import java.util.List;

// para coger la hora actual
import java.time.LocalTime;

public class Reportar extends BaseActivity {

    private BottomNavigationView navbar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.reportar);

        navbar = findViewById(R.id.bottomNav);

        setupBottomNav();

        // inicializar spinners de fecha y hora
        inicializarSpinnersFecha();
        inicializarSpinnersHora();

        // TIENE QUE SER EL FINAL DEL onCreate!! fuerza al Navbar a no tener relleno inferior
        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
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

    private void inicializarSpinnersHora() {
        Spinner spinHora = findViewById(R.id.spinHora);
        Spinner spinMinuto = findViewById(R.id.spinMin);
        Spinner spinSegundo = findViewById(R.id.spinSeg);

        // la hora actual
        Calendar calendar = Calendar.getInstance();
        int horaActual = calendar.get(Calendar.HOUR_OF_DAY);
        int minutoActual = calendar.get(Calendar.MINUTE);
        int segundoActual = calendar.get(Calendar.SECOND);

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
        spinMinuto.setAdapter(adapterMinuto);

        ArrayAdapter<String> adapterSegundo = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, segundos);
        adapterSegundo.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinSegundo.setAdapter(adapterSegundo);

        // seleccion de hora actual como default
        spinHora.setSelection(horaActual);
        spinMinuto.setSelection(minutoActual);
        spinSegundo.setSelection(segundoActual);
    }

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