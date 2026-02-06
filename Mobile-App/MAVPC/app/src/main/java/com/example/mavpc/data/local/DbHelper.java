package com.example.mavpc.data.local;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.util.Log;

import com.example.mavpc.model.Camara;
import com.example.mavpc.model.Usuario;

import java.util.ArrayList;
import java.util.List;

// clase que crea la base de datos y los metodos para llamarla
public class DbHelper extends SQLiteOpenHelper {
    private static final int DATABASE_VERSION = 6;
    private static final String DATABASE_NAME = "UserYCamFavs.db";

    private static final String TABLA_USUARIO = "usuario_sesion";
    private static final String TABLA_CAMARAS = "camaras_fav";

    public DbHelper(Context context) {
        super(context, DATABASE_NAME, null, DATABASE_VERSION);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        // Tabla Usuario (Solo tendrá un usuario a la vez, el que esta en la sesion iniciada)
        String crearTablaUsuario = "CREATE TABLE " + TABLA_USUARIO + "(" + "id INTEGER PRIMARY KEY, " + "username TEXT, " + "email TEXT, " + "password TEXT, " + "pfpUrl TEXT)";
        db.execSQL(crearTablaUsuario);

        // Tabla Camaras Favoritas (Solo se guardan las camaras favoritas del usuario de la tabla "usuario_sesion")
        String crearTablaCamaras = "CREATE TABLE " + TABLA_CAMARAS + "(" + "id INTEGER PRIMARY KEY, " + "name TEXT, " + "urlImage TEXT, " + "latitude TEXT, " + "longitude TEXT, " + "road TEXT, " + "km TEXT, " + "direction TEXT)";
        db.execSQL(crearTablaCamaras);
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLA_USUARIO);
        db.execSQL("DROP TABLE IF EXISTS " + TABLA_CAMARAS);
        onCreate(db);
    }

    // inserta en sqlite el usuario pasado
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

    // actualiza en sqlite el usuario pasado
    public void updateUsuario(Usuario usuario) {
        if (usuario == null) return;

        SQLiteDatabase db = this.getWritableDatabase();

        ContentValues values = new ContentValues();
        // NO tocamos el ID, porque es el mismo usuario
        values.put("username", usuario.getUsername());
        values.put("email", usuario.getEmail());
        values.put("password", usuario.getPassword());
        values.put("pfpUrl", usuario.getPfpUrl());

        // Ejecutamos UPDATE en lugar de DELETE + INSERT
        // "id = ?" significa: busca la fila donde la columna id sea igual al id del usuario
        int filasAfectadas = db.update(TABLA_USUARIO, values, "id = ?", new String[]{String.valueOf(usuario.getId())});

        if (filasAfectadas > 0) {
            Log.d("DB_UPDATE", "Perfil actualizado correctamente.");
        } else {
            Log.e("DB_UPDATE", "No se encontró el usuario para actualizar.");
        }

        db.close();
    }

    // devuelve el usuario que hay en sqlite
    public Usuario getUsuarioSesion() {
        SQLiteDatabase db = this.getReadableDatabase();
        Usuario usuario = null;

        Cursor cursor = db.rawQuery("SELECT * FROM " + TABLA_USUARIO + " LIMIT 1", null);

        if (cursor.moveToFirst()) {
            usuario = new Usuario(cursor.getInt(0), // id
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

    // borra de sqlite el contenido de las dos tablas para cerrar la sesion
    public void logoff() {
        SQLiteDatabase db = this.getWritableDatabase();
        db.delete(TABLA_USUARIO, null, null);
        db.delete(TABLA_CAMARAS, null, null);
        db.close();
    }

    // inserta en sqlite una camara favorita
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

    // inserta en sqlite una lista de camaras favoritas
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

    // elimina de sqlite una camara de la lista de favoritas del usuario
    public void deleteFavCam(Camara c) {
        SQLiteDatabase db = this.getWritableDatabase();
        db.delete(TABLA_CAMARAS, "id = ?", new String[]{String.valueOf(c.getId())});
        db.close();
    }

    // devuelve todas las camaras favoritas del usuario
    public List<Camara> getFavCams() {
        List<Camara> lista = new ArrayList<>();
        SQLiteDatabase db = this.getReadableDatabase();
        Cursor cursor = db.rawQuery("SELECT * FROM " + TABLA_CAMARAS, null);

        if (cursor.moveToFirst()) {
            do {
                // 1. Obtenemos los índices de las columnas por su nombre
                // (Asegúrate de que estos nombres coinciden EXACTAMENTE con tu CREATE TABLE)
                int indexId = cursor.getColumnIndex("id");
                int indexName = cursor.getColumnIndex("name");
                int indexUrl = cursor.getColumnIndex("urlImage");
                int indexLat = cursor.getColumnIndex("latitude");
                int indexLon = cursor.getColumnIndex("longitude");
                int indexRoad = cursor.getColumnIndex("road");
                int indexKm = cursor.getColumnIndex("km");
                int indexDir = cursor.getColumnIndex("direction");

                // 2. Leemos usando esos índices
                // Nota: getColumnIndex devuelve -1 si no encuentra la columna,
                // por eso es bueno comprobar o asegurarse que los nombres están bien.

                Camara c = new Camara(cursor.getInt(indexId), cursor.getString(indexName), cursor.getString(indexUrl), cursor.getString(indexLat), cursor.getString(indexLon), cursor.getString(indexRoad), cursor.getString(indexKm), cursor.getString(indexDir));

                lista.add(c);
            } while (cursor.moveToNext());
        }
        cursor.close();
        db.close();
        return lista;
    }

    // devuelve un booleano que expresa si una camara es favorita o no del usuario
    public boolean isFavourite(Camara c) {
        // Obtener base de datos en modo lectura
        SQLiteDatabase db = this.getReadableDatabase();

        // '?' para evitar inyección SQL y errores de formato
        Cursor cursor = db.rawQuery("SELECT id FROM " + TABLA_CAMARAS + " WHERE id = ?", new String[]{String.valueOf(c.getId())});

        boolean existe = (cursor.getCount() > 0);

        cursor.close();
        db.close();

        return existe;
    }
}