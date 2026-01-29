package com.example.demo;

import java.net.HttpURLConnection;
import java.net.URI;
import java.net.URL;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.scheduling.annotation.Scheduled;
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
    
    public List<Incidencia> obtenerTodasIncidenciasDelDia(String anio, String mes, String dia) {
        List<Incidencia> todasLasDelDia = new ArrayList<>();
        int paginaActual = 1;
        int totalPaginas = 1;
        ObjectMapper mapper = new ObjectMapper();

        try {
            do {
                // Construimos la URL con la fecha Y el parámetro de página
                String url = BASE_URL + "/incidences/byDate/" + anio + "/" + mes + "/" + dia + "?_page=" + paginaActual;
                
                JsonNode root = restTemplate.getForObject(url, JsonNode.class);

                if (root != null && root.has("incidences")) {
                    // En la primera página, leemos el total de páginas para esa fecha
                    if (paginaActual == 1 && root.has("totalPages")) {
                        totalPaginas = root.get("totalPages").asInt();
                    }

                    // Extraemos la lista de esta página
                    JsonNode nodes = root.get("incidences");
                    List<Incidencia> paginaLista = mapper.convertValue(
                        nodes, 
                        new TypeReference<List<Incidencia>>() {}
                    );

                    // Añadimos los resultados a la lista acumulada
                    todasLasDelDia.addAll(paginaLista);
                    
                    paginaActual++;
                } else {
                    break;
                }
            } while (paginaActual <= totalPaginas);

        } catch (Exception e) {
            System.err.println("Error al obtener incidencias del día paginadas: " + e.getMessage());
        }

        return todasLasDelDia;
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

        String dominioAntiguo = "https://www.trafikoa.eus";
        String dominioNuevo = "https://apps.trafikoa.euskadi.eus";

        do {
            String urlConPagina = URL_BASE_EUSKADI + "?_page=" + paginaActual;
            
            try {
                JsonNode root = restTemplate.getForObject(urlConPagina, JsonNode.class);

                if (root != null && root.has("cameras")) {
                    // 1. Obtener total de páginas
                    if (paginaActual == 1) {
                        totalPaginas = root.get("totalPages").asInt();
                        System.out.println("Total de páginas detectadas: " + totalPaginas);
                    }

                    // 2. Convertir JSON a Lista
                    JsonNode camerasNode = root.get("cameras");
                    ObjectMapper mapper = new ObjectMapper();
                    
                    List<Camara> listaPagina = mapper.convertValue(
                        camerasNode, 
                        new TypeReference<List<Camara>>() {}
                    );

                    // Lista temporal para almacenar SOLO las válidas
                    List<Camara> camarasParaGuardar = new ArrayList<>();

                    // --- BUCLE DE PROCESAMIENTO ---
                    for (Camara camara : listaPagina) {
                        
                        // A. Limpiar ID para autoincremento
                        camara.setId(null); 

                        // B. Lógica de corrección de URL (Hacer esto ANTES de validar)
                        String urlCamara = camara.getUrlImage();
                        if (urlCamara != null && urlCamara.contains(dominioAntiguo)) {
                            String nuevaUrl = urlCamara.replace(dominioAntiguo, dominioNuevo);
                            camara.setUrlImage(nuevaUrl);
                            urlCamara = nuevaUrl; // Actualizamos la variable local para usarla en la validación
                        }

                        // C. FILTRADO: Validar URL (No null y Status 200)
                        if (urlCamara != null && esUrlValida(urlCamara)) {
                            camarasParaGuardar.add(camara);
                        } else {
                            // Opcional: Log para saber cuáles se ignoran
                            // System.out.println("Ignorando cámara sin imagen válida: " + camara.getSourceId());
                        }
                    }
                    // ------------------------------

                    // 3. Guardar en BD solo las filtradas
                    if (!camarasParaGuardar.isEmpty()) {
                        camaraDao.saveAll(camarasParaGuardar);
                        System.out.println("Guardada página " + paginaActual + ". Insertadas: " + camarasParaGuardar.size() + " (Descartadas: " + (listaPagina.size() - camarasParaGuardar.size()) + ")");
                    } else {
                        System.out.println("Página " + paginaActual + " procesada, pero ninguna cámara tenía imagen válida.");
                    }

                    paginaActual++;
                    
                    // NOTA: Al verificar URLs una por una, el proceso será más lento.
                    // Thread.sleep(100); // Quizás ya no necesites sleep si la red hace de cuello de botella natural
                } else {
                    break;
                }
            } catch (Exception e) {
                System.err.println("Error en la página " + paginaActual + ": " + e.getMessage());
            }

        } while (paginaActual <= totalPaginas);
        
        System.out.println("Sincronización finalizada con éxito.");
    }

    /**
     * Método auxiliar para verificar si una URL devuelve código 200.
     * Usa HEAD para ser más rápido (no descarga la imagen).
     */
    private boolean esUrlValida(String urlString) {
        try {
            // ✅ Usar URI en lugar de URL directamente
            URI uri = new URI(urlString);
            URL url = uri.toURL();
            
            HttpURLConnection huc = (HttpURLConnection) url.openConnection();
            
            huc.setRequestMethod("HEAD");
            huc.setConnectTimeout(2000);
            huc.setReadTimeout(2000);
            
            int responseCode = huc.getResponseCode();
            
            return responseCode == HttpURLConnection.HTTP_OK; 
            
        } catch (Exception e) {
            return false;
        }
    }
    
    @Scheduled(fixedRate = 900000) // Cada 15 minutos
    public void SubirIncidenciasDelDia() {
        LocalDate hoy = LocalDate.now();
        String anio = String.valueOf(hoy.getYear());
        String mes = String.format("%02d", hoy.getMonthValue());
        String dia = String.format("%02d", hoy.getDayOfMonth());
        
        int paginaActual = 1;
        int totalPaginas = 1;
        int nuevasIncidencias = 0;
        int duplicadasTotal = 0;
        
        String URL_BASE = "https://api.euskadi.eus/traffic/v1.0/incidences/byDate/" 
                          + anio + "/" + mes + "/" + dia;

        ObjectMapper mapper = new ObjectMapper(); // Instanciar una sola vez fuera del bucle

        do {
            String urlConPagina = URL_BASE + "?_page=" + paginaActual;
            
            try {
                JsonNode root = restTemplate.getForObject(urlConPagina, JsonNode.class);

                if (root != null && root.has("incidences")) {
                    if (paginaActual == 1) {
                        totalPaginas = root.get("totalPages").asInt();
                        System.out.println("📅 Sincronizando: " + dia + "/" + mes + "/" + anio);
                    }

                    JsonNode incidenciasNode = root.get("incidences");
                    List<Incidencia> listaPagina = mapper.convertValue(
                        incidenciasNode,
                        new TypeReference<List<Incidencia>>() {}
                    );

                    List<Incidencia> incidenciasAInsertar = new ArrayList<>();
                    int duplicadasEnPagina = 0;

                    for (Incidencia incidencia : listaPagina) {
                        // COMPROBACIÓN: ¿Existe ya este incidence_id en la BD?
                        if (!incidenciaDao.existsByIncidenceId(incidencia.getIncidenceId())) {
                            incidenciasAInsertar.add(incidencia);
                        } else {
                            duplicadasEnPagina++;
                        }
                    }

                    if (!incidenciasAInsertar.isEmpty()) {
                        incidenciaDao.saveAll(incidenciasAInsertar);
                        nuevasIncidencias += incidenciasAInsertar.size();
                    }
                    
                    duplicadasTotal += duplicadasEnPagina;
                    System.out.println("📄 Página " + paginaActual + "/" + totalPaginas 
                                       + " -> Nuevas: " + incidenciasAInsertar.size() 
                                       + " | Omitidas: " + duplicadasEnPagina);

                    paginaActual++;
                    Thread.sleep(100); 

                } else {
                    break;
                }
            } catch (Exception e) {
                System.err.println("❌ Error en página " + paginaActual + ": " + e.getMessage());
                break;
            }

        } while (paginaActual <= totalPaginas);

        System.out.println("🏁 Sincronización finalizada. Nuevas: " + nuevasIncidencias + " | Duplicadas: " + duplicadasTotal);
    }
}