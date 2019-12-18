package com.example.alldone;

import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.MenuItem;
import android.widget.ArrayAdapter;
import android.widget.Toast;

import com.google.android.material.navigation.NavigationView;

import java.util.ArrayList;

import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import org.json.JSONObject;
import org.json.JSONArray;
import org.json.JSONException;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;


public class Takenlijst extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mToggle;

    private static final String TAG = "Takenlijst";

    private String userData;

    private ArrayList<String> titleList = new ArrayList<>();
    private ArrayList<String> usersList = new ArrayList<>();
    private ArrayList<String> priorityList = new ArrayList<>();
    private ArrayList<String> statusList = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_takenlijst);

        /*if(!SharedPrefManager.getInstance(this).isLoggedIn()){
            finish();
            startActivity(new Intent(this, MainActivity.class));
        } */

        initArrays();

        mDrawerLayout = (DrawerLayout) findViewById(R.id.drawerLayout);
        mToggle = new ActionBarDrawerToggle(this, mDrawerLayout, R.string.open, R.string.close);

        mDrawerLayout.addDrawerListener(mToggle);
        mToggle.syncState();

        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        NavigationView navigationView = (NavigationView) findViewById(R.id.nv1);
        navigationView.setNavigationItemSelectedListener(this);
    }

    private void initArrays(){
        //titleList.add("Koffiebonen bijvullen");
        //usersList.add("Open voor inschrijving");
        //priorityList.add("");
        //statusList.add("");
        //titleList.add("Bureau repareren");
        //usersList.add("Amy, Daphne");
        //priorityList.add("!!");
        //statusList.add("Bezig");

        String ServerURL = "http://192.168.188.62/alldone/v1/fetch_tasks.php";
        //String ServerURL = "http://145.137.122.231/alldone/v1/fetch_tasks.php";
        // new updateData().execute(ServerURL);
        getJSON(ServerURL);
    }

    private void getJSON(final String urlWebService) {

        class GetJSON extends AsyncTask<Void, Void, String> {

            @Override
            protected void onPreExecute() {
                super.onPreExecute();
            }

            @Override
            protected void onPostExecute(String s) {
                super.onPostExecute(s);

                try {
                    loadIntoListView(s);
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }

            @Override
            protected String doInBackground(Void... voids) {
                try {
                    URL url = new URL(urlWebService);
                    HttpURLConnection con = (HttpURLConnection) url.openConnection();
                    StringBuilder sb = new StringBuilder();
                    BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(con.getInputStream()));
                    String json;
                    while ((json = bufferedReader.readLine()) != null) {
                        sb.append(json + "\n");
                    }
                    return sb.toString().trim();
                } catch (Exception e) {
                    return null;
                }
            }
        }
        GetJSON getJSON = new GetJSON();
        getJSON.execute();
    }

    private void loadIntoListView(String json) throws JSONException {
        JSONArray jsonArray1 = new JSONArray(json);
        ArrayList<String> titles = new ArrayList<>();
        for (int i = 0; i < jsonArray1.length(); i++) {
            JSONObject obj = jsonArray1.getJSONObject(i);
            titles.add(obj.getString("title"));
        }

        JSONArray jsonArray2 = new JSONArray(json);
        ArrayList<String> priorities = new ArrayList<>();
        for (int i = 0; i < jsonArray2.length(); i++) {
            JSONObject obj = jsonArray2.getJSONObject(i);
            priorities.add(obj.getString("priority"));
        }

        RecyclerView recyclerView = findViewById(R.id.recycler_view);
        TakenLijstViewAdapter adapter = new TakenLijstViewAdapter(titles, /*, usersList,*/ priorities, /*statusList,*/ this);
        recyclerView.setAdapter(adapter);
        recyclerView.setLayoutManager(new LinearLayoutManager(this));
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {

        if (mToggle.onOptionsItemSelected(item)) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    public boolean onNavigationItemSelected(MenuItem menuItem) {
        switch (menuItem.getItemId()) {
            case (R.id.home):
                //Intent intent1 = new Intent(getApplicationContext(), MainActivity.class);
                //startActivity(intent1);
                break;
            case (R.id.new_task):
                Intent intent2 = new Intent(getApplicationContext(), MaakTaak.class);
                intent2.putExtra("userdata", userData);
                startActivity(intent2);
                break;
            case (R.id.task_list):
                //Intent intent3 = new Intent(getApplicationContext(), Takenlijst.class);
                //startActivity(intent3);
                break;
            case (R.id.profile):
                Intent intent4 = new Intent(getApplicationContext(), Profiel.class);
                intent4.putExtra("userdata", userData);
                startActivity(intent4);
                break;
            case(R.id.log_out):
                Toast.makeText(getApplicationContext(), "Je bent uitgelogd",Toast.LENGTH_SHORT).show();
                Intent intent5 = new Intent(getApplicationContext(), MainActivity.class);
                intent5.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                intent5.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intent5);
        }
        return true;
    }
}
