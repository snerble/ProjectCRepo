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

import java.util.ArrayList;
import java.util.List;

public class TakenDetails extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mToggle;

    public String title;
    public String priority;
    public String description;
    public String id;
    public Button deleteBtn;
    public Button editBtn;

    private TabLayout tabLayout;
    private ViewPager viewpager;

    //String ServerURL = "http://145.137.123.23/alldone/v1/delete_task.php";
    //String ServerURL = "http://145.137.121.233/alldone/v1/delete_task.php";
    //String ServerURL = "http://145.137.121.231/alldone/v1/delete_task.php";
    //String ServerURL = "http://145.137.122.181/alldone/v1/delete_task.php";
    String ServerURL = "http://145.137.121.58/alldone/v1/delete_task.php";
    //String ServerURL = "http://192.168.188.62/alldone/v1/delete_task.php";

    // Getting the query to insert data, !IP address differs from location :)


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_taken_details2);

        mDrawerLayout = (DrawerLayout) findViewById(R.id.drawerLayout);
        mToggle = new ActionBarDrawerToggle(this, mDrawerLayout, R.string.open, R.string.close);

        mDrawerLayout.addDrawerListener(mToggle);
        mToggle.syncState();

        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        NavigationView navigationView = (NavigationView) findViewById(R.id.nv1);
        navigationView.setNavigationItemSelectedListener(this);

        Intent intent = getIntent();

        title = intent.getStringExtra("title");
        priority = intent.getStringExtra("priority");
        description = intent.getStringExtra("description");
        id = intent.getStringExtra("id");
        TextView titleText = findViewById(R.id.titleTxt);
        TextView priorityText = findViewById(R.id.priorityTxt);
        TextView status = findViewById(R.id.users);

        titleText.setText(title);
        priorityText.setText(priority);
        status.setText(id);

        deleteBtn = findViewById(R.id.delete);
        editBtn = findViewById(R.id.edit);

        tabLayout = findViewById(R.id.takenTabs);
        viewpager = findViewById(R.id.viewpager);

        setupViewPager(viewpager);

        tabLayout.setupWithViewPager(viewpager);

        deleteBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                DeleteData(id);
            }
        });
        editBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Context oof = v.getContext();
                Intent intent = new Intent(oof, BewerkTaak.class);
                intent.putExtra("title", title);
                intent.putExtra("priority", priority);
                intent.putExtra("description", description);
                intent.putExtra("id", id);
                oof.startActivity(intent);
            }
        });
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

        Bundle bundle = new Bundle();
        bundle.putString("Description", description);
        Fragment fragmentDesc = new DescriptionTab();
        fragmentDesc.setArguments(bundle);

        viewPagerAdapter.addFragment(fragmentDesc, "Beschrijving"); //new DescriptionTab(), "Beschrijving"
        viewPagerAdapter.addFragment(new UsersTab(), "Ingeschreven");
        viewPagerAdapter.addFragment(new CommentsTab(), "Opmerkingen");
        viewpager.setAdapter(viewPagerAdapter);
    }

    public void DeleteData(final String id){

        class SendPostReqAsyncTask extends AsyncTask<String, Void, String> {
            // Using AsyncTask to execute heavier tasks in the background on a dedicated thread
            // --> the app runs on a single thread, thus executing DeleteData() which takes time to get a responsive could make the app non-responsive.

            @Override
            protected String doInBackground(String... params) {
                // The code is being executed in the background (thus doInBackground())

                String IdHolder = id;

                List<NameValuePair> nameValuePairs = new ArrayList<NameValuePair>(); // Making an list for the to be inserted data.

                nameValuePairs.add(new BasicNameValuePair("id", IdHolder));
                // Matching the value with the key from the table.

                try { // Try so we can test for errors
                    HttpClient httpClient = new DefaultHttpClient();

                    HttpPost httpPost = new HttpPost(ServerURL);

                    httpPost.setEntity(new UrlEncodedFormEntity(nameValuePairs));

                    HttpResponse httpResponse = httpClient.execute(httpPost);

                    HttpEntity httpEntity = httpResponse.getEntity();
                }
                catch (Exception e) {
                    System.out.println("Exception was thrown");
                    e.printStackTrace();
                } // Error message if something went wrong

                return "Taak verwijderd";

            }

            @Override
            protected void onPostExecute(String result) {
                Toast.makeText(TakenDetails.this, "Taak verwijderd!", Toast.LENGTH_LONG).show();
                Intent intent0 = new Intent(getApplicationContext(), Takenlijst.class);
                intent0.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                intent0.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intent0);
            }
        }

        SendPostReqAsyncTask sendPostReqAsyncTask = new SendPostReqAsyncTask();

        sendPostReqAsyncTask.execute(id);
    }
}