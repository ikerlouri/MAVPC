package com.example.mavpc.controladores;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.example.mavpc.R;
import com.example.mavpc.data.api.ApiService;
import com.example.mavpc.data.local.DbHelper;
import com.example.mavpc.model.Usuario;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class Registro extends BaseActivity {

    EditText etEmail, etUsername, etPassword;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.registro);

        etEmail = findViewById(R.id.etEmail);
        etUsername = findViewById(R.id.etUsername);
        etPassword = findViewById(R.id.etPassword);

        Button btnRegister = (Button) findViewById(R.id.btnRegister);
        btnRegister.setOnClickListener(v -> validarInputs());

        TextView tvRegister = (TextView) findViewById(R.id.tvLogin);
        tvRegister.setOnClickListener(v -> login());
    }

    private void validarInputs() {
        quitarFocoYTeclado();

        String txtEmail = etEmail.getText().toString();
        String txtUsername = etUsername.getText().toString();
        String txtPassword = etPassword.getText().toString();

        if (txtEmail.isEmpty() || txtUsername.isEmpty() || txtPassword.isEmpty()) {
            Toast.makeText(this, "Por favor, rellena todos los campos", Toast.LENGTH_SHORT).show();
            return;
        }

        if (!txtEmail.contains("@")) {
            Toast.makeText(this, "Introduce un email válido", Toast.LENGTH_SHORT).show();
            return;
        }

        comprobarDisponibilidad(txtUsername, txtEmail, txtPassword);
    }

    private void comprobarDisponibilidad(String usuario, String email, String password) {
        String BASE_URL = "https://mavpc.up.railway.app/api/";

        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();

        ApiService service = retrofit.create(ApiService.class);

        // llamada a la api
        Call<Boolean> call = service.comprobarUsuarioRegistro(usuario, email);
        call.enqueue(new Callback<Boolean>() {
            @Override
            public void onResponse(Call<Boolean> call, Response<Boolean> response) {
                if (response.isSuccessful() && response.body() != null) {
                    boolean yaExiste = response.body();

                    if (yaExiste) {
                        Toast.makeText(Registro.this, "Este usuario o email ya están registrados", Toast.LENGTH_LONG).show();
                    } else {
                        registrarUsuario(service, usuario, email, password);
                    }
                } else {
                    Toast.makeText(Registro.this, "Error comprobando datos", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Boolean> call, Throwable t) {
                Toast.makeText(Registro.this, "Fallo de conexión: " + t.getMessage(), Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void registrarUsuario(ApiService service, String usuario, String email, String password) {
        Usuario nuevoUsuario = new Usuario();
        nuevoUsuario.setUsername(usuario);
        nuevoUsuario.setEmail(email);

        String hashedPass = hashearPassword(password);
        nuevoUsuario.setPassword(hashedPass);

        Call<Void> callRegistro = service.registrarUsuario(nuevoUsuario);

        callRegistro.enqueue(new Callback<Void>() {
            @Override
            public void onResponse(Call<Void> call, Response<Void> response) {
                // solo comprobamos el código HTTP (200-299)
                if (response.isSuccessful()) {
                    Toast.makeText(Registro.this, "¡Registro completado!", Toast.LENGTH_LONG).show();

                    // LLAMADA PARA OBTENER EL USUARIO COMPLETO (ID, PFP, ETC.)
                    Call<Usuario> callGetUsuario = service.cargarUsuarioPorUsername(usuario);

                    callGetUsuario.enqueue(new Callback<Usuario>() {
                        @Override
                        public void onResponse(Call<Usuario> callUser, Response<Usuario> responseUser) {
                            if (responseUser.isSuccessful() && responseUser.body() != null) {
                                Usuario usuarioDesdeApi = responseUser.body();

                                // IMPORTANTE: La API devuelve el user con el ID correcto, pero
                                // seguramente con la pass hasheada o null.
                                // Le ponemos la pass plana para el auto-login local.
                                usuarioDesdeApi.setPassword(password);

                                // Guardamos en SQLite
                                DbHelper dbHelper = new DbHelper(Registro.this);
                                dbHelper.insertUsuarioSesion(usuarioDesdeApi);

                                Toast.makeText(Registro.this, "¡Bienvenido " + usuarioDesdeApi.getUsername() + "!", Toast.LENGTH_LONG).show();

                                // Navegamos
                                Intent intent = new Intent(Registro.this, Explorar.class);
                                startActivity(intent);
                                finish(); // Cerramos registro

                            } else {
                                // Registro bien, pero falló la descarga de datos
                                Toast.makeText(Registro.this, "Usuario creado, pero error al recuperar datos. Logéate manualmente.", Toast.LENGTH_LONG).show();
                                finish();
                            }
                        }

                        @Override
                        public void onFailure(Call<Usuario> callUser, Throwable t) {
                            Log.e("API_ERROR", "Error recuperando usuario: " + t.getMessage());
                            Toast.makeText(Registro.this, "Error de red tras registro", Toast.LENGTH_SHORT).show();
                        }
                    });
                } else {
                    // error del servidor (500 o 400)
                    Toast.makeText(Registro.this, "Error al crear usuario: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Void> call, Throwable t) {
                Log.e("ERROR_REGISTRO", "Causa del fallo: " + t.getMessage());
                Toast.makeText(Registro.this, "Error de red al registrar", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private String hashearPassword(String password) {
        try {
            // crear instancia de SHA-256
            MessageDigest digest = MessageDigest.getInstance("SHA-256");

            // pasar a byes y hashear
            byte[] hash = digest.digest(password.getBytes());

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

    private void quitarFocoYTeclado() {
        View view = this.getCurrentFocus();

        // Si hay algo con foco
        if (view != null) {
            //Quitar el foco (el cursor desaparece)
            view.clearFocus();

            //Esconder el teclado
            InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
            imm.hideSoftInputFromWindow(view.getWindowToken(), 0);
        }
    }

    private void login() {
        Intent intent = new Intent(Registro.this, Login.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
        startActivity(intent);
        overridePendingTransition(0, 0);

        finish();
    }
}