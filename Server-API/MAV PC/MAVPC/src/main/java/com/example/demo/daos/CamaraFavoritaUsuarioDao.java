package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.CamaraFavoritaUsuario;
import java.util.List;


public interface CamaraFavoritaUsuarioDao extends JpaRepository<CamaraFavoritaUsuario, Integer>{

	List<CamaraFavoritaUsuario> findByIdUsuario(int idUsuario);
	
}
