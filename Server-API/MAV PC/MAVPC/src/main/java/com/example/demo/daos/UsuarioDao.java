package com.example.demo.daos;

import org.springframework.data.jpa.repository.JpaRepository;

import com.example.demo.modelos.Usuario;

//interfaz que devuelve todos los metodos Jpa de Usuario
public interface UsuarioDao extends JpaRepository<Usuario, Integer>{
 
	//Comprueba si un usuario existe con su usuario y contraseña
	boolean existsByUsuarioAndContrasena(String nombreUsuario, String password);

	//Comprueba si un usuario existe con su usuario y EMAIL
	boolean existsByUsuarioOrEmail(String nombreUsuario, String email);
	
	//Devuelve un usuario a traves de su nombre
	Usuario findByUsuario(String nombreUsuario);
}
