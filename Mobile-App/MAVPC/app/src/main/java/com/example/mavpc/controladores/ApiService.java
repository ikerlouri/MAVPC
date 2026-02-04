package com.example.mavpc.controladores;

// modelos
import com.example.mavpc.modelos.Camara;
import com.example.mavpc.modelos.Incidencia;
import com.example.mavpc.modelos.Usuario;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.PUT;
import retrofit2.http.Query;

public interface ApiService {
    // solo se pone la parte final de la url de la API

    //incidencia
    @GET("incidencias/listarActual")
    Call<List<Incidencia>> obtenerIncidenciasHoy();
    @GET("incidencias/byDate")
    Call<List<Incidencia>> obtenerIncidenciasFecha(
            @Query("anio") String anio,
            @Query("mes") String mes,
            @Query("dia") String dia
    );
    @POST("incidencias")
    Call<Void> crearIncidencia(@Body Incidencia incidencia);

    //camara
    @GET("camaras")
    Call<List<Camara>> obtenerCamaras();
    @GET("usuarios/favoritos")
    Call<List<Camara>> cargarCamsFavoritasUsuario(@Query("idUsuario") int idUsuario);
    @POST("usuarios/favoritos")
    Call<Void> guardarCamFavorita(@Body int idCamara, int idUsuario);
    @DELETE("usuarios/favoritos")
    Call<Void> eliminarCamFavorita(@Body int idCamaraUsuario);


    //usuario
    @GET("usuarios/comprobarUsuario")
    Call<Boolean> comprobarUsuarioLogin(
            @Query("usuario") String usuario,
            @Query("contrasena") String contrasena
    );
    @GET("usuarios/comprobarUsuarioEmail")
    Call<Boolean> comprobarUsuarioRegistro(
            @Query("usuario") String usuario,
            @Query("email") String email
    );
    @POST("usuarios")
    Call<Void> registrarUsuario(@Body Usuario usuario);
    @GET("usuarios/buscarUsuarioPorNombre")
    Call<Usuario> cargarUsuarioPorUsername(@Query("usuario") String username);
    @PUT("usuarios")
    Call<Void> actualizarUsuario(@Body Usuario usuario);
    @DELETE("usuarios")
    Call<Void> eliminarUsuario(@Body int idUsuario);
}