package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.IncidenciaCreada;

//interfaz que devuelve todos los metodos Jpa de IncidenciaCreada
public interface IncidenciaCreadaDao extends JpaRepository<IncidenciaCreada, Integer> {
	
}
