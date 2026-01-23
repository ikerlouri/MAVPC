package com.example.demo.controllers;

import java.time.LocalDate;
import java.util.ArrayList;
import java.util.List;

import org.modelmapper.ModelMapper;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import com.example.demo.TraficoService;
import com.example.demo.daos.IncidenciaCreadaDao;
import com.example.demo.daos.IncidenciaDao;
import com.example.demo.modelos.Incidencia;
import com.example.demo.modelos.IncidenciaCreada;

@RestController
@RequestMapping("/api/incidencias")
public class IncidenciaController {

    @Autowired
    private TraficoService traficoService;
    @Autowired
    private IncidenciaDao incidenciaDao;
    @Autowired
    private IncidenciaCreadaDao incidenciaCreadaDao;
    @Autowired
    private ModelMapper modelMapper;
    
    @GetMapping
    public Object verTrafico() {
        return traficoService.obtenerTodasIncidencias();
    }
    
  //guardar incidencias creadas
    @PostMapping("/guardar")
    public void guardarIncidencia(@RequestBody Incidencia incidencia) {
        IncidenciaCreada nuevaIncidencia = modelMapper.map(incidencia, IncidenciaCreada.class);     
        incidenciaCreadaDao.save(nuevaIncidencia);
    }
    
    //guardar las de este año
    @GetMapping("/sincronizar")
    public String sincronizarTodo() {
        try {
            traficoService.SubirIncidenciasDelDia();
            return "Sincronización completa finalizada con éxito.";
        } catch (Exception e) {
            return "Error durante la sincronización: " + e.getMessage();
        }
    }
    //Listar incidencias pasandole año, mes y dia. URL ej.()/listarIncidencias?anio=2026&mes=01&dia=15
    @GetMapping("/listar/byDate")
    public List<Incidencia> listarIncidenciasPorFecha(
            @RequestParam String anio, 
            @RequestParam String mes, 
            @RequestParam String dia) {
        
        List<Incidencia> incidenciasApi = new ArrayList<>(traficoService.obtenerTodasIncidenciasDelDia(anio, mes, dia));
        List<Incidencia> incidenciasCreadas = incidenciaDao.findAll();
        
        incidenciasApi.addAll(incidenciasCreadas);
        
        return incidenciasApi;
    }
    
    @GetMapping("/eliminar")
    public void eliminarIncidencia(@RequestParam Integer id) {
        incidenciaDao.deleteById(id);
    }
    
    @GetMapping("/listarActual")
    public List<Incidencia> listarIncidenciasHoy() {
        LocalDate hoy = LocalDate.now();
        String anio = String.valueOf(hoy.getYear());
        String mes  = String.format("%02d", hoy.getMonthValue()); 
        String dia  = String.format("%02d", hoy.getDayOfMonth());

        // 1. Obtenemos incidencias de la API
        List<Incidencia> listaFinal = new ArrayList<>(
            traficoService.obtenerTodasIncidenciasDelDia(anio, mes, dia)
        );

        // 2. Obtenemos las de tu base de datos (IncidenciaCreada)
        List<IncidenciaCreada> deMiBaseDeDatos = incidenciaCreadaDao.findAll();
        System.out.println(deMiBaseDeDatos);
        // 3. Convertimos automáticamente usando ModelMapper
        List<Incidencia> convertidas = deMiBaseDeDatos.stream()
            .map(incCreada -> modelMapper.map(incCreada, Incidencia.class))
            .toList();

        // 4. Juntamos ambas
        listaFinal.addAll(convertidas);

        return listaFinal;
    }
}
    
