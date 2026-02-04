package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Incidencia;

//interfaz que devuelve todos los metodos Jpa de Incidencia
public interface IncidenciaDao extends JpaRepository<Incidencia, Integer> {
	
	//Comprueba que en la base dos incidencias no tenga las misma id de incidencia 
	boolean existsByIncidenceId(String incidenceId);
}
