package com.example.mavpc.controladores;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.core.content.ContextCompat;
import androidx.core.graphics.drawable.RoundedBitmapDrawable;
import androidx.core.graphics.drawable.RoundedBitmapDrawableFactory;

import com.example.mavpc.R;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.yalantis.ucrop.UCrop;

import java.io.File;
import java.io.InputStream;

public class Perfil extends BaseActivity {

    private BottomNavigationView navbar;
    private EditText etUsername, etEmail, etPassword;
    private ImageView ivPfp;
    private Button btnEditProfile, btnLogout;
    private LinearLayout layoutEditButtons;
    private View btnDeletePfp, btnChangePfp;

    private boolean isEditing = false;

    // abre la galería nativa del movil
    private final ActivityResultLauncher<String> openGalleryLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), uri -> {
                if (uri != null) {
                    // una vez elegida la foto, lanzamos el editor
                    startUCrop(uri);
                }
            });

    // maneja el resultado del recorte
    private final ActivityResultLauncher<Intent> uCropLauncher =
            registerForActivityResult(new ActivityResultContracts.StartActivityForResult(), result -> {
                if (result.getResultCode() == RESULT_OK && result.getData() != null) {
                    final Uri resultUri = UCrop.getOutput(result.getData());
                    if (resultUri != null) {
                        actualizarFotoPerfil(resultUri);
                    }
                } else if (result.getResultCode() == UCrop.RESULT_ERROR) {
                    final Throwable cropError = UCrop.getError(result.getData());
                    Toast.makeText(this, "Error: " + cropError.getMessage(), Toast.LENGTH_SHORT).show();
                }
            });

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.perfil);

        etUsername = findViewById(R.id.etUsername);
        etEmail = findViewById(R.id.etEmail);
        etPassword = findViewById(R.id.etPassword);
        ivPfp = findViewById(R.id.ivPfp);
        btnLogout = findViewById(R.id.btnLogout);
        layoutEditButtons = findViewById(R.id.layoutEditButtons);

        navbar = findViewById(R.id.bottomNav);
        setupBottomNav();

        btnEditProfile = findViewById(R.id.btnEditProfile);
        btnEditProfile.setOnClickListener(v -> {
            if (!isEditing) activarEdicion(true);
            else {
                guardarDatosEnApi();
                activarEdicion(false);
            }
        });

        btnChangePfp = findViewById(R.id.btnChangePfp);
        btnChangePfp.setOnClickListener(v -> {
            // abre la galería
            openGalleryLauncher.launch("image/*");
        });

        btnDeletePfp = findViewById(R.id.btnDeletePfp);
        btnDeletePfp.setOnClickListener(v -> {
            // cambios en la imageview
            ivPfp.setImageResource(R.drawable.ic_user);
            ivPfp.setImageTintList(ColorStateList.valueOf(getResources().getColor(R.color.white)));
        });

        btnLogout.setOnClickListener(v -> logout());

        cargarDatosPerfil();

        // TIENE QUE SER EL FINAL DEL onCreate!! fuerza al Navbar a no tener relleno inferior
        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
    }

    private void logout() {
        Intent intent = new Intent(Perfil.this, Login.class);

        // CLEAR_TASK borra todas las actividades existentes.
        // NEW_TASK crea una tarea nueva y limpia.
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);

        startActivity(intent);
        overridePendingTransition(0, 0);
    }

    // ventana de ajuste de pfp
    private void startUCrop(Uri sourceUri) {
        String destinationFileName = "pfp_cropped.jpg";
        Uri destinationUri = Uri.fromFile(new File(getCacheDir(), destinationFileName));

        UCrop.Options options = new UCrop.Options();

        // ajustes esteticos
        options.setCircleDimmedLayer(true);
        options.setShowCropFrame(false);
        options.setShowCropGrid(false);
        options.setCompressionQuality(90);
        options.setToolbarColor(ContextCompat.getColor(this, R.color.dark_grey));
        options.setStatusBarColor(ContextCompat.getColor(this, R.color.dark_grey));
        options.setActiveControlsWidgetColor(ContextCompat.getColor(this, R.color.cake_green));
        options.setToolbarWidgetColor(ContextCompat.getColor(this, R.color.white));

        // preparamos un Intent normal de uCrop
        Intent uCropIntent = UCrop.of(sourceUri, destinationUri)
                .withAspectRatio(1, 1)
                .withMaxResultSize(1000, 1000)
                .withOptions(options)
                .getIntent(this);

        // forzamos a que use AjustePfp que esta en modo inmersivo
        uCropIntent.setClass(this, AjustePfp.class);

        uCropLauncher.launch(uCropIntent);
    }

    private void actualizarFotoPerfil(Uri imageUri) {
        try {
            // cambios en la imageview
            ivPfp.setImageTintList(null);
            ivPfp.clearColorFilter();
            ivPfp.setImageDrawable(null);
            ivPfp.setImageURI(imageUri);



        } catch (Exception e) {
            e.printStackTrace();
            Toast.makeText(this, "Error al cargar la imagen", Toast.LENGTH_SHORT).show();
        }
    }

    private void activarEdicion(boolean activar) {
        isEditing = activar;
        configurarEditText(etUsername, activar);
        configurarEditText(etEmail, activar);
        configurarEditText(etPassword, activar);

        if (activar) {
            layoutEditButtons.setVisibility(View.VISIBLE);
            btnEditProfile.setText("Guardar Cambios");
            btnEditProfile.setBackgroundTintList(ContextCompat.getColorStateList(this, R.color.cake_green));
            btnEditProfile.setTextColor(getResources().getColor(R.color.black));
        } else {
            layoutEditButtons.setVisibility(View.GONE);
            btnEditProfile.setText("Editar Perfil");
            btnEditProfile.setBackgroundTintList(ContextCompat.getColorStateList(this, R.color.dark_grey));
            btnEditProfile.setTextColor(getResources().getColor(R.color.white));
        }
    }

    private void configurarEditText(EditText et, boolean habilitar) {
        et.setFocusable(habilitar);
        et.setFocusableInTouchMode(habilitar);
        et.setClickable(habilitar);
        et.setCursorVisible(habilitar);
        if (habilitar) et.setSelection(et.getText().length());
    }

    private void cargarDatosPerfil() {
        // wip
    }

    private void guardarDatosEnApi() {
        // wip
        Toast.makeText(this, "Perfil actualizado (mentira)", Toast.LENGTH_SHORT).show();
    }

    private void setupBottomNav() {
        navbar.setSelectedItemId(R.id.nav_perfil);

        navbar.setOnItemSelectedListener(item -> {
            int id = item.getItemId();

            if (id == R.id.nav_explorar) {
                Intent intent = new Intent(Perfil.this, Explorar.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_favoritos) {
                Intent intent = new Intent(Perfil.this, Favoritos.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_reportar) {
                Intent intent = new Intent(Perfil.this, Reportar.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
                startActivity(intent);
                overridePendingTransition(0, 0);
            }
            if (id == R.id.nav_perfil) return true;
            return false;
        });
    }
}