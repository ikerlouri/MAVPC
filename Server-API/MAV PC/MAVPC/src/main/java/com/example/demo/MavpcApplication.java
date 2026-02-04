package com.example.demo;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.scheduling.annotation.EnableAsync;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
@EnableAsync
public class MavpcApplication {

	//Funcion que inicia toda la aplicacion Spring
	public static void main(String[] args) {
		SpringApplication.run(MavpcApplication.class, args);
	}

}
