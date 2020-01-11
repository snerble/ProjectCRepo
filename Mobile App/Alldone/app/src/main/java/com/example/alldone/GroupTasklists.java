package com.example.alldone;

import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class GroupTasklists extends AppCompatActivity {

    ListView listView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_grouptasklists);

        listView = findViewById(R.id.lv);

        class RetrieveTasklists extends AsyncTask<String, Void, Response> {
            @Override
            protected Response doInBackground(String... url) {
                return Connection.Send("group", "GET");
            }

            @Override
            protected void onPostExecute(Response response) {
                try {
                    JSONArray responseJson = response.GetJSON().getJSONArray("results");

                    final JSONObject[] elements = new JSONObject[responseJson.length()];
                    for (int i = 0; i < responseJson.length(); i++) {
                        elements[i] = responseJson.getJSONObject(i);
                    }

                    MyAdapter adapter = new MyAdapter(GroupTasklists.this, elements);
                    listView.setAdapter(adapter);
                } catch (JSONException e) {
                    // Won't happen
                    throw new RuntimeException(e);
                }
            }
        }

        System.out.println("Getting tasklists from server");
        new RetrieveTasklists().execute();

        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                JSONObject group = ((MyAdapter)parent.getAdapter()).Groups[position];
                try {
                    Context context = GroupTasklists.this;
                    Intent intent = new Intent(context , Takenlijst.class);
                    intent.putExtra("id", group.getInt("Id"));
                    context.startActivity(intent);
                } catch (JSONException e) {
                    // Won't happen
                    throw new RuntimeException(e);
                }

            }
        });
    }

    class MyAdapter extends ArrayAdapter<JSONObject> {

        Context context;
        JSONObject Groups[];

        MyAdapter (Context c, JSONObject... groups) {
            super(c, R.layout.layout_grouptasklists, R.id.textView1, groups);
            this.context = c;
            this.Groups = groups;
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater layoutInflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View row = layoutInflater.inflate(R.layout.layout_grouptasklists, parent, false);
            TextView myTitle = row.findViewById(R.id.textView1);
            TextView mySubtitle = row.findViewById(R.id.textView2);

            JSONObject group = this.Groups[position];
            try {
                myTitle.setText(group.getString("Name"));
                String description = group.optString("Description");
                mySubtitle.setText(description == null ? "" : description);
            } catch (JSONException e) {
                // Won't happen
                throw new RuntimeException(e);
            }

            return row;
        }
    }
}
