package com.example.demo.servicios;

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
    @Autowired
    private RestTemplate restTemplate;
    
    private final String BASE_URL = "https://api.euskadi.eus/traffic/v1.0";

    public Object obtenerTodasIncidencias() {
        return restTemplate.getForObject(BASE_URL + "/incidences", Object.class);
    }
    
    
    public List<Incidencia> obtenerTodasIncidenciasDelDia(String anio, String mes, String dia) {
        List<Incidencia> todasLasDelDia = new ArrayList<>();
        int paginaActual = 1;
        int totalPaginas = 1;
        ObjectMapper mapper = new ObjectMapper();

        try {
            // Bucle para recorrer todas las páginas disponibles en la API
            do {
                String url = BASE_URL + "/incidences/byDate/" + anio + "/" + mes + "/" + dia + "?_page=" + paginaActual;
                
                // Obtenemos la respuesta como un Nodo JSON genérico para poder inspeccionarlo antes de convertir
                JsonNode root = restTemplate.getForObject(url, JsonNode.class);

                if (root != null && root.has("incidences")) {
                    // Solo en la primera vuelta leemos cuántas páginas hay en total
                    if (paginaActual == 1 && root.has("totalPages")) {
                        totalPaginas = root.get("totalPages").asInt();
                    }

                    // Extraemos el array "incidences" del JSON
                    JsonNode nodes = root.get("incidences");
                    
                    // Convertimos ese nodo JSON a una Lista de objetos Java 'Incidencia'
                    List<Incidencia> paginaLista = mapper.convertValue(
                        nodes, 
                        new TypeReference<List<Incidencia>>() {}
                    );

                    todasLasDelDia.addAll(paginaLista);
                    
                    paginaActual++;
                } else {
                    break; // Si no hay incidencias o respuesta, salimos
                }
            } while (paginaActual <= totalPaginas);

        } catch (Exception e) {
            System.err.println("Error al obtener incidencias del día paginadas: " + e.getMessage());
        }

        return todasLasDelDia;
    }


    public List<Camara> obtenerCamaras() {
        try {
            // Realiza una petición GET a la API
            JsonNode root = restTemplate.getForObject("https://api.euskadi.eus/traffic/v1.0/cameras?_pageSize=1000", JsonNode.class);
            
            // Verifica que la respuesta no sea nula y contenga el nodo "cameras"
            if (root != null && root.has("cameras")) {
                JsonNode camerasNode = root.get("cameras");
                ObjectMapper mapper = new ObjectMapper();
                
                // Mapea el nodo JSON "cameras" a una lista de objetos Java de tipo Camara
                return mapper.readerFor(new TypeReference<List<Camara>>() {})
                             .readValue(camerasNode);
            }
        } catch (Exception e) {
            // Registra cualquier fallo durante la petición o la conversión de datos
            System.out.println("Error al obtener o deserializar cámaras: " + e.getMessage());
        }
        // Retorna una lista vacía en lugar de null para evitar errores en quien llame a la función
        return Collections.emptyList();
    }
    
    //Metodo para subir las camaras de la api a nuestra base de datos
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

                //Recoge la pagina del Json solo si tiene camaras
                if (root != null && root.has("cameras")) {
                    if (paginaActual == 1) {
                        totalPaginas = root.get("totalPages").asInt();
                        System.out.println("Total de páginas detectadas: " + totalPaginas);
                    }

                    JsonNode camerasNode = root.get("cameras");
                    ObjectMapper mapper = new ObjectMapper();
                    
                    //Mapea el JSON con camaras y los convierte en el objeto camara en Java
                    List<Camara> listaPagina = mapper.convertValue(
                        camerasNode, 
                        new TypeReference<List<Camara>>() {}
                    );

                    List<Camara> camarasParaGuardar = new ArrayList<>();

                    for (Camara camara : listaPagina) {
                        
                        // Reseteamos ID a null para que la BD genere uno nuevo (autoincrement)
                        camara.setId(null); 

                        //Actualiza dominios obsoletos
                        String urlCamara = camara.getUrlImage();
                        if (urlCamara != null && urlCamara.contains(dominioAntiguo)) {
                            String nuevaUrl = urlCamara.replace(dominioAntiguo, dominioNuevo);
                            camara.setUrlImage(nuevaUrl);
                            urlCamara = nuevaUrl;
                        }

                        //Solo guardamos si la URL responde (evita camaras que no esten operativas en la app)
                        if (urlCamara != null && esUrlValida(urlCamara)) {
                            camarasParaGuardar.add(camara);
                        }
                    }

                    //Guardamos todas las camaras en la base
                    if (!camarasParaGuardar.isEmpty()) {
                        camaraDao.saveAll(camarasParaGuardar);
                        System.out.println("Guardada página " + paginaActual + ". Insertadas: " + camarasParaGuardar.size());
                    } else {
                        System.out.println("Página " + paginaActual + " procesada, pero ninguna cámara tenía imagen válida.");
                    }

                    paginaActual++;
                    
                } else {
                    break;
                }
            } catch (Exception e) {
                System.err.println("Error en la página " + paginaActual + ": " + e.getMessage());
            }

        } while (paginaActual <= totalPaginas);
        
        System.out.println("Sincronización finalizada con éxito.");
    }

    //funcion para comprobar si una camara esta operativa teniendo en cuenta que el servidor responde un 200
    private boolean esUrlValida(String urlString) {
        try {
            URI uri = new URI(urlString);
            URL url = uri.toURL();
            
            HttpURLConnection huc = (HttpURLConnection) url.openConnection();
            
            huc.setRequestMethod("HEAD"); // Clave para rendimiento
            huc.setConnectTimeout(2000);  // Si tarda más de 2s, asumimos que está caída
            huc.setReadTimeout(2000);
            
            int responseCode = huc.getResponseCode();
            
            // Devuelve true solo si el servidor responde "200 OK"
            return responseCode == HttpURLConnection.HTTP_OK; 
            
        } catch (Exception e) {
            return false;
        }
    }
    
    //funcion que se ejecuta cada cierto tiempo que guarda las incidencias en la base
    @Scheduled(fixedRate = 900000) // Se ejecuta 15 minutos
    public void SubirIncidenciasDelDia() {
        // Cálculo de la fecha de hoy para construir la URL dinámica
        LocalDate hoy = LocalDate.now();
        String anio = String.valueOf(hoy.getYear());
        String mes = String.format("%02d", hoy.getMonthValue());
        String dia = String.format("%02d", hoy.getDayOfMonth());
        
        int paginaActual = 1;
        int totalPaginas = 1;
        int nuevasIncidencias = 0;
        int duplicadasTotal = 0;
        
        //Url de la API de Open data donde recoge las incidencias pasandole año, mes y dia
        String URL_BASE = "https://api.euskadi.eus/traffic/v1.0/incidences/byDate/" 
                          + anio + "/" + mes + "/" + dia;

        //Objeto para mapear las incidencias
        ObjectMapper mapper = new ObjectMapper(); 

        do {
        	//URL que indica la pagina del JSON de la API
            String urlConPagina = URL_BASE + "?_page=" + paginaActual;
            
            try {
                JsonNode root = restTemplate.getForObject(urlConPagina, JsonNode.class);

                if (root != null && root.has("incidences")) {
                    // Detectar total de páginas solo en la primera iteración
                    if (paginaActual == 1) {
                        totalPaginas = root.get("totalPages").asInt();
                        System.out.println("Sincronizando: " + dia + "/" + mes + "/" + anio);
                    }

                    //Convierte el JSON de incidencias en objetos de Java
                    JsonNode incidenciasNode = root.get("incidences");
                    List<Incidencia> listaPagina = mapper.convertValue(
                        incidenciasNode,
                        new TypeReference<List<Incidencia>>() {}
                    );

                    List<Incidencia> incidenciasAInsertar = new ArrayList<>();
                    int duplicadasEnPagina = 0;

                    // FILTRADO DE DUPLICADOS:
                    // Iteramos lo que viene de la API y preguntamos a la base de datos si ya tiene ese ID específico.
                    for (Incidencia incidencia : listaPagina) {
                        //evita errores de Primary Key duplicada
                        if (!incidenciaDao.existsByIncidenceId(incidencia.getIncidenceId())) {
                            incidenciasAInsertar.add(incidencia);
                        } else {
                            duplicadasEnPagina++;
                        }
                    }

                    // Solo llamamos a la base de datos si hay algo nuevo que guardar
                    if (!incidenciasAInsertar.isEmpty()) {
                        incidenciaDao.saveAll(incidenciasAInsertar);
                        nuevasIncidencias += incidenciasAInsertar.size();
                    }
                    
                    duplicadasTotal += duplicadasEnPagina;
                    System.out.println("Página " + paginaActual + "/" + totalPaginas 
                                       + " Nuevas: " + incidenciasAInsertar.size() 
                                       + " | Omitidas: " + duplicadasEnPagina);
                    //Pasa a la siguiente pagina
                    paginaActual++;
                    Thread.sleep(100); // Pausa de 100ms
                } else {
                    break;
                }
            } catch (Exception e) {
                System.err.println("Error en página " + paginaActual + ": " + e.getMessage());
                break; // Si falla una página, rompemos el bucle para evitar bucles infinitos
            }

        } while (paginaActual <= totalPaginas);

        System.out.println("Sincronización finalizada. Nuevas: " + nuevasIncidencias + " | Duplicadas: " + duplicadasTotal);
    }
}