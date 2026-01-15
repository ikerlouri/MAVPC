package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Incidencia;
import java.util.List;


public interface IncidenciaDao extends JpaRepository<Incidencia, Integer> {

	
	List<Incidencia> findByProvincia(String provincia);
	
}
