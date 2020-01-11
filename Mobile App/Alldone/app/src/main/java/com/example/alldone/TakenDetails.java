package com.example.alldone;

import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.MenuItem;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;
import com.google.android.material.navigation.NavigationView;
import com.google.android.material.tabs.TabLayout;

import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;

import androidx.fragment.app.Fragment;
import androidx.viewpager.widget.ViewPager;

import android.view.View;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.client.HttpClient;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicNameValuePair;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

public class TakenDetails extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mToggle;

    public static int id = -1;
    public String title;
    public String description;
    public int priority;

    public Button deleteBtn;
    public Button editBtn;

    private TabLayout tabLayout;
    private ViewPager viewpager;

    private UsersTab userFragment;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        GetStatus_Task.view = TakenDetails.this;

        setContentView(R.layout.activity_taken_details2);

        mDrawerLayout = findViewById(R.id.drawerLayout);
        mToggle = new ActionBarDrawerToggle(this, mDrawerLayout, R.string.open, R.string.close);

        mDrawerLayout.addDrawerListener(mToggle);
        mToggle.syncState();

        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        NavigationView navigationView = findViewById(R.id.nv1);
        navigationView.setNavigationItemSelectedListener(this);

        Intent intent = getIntent();

        id = intent.getIntExtra("id", id);
        title = intent.getStringExtra("title");
        description = intent.getStringExtra("description");
        priority = intent.getIntExtra("priority", -1);

        TextView titleText = findViewById(R.id.titleTxt);
        TextView priorityText = findViewById(R.id.priorityTxt);

        titleText.setText(title);
        switch (priority) {
            case 0:
                priorityText.setText("");
                break;
            case 1:
                priorityText.setText("!");
                break;
            case 2:
                priorityText.setText("!!");
                break;
            case 3:
                priorityText.setText("!!!");
                break;
        }
        // TODO set by using a request getting the enrollments
//        status.setText(id);

        deleteBtn = findViewById(R.id.delete);
        editBtn = findViewById(R.id.edit);

        tabLayout = findViewById(R.id.takenTabs);
        viewpager = findViewById(R.id.viewpager);

        setupViewPager(viewpager);

        tabLayout.setupWithViewPager(viewpager);

        deleteBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                new DeleteTask().execute();
            }
        });
        editBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Context context = v.getContext();
                Intent intent = new Intent(context, BewerkTaak.class);
                intent.putExtra("title", title);
                intent.putExtra("priority", priority);
                intent.putExtra("description", description);
                intent.putExtra("id", id);
                context.startActivity(intent);
            }
        });
    }

    class DeleteTask extends AsyncTask<Void, Void, Response> {
        @Override
        protected Response doInBackground(Void... voids) {
            try {
                JSONObject json = new JSONObject()
                        .put("task", id);
                return Connection.Send("task", "DELETE", json.toString());
            } catch (JSONException e) {
                // Won't happen
                throw new RuntimeException(e);
            }
        }

        @Override
        protected void onPostExecute(Response response) {
            if (response.IsSuccessful() || response.StatusCode == 400) {
                Toast.makeText(getApplicationContext(), "Taak verwijderd!", Toast.LENGTH_LONG).show();
                Intent intent = new Intent(getApplicationContext(), Takenlijst.class);
                intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intent);
            } else {
                if (response.StatusCode == 401) {
                    Toast.makeText(getApplicationContext(), "Je bent niet bevoegd om deze taak te verwijderen.", Toast.LENGTH_LONG).show();
                } else {
                    Toast.makeText(getApplicationContext(), "Iets ging verkeerd. Probeer het later nog een keer.", Toast.LENGTH_LONG).show();
                }
            }
        }
    }

    public static class GetStatus_Task extends AsyncTask<Void, Void, Response> {
        public static TakenDetails view;
        public UsersTab userFragment;

        @Override
        protected Response doInBackground(Void... voids) {
            try {
                JSONObject json = new JSONObject()
                        .put("task", view.id);
                return Connection.Send("taskenroll", "GET", json.toString());
            } catch (JSONException e) {
                // WONT HAPPEN GODDAMN
                throw new RuntimeException(e);
            }
        }

        @Override
        protected void onPostExecute(Response response) {
            if (response.IsSuccessful()) {
                TextView status = view.findViewById(R.id.users);

                if (response.StatusCode == 200) {
                    try {
                        JSONArray responseJson = response.GetJSON().getJSONArray("results");

                        boolean areAllDone = true;
                        final JSONObject[] elements = new JSONObject[responseJson.length()];
                        for (int i = 0; i < responseJson.length(); i++) {
                            elements[i] = responseJson.getJSONObject(i);
                            if (!elements[i].has("End"))
                                areAllDone = false;
                        }

                        userFragment.UpdateList(elements);

                        if (areAllDone) status.setText("Done");
                        else status.setText("In progress");
                    } catch (JSONException e) {
                        throw new RuntimeException(e);
                    }
                } else {
                    status.setText("To-do");
                }
            } else {
                Toast.makeText(view.getApplicationContext(), "Iets ging verkeerd. Probeer het later nog een keer.", Toast.LENGTH_LONG).show();
            }
        }
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
                Intent intent3 = new Intent(getApplicationContext(), Takenlijst.class);
                startActivity(intent3);
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

    private void setupViewPager(ViewPager viewpager) {

        ViewPagerAdapter viewPagerAdapter = new ViewPagerAdapter(getSupportFragmentManager());

        Bundle descBundle = new Bundle();
        descBundle.putString("Description", description);
        Fragment descFragment = new DescriptionTab();
        descFragment.setArguments(descBundle);
        viewPagerAdapter.addFragment(descFragment, "Beschrijving"); //new DescriptionTab(), "Beschrijving"

        userFragment = new UsersTab();
        viewPagerAdapter.addFragment(userFragment, "Ingeschreven");

        GetStatus_Task task = new GetStatus_Task();
        task.userFragment = userFragment;
        task.execute();

//        viewPagerAdapter.addFragment(new CommentsTab(), "Opmerkingen");
        viewpager.setAdapter(viewPagerAdapter);
    }

}