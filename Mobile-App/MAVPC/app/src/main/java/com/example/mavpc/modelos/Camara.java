package com.example.mavpc.modelos;

import com.google.gson.annotations.SerializedName;

public class Camara {
    @SerializedName("cameraId")
    private int id;
    @SerializedName("cameraName")
    private String name;
    private String urlImage;
    private String latitude;
    private String longitude;
    private String road;
    @SerializedName("kilometer")
    private String km;
    @SerializedName("address")
    private String direction;

    public Camara(int id, String name, String urlImage, String latitude, String longitude, String road, String km, String direction) {
        this.id = id;
        this.name = name;
        this.urlImage = urlImage;
        this.latitude = latitude;
        this.longitude = longitude;
        this.road = road;
        this.km = km;
        this.direction = direction;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getUrlImage() {
        return urlImage;
    }

    public void setUrlImage(String urlImage) {
        this.urlImage = urlImage;
    }

    public String getLatitude() {
        return latitude;
    }

    public void setLatitude(String latitude) {
        this.latitude = latitude;
    }

    public String getLongitude() {
        return longitude;
    }

    public void setLongitude(String longitude) {
        this.longitude = longitude;
    }

    public String getRoad() {
        return road;
    }

    public void setRoad(String road) {
        this.road = road;
    }

    public String getKm() {
        return km;
    }

    public void setKm(String km) {
        this.km = km;
    }

    public String getDirection() {
        return direction;
    }

    public void setDirection(String direction) {
        this.direction = direction;
    }
}