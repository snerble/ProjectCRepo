package com.example.alldone;

import android.content.Intent;
import android.os.Bundle;
import android.view.MenuItem;
import android.widget.ImageView;
import android.widget.TextView;

import com.google.android.material.navigation.NavigationView;

import org.json.JSONException;
import org.json.JSONObject;

import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;



public class Profiel extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mToggle;

    private String name;
    private String job;
    private String userData;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_profiel);

        mDrawerLayout = (DrawerLayout) findViewById(R.id.drawerLayout);
        mToggle = new ActionBarDrawerToggle(this, mDrawerLayout, R.string.open, R.string.close);

        mDrawerLayout.addDrawerListener(mToggle);
        mToggle.syncState();

        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        NavigationView navigationView = (NavigationView) findViewById(R.id.nv1);
        navigationView.setNavigationItemSelectedListener(this);

        Intent intent=getIntent();

        userData = intent.getStringExtra("userdata");

        int jobNr = 0;
        try {
            JSONObject reader = new JSONObject(userData);
            name = reader.getString("username");
            jobNr = reader.getInt("accesslevel");
        } catch (JSONException e) {
            e.printStackTrace();
        }

        switch (jobNr) {
            case 2:
                job = "Administrator";
                break;

            default:
                job = "Technische staff";
                break;
        }
        TextView nameText = findViewById(R.id.name);
        TextView jobText = findViewById(R.id.job);

        ImageView img= (ImageView) findViewById(R.id.profilePic);
        img.setImageResource(R.drawable.no_user);

        nameText.setText(name);
        jobText.setText(job);
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
                Intent intent3 = new Intent(getApplicationContext(), Takenlijst.class);
                intent3.putExtra("userdata", userData);
                startActivity(intent3);
                break;
            case (R.id.profile):
                Intent intent4 = new Intent(getApplicationContext(), Profiel.class);
                intent4.putExtra("userdata", userData);
                startActivity(intent4);
                break;
            case(R.id.log_out):
                Intent intent5  = new Intent(getApplicationContext(), MainActivity.class);
                startActivity(intent5);
                break;
        }
        return true;
    }
}
