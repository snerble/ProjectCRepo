package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.os.Bundle;

import java.util.ArrayList;

public class Takenlijst extends AppCompatActivity {

    private static final String TAG = "Takenlijst";

    private ArrayList<String> titleList = new ArrayList<>();
    private ArrayList<String> usersList = new ArrayList<>();
    private ArrayList<String> priorityList = new ArrayList<>();


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_takenlijst);

        initArrays();
    }

    private void initArrays(){
        titleList.add("Koffiebonen bijvullen");
        usersList.add("Open voor inschrijving");
        priorityList.add("");
        titleList.add("Bureau repareren");
        usersList.add("Amy, Daphne");
        priorityList.add("!!");

        initRecyclerView();
    }

    private void initRecyclerView(){
        RecyclerView recyclerView = findViewById(R.id.recycler_view);
        TakenLijstViewAdapter adapter = new TakenLijstViewAdapter(titleList, usersList, priorityList, this);
        recyclerView.setAdapter(adapter);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));
    }
}
