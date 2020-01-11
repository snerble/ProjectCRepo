package com.example.alldone;

import android.content.Context;
import android.content.Intent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.TextView;

import java.util.ArrayList;

public class ListAdapter extends BaseAdapter {
    // ListAdapter adapts the items tasklist

    Context c;
    ArrayList<com.example.alldone.model.Task> tasks;
    LayoutInflater inflater;

    public ListAdapter(Context c, ArrayList<com.example.alldone.model.Task> tasks) {
        this.c = c;
        this.tasks = tasks;

        inflater = (LayoutInflater) c.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
    }

    @Override
    public int getCount() {
        return tasks.size();
    }

    @Override
    public Object getItem(int position) {
        return tasks.get(position);
    }

    @Override
    public long getItemId(int position) {
        return tasks.get(position).getId();
    }

    @Override
    public View getView(final int position, View convertView, ViewGroup parent) {
        if(convertView == null)
        {
            convertView = inflater.inflate(R.layout.model,parent,false);
        }

        TextView titleTxt =  convertView.findViewById(R.id.titleTxt);
        TextView priorTxt =  convertView.findViewById(R.id.priorTxt);

        titleTxt.setText(tasks.get(position).getTitle());
        priorTxt.setText(tasks.get(position).getPriority());

        convertView.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Context yeet = v.getContext();
                Intent intent = new Intent(yeet, TakenDetails.class);
                intent.putExtra("title", tasks.get(position).getTitle());
                intent.putExtra("priority", tasks.get(position).getPriority());
                intent.putExtra("description", tasks.get(position).getDescription());
                intent.putExtra("id", tasks.get(position).getId_string());


                //Bundle bundle = new Bundle();
                //bundle.putString("title", tasks.get(position).getTitle());
                //bundle.putString("priority", tasks.get(position).getPriority());
                //bundle.putString("description", tasks.get(position).getDescription());
                //bundle.putInt("id", tasks.get(position).getId());
                //your_fragment.setArguments(bundle);
                //intent.putExtra("status", usersList.get(position));
                yeet.startActivity(intent);
            }
        });

        return convertView;
    }
}
