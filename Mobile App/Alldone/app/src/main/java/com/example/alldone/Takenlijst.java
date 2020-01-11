package com.example.alldone;

import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import android.view.LayoutInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.drawerlayout.widget.DrawerLayout;

import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.google.android.material.navigation.NavigationView;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class Takenlijst extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {
    private DrawerLayout mDrawerLayout;
    private ActionBarDrawerToggle mToggle;

    public int id;
    public String tasks;

    ListView listView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_takenlijst);

        mDrawerLayout = (DrawerLayout) findViewById(R.id.drawerLayout);
        mToggle = new ActionBarDrawerToggle(this, mDrawerLayout, R.string.open, R.string.close);

        mDrawerLayout.addDrawerListener(mToggle);
        mToggle.syncState();

        Intent intent = getIntent();
        id = intent.getIntExtra("id", -1);
        tasks = intent.getStringExtra("tasks");

        listView = findViewById(R.id.lv);

        FloatingActionButton fab = findViewById(R.id.fab);
        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent makeTask = new Intent(getApplicationContext(), MaakTaak.class);
                makeTask.putExtra("groupid", id);
                startActivity(makeTask);
            }
        });

        class RetrieveTasks extends AsyncTask<String, Void, Response> {
            @Override
            protected Response doInBackground(String... url) {
                try {
                    JSONObject json = new JSONObject()
                            .put("group", id);
                    return Connection.Send("task", "GET", json.toString());
                } catch (JSONException e) {
                    throw new RuntimeException(e);
                }
            }

            @Override
            protected void onPostExecute(Response response) {
                try {
                    JSONArray responseJson = response.GetJSON().getJSONArray("results");

                    final JSONObject[] elements = new JSONObject[responseJson.length()];
                    for (int i = 0; i < responseJson.length(); i++) {
                        elements[i] = responseJson.getJSONObject(i);
                    }

                    MyAdapter adapter = new MyAdapter(Takenlijst.this, elements);
                    listView.setAdapter(adapter);
                } catch (JSONException e) {
                    // Won't happen
                    throw new RuntimeException(e);
                }
            }
        }

        System.out.println("Getting tasklists from server");
        new RetrieveTasks().execute();

        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                JSONObject group = ((MyAdapter)parent.getAdapter()).Tasks[position];
                try {
                    OpenTasklist_Task task = new OpenTasklist_Task();
                    task.group_id = group.getInt("Id");
                    task.execute(group);
                } catch (JSONException e) {
                    throw new RuntimeException(e);
                }
            }
        });
    }

    class OpenTasklist_Task extends AsyncTask<JSONObject, Float, Response> {
        public int group_id;

        @Override
        protected Response doInBackground(JSONObject... jsonObjects) {
            if (jsonObjects.length < 1)
                throw new IllegalArgumentException("At least one JSONObject must be passed as an argument.");

            JSONObject group = jsonObjects[0];
            try {
                JSONObject json = new JSONObject()
                        .put("group", group.getInt("Id"));
                return Connection.Send("task", "GET", json.toString());
            } catch (JSONException e) {
                // Won't happen
                throw new RuntimeException(e);
            }
        }

        @Override
        protected void onPostExecute(Response response) {
            response.PrettyPrint();

            Toast.makeText(Takenlijst.this, response.toString(), Toast.LENGTH_SHORT).show();

            Context context = Takenlijst.this;
            Intent intent = new Intent(context , TakenDetails.class);
            intent.putExtra("id", group_id);
            intent.putExtra("tasks", response.Data);
            context.startActivity(intent);
        }
    }

    class MyAdapter extends ArrayAdapter<JSONObject> {

        Context context;
        JSONObject Tasks[];

        MyAdapter (Context c, JSONObject... tasks) {
            super(c, R.layout.layout_takenlijst2, R.id.textView1, tasks);
            this.context = c;
            this.Tasks = tasks;
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater layoutInflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View row = layoutInflater.inflate(R.layout.layout_takenlijst2, parent, false);
            TextView myTitle = row.findViewById(R.id.textView1);
            TextView mySubtitle = row.findViewById(R.id.textView2);

            JSONObject task = this.Tasks[position];
            try {
                myTitle.setText(task.getString("Title"));
                String description = task.optString("Description");
                mySubtitle.setText(description == null ? "" : description);
            } catch (JSONException e) {
                // Won't happen
                throw new RuntimeException(e);
            }

            return row;
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
