package com.example.demo.servicios;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.mail.javamail.MimeMessageHelper;
import org.springframework.stereotype.Service;

import jakarta.mail.internet.MimeMessage;

@Service
public class EmailSchedulerService {

    @Autowired
    private JavaMailSender mailSender;


    public void enviarCorreoBienvenida(String email, String usuario) {
        if (email == null || !email.matches("^[A-Za-z0-9+_.-]+@(.+)$")) {
            System.err.println("Correo no enviado: La dirección de email es inválida o está vacía.");
            return; 
        }

        try {
            MimeMessage message = mailSender.createMimeMessage();
            MimeMessageHelper helper = new MimeMessageHelper(message, "utf-8");
            
            helper.setFrom("Mavpc soporte <mavpc1459@gmail.com>");
            helper.setTo(email);
            helper.setSubject("Bienvenido a MAVPC - ¡Ya puedes empezar!");
            
            String textoCuerpo = "Hola, " + usuario + ":\n\n"
                    + "¡Es un placer tenerte con nosotros! Te has unido a la app de MAVPC...";
            
            helper.setText(textoCuerpo);

            mailSender.send(message);
            System.out.println("Correo enviado con éxito a: " + email);
            
        } catch (Exception e) {
            System.err.println("Error silencioso al enviar correo a " + email + ": " + e.getMessage());
        }
    }
}