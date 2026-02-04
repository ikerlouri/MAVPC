package com.example.mavpc.controladores;

import android.content.Context;
import android.content.Intent;
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
import com.example.mavpc.modelos.Camara; // Importa tu modelo

import java.util.List;

public class FavoritosAdapter extends ArrayAdapter<Camara> {

    private Context context;
    private List<Camara> listaCamaras;

    public FavoritosAdapter(Context context, List<Camara> lista) {
        super(context, R.layout.item_camara_fav, lista);
        this.context = context;
        this.listaCamaras = lista;
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        // Inflar la vista si no existe
        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(R.layout.item_camara_fav, parent, false);
        }

        // Obtener la cámara actual
        Camara camara = listaCamaras.get(position);

        // Referencias UI
        TextView tvNombre = convertView.findViewById(R.id.tvNombreItem);
        ImageView ivImagen = convertView.findViewById(R.id.ivCamaraItem);
        Button btnIr = convertView.findViewById(R.id.btnIrMapa);

        // Setear datos
        tvNombre.setText(camara.getName());

        // Cargar imagen con Glide
        if (camara.getUrlImage() != null && !camara.getUrlImage().isEmpty()) {
            Glide.with(context)
                    .load(camara.getUrlImage())
                    .placeholder(R.drawable.ic_launcher_foreground) // Tu imagen por defecto
                    .into(ivImagen);
        }

        // --- LÓGICA DEL BOTÓN IR ---
        btnIr.setOnClickListener(v -> {
            Intent intent = new Intent(context, Explorar.class);
            // Pasamos las coordenadas a la actividad Explorar
            intent.putExtra("LAT_DESTINO", camara.getLatitude());
            intent.putExtra("LON_DESTINO", camara.getLongitude());
            intent.putExtra("ID_CAMARA_FOCUS", camara.getId()); // Opcional, por si quieres abrir el popup directo

            // Para que no cree una nueva instancia si ya existe, o para ir directo
            intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
            context.startActivity(intent);
        });

        return convertView;
    }
}