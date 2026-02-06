package com.example.mavpc.data.api;

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

// clase con los metodos que llaman a la api
public interface ApiService {
    // incidencias
    // devuelve las incidencias del dia
    @GET("incidencias/listarActual")
    Call<List<Incidencia>> obtenerIncidenciasHoy();

    // devuelve las incidencias de la fecha que se le pase
    @GET("incidencias/byDate")
    Call<List<Incidencia>> obtenerIncidenciasFecha(@Query("anio") String anio, @Query("mes") String mes, @Query("dia") String dia);

    // se inserta en la base de datos la incidencia que se le pase
    @POST("incidencias")
    Call<Void> crearIncidencia(@Body Incidencia incidencia);


    // camaras
    // devuelve una lista con todas las camaras
    @GET("camaras")
    Call<List<Camara>> obtenerCamaras();

    // devuelve una lista con todas las camaras favoritas del usuario cuyo id se le pase
    @GET("usuarios/favoritos")
    Call<List<Camara>> cargarCamsFavoritasUsuario(@Query("idUsuario") int idUsuario);

    // guarda la relacion de favorito de la camara y usuario que se le pase
    @POST("usuarios/favoritos")
    Call<Void> guardarCamFavorita(@Body CamFavoritaUsuario camFav);

    // elimina la camara que se le pase de la lista de favoritos del usuario que se le pase
    @DELETE("usuarios/favoritos")
    Call<Void> eliminarCamFavorita(@Query("idUsuario") int idUsuario, @Query("idCamara") int idCamara);


    // usuarios
    // devuelve un booleano que expresa si existe un usuario con el nombre y contraseña que se le pasen, se usa para logearse
    @GET("usuarios/comprobarUsuario")
    Call<Boolean> comprobarUsuarioLogin(@Query("usuario") String usuario, @Query("contrasena") String contrasena);

    // devuelve un booleano que expresa si existe un usuario con el nombre y mail que se le pasen, se usa para comprobacion si ya existe un usuario con esas credenciales a la hora de registrar uno nuevo
    @GET("usuarios/comprobarUsuarioEmail")
    Call<Boolean> comprobarUsuarioRegistro(@Query("usuario") String usuario, @Query("email") String email);

    // inserta en la base de datos el usuario dado
    @POST("usuarios")
    Call<Void> registrarUsuario(@Body Usuario usuario);

    // devuelve el usuario que tenga ese nombre si es que lo hay en la base de datos
    @GET("usuarios/buscarUsuarioPorNombre")
    Call<Usuario> cargarUsuarioPorUsername(@Query("usuario") String username);

    // actualiza la informacion del usuario dado
    @PUT("usuarios")
    Call<Void> actualizarUsuario(@Body Usuario usuario);

    // elimina de la base de datos el usuario dado
    @DELETE("usuarios")
    Call<Void> eliminarUsuario(@Query("idUsuario") int idUsuario);
}