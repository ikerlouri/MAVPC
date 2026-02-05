package com.example.mavpc.adapters;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.bumptech.glide.Glide;
import com.example.mavpc.R;
import com.example.mavpc.controladores.Explorar;
import com.example.mavpc.model.Camara; // Importa tu modelo

import java.util.List;

// Adaptador para el listview de las camaras favoritas
public class FavoritosAdapter extends ArrayAdapter<Camara> {

    private Context context;
    private List<Camara> listaCamaras;

    public FavoritosAdapter(Context context, List<Camara> lista) {
        super(context, R.layout.item_camara_fav, lista);
        this.context = context;
        this.listaCamaras = lista;
    }

    // El metodo en si es un for each
    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        // Inflar vista
        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(R.layout.item_camara_fav, parent, false);
        }

        // Obtener el objeto de esta posición
        Camara camara = listaCamaras.get(position);
        Log.d("FAVORITOSADAPTER", camara.toString());

        TextView tvNombre = convertView.findViewById(R.id.tvNombreItem);
        ImageView ivImagen = convertView.findViewById(R.id.ivCamaraItem);
        Button btnIr = convertView.findViewById(R.id.btnIrMapa);

        // Escribir nombre
        tvNombre.setText(camara.getName());

        // Cargar imagen
        if (camara.getUrlImage() != null && !camara.getUrlImage().isEmpty()) {
            Glide.with(context)
                    .load(camara.getUrlImage())
                    .placeholder(R.drawable.ic_camera)
                    .into(ivImagen);
        }

        // Boton para abrir mapa en la posicion de la camara
        btnIr.setOnClickListener(v -> {
            Intent intent = new Intent(context, Explorar.class);

            try {
                double lat = Double.parseDouble(camara.getLatitude());
                double lon = Double.parseDouble(camara.getLongitude());

                intent.putExtra("LAT_DESTINO", lat);
                intent.putExtra("LON_DESTINO", lon);
            } catch (NumberFormatException | NullPointerException e) {
                e.printStackTrace();
            }

            intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
            context.startActivity(intent);
            // para desactivar la animacion al cambiar de activity
            if (context instanceof Activity) {
                ((Activity) context).overridePendingTransition(0, 0);
            }
        });

        return convertView;
    }
}