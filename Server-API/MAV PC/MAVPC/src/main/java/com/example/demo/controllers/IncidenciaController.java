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
        return traficoService.obtenerTodasIncidencias();
    }
    
    @GetMapping("/buscar/{provincia}")
    public List<Map<String, Object>> buscar(@PathVariable String provincia) {
        return traficoService.obtenerIncidenciasPorProvincia(provincia);
    }
    
  //guardar incidencias creadas
    @PostMapping("/guardarIncidencias")
    public void guardarIncidencia(@RequestBody Incidencia incidencia) {
    	incidenciaDao.save(incidencia);
    } 
    
    //guardar las de este año
    @GetMapping("/sincronizar")
    public String sincronizarTodo() {
        try {
            traficoService.SubirIncidencias2026();
            return "Sincronización completa finalizada con éxito.";
        } catch (Exception e) {
            return "Error durante la sincronización: " + e.getMessage();
        }
    }
    
    @GetMapping()
    public List<Incidencia> listarIncidencias(){
		return null;
    	
    }
    
    //listar incidencias
    
    //listar incidencias creadas y del dia
    
}