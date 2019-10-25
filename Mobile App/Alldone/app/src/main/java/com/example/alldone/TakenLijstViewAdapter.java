package com.example.alldone;

import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import android.widget.TextView;

import com.example.alldone.R;

import java.lang.reflect.Array;
import java.util.ArrayList;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

public class TakenLijstViewAdapter extends RecyclerView.Adapter<TakenLijstViewAdapter.ViewHolder>{

    private static final String TAG = "TakenLijstViewAdapter";

    private ArrayList<String> titleList = new ArrayList<>();
    private ArrayList<String> usersList = new ArrayList<>();
    private ArrayList<String> priorityList = new ArrayList<>();
    private Context mContext;

    public TakenLijstViewAdapter(ArrayList<String> titleList, ArrayList<String> usersList, ArrayList<String> priorityList, Context mContext) {
        this.titleList = titleList;
        this.usersList = usersList;
        this.priorityList = priorityList;
        this.mContext = mContext;
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.layout_takenlijst, parent, false);
        ViewHolder holder = new ViewHolder(view);
        return holder;
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, final int position) {
        Log.d(TAG, "onBindViewHolder: called.");

        holder.title.setText(titleList.get(position));
        holder.users.setText(usersList.get(position));
        holder.priority.setText(priorityList.get(position));

        holder.parentLayout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Log.d(TAG, "onClick: clicked on " + titleList.get(position));
                Context context = v.getContext();
                Intent intent = new Intent(context, TakenDetails.class);
                intent.putExtra("title", titleList.get(position));
                intent.putExtra("status", usersList.get(position));
                context.startActivity(intent);
            }
        });
    }

    @Override
    public int getItemCount() {
        return titleList.size();
    }

    public class ViewHolder extends RecyclerView.ViewHolder{

        TextView title;
        TextView users;
        TextView priority;
        RelativeLayout parentLayout;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            title = itemView.findViewById(R.id.title);
            users = itemView.findViewById(R.id.users);
            priority = itemView.findViewById(R.id.priority);
            parentLayout = itemView.findViewById(R.id.parent_layout);
        }
    }
}
