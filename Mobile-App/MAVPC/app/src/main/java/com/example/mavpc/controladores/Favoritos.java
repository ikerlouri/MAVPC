package com.example.mavpc.controladores;

import android.content.Intent;
import android.os.Bundle;

import com.example.mavpc.R;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class Favoritos extends BaseActivity {

    private BottomNavigationView navbar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.favoritos);

        navbar = findViewById(R.id.bottomNav);
        setupBottomNav();

        // TIENE QUE SER EL FINAL DEL onCreate!! fuerza al Navbar a no tener relleno inferior
        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
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