package com.example.demo;

import java.util.Collections;
import java.util.List;
import java.util.Map;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;

import com.example.demo.daos.CamaraDao;
import com.example.demo.daos.IncidenciaDao;
import com.example.demo.modelos.Camara;
import com.example.demo.modelos.Incidencia;

import tools.jackson.core.type.TypeReference;
import tools.jackson.databind.JsonNode;
import tools.jackson.databind.ObjectMapper;

@Service
public class TraficoService {

	@Autowired
    private CamaraDao camaraDao;
	@Autowired
    private IncidenciaDao incidenciaDao;
    private final RestTemplate restTemplate = new RestTemplate();
    private final String BASE_URL = "https://api.euskadi.eus/traffic/v1.0";

    // 1. Todas las incidencias 
    public Object obtenerTodasIncidencias() {
        return restTemplate.getForObject(BASE_URL + "/incidences", Object.class);
    }

    // 2. Incidencias por Provincia (Ej: "Gipuzkoa", "Bizkaia", "Araba")
    public List<Map<String, Object>> obtenerIncidenciasPorProvincia(String provincia) {
        // 1. Llamamos a la API de Euskadi y recibimos la lista completa
        // Usamos Map para manejar el JSON dinámico de la API
        Map<String, Object> respuesta = restTemplate.getForObject(BASE_URL + "/incidences", Map.class);
        
        // 2. Extraemos la lista de incidencias (suelen venir bajo la llave "incidences")
        List<Map<String, Object>> todas = (List<Map<String, Object>>) respuesta.get("incidences");

        // 3. Filtramos por el nombre de la provincia
        return todas.stream()
            .filter(i -> provincia.equalsIgnoreCase((String) i.get("province")))
            .toList();
    }

    // 3. Obtener Cámaras de tráfico
    public List<Camara> obtenerCamaras() {
        try {
            JsonNode root = restTemplate.getForObject("https://api.euskadi.eus/traffic/v1.0/cameras?_pageSize=1000", JsonNode.class);
            
            if (root != null && root.has("cameras")) {
                JsonNode camerasNode = root.get("cameras");
                
                ObjectMapper mapper = new ObjectMapper();
                // Usamos TypeReference para que Jackson sepa exactamente cómo manejar la lista
                return mapper.readerFor(new TypeReference<List<Camara>>() {})
                             .readValue(camerasNode);
            }
        } catch (Exception e) {
            System.out.println("Error al deserializar: " + e.getMessage());
        }
        return Collections.emptyList();
    }
    
    public void SubirCamaras() {
        int paginaActual = 1;
        int totalPaginas = 1;
        String URL_BASE_EUSKADI = "https://api.euskadi.eus/traffic/v1.0/cameras";

        do {
            // El parámetro correcto es _page (con guion bajo)
            String urlConPagina = URL_BASE_EUSKADI + "?_page=" + paginaActual;
            
            try {
                JsonNode root = restTemplate.getForObject(urlConPagina, JsonNode.class);

                if (root != null && root.has("cameras")) {
                    // 1. En la primera página, leemos cuántas hay en total
                    if (paginaActual == 1) {
                        totalPaginas = root.get("totalPages").asInt();
                        System.out.println("Total de páginas detectadas: " + totalPaginas);
                    }

                    // 2. Extraer y convertir la lista de cámaras de la página actual
                    JsonNode camerasNode = root.get("cameras");
                    ObjectMapper mapper = new ObjectMapper();
                    List<Camara> listaPagina = mapper.convertValue(
                        camerasNode, 
                        new TypeReference<List<Camara>>() {}
                    );

                    // 3. Guardar en BD (Hibernate hará Insert o Update según el ID)
                    camaraDao.saveAll(listaPagina);
                    System.out.println("Procesada página " + paginaActual + " de " + totalPaginas);

                    paginaActual++;
                    
                    // Opcional: una pequeña pausa para ser respetuosos con la API
                    Thread.sleep(100); 

                } else {
                    break;
                }
            } catch (Exception e) {
                System.err.println("Error en la página " + paginaActual + ": " + e.getMessage());
                break;
            }

        } while (paginaActual <= totalPaginas);
        
        System.out.println("Sincronización finalizada con éxito.");
    }
    public void SubirIncidencias2026() {
        int paginaActual = 1;
        int totalPaginas = 1;
        String URL_BASE_EUSKADI = "https://api.euskadi.eus/traffic/v1.0/incidences/byYear/2026";

        do {
            // El parámetro correcto es _page (con guion bajo)
            String urlConPagina = URL_BASE_EUSKADI + "?_page=" + paginaActual;
            
            try {
                JsonNode root = restTemplate.getForObject(urlConPagina, JsonNode.class);

                if (root != null && root.has("incidences")) {
                    // 1. En la primera página, leemos cuántas hay en total
                    if (paginaActual == 1) {
                        totalPaginas = root.get("totalPages").asInt();
                        System.out.println("Total de páginas detectadas: " + totalPaginas);
                    }

                    // 2. Extraer y convertir la lista de cámaras de la página actual
                    JsonNode incidenciasNode = root.get("incidences");
                    ObjectMapper mapper = new ObjectMapper();
                    List<Incidencia> listaPagina = mapper.convertValue(
                        incidenciasNode, 
                        new TypeReference<List<Incidencia>>() {}
                    );

                    // 3. Guardar en BD (Hibernate hará Insert o Update según el ID)
                    incidenciaDao.saveAll(listaPagina);
                    System.out.println("Procesada página " + paginaActual + " de " + totalPaginas);

                    paginaActual++;
                    
                    // Opcional: una pequeña pausa para ser respetuosos con la API
                    Thread.sleep(100); 

                } else {
                    break;
                }
            } catch (Exception e) {
                System.err.println("Error en la página " + paginaActual + ": " + e.getMessage());
                break;
            }

        } while (paginaActual <= totalPaginas);
        
        System.out.println("Sincronización finalizada con éxito.");
    }
}