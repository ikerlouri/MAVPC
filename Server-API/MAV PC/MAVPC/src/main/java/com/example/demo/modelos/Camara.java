package com.example.demo.modelos;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.persistence.*;

// Entidad que guarda la info de las cámaras en la base de datos
@Entity
@Table(name = "camaras")
@JsonIgnoreProperties(ignoreUnknown = true) // Si la API trae datos extra que no usamos, los ignora
public class Camara {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @Column(name = "id")
    private Integer id; // Identificador único (se autogenera)

    // Mapeo: la API lo llama "cameraName", nosotros lo guardamos como "nombre"
    @JsonProperty("cameraName")
    private String nombre;

    @JsonProperty("urlImage")
    private String urlImage; // Enlace a la foto actual de la cámara

    // Coordenadas de ubicación
    @JsonProperty("latitude")
    private String latitud; 

    @JsonProperty("longitude")
    private String longitud;

    // Detalles de la ubicación física
    @JsonProperty("road")
    private String carretera;

    @JsonProperty("kilometer")
    private String kilometro;

    @JsonProperty("address")
    private String direccion;

    // --- Getters y Setters (Necesarios para acceder a los datos) ---

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getNombre() {
        return nombre;
    }

    public void setNombre(String nombre) {
        this.nombre = nombre;
    }

    public String getUrlImage() {
        return urlImage;
    }

    public void setUrlImage(String urlImage) {
        this.urlImage = urlImage;
    }

    public String getLatitud() {
        return latitud;
    }

    public void setLatitud(String latitud) {
        this.latitud = latitud;
    }

    public String getLongitud() {
        return longitud;
    }

    public void setLongitud(String longitud) {
        this.longitud = longitud;
    }

    public String getCarretera() {
        return carretera;
    }

    public void setCarretera(String carretera) {
        this.carretera = carretera;
    }

    public String getKilometro() {
        return kilometro;
    }

    public void setKilometro(String kilometro) {
        this.kilometro = kilometro;
    }

    public String getDireccion() {
        return direccion;
    }

    public void setDireccion(String direccion) {
        this.direccion = direccion;
    }
}