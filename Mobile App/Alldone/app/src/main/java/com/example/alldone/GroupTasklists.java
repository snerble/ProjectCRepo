package com.example.alldone;

import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.text.InputType;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.floatingactionbutton.FloatingActionButton;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import org.w3c.dom.Text;

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

        FloatingActionButton sharefab = findViewById(R.id.shareFab);
        sharefab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                final AlertDialog.Builder msgbox = getInputBox("Voer een groepscode in");
                msgbox.setPositiveButton("OK", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        EditText text = ((AlertDialog) dialog).findViewById((R.id.titleEditText));
                        new JoinGroup_Task().execute(text.getText().toString());
                    }
                });
                msgbox.show();
            }
        });

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

    class JoinGroup_Task extends AsyncTask<String, Void, Response> {
        @Override
        protected Response doInBackground(String... args) {
            try {
                JSONObject json = new JSONObject()
                        .put("code", args[0]);
                return Connection.Send("groupsharing", "POST", json.toString());
            } catch (JSONException e) {
                throw new RuntimeException(e);
            }
        }

        @Override
        protected void onPostExecute(Response response) {
            if (response.IsSuccessful()) {
                Toast.makeText(getApplicationContext(), "Groep successvol toegetreden!", Toast.LENGTH_LONG).show();
            } else if (response.StatusCode == 400) {
                Toast.makeText(getApplicationContext(), "De ingevoerde code klopt niet.", Toast.LENGTH_LONG).show();
            } else if (response.StatusCode == 409) {
                Toast.makeText(getApplicationContext(), "Je zit al in die groep!", Toast.LENGTH_LONG).show();
            } else if (response.StatusCode == 410) {
                Toast.makeText(getApplicationContext(), "De ingevoerde code is verlopen.", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(getApplicationContext(), "Iets ging verkeerd. Probeer het later nog een keer.", Toast.LENGTH_LONG).show();
            }
        }
    }

    class MyAdapter extends ArrayAdapter<JSONObject> {

        Context context;
        JSONObject[] Groups;

        MyAdapter (Context c, JSONObject... groups) {
            super(c, R.layout.layout_grouptasklists, groups);
            this.context = c;
            this.Groups = groups;
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater layoutInflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View row = layoutInflater.inflate(R.layout.layout_grouptasklists, parent, false);
            TextView myTitle = row.findViewById(R.id.usernameText);
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

    private AlertDialog.Builder getInputBox(String title) {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle(title);

        // Set up the input
        final EditText input = new EditText(this);
        input.setId(R.id.titleEditText);
        // Specify the type of input expected
        input.setInputType(InputType.TYPE_CLASS_TEXT);
        builder.setView(input);

        // Set up the cancel button
        builder.setNegativeButton("Annuleren", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                dialog.cancel();
            }
        });

        return builder;
    }
}
