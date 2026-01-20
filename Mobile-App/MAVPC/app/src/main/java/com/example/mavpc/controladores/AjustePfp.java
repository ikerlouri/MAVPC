package com.example.mavpc.controladores;

import android.os.Bundle;
import android.view.View;

import com.yalantis.ucrop.UCropActivity;

// clase que extiende de la clase para ajustar la pfp, para asi forzarle el modo inmersivo
public class AjustePfp extends UCropActivity {

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        ocultarBarras();
    }

    @Override
    public void onWindowFocusChanged(boolean hasFocus) {
        super.onWindowFocusChanged(hasFocus);
        if (hasFocus) {
            ocultarBarras();
        }
    }

    private void ocultarBarras() {
        // Código estándar para FullScreen inmersivo (Sticky)
        getWindow().getDecorView().setSystemUiVisibility(
                View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                        | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                        | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                        | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION // Oculta barra inferior
                        | View.SYSTEM_UI_FLAG_FULLSCREEN      // Oculta barra superior
                        | View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY);
    }
}