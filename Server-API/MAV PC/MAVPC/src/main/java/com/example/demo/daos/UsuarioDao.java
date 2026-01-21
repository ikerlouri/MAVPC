package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Usuario;
import java.util.List;


public interface UsuarioDao extends JpaRepository<Usuario, Integer>{
 
	//Comprueba si un usuario existe con su usuario y contraseña
	boolean existsByUsuarioAndContrasena(String nombreUsuario, String password);

	boolean existsByUsuarioOrEmail(String nombreUsuario, String email);
	
	Usuario findByUsuario(String usuario);
}
