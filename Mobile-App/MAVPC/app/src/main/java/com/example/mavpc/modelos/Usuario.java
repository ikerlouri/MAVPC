package com.example.mavpc.modelos;

public class Usuario {
    private String id;
    private String username;
    private String email;
    private String password;
    private String pfpUrl;

    public Usuario(String id, String username, String email, String password, String pfpUrl) {
        this.id = id;
        this.username = username;
        this.email = email;
        this.password = password;
        this.pfpUrl = pfpUrl;
    }

    public Usuario() {}

    public String getId() {
        return id;
    }

    public void setId(String id) {
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