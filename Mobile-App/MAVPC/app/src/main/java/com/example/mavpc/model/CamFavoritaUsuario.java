package com.example.mavpc.model;

public class CamFavoritaUsuario {
    private int id;
    private int idUsuario;
    private int idCamara;

    public CamFavoritaUsuario(int id, int idUsuario, int idCamara) {
        this.id = id;
        this.idUsuario = idUsuario;
        this.idCamara = idCamara;
    }

    public CamFavoritaUsuario() {}

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public int getIdUsuario() {
        return idUsuario;
    }

    public void setIdUsuario(int idUsuario) {
        this.idUsuario = idUsuario;
    }

    public int getIdCamara() {
        return idCamara;
    }

    public void setIdCamara(int idCamara) {
        this.idCamara = idCamara;
    }
}