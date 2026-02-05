package com.example.mavpc.controladores;

import android.content.Intent;
import android.content.res.ColorStateList;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.ColorDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.util.Base64;
import android.util.Log;
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
import com.example.mavpc.data.api.ApiService;
import com.example.mavpc.data.local.DbHelper;
import com.example.mavpc.model.Usuario;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.yalantis.ucrop.UCrop;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class Perfil extends BaseActivity {

    private BottomNavigationView navbar;
    private EditText etUsername, etEmail, etPassword;
    private ImageView ivPfp;

    // Inicializamos a null para saber si el usuario tocó la imagen o no
    private String pfpBase64 = null;

    private Button btnEditProfile, btnLogout, btnEliminarCuenta;
    private LinearLayout layoutEditButtons;
    private View btnDeletePfp, btnChangePfp;

    private boolean isEditing = false;

    // Abre la galería nativa del movil
    private final ActivityResultLauncher<String> openGalleryLauncher =
            registerForActivityResult(new ActivityResultContracts.GetContent(), uri -> {
                if (uri != null) {
                    // Una vez elegida la foto, lanzamos el editor
                    startUCrop(uri);
                }
            });

    // Maneja el resultado del recorte
    private final ActivityResultLauncher<Intent> uCropLauncher =
            registerForActivityResult(new ActivityResultContracts.StartActivityForResult(), result -> {
                if (result.getResultCode() == RESULT_OK && result.getData() != null) {
                    final Uri resultUri = UCrop.getOutput(result.getData());
                    if (resultUri != null) {
                        cambiarFotoPerfil(resultUri);
                    }
                } else if (result.getResultCode() == UCrop.RESULT_ERROR) {
                    final Throwable cropError = UCrop.getError(result.getData());
                    Toast.makeText(this, "Error recuadro: " + cropError.getMessage(), Toast.LENGTH_SHORT).show();
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
        btnLogout.setOnClickListener(v -> logout());

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
            openGalleryLauncher.launch("image/*");
        });

        btnDeletePfp = findViewById(R.id.btnDeletePfp);
        btnDeletePfp.setOnClickListener(v -> {
            // Ponemos la imagen por defecto
            ivPfp.setImageResource(R.drawable.ic_user);
            ivPfp.setImageTintList(ColorStateList.valueOf(getResources().getColor(R.color.white)));

            // IMPORTANTE: Marcamos que se quiere borrar la imagen enviando string vacío
            pfpBase64 = "";
        });

        btnEliminarCuenta = findViewById(R.id.btnEliminarCuenta);
        btnEliminarCuenta.setOnClickListener(v -> btnEliminarCuenta());

        cargarDatosPerfil();

        findViewById(R.id.bottomNav).setOnApplyWindowInsetsListener(null);
    }

    private void btnEliminarCuenta() {
        AlertDialog.Builder builder = new AlertDialog.Builder(Perfil.this);
        LayoutInflater inflater = getLayoutInflater();
        View view = inflater.inflate(R.layout.dialog_deleteaccount, null);
        builder.setView(view);

        AlertDialog dialog = builder.create();
        if (dialog.getWindow() != null) {
            dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        }

        Button btnConfiElim = view.findViewById(R.id.btnConfiElim);
        btnConfiElim.setOnClickListener(v -> {
            DbHelper dbHelper = new DbHelper(Perfil.this);
            Usuario currentUser = dbHelper.getUsuarioSesion();

            if (currentUser == null) {
                Toast.makeText(Perfil.this, "Error: No se detecta usuario.", Toast.LENGTH_SHORT).show();
                return;
            }

            String BASE_URL = "https://mavpc.up.railway.app/api/";
            Retrofit retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
            ApiService service = retrofit.create(ApiService.class);

            btnConfiElim.setEnabled(false);
            Toast.makeText(Perfil.this, "Eliminando cuenta...", Toast.LENGTH_SHORT).show();

            service.eliminarUsuario(currentUser.getId()).enqueue(new Callback<Void>() {
                @Override
                public void onResponse(Call<Void> call, Response<Void> response) {
                    btnConfiElim.setEnabled(true);

                    if (response.isSuccessful()) {
                        dbHelper.logoff();
                        Intent intent = new Intent(Perfil.this, Login.class);
                        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intent);
                        overridePendingTransition(0, 0);
                        dialog.dismiss();
                    } else {
                        Log.e("API", "Error al eliminar: " + response.code());
                        Toast.makeText(Perfil.this, "Error al eliminar cuenta (" + response.code() + ")", Toast.LENGTH_SHORT).show();
                    }
                }

                @Override
                public void onFailure(Call<Void> call, Throwable t) {
                    btnConfiElim.setEnabled(true);
                    Log.e("API", "Fallo de conexión: " + t.getMessage());
                    Toast.makeText(Perfil.this, "Error de conexión.", Toast.LENGTH_SHORT).show();
                }
            });
        });

        Button btnCancelar = view.findViewById(R.id.btnCancelar);
        btnCancelar.setOnClickListener(v -> dialog.dismiss());

        dialog.show();
    }

    private void logout() {
        AlertDialog.Builder builder = new AlertDialog.Builder(Perfil.this);
        LayoutInflater inflater = getLayoutInflater();
        View view = inflater.inflate(R.layout.dialog_logout, null);
        builder.setView(view);

        AlertDialog dialog = builder.create();
        if (dialog.getWindow() != null) {
            dialog.getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        }

        Button btnSalir = view.findViewById(R.id.btnSalir);
        btnSalir.setOnClickListener(v -> {
            DbHelper dbHelper = new DbHelper(Perfil.this);
            dbHelper.logoff();

            Intent intent = new Intent(Perfil.this, Login.class);
            intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
            startActivity(intent);
            overridePendingTransition(0, 0);
            dialog.dismiss();
        });

        Button btnCancelar = view.findViewById(R.id.btnCancelar);
        btnCancelar.setOnClickListener(v -> dialog.dismiss());

        dialog.show();
    }

    private void startUCrop(Uri sourceUri) {
        String destinationFileName = "pfp_cropped.jpg";
        Uri destinationUri = Uri.fromFile(new File(getCacheDir(), destinationFileName));

        UCrop.Options options = new UCrop.Options();
        options.setCircleDimmedLayer(true);
        options.setShowCropFrame(false);
        options.setShowCropGrid(false);
        options.setCompressionQuality(90);
        options.setToolbarColor(ContextCompat.getColor(this, R.color.dark_grey));
        options.setStatusBarColor(ContextCompat.getColor(this, R.color.dark_grey));
        options.setActiveControlsWidgetColor(ContextCompat.getColor(this, R.color.cake_green));
        options.setToolbarWidgetColor(ContextCompat.getColor(this, R.color.white));

        Intent uCropIntent = UCrop.of(sourceUri, destinationUri)
                .withAspectRatio(1, 1)
                .withMaxResultSize(800, 800)
                .withOptions(options)
                .getIntent(this);

        uCropIntent.setClass(this, AjustePfp.class);
        uCropLauncher.launch(uCropIntent);
    }

    private void cambiarFotoPerfil(Uri imageUri) {
        try {
            ivPfp.setImageTintList(null);
            ivPfp.clearColorFilter();
            ivPfp.setImageDrawable(null);
            ivPfp.setImageURI(imageUri);

            pfpBase64 = convertirUriABase64(imageUri);

        } catch (Exception e) {
            e.printStackTrace();
            Toast.makeText(this, "Error al procesar la imagen", Toast.LENGTH_SHORT).show();
        }
    }

    private void activarEdicion(boolean activar) {
        isEditing = activar;
        configurarEditText(etUsername, activar);
        configurarEditText(etPassword, activar);

        if (activar) {
            layoutEditButtons.setVisibility(View.VISIBLE);

            btnLogout.setVisibility(View.GONE);

            btnEditProfile.setText("Confirmar Cambios");
            btnEditProfile.setBackgroundTintList(ContextCompat.getColorStateList(this, R.color.cake_green));
            btnEditProfile.setTextColor(getResources().getColor(R.color.black));
        } else {
            layoutEditButtons.setVisibility(View.GONE);

            btnLogout.setVisibility(View.VISIBLE);

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

        if (currentUser != null) {
            etUsername.setText(currentUser.getUsername());
            etEmail.setText(currentUser.getEmail());
            etPassword.setText(currentUser.getPassword());

            String datosImagen = currentUser.getPfpUrl();

            // --- IMPORTANTE: LIMPIAR EL TINTE BLANCO SIEMPRE ---
            ivPfp.setImageTintList(null);       // Quita el color sólido
            ivPfp.clearColorFilter();           // Limpia filtros
            // ---------------------------------------------------

            if (datosImagen != null && !datosImagen.isEmpty()) {

                // Caso A: Es URL (http...)
                if (datosImagen.startsWith("http") || datosImagen.startsWith("https")) {
                    Glide.with(this)
                            .load(datosImagen)
                            .placeholder(R.drawable.ic_user)
                            .error(R.drawable.ic_user)
                            .circleCrop()
                            .into(ivPfp);
                }
                // Caso B: Es Base64 (lo que tienes en SQLite)
                else {
                    try {
                        String cleanBase64 = datosImagen;
                        if (datosImagen.contains(",")) {
                            cleanBase64 = datosImagen.substring(datosImagen.indexOf(",") + 1);
                        }

                        byte[] imageBytes = Base64.decode(cleanBase64, Base64.DEFAULT);

                        Glide.with(this)
                                .asBitmap() // Forzamos que lo trate como Bitmap
                                .load(imageBytes)
                                .placeholder(R.drawable.ic_user)
                                .error(R.drawable.ic_user)
                                .circleCrop()
                                .into(ivPfp);

                    } catch (Exception e) {
                        e.printStackTrace();
                        ivPfp.setImageResource(R.drawable.ic_user);
                        // Si falla, aquí sí podríamos querer el tinte blanco si tu icono es negro
                        // ivPfp.setImageTintList(ColorStateList.valueOf(Color.WHITE));
                    }
                }
            } else {
                // Si no hay foto, ponemos el icono por defecto
                ivPfp.setImageResource(R.drawable.ic_user);
                // Opcional: Si tu icono es negro y el fondo oscuro, aquí sí querrías ponerlo blanco de nuevo
                ivPfp.setImageTintList(ColorStateList.valueOf(getResources().getColor(R.color.white)));
            }
        }
    }

    private void actualizarPerfil() {
        String nuevoUsername = etUsername.getText().toString().trim();
        String nuevoEmail = etEmail.getText().toString().trim();
        String inputPassword = etPassword.getText().toString().trim();

        if (nuevoUsername.isEmpty() || nuevoEmail.isEmpty() || inputPassword.isEmpty()) {
            Toast.makeText(this, "No puedes dejar ningún campo vacío", Toast.LENGTH_SHORT).show();
            return;
        }

        DbHelper dbHelper = new DbHelper(Perfil.this);
        Usuario usuarioActual = dbHelper.getUsuarioSesion();
        if (usuarioActual == null) {
            Toast.makeText(this, "Error: No hay sesión activa", Toast.LENGTH_SHORT).show();
            return;
        }

        String passwordFinal = hashearPassword(inputPassword);

        String imagenFinalEnviar;
        if (pfpBase64 != null) {
            imagenFinalEnviar = pfpBase64;
        } else {
            imagenFinalEnviar = obtenerImagenActualDePantalla();
        }

        Usuario usuarioActualizado = new Usuario(
                usuarioActual.getId(),
                nuevoUsername,
                nuevoEmail,
                passwordFinal,
                imagenFinalEnviar
        );

        String BASE_URL = "https://mavpc.up.railway.app/api/";
        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        ApiService service = retrofit.create(ApiService.class);

        btnEditProfile.setEnabled(false);

        service.actualizarUsuario(usuarioActualizado).enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                btnEditProfile.setEnabled(true);

                if (response.isSuccessful()) {
                    // Actualizar en sqlite
                    usuarioActualizado.setPassword(inputPassword); // en sqlite se guarda la contraseña sin hashear
                    dbHelper.updateUsuario(usuarioActualizado);
                    Toast.makeText(Perfil.this, "Perfil actualizado correctamente", Toast.LENGTH_SHORT).show();
                } else {
                    int statusCode = response.code();
                    String errorMsg = "Error desconocido";
                    try {
                        if (response.errorBody() != null) {
                            errorMsg = response.errorBody().string();
                        }
                    } catch (IOException e) { e.printStackTrace(); }

                    Log.e("API_ERROR", "Código: " + statusCode + " | Mensaje: " + errorMsg);

                    if (statusCode == 400) {
                        Toast.makeText(Perfil.this, "Datos inválidos.", Toast.LENGTH_LONG).show();
                    } else if (statusCode == 413) {
                        Toast.makeText(Perfil.this, "La imagen es demasiado grande.", Toast.LENGTH_LONG).show();
                    } else {
                        Toast.makeText(Perfil.this, "Error del servidor: " + statusCode, Toast.LENGTH_LONG).show();
                    }
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                btnEditProfile.setEnabled(true);
                Toast.makeText(Perfil.this, "Fallo de conexión: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private String hashearPassword(String txtPassword) {
        try {
            MessageDigest digest = MessageDigest.getInstance("SHA-256");
            byte[] hash = digest.digest(txtPassword.getBytes());
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

    private String obtenerImagenActualDePantalla() {
        try {
            if (ivPfp.getDrawable() == null) return null;

            Bitmap bitmap;
            // Si es un BitmapDrawable (normalmente lo es con Glide o setImageURI)
            if (ivPfp.getDrawable() instanceof BitmapDrawable) {
                bitmap = ((BitmapDrawable) ivPfp.getDrawable()).getBitmap();
            } else {
                return null;
            }

            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            bitmap.compress(Bitmap.CompressFormat.JPEG, 70, baos);
            byte[] imageBytes = baos.toByteArray();
            return Base64.encodeToString(imageBytes, Base64.NO_WRAP);

        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }

    private String convertirUriABase64(Uri uri) {
        try {
            InputStream imageStream = getContentResolver().openInputStream(uri);
            Bitmap selectedImage = BitmapFactory.decodeStream(imageStream);

            // Redimensionar a Max 800px para evitar Error 500
            int maxWidth = 800;
            int maxHeight = 800;
            if (selectedImage.getWidth() > maxWidth || selectedImage.getHeight() > maxHeight) {
                float scale = Math.min(((float)maxWidth / selectedImage.getWidth()), ((float)maxHeight / selectedImage.getHeight()));

                android.graphics.Matrix matrix = new android.graphics.Matrix();
                matrix.postScale(scale, scale);

                Bitmap resizedBitmap = Bitmap.createBitmap(selectedImage, 0, 0, selectedImage.getWidth(), selectedImage.getHeight(), matrix, true);
                selectedImage = resizedBitmap;
            }

            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            selectedImage.compress(Bitmap.CompressFormat.JPEG, 70, baos);
            byte[] imageBytes = baos.toByteArray();

            return Base64.encodeToString(imageBytes, Base64.NO_WRAP);

        } catch (Exception e) {
            e.printStackTrace();
            Toast.makeText(this, "Error al procesar imagen", Toast.LENGTH_SHORT).show();
            return null;
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
            return id == R.id.nav_perfil;
        });
    }
}