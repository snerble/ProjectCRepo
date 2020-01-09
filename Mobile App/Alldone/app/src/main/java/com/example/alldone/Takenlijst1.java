package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;

import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import java.util.ArrayList;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;

import android.content.Intent;
import com.google.android.material.navigation.NavigationView;
import android.view.MenuItem;
import android.view.View;
import android.widget.Toast;

import android.os.AsyncTask;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;

import android.content.Intent;
import com.google.android.material.navigation.NavigationView;
import android.view.MenuItem;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;

public class Takenlijst1 extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mToggle;

    ListView listView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_takenlijst1);

        mDrawerLayout = (DrawerLayout) findViewById(R.id.drawerLayout);
        mToggle = new ActionBarDrawerToggle(this, mDrawerLayout, R.string.open, R.string.close);

        mDrawerLayout.addDrawerListener(mToggle);
        mToggle.syncState();

        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        NavigationView navigationView = (NavigationView) findViewById(R.id.nv1);
        navigationView.setNavigationItemSelectedListener(this);

        listView = (ListView) findViewById(R.id.listView);
        getJSON("http://192.168.188.62/alldone/v1/fetch_data.php");
        //getJSON("http://145.137.121.233/aldone/v1/fetch_data.php");
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
                //Toast.makeText(getApplicationContext(), s, Toast.LENGTH_SHORT).show();
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
        JSONArray jsonArray = new JSONArray(json);
        String[] tasks = new String[jsonArray.length()];
        for (int i = 0; i < jsonArray.length(); i++) {
            JSONObject obj = jsonArray.getJSONObject(i);
            tasks[i] = obj.getString("title");
        }
        ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, tasks);
        listView.setAdapter(arrayAdapter);
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
                startActivity(intent2);
                break;
            case (R.id.task_list):
                //Intent intent3 = new Intent(getApplicationContext(), Takenlijst.class);
                //startActivity(intent3);
                break;
            case (R.id.profile):
                Intent intent4 = new Intent(getApplicationContext(), Profiel.class);
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
