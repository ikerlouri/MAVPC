package com.example.mavpc.controladores;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.util.Base64;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AlertDialog;
import androidx.core.content.ContextCompat;

import com.bumptech.glide.Glide;
import com.example.mavpc.R;

import com.example.mavpc.database.DbHelper;
import com.example.mavpc.modelos.Usuario;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.yalantis.ucrop.UCrop;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.InputStream;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import retrofit2.Call;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;


public class Perfil extends BaseActivity {

    private BottomNavigationView navbar;
    private EditText etUsername, etEmail, etPassword;
    private ImageView ivPfp;
    private String pfpBase64;
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
                        cambiarFotoPerfil(resultUri);
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
                actualizarPerfil();
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
        // Crear Builder
        AlertDialog.Builder builder = new AlertDialog.Builder(Perfil.this);
        // Inflar el diseño personalizado
        LayoutInflater inflater = getLayoutInflater();
        View view = inflater.inflate(R.layout.dialog_logout, null);

        builder.setView(view); // Asignamos la vista (estilo) al dialog

        // Crear el Dialog pero NO mostrarlo todavía
        AlertDialog dialog = builder.create();

        // Fondo transparente
        if (dialog.getWindow() != null) {
            dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        }

        // Configurar los botones del diseño propio
        Button btnSalir = view.findViewById(R.id.btnSalir);
        btnSalir.setOnClickListener(v -> {
            DbHelper dbHelper = new DbHelper(Perfil.this);
            dbHelper.logoff(); // O logoff()

            Intent intent = new Intent(Perfil.this, Login.class);
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            startActivity(intent);
            overridePendingTransition(0, 0);

            dialog.dismiss();
        });

        Button btnCancelar = view.findViewById(R.id.btnCancelar);
        btnCancelar.setOnClickListener(v -> {
            dialog.dismiss();
        });

        // Mostrar
        dialog.show();
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

    private void cambiarFotoPerfil(Uri imageUri) {
        try {
            // cambios en la imageview
            ivPfp.setImageTintList(null);
            ivPfp.clearColorFilter();
            ivPfp.setImageDrawable(null);
            ivPfp.setImageURI(imageUri);
            pfpBase64 = convertirUriABase64(imageUri);
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
        DbHelper dbHelper = new DbHelper(Perfil.this);
        Usuario currentUser = dbHelper.getUsuarioSesion();

        etUsername.setText(currentUser.getUsername());
        etEmail.setText(currentUser.getEmail());
        etPassword.setText(currentUser.getPassword());

        String urlImagen = currentUser.getPfpUrl();
        // Usar Glide para cargarla
        if (urlImagen != null && !urlImagen.isEmpty()) {
            Glide.with(this)
                    .load(urlImagen)
                    .placeholder(R.drawable.ic_user) // imagen mientras carga
                    .error(R.drawable.ic_user)       // imagen si falla la carga o la URL está rota
                    .circleCrop()                    // la recorta en círculo
                    .into(ivPfp);
        }
    }

    private void actualizarPerfil() {
        String nuevoUsername = etUsername.getText().toString().trim();
        String nuevoEmail = etEmail.getText().toString().trim();
        String nuevaPassword = etPassword.getText().toString().trim();

        // Validaciones básicas
        if (nuevoUsername.isEmpty() || nuevoEmail.isEmpty() ||nuevaPassword.isEmpty()) {
            Toast.makeText(this, "No puedes dejar ningún campo vacío", Toast.LENGTH_SHORT).show();
            return;
        }

        // Recuperar el usuario actual para mantener su id
        DbHelper dbHelper = new DbHelper(Perfil.this);
        Usuario usuarioActual = dbHelper.getUsuarioSesion();
        if (usuarioActual == null) {
            Toast.makeText(this, "Error: No hay sesión activa", Toast.LENGTH_SHORT).show();
            return;
        }

        String passwordFinal = hashearPassword(nuevaPassword);

        // Crear el usuario con los datos nuevos
        Usuario usuarioActualizado = new Usuario(
                usuarioActual.getId(), // el id no debe cambiar
                nuevoUsername,
                nuevoEmail,
                passwordFinal,
                pfpBase64
        );

        // Llamada a la API (Retrofit)
        String BASE_URL = "https://mavpc.up.railway.app/api/";
        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        ApiService service = retrofit.create(ApiService.class);

        service.actualizarUsuario(usuarioActualizado);
        dbHelper.insertUsuarioSesion(usuarioActualizado);
    }

    private String hashearPassword(String txtPassword) {
        try {
            // crear instancia de SHA-256
            MessageDigest digest = MessageDigest.getInstance("SHA-256");

            // pasar a byes y hashear
            byte[] hash = digest.digest(txtPassword.getBytes());

            // convertir a hexadecimal
            StringBuilder hexString = new StringBuilder();
            for (byte b : hash) {
                String hex = Integer.toHexString(0xff & b);
                if (hex.length() == 1) hexString.append('0');
                hexString.append(hex);
            }

            return hexString.toString();
        } catch (NoSuchAlgorithmException e) {
            e.printStackTrace();
            return null;
        }
    }

    private String convertirUriABase64(Uri uri) {
        try {
            // Abrir el archivo desde la URI
            InputStream inputStream = getContentResolver().openInputStream(uri);
            Bitmap bitmap = BitmapFactory.decodeStream(inputStream);

            // Comprimir la imagen (Importante para no enviar un texto kilométrico)
            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            // Comprimimos a JPEG con calidad 50% para reducir tamaño
            bitmap.compress(Bitmap.CompressFormat.JPEG, 50, outputStream);
            byte[] imagenBytes = outputStream.toByteArray();

            // Convertir bytes a String Base64
            // El flag NO_WRAP evita saltos de línea que rompen el JSON
            return Base64.encodeToString(imagenBytes, Base64.NO_WRAP);
        } catch (Exception e) {
            e.printStackTrace();
            return null; // Si falla, devolvemos null
        }
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