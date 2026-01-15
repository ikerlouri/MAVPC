package com.example.demo.modelos;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import jakarta.persistence.*;

@Entity
@Table(name = "camaras")
@JsonIgnoreProperties(ignoreUnknown = true)
public class Camara {

    @Id
    @JsonProperty("cameraId")
    @Column(name = "id")
    private String id; 

    @JsonProperty("cameraName")
    private String nombre;

    @JsonProperty("urlImage")
    private String urlImage;

    @JsonProperty("latitude")
    private String latitud; 

    @JsonProperty("longitude")
    private String longitud;

    @JsonProperty("road")
    private String carretera;

    @JsonProperty("kilometer")
    private String kilometro;

    @JsonProperty("address")
    private String direccion;

	public String getId() {
		return id;
	}

	public void setId(String id) {
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