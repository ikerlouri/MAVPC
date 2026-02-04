package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Camara;

//interfaz que devuelve todos los metodos Jpa de Camara
public interface CamaraDao extends JpaRepository<Camara, Integer> { 
	
}