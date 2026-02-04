package com.example.mavpc.database;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.util.Log;

import com.example.mavpc.modelos.Camara;
import com.example.mavpc.modelos.Usuario;

import java.util.ArrayList;
import java.util.List;

public class DbHelper extends SQLiteOpenHelper {
    private static final int DATABASE_VERSION = 3;
    private static final String DATABASE_NAME = "UserYCamFavs.db";

    // Nombres de Tablas
    private static final String TABLA_USUARIO = "usuario_sesion";
    private static final String TABLA_CAMARAS = "camaras_fav";

    public DbHelper(Context context) {
        super(context, DATABASE_NAME, null, DATABASE_VERSION);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        // Tabla Usuario (Para guardar al que se loguea)
        String crearTablaUsuario = "CREATE TABLE " + TABLA_USUARIO + "(" +
                "id INTEGER PRIMARY KEY, " +
                "username TEXT, " +
                "email TEXT, " +
                "password TEXT, " +
                "pfpUrl TEXT)";
        db.execSQL(crearTablaUsuario);

        // Tabla Camaras Favoritas
        String crearTablaCamaras = "CREATE TABLE " + TABLA_CAMARAS + "(" +
                "id INTEGER PRIMARY KEY, " +
                "name TEXT, " +
                "urlImage TEXT, " +
                "latitude TEXT, " +
                "longitude TEXT, " +
                "road TEXT, " +
                "km TEXT, " +
                "direction TEXT)";
        db.execSQL(crearTablaCamaras);
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLA_USUARIO);
        db.execSQL("DROP TABLE IF EXISTS " + TABLA_CAMARAS);
        onCreate(db);
    }

    // Metodos usuario
    public void insertUsuarioSesion(Usuario usuario) {
        if (usuario == null) return;

        SQLiteDatabase db = this.getWritableDatabase();

        // Borrar usuario y camaras anteriores
        db.delete(TABLA_USUARIO, null, null);
        db.delete(TABLA_CAMARAS, null, null);

        // Insertar el nuevo
        ContentValues values = new ContentValues();
        values.put("id", usuario.getId());
        values.put("username", usuario.getUsername());
        values.put("email", usuario.getEmail());
        values.put("password", usuario.getPassword());
        values.put("pfpUrl", usuario.getPfpUrl());

        long resultado = db.insert(TABLA_USUARIO, null, values);

        if (resultado == -1) {
            Log.e("DB_ERROR", "No se pudo guardar el usuario. ID: " + usuario.getId());
        } else {
            Log.d("DB_SUCCESS", "Usuario guardado correctamente en fila: " + resultado);
        }

        db.close();
    }

    public Usuario getUsuarioSesion() {
        SQLiteDatabase db = this.getReadableDatabase();
        Usuario usuario = null;

        Cursor cursor = db.rawQuery("SELECT * FROM " + TABLA_USUARIO + " LIMIT 1", null);

        if (cursor.moveToFirst()) {
            usuario = new Usuario(
                    cursor.getInt(0), // id
                    cursor.getString(1), // username
                    cursor.getString(2), // email
                    cursor.getString(3), // password
                    cursor.getString(4)  // pfpUrl
            );
        }
        cursor.close();
        db.close();
        return usuario; // Devuelve null si no hay nadie logueado
    }

    public void logoff() {
        SQLiteDatabase db = this.getWritableDatabase();
        db.delete(TABLA_USUARIO, null, null);
        db.delete(TABLA_CAMARAS, null, null); // Borramos también sus favoritos
        db.close();
    }

    // Metodos camaras
    public void insertCam(Camara c) {
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues values = new ContentValues();

        values.put("id", c.getId());
        values.put("name", c.getName());
        values.put("urlImage", c.getUrlImage());
        values.put("latitude", c.getLatitude());
        values.put("longitude", c.getLongitude());
        values.put("road", c.getRoad());
        values.put("km", c.getKm());
        values.put("direction", c.getDirection());

        // insertWithOnConflict devuelve el ID de la fila o -1 si falla y reemplaza si ya existe (para no duplicar)
        long resultado = db.insertWithOnConflict(TABLA_CAMARAS, null, values, SQLiteDatabase.CONFLICT_REPLACE);

        if (resultado == -1) {
            Log.e("DB_ERROR", "Error crítico: No se pudo insertar la cámara " + c.getName());
        } else {
            Log.d("DB_SUCCESS", "Guardada cámara con ID: " + c.getId() + " en fila: " + resultado);
        }
        db.close();
    }

    public void insertCamList(List<Camara> camList) {
        if (camList == null) return;

        // con transaccion por rendimiento
        SQLiteDatabase db = this.getWritableDatabase();
        // Iniciar la transacción
        db.beginTransaction();

        try {
            ContentValues values = new ContentValues();

            for (Camara c : camList) {
                // Limpiar valores anteriores para asegurar que no se mezclan datos
                values.clear();

                values.put("id", c.getId());
                values.put("name", c.getName());
                values.put("urlImage", c.getUrlImage());
                values.put("latitude", c.getLatitude());
                values.put("longitude", c.getLongitude());
                values.put("road", c.getRoad());
                values.put("km", c.getKm());
                values.put("direction", c.getDirection());

                db.insertWithOnConflict(TABLA_CAMARAS, null, values, SQLiteDatabase.CONFLICT_REPLACE);
            }

            // Marcamos que ha ido bien
            db.setTransactionSuccessful();

        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            // Cerrar la transacción (se guardan los datos si fue Successful)
            db.endTransaction();
            db.close();
        }
    }

    public void deleteFavCam(Camara c) {
        SQLiteDatabase db = this.getWritableDatabase();
        db.delete(TABLA_CAMARAS, "id = ?", new String[]{String.valueOf(c.getId())});
        db.close();
    }

    public List<Camara> getFavCams() {
        List<Camara> lista = new ArrayList<>();
        SQLiteDatabase db = this.getReadableDatabase();
        Cursor cursor = db.rawQuery("SELECT * FROM " + TABLA_CAMARAS, null);

        if (cursor.moveToFirst()) {
            do {
                Camara c = new Camara(
                        cursor.getInt(0),
                        cursor.getString(1),
                        cursor.getString(2),
                        cursor.getString(3),
                        cursor.getString(4),
                        cursor.getString(5),
                        cursor.getString(6),
                        cursor.getString(7)
                );
                lista.add(c);
            } while (cursor.moveToNext());
        }
        cursor.close();
        db.close();
        return lista;
    }

    public boolean isFavourite(Camara c) {
        // Obtener base de datos en modo lectura
        SQLiteDatabase db = this.getReadableDatabase();

        // '?' para evitar inyección SQL y errores de formato
        Cursor cursor = db.rawQuery(
                "SELECT id FROM " + TABLA_CAMARAS + " WHERE id = ?",
                new String[]{ String.valueOf(c.getId()) }
        );

        boolean existe = (cursor.getCount() > 0);

        cursor.close();
        db.close();

        return existe;
    }
}