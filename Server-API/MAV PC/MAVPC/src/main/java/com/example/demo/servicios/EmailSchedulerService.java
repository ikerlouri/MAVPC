package com.example.demo.servicios;

import org.springframework.stereotype.Service;
import org.springframework.web.client.RestTemplate;
import org.springframework.http.HttpEntity;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.scheduling.annotation.Async;

import java.util.HashMap;
import java.util.Map;
import java.util.List;
import java.util.ArrayList;

@Service
public class EmailSchedulerService {

	@org.springframework.beans.factory.annotation.Value("${BREVO_API_KEY}")
	private String brevoApiKey;
	
    private final String BREVO_API_URL = "https://api.brevo.com/v3/smtp/email";

    @Async // Mantenemos la asincronía para que sea rápido
    public void enviarCorreoBienvenida(String emailDestino, String nombreUsuario) {
        try {
            RestTemplate restTemplate = new RestTemplate();

            // 1. Configurar Cabeceras (Headers)
            HttpHeaders headers = new HttpHeaders();
            headers.setContentType(MediaType.APPLICATION_JSON);
            headers.set("api-key", brevoApiKey);

            // 2. Construir el cuerpo del JSON (Payload) manualmente con Mapas
            Map<String, Object> body = new HashMap<>();
            
            // Remitente
            Map<String, String> sender = new HashMap<>();
            sender.put("name", "Soporte MAVPC");
            sender.put("email", "mavpc1459@gmail.com"); // Puedes poner tu correo de registro en Brevo
            body.put("sender", sender);

            // Destinatario (es una lista)
            List<Map<String, String>> toList = new ArrayList<>();
            Map<String, String> toUser = new HashMap<>();
            toUser.put("email", emailDestino);
            toUser.put("name", nombreUsuario);
            toList.add(toUser);
            body.put("to", toList);

            // Asunto y Contenido
            body.put("subject", "Bienvenido a MAVPC");
            
            String htmlContent = "<html>" +
                "<body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>" +
                "<div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1);'>" +
                "<h2 style='color: #333;'>¡Hola, " + nombreUsuario + "!</h2>" +
                "<p style='font-size: 16px; color: #555;'>Gracias por registrarte en nuestra aplicación. Estamos encantados de tenerte con nosotros.</p>" +
                "<p style='font-size: 16px; color: #555;'>Tu cuenta ha sido creada con éxito y ya puedes empezar a explorar todas las funcionalidades que ofrecemos.</p>" +
                "<br>" +
                "<p style='font-size: 14px; color: #777;'>Si tienes alguna pregunta, no dudes en contactarnos.</p>" +
                "<p style='font-size: 14px; color: #777;'>Saludos,<br>El equipo de MAVPC</p>" +
                "</div>" +
                "</body>" +
                "</html>";
                
            body.put("htmlContent", htmlContent);

            // 3. Empaquetar la petición
            HttpEntity<Map<String, Object>> request = new HttpEntity<>(body, headers);

            // 4. ¡ENVIAR! (Esto sale por el puerto 443, que Railway SÍ permite)
            restTemplate.postForEntity(BREVO_API_URL, request, String.class);
            
            System.out.println("ÉXITO: Correo enviado vía API a " + emailDestino);

        } catch (Exception e) {
            System.err.println("ERROR API: " + e.getMessage());
            e.printStackTrace();
        }
    }
}