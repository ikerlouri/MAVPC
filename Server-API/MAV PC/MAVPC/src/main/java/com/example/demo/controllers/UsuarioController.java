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
import com.example.demo.modelos.Camara;
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
	
	@GetMapping
	public List<Usuario> listarUsuarios() {
		return usuarioDao.findAll();
	}
	
	@DeleteMapping
	public void BorrarUsuario(@RequestParam int idUsuario) {
		usuarioDao.deleteById(idUsuario);
	}
	
	
	@PostMapping
	public void GuardarUsuario(@RequestBody Usuario usuario) {
		usuarioDao.save(usuario);
		eService.enviarCorreoBienvenida(usuario.getEmail(), usuario.getUsuario());
	}
	
	@PutMapping
	public void ActualizarUsuario(@RequestBody Usuario usuario) {
		usuarioDao.save(usuario);
	}
	
	@GetMapping("/favoritos")
	public List<CamaraFavoritaUsuario> listarFavoritos(@RequestParam int idUsuario) {	
		return camaraFavoritaUsuarioDao.findByIdUsuario(idUsuario) ; 
	}
	
	@PostMapping("/favoritos")
	public void guardarFavoritos(
			@RequestParam int idCamara,
			@RequestParam int idUsuario) {
		
	CamaraFavoritaUsuario camaraFavoritaUsuario = new CamaraFavoritaUsuario();
	camaraFavoritaUsuario.setIdCamara(idCamara);
	camaraFavoritaUsuario.setIdUsuario(idUsuario);
	camaraFavoritaUsuarioDao.save(camaraFavoritaUsuario);	
	}
	
	@DeleteMapping("/favoritos")
	public void borrarFavoritos(@RequestParam int id) {
		camaraFavoritaUsuarioDao.deleteById(id);	
	}
 	
	@GetMapping("/comprobarUsuario")
	public boolean comprobarUsuario(@RequestParam String usuario, @RequestParam String contrasena) {
	return usuarioDao.existsByUsuarioAndContrasena(usuario, contrasena);
	}
	
	@GetMapping("/comprobarUsuarioEmail")
	public boolean comprobarUsuarioEmail(@RequestParam String usuario, @RequestParam String email) {
	return usuarioDao.existsByUsuarioOrEmail(usuario, email);
	}
	
	@GetMapping("/buscarUsuarioPorNombre")
	public Usuario buscarUsuarioPorNombre(@RequestParam String usuario) {
		return usuarioDao.findByUsuario(usuario);
	}
	
}
