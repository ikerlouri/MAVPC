package com.example.mavpc.model;

import com.google.gson.annotations.SerializedName;

// representa cada usuario con todos sus datos
public class Usuario {
    private int id;
    @SerializedName("usuario")
    private String username;
    private String email;
    @SerializedName("contrasena")
    private String password;
    @SerializedName("urlImage")
    private String pfpUrl;

    public Usuario(int id, String username, String email, String password, String pfpUrl) {
        this.id = id;
        this.username = username;
        this.email = email;
        this.password = password;
        this.pfpUrl = pfpUrl;
    }

    @Override
    public String toString() {
        return "Usuario{" + "id=" + id + ", username='" + username + '\'' + ", email='" + email + '\'' + ", password='" + password + '\'' + ", pfpUrl='" + pfpUrl + '\'' + '}';
    }

    public Usuario() {
    }

    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getEmail() {
        return email;
    }

    public void setEmail(String email) {
        this.email = email;
    }

    public String getPassword() {
        return password;
    }

    public void setPassword(String password) {
        this.password = password;
    }

    public String getPfpUrl() {
        return pfpUrl;
    }

    public void setPfpUrl(String pfpUrl) {
        this.pfpUrl = pfpUrl;
    }
}