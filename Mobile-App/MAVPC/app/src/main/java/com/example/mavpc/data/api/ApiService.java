package com.example.mavpc.data.api;

// modelos
import com.example.mavpc.model.CamFavoritaUsuario;
import com.example.mavpc.model.Camara;
import com.example.mavpc.model.Incidencia;
import com.example.mavpc.model.Usuario;

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

    //incidencias
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

    //camaras
    @GET("camaras")
    Call<List<Camara>> obtenerCamaras();
    @GET("usuarios/favoritos")
    Call<List<Camara>> cargarCamsFavoritasUsuario(@Query("idUsuario") int idUsuario);
    @POST("usuarios/favoritos")
    Call<Void> guardarCamFavorita(@Body CamFavoritaUsuario camFav);
    @DELETE("usuarios/favoritos")
    Call<Void> eliminarCamFavorita(
            @Query("idUsuario") int idUsuario,
            @Query("idCamara") int idCamara
    );

    //usuarios
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
    Call<Void> eliminarUsuario(@Query("idUsuario") int idUsuario);
}