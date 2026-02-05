package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.CamaraFavoritaUsuario;
import java.util.List;

//interfaz que devuelve todos los metodos Jpa de CamaraFavoritaUsuario
public interface CamaraFavoritaUsuarioDao extends JpaRepository<CamaraFavoritaUsuario, Integer>{

	//Funcion que devuelve las camaras favoritas de un usuario
	List<CamaraFavoritaUsuario> findByIdUsuario(int idUsuario);
	
	@jakarta.transaction.Transactional
    void deleteByIdUsuarioAndIdCamara(int idUsuario, int idCamara);
	
	
	
}
