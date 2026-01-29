package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Incidencia;


public interface IncidenciaDao extends JpaRepository<Incidencia, Integer> {

	boolean existsByIncidenceId(String incidenceId);
}
