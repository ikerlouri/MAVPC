package com.example.demo.modelos;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import com.fasterxml.jackson.annotation.JsonProperty;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.Table;

@Entity
@Table(name = "camarasfavoritas_usuarios")
@JsonIgnoreProperties(ignoreUnknown = true)
public class CamaraFavoritaUsuario {
	
	@Id
	@GeneratedValue(strategy = GenerationType.IDENTITY)
	@JsonProperty(access = JsonProperty.Access.READ_ONLY)
	@Column(name = "id")
	private Integer id;
	
	@Column(name = "id_camara")
	private Integer idCamara;
	
	@Column(name = "id_usuario")
	private Integer idUsuario;

	public CamaraFavoritaUsuario() {
	}

	public Integer getId() {
		return id;
	}

	public void setId(Integer id) {
		this.id = id;
	}

	public Integer getIdCamara() {
		return idCamara;
	}

	public void setIdCamara(Integer idCamara) {
		this.idCamara = idCamara;
	}

	public Integer getIdUsuario() {
		return idUsuario;
	}

	public void setIdUsuario(Integer idUsuario) {
		this.idUsuario = idUsuario;
	}

}
