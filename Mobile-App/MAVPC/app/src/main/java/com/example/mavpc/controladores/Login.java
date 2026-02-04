package com.example.mavpc.controladores;

import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.example.mavpc.R;
import com.example.mavpc.database.DbHelper;
import com.example.mavpc.modelos.Camara;
import com.example.mavpc.modelos.Usuario;

import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class Login extends BaseActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.login);

        Button btnLogin = (Button) findViewById(R.id.btnLogin);
        btnLogin.setOnClickListener(v -> login());

        TextView tvRegister = (TextView) findViewById(R.id.tvRegister);
        tvRegister.setOnClickListener(v -> register());
    }

    private void login() {
        quitarFocoYTeclado();

        TextView etUsername = findViewById(R.id.etUsername);
        TextView etPassword = findViewById(R.id.etPassword);

        String username = etUsername.getText().toString();
        String password = etPassword.getText().toString();

        if (username.isEmpty() || password.isEmpty()) {
            Toast.makeText(this, "Por favor rellena los campos", Toast.LENGTH_SHORT).show();
            return;
        }

        String hashedPass = hashearPassword(password);

        String BASE_URL = "https://mavpc.up.railway.app/api/";
        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        ApiService service = retrofit.create(ApiService.class);

        // Comprobar Login
        Call<Boolean> call = service.comprobarUsuarioLogin(username, hashedPass);
        call.enqueue(new Callback<Boolean>() {
            @Override
            public void onResponse(Call<Boolean> call, Response<Boolean> response) {
                if (response.isSuccessful() && response.body() != null) {
                    if (response.body()) { // Si el usuario es correcto
                        // Cargar Usuario
                        Call<Usuario> callUser = service.cargarUsuarioPorUsername(username);
                        callUser.enqueue(new Callback<Usuario>() {
                            @Override
                            public void onResponse(Call<Usuario> call, Response<Usuario> responseUser) {
                                if (responseUser.isSuccessful() && responseUser.body() != null) {
                                    Usuario usuarioLogueado = responseUser.body();

                                    // Para que en el dispositivo se trabaje con la contraseña base y no hasheada
                                    usuarioLogueado.setPassword(password);

                                    // Guardamos usuario en SQLite
                                    DbHelper dbHelper = new DbHelper(Login.this);
                                    dbHelper.insertUsuarioSesion(usuarioLogueado);

                                    // Cargar Cámaras Favoritas
                                    Call<List<Camara>> callCams = service.cargarCamsFavoritasUsuario(usuarioLogueado.getId());

                                    callCams.enqueue(new Callback<List<Camara>>() {
                                        @Override
                                        public void onResponse(Call<List<Camara>> call, Response<List<Camara>> responseCams) {
                                            if (responseCams.isSuccessful() && responseCams.body() != null) {
                                                dbHelper.insertCamList(responseCams.body());
                                            }

                                            irAExplorar();
                                        }

                                        @Override
                                        public void onFailure(Call<List<Camara>> call, Throwable t) {
                                            // Si falla internet al cargar cámaras, entramos igual
                                            Log.e("LOGIN", "Fallo al cargar cámaras favoritas: " + t.getMessage());
                                            irAExplorar();
                                        }
                                    });

                                } else {
                                    Toast.makeText(Login.this, "Error al descargar datos del usuario", Toast.LENGTH_SHORT).show();
                                }
                            }

                            @Override
                            public void onFailure(Call<Usuario> call, Throwable t) {
                                Toast.makeText(Login.this, "Fallo al recuperar perfil", Toast.LENGTH_SHORT).show();
                            }
                        });

                    } else {
                        Toast.makeText(Login.this, "Usuario o contraseña incorrectos", Toast.LENGTH_SHORT).show();
                    }
                } else {
                    Toast.makeText(Login.this, "Error servidor: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<Boolean> call, Throwable t) {
                Toast.makeText(Login.this, "Fallo de conexión", Toast.LENGTH_SHORT).show();
            }
        });
    }

    // metodo auixiliar para no repetir codigo
    private void irAExplorar() {
        Intent intent = new Intent(Login.this, Explorar.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
        startActivity(intent);
        overridePendingTransition(0, 0);
        finish();
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

    private void register() {
        Intent intent = new Intent(Login.this, Registro.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
        startActivity(intent);
        overridePendingTransition(0, 0);

        finish();
    }
}