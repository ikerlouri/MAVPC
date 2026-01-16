package com.example.mavpc.controladores;

// modelos
import com.example.mavpc.modelos.Camara;
import com.example.mavpc.modelos.Incidencia;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.GET;

public interface ApiService {
    // solo se pone la parte final de la url de la API
    @GET("incidencias/listarActual")
    Call<List<Incidencia>> obtenerIncidencias();

    @GET("camaras")
    Call<List<Camara>> obtenerCamaras();
}