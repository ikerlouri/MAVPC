package com.example.demo;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import com.example.demo.daos.IncidenciaDao; 
import com.example.demo.modelos.Incidencia;

@RestController
@RequestMapping("/api/incidencias")
public class IncidenciaController {

    @Autowired
    private TraficoService traficoService;
    @Autowired
    private IncidenciaDao incidenciaDao;
    
    @GetMapping
    public Object verTrafico() {
        return traficoService.obtenerDatosTrafico();
    }
	
    @GetMapping("/listarBase")
    public List<Incidencia> listarIncidencias() {
        return incidenciaDao.findAll();
    }
    
    @PostMapping("/guardarIncidencias")
    public void guardarIncidencia(@RequestBody Incidencia incidencia) {
    	incidenciaDao.save(incidencia);
    }

    
}