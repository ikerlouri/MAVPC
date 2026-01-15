package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Camara;

public interface CamaraDao extends JpaRepository<Camara, String> { 
	
}