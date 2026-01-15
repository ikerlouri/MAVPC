package com.example.demo.controllers;

import java.util.List;
import java.util.Map;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.demo.TraficoService;
import com.example.demo.daos.CamaraDao;
import com.example.demo.modelos.Camara;

@RestController
@RequestMapping("/api/camaras")
public class CamaraController {

    @Autowired
    private TraficoService traficoService;
    @Autowired
    private CamaraDao camaraDao;
    
    @GetMapping
    public List<Camara> listarCamaras() {
        return camaraDao.findAll();
    }
    
    @GetMapping("/buscar/{provincia}")
    public List<Map<String, Object>> buscar(@PathVariable String provincia) {
        return traficoService.obtenerIncidenciasPorProvincia(provincia);
    }
    
    @PostMapping("/guardar")
    public String guardarCamara(@RequestBody Camara camara) {
    	try {
            camaraDao.save(camara);
            return "Sincronización completa finalizada con éxito.";
        } catch (Exception e) {
            return "Error durante la sincronización: " + e.getMessage();
        }
    }
    
}