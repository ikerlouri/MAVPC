package com.example.mavpc.modelos;

import com.google.gson.annotations.SerializedName;

public class Incidencia {
    @SerializedName("incidenceId")
    private int id;

    @SerializedName("incidenceType")
    private String type;

    private String cause;

    @SerializedName("incidenceLevel")
    private String level;

    private String road;

    private String cityTown;

    private String province;

    private String direction;

    private String startDate;

    private String latitude;

    private String longitude;

    public Incidencia(String longitude, String latitude, String startDate, String direction, String province, String cityTown, String road, String level, String cause, String type, int id) {
        this.longitude = longitude;
        this.latitude = latitude;
        this.startDate = startDate;
        this.direction = direction;
        this.province = province;
        this.cityTown = cityTown;
        this.road = road;
        this.level = level;
        this.cause = cause;
        this.type = type;
        this.id = id;
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getType() {
        return type;
    }

    public void setType(String type) {
        this.type = type;
    }

    public String getCause() {
        return cause;
    }

    public void setCause(String cause) {
        this.cause = cause;
    }

    public String getLevel() {
        return level;
    }

    public void setLevel(String level) {
        this.level = level;
    }

    public String getRoad() {
        return road;
    }

    public void setRoad(String road) {
        this.road = road;
    }

    public String getCityTown() {
        return cityTown;
    }

    public void setCityTown(String cityTown) {
        this.cityTown = cityTown;
    }

    public String getProvince() {
        return province;
    }

    public void setProvince(String province) {
        this.province = province;
    }

    public String getDirection() {
        return direction;
    }

    public void setDirection(String direction) {
        this.direction = direction;
    }

    public String getStartDate() {
        return startDate;
    }

    public void setStartDate(String startDate) {
        this.startDate = startDate;
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
}