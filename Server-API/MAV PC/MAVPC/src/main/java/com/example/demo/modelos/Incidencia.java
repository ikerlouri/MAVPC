package com.example.demo.modelos;

import java.time.LocalDateTime;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "incidencias")
@JsonIgnoreProperties(ignoreUnknown = true)
public class Incidencia {
    
	//ID personal de la api para manejar las incidencias
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY) 
    @JsonIgnore
    @Column(name = "id")
    private int id;
    @JsonProperty("incidenceId")
    @Column(name = "incidence_id", unique = true, nullable = false)
    private String incidenceId;  // ID de la API (para detectar duplicados)
    
    //tipo de incidencias
   @JsonProperty("incidenceType")
    private String tipo;
   
   //Region autonoma donde ha ocurrido la incidencia
   @JsonProperty("autonomousRegion")
    private String regionAutonoma;
   
   //Provincia donde ha ocurrido la incidencia
   @JsonProperty("province")
    private String provincia;
   
   //
   @JsonProperty("cause")
    private String causa;
   @JsonProperty("cityTown")
    private String ciudadPueblo;
   @JsonProperty("startDate")
    private LocalDateTime fecha_inicio;
   @JsonProperty("incidenceLevel")
    private String nivelIncidencia;
   @JsonProperty("road")
    private String carretera;
   @JsonProperty("direction")
    private String direccion;
   @JsonProperty("latitude")
    private Double latitud;
   @JsonProperty("longitude")
    private Double longitud;                                      

   
    // Constructor vacío obligatorio para JPA
    public Incidencia() {}


	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public String getIncidenceId() {
		return incidenceId;
	}

	public void setIncidenceId(String incidenceId) {
		this.incidenceId = incidenceId;
	}


	public String getTipo() {
		return tipo;
	}

	public void setTipo(String tipo) {
		this.tipo = tipo;
	}

	public String getRegionAutonoma() {
		return regionAutonoma;
	}

	public void setRegionAutonoma(String regionAutonoma) {
		this.regionAutonoma = regionAutonoma;
	}

	public String getProvincia() {
		return provincia;
	}

	public void setProvincia(String provincia) {
		this.provincia = provincia;
	}

	public String getCausa() {
		return causa;
	}

	public void setCausa(String causa) {
		this.causa = causa;
	}

	public String getCiudadPueblo() {
		return ciudadPueblo;
	}

	public void setCiudadPueblo(String ciudadPueblo) {
		this.ciudadPueblo = ciudadPueblo;
	}

	public LocalDateTime getFecha_inicio() {
		return fecha_inicio;
	}

	public void setFecha_inicio(LocalDateTime fecha_inicio) {
		this.fecha_inicio = fecha_inicio;
	}

	public String getNivelIncidencia() {
		return nivelIncidencia;
	}

	public void setNivelIncidencia(String nivelIncidencia) {
		this.nivelIncidencia = nivelIncidencia;
	}

	public String getCarretera() {
		return carretera;
	}

	public void setCarretera(String carretera) {
		this.carretera = carretera;
	}

	public String getDireccion() {
		return direccion;
	}

	public void setDireccion(String direccion) {
		this.direccion = direccion;
	}

	public Double getLatitud() {
		return latitud;
	}

	public void setLatitud(Double latitud) {
		this.latitud = latitud;
	}

	public Double getLongitud() {
		return longitud;
	}

	public void setLongitud(Double longitud) {
		this.longitud = longitud;
	}
	
}