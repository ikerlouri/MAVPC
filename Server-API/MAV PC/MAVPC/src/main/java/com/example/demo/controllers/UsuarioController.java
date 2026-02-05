package com.example.demo.controllers;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.example.demo.daos.CamaraFavoritaUsuarioDao;
import com.example.demo.daos.UsuarioDao;
import com.example.demo.modelos.CamaraFavoritaUsuario;
import com.example.demo.modelos.Usuario;
import com.example.demo.servicios.EmailSchedulerService;

@RestController
@RequestMapping("/api/usuarios")
public class UsuarioController {

	@Autowired
	private UsuarioDao usuarioDao;
	
	@Autowired
	private EmailSchedulerService eService;
	
	@Autowired
	private CamaraFavoritaUsuarioDao camaraFavoritaUsuarioDao;
	
	// Obtiene la lista completa de todos los usuarios registrados
	@GetMapping
	public List<Usuario> listarUsuarios() {
		return usuarioDao.findAll();
	}
	
	// Elimina un usuario de la base de datos según su ID
	@DeleteMapping
	public void BorrarUsuario(@RequestParam int idUsuario) {
		usuarioDao.deleteById(idUsuario);
	}
	
	// Registra un nuevo usuario y envía automáticamente un correo de bienvenida
	@PostMapping
	public void GuardarUsuario(@RequestBody Usuario usuario) {
		usuarioDao.save(usuario);
		eService.enviarCorreoBienvenida(usuario.getEmail(), usuario.getUsuario());
	}
	
	// Actualiza la información de un usuario existente
	@PutMapping
	public void ActualizarUsuario(@RequestBody Usuario usuario) {
		usuarioDao.save(usuario);
	}
	
	// Recupera todas las cámaras marcadas como favoritas por un usuario específico
	@GetMapping("/favoritos")
	public List<CamaraFavoritaUsuario> listarFavoritos(@RequestParam int idUsuario) {	
		return camaraFavoritaUsuarioDao.findByIdUsuario(idUsuario); 
	}
	
	// Vincula una cámara como favorita a un usuario concreto
	@PostMapping("/favoritos")
	public void guardarFavoritos(@RequestBody CamaraFavoritaUsuario camaraFavoritaUsuario) {
	camaraFavoritaUsuario.setId(null);
	camaraFavoritaUsuarioDao.save(camaraFavoritaUsuario);	
	}
	
	// Elimina una cámara de la lista de favoritos mediante el ID del registro
	@DeleteMapping("/favoritos")
	public void borrarFavoritos(@RequestParam int idUsuario, @RequestParam int idCamara ) {
		camaraFavoritaUsuarioDao.deleteByIdUsuarioAndIdCamara(idUsuario, idCamara);	
	}
 	
	// Valida las credenciales (login) comprobando si existen usuario y contraseña
	@GetMapping("/comprobarUsuario")
	public boolean comprobarUsuario(@RequestParam String usuario, @RequestParam String contrasena) {
	return usuarioDao.existsByUsuarioAndContrasena(usuario, contrasena);
	}
	
	// Verifica si un nombre de usuario o email ya están en uso (para registros)
	@GetMapping("/comprobarUsuarioEmail")
	public boolean comprobarUsuarioEmail(@RequestParam String usuario, @RequestParam String email) {
	return usuarioDao.existsByUsuarioOrEmail(usuario, email);
	}
	
	// Busca y retorna los datos completos de un usuario por su nombre de usuario
	@GetMapping("/buscarUsuarioPorNombre")
	public Usuario buscarUsuarioPorNombre(@RequestParam String usuario) {
		return usuarioDao.findByUsuario(usuario);
	}
	
}