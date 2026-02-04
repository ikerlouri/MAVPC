package com.example.demo.controllers;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.example.demo.daos.CamaraDao;
import com.example.demo.modelos.Camara;
import com.example.demo.servicios.TraficoService;

@RestController
@RequestMapping("/api/camaras")
public class CamaraController {

    @Autowired
    private TraficoService traficoService;
    @Autowired
    private CamaraDao camaraDao;
    
    //lista todas las camaras de la bbdd
    @GetMapping
    public List<Camara> listarCamaras() {
        return camaraDao.findAll();
    }
    
    //elimina una camara pasandole una id
    @DeleteMapping
    public void eliminarCamaras(@RequestParam int id) {
        camaraDao.deleteById(id);
    }
    
    //guarda una camara pasandole un objeto Camara
    @PostMapping
    public String guardarCamara(@RequestBody Camara camara) {
    	try {  		
            camaraDao.save(camara);
            return "Camara guardada con éxito.";
        } catch (Exception e) {
            return "Error durante la sincronización: " + e.getMessage();
        }
    }
    
    //Guarda las camaras que estan operativas de la API a nuestra base de datos
    @GetMapping("/sincronizar")
    public void sincronizar() {
    	traficoService.SubirCamaras();
    }
    
}