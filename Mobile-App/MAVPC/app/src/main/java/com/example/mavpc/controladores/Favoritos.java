package com.example.mavpc.controladores;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ListView;
import android.widget.TextView;

import com.example.mavpc.R;
import com.example.mavpc.database.DbHelper;
import com.example.mavpc.modelos.Camara;
import com.google.android.material.bottomnavigation.BottomNavigationView;

import java.util.List;

public class Favoritos extends BaseActivity {

    private ListView lvFavoritos;
    private TextView tvVacio;
    private DbHelper dbHelper;
    private BottomNavigationView navbar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.favoritos);

        // Inicializar vistas y DB
        dbHelper = new DbHelper(Favoritos.this);

        lvFavoritos = findViewById(R.id.lvFavoritos);
        tvVacio = findViewById(R.id.tvVacio);
        navbar = findViewById(R.id.bottomNav);

        setupBottomNav();

        // TIENE QUE SER EL FINAL DEL onCreate!! fuerza al Navbar a no tener relleno inferior
        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
    }

    // Usamos onResume para recargar la lista si volvemos de otra pantalla (por si el usuario borró un favorito desde Explorar y volvió aquí)
    @Override
    protected void onResume() {
        super.onResume();
        cargarListaFavoritos();

        // Mantener seleccionado el item correcto del navbar
        navbar.setSelectedItemId(R.id.nav_favoritos);
    }

    private void cargarListaFavoritos() {
        List<Camara> lista = dbHelper.getFavCams();

        if (lista.isEmpty()) {
            tvVacio.setVisibility(View.VISIBLE);
            lvFavoritos.setVisibility(View.GONE);
        } else {
            tvVacio.setVisibility(View.GONE);
            lvFavoritos.setVisibility(View.VISIBLE);

            FavoritosAdapter adapter = new FavoritosAdapter(this, lista);
            lvFavoritos.setAdapter(adapter);
        }
    }

    private void setupBottomNav() {
        navbar.setSelectedItemId(R.id.nav_favoritos);

        navbar.setOnItemSelectedListener(item -> {
            int id = item.getItemId();

            if (id == R.id.nav_explorar) {
                Intent intent = new Intent(Favoritos.this, Explorar.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_favoritos) return true;
            if (id == R.id.nav_reportar) {
                Intent intent = new Intent(Favoritos.this, Reportar.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_perfil) {
                Intent intent = new Intent(Favoritos.this, Perfil.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            return false;
        });
    }

}