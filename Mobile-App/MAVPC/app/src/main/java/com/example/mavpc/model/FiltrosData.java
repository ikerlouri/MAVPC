package com.example.mavpc.model;

import java.util.ArrayList;
import java.util.List;

// clase para facilitar la recoleccion de los datos de filtrado de marcadores del mapa
public class FiltrosData {
    private String dia, mes, anio;
    private List<String> provinciasSeleccionadas = new ArrayList<>(); // Guipúzcoa, Álava, Vizcaya
    private List<String> tiposSeleccionados = new ArrayList<>(); // Incidencia, Obra, Cámara
    private List<String> gravedadSeleccionada = new ArrayList<>(); // Grave, Medio, Leve

    public String getDia() {
        return dia;
    }

    public void setDia(String dia) {
        this.dia = dia;
    }

    public String getMes() {
        return mes;
    }

    public void setMes(String mes) {
        this.mes = mes;
    }

    public String getAnio() {
        return anio;
    }

    public void setAnio(String anio) {
        this.anio = anio;
    }

    public List<String> getProvinciasSeleccionadas() {
        return provinciasSeleccionadas;
    }

    public void setProvinciasSeleccionadas(List<String> provinciasSeleccionadas) {
        this.provinciasSeleccionadas = provinciasSeleccionadas;
    }

    public List<String> getTiposSeleccionados() {
        return tiposSeleccionados;
    }

    public void setTiposSeleccionados(List<String> tiposSeleccionados) {
        this.tiposSeleccionados = tiposSeleccionados;
    }

    public List<String> getGravedadSeleccionada() {
        return gravedadSeleccionada;
    }

    public void setGravedadSeleccionada(List<String> gravedadSeleccionada) {
        this.gravedadSeleccionada = gravedadSeleccionada;
    }
}