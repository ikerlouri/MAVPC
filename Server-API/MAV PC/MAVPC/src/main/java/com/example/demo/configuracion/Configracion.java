package com.example.demo.configuracion;

import org.modelmapper.ModelMapper;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.web.servlet.config.annotation.CorsRegistry;
import org.springframework.web.servlet.config.annotation.WebMvcConfigurer;

import com.example.demo.modelos.Incidencia;
import com.example.demo.modelos.IncidenciaCreada;

@Configuration
public class Configracion {

    @Bean
    public ModelMapper modelMapper() {
        ModelMapper modelMapper = new ModelMapper();
        modelMapper.typeMap(Incidencia.class, IncidenciaCreada.class).addMappings(mapper -> {
            mapper.map(src -> src.getId(), IncidenciaCreada::setId);
            mapper.map(src -> src.getTipo(), IncidenciaCreada::setTipo);
        });
        return modelMapper;
    }

    // NUEVO: Esto permite que C# haga POST, DELETE, etc., sin que Spring lo bloquee
    @Bean
    public SecurityFilterChain securityFilterChain(HttpSecurity http) throws Exception {
        http
            .csrf(csrf -> csrf.disable()) // Deshabilita CSRF para permitir POST/DELETE externos
            .authorizeHttpRequests(auth -> auth.anyRequest().permitAll()); // Permite todas las rutas
        return http.build();
    }

    // NUEVO: Configuración global de CORS
    @Bean
    public WebMvcConfigurer corsConfigurer() {
        return new WebMvcConfigurer() {
            @Override
            public void addCorsMappings(CorsRegistry registry) {
                registry.addMapping("/**")
                        .allowedOrigins("*") // Permite cualquier origen
                        .allowedMethods("GET", "POST", "PUT", "DELETE", "OPTIONS");
            }
        };
    }
}