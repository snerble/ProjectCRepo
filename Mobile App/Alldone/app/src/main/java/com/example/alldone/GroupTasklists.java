package com.example.alldone;

import android.app.ProgressDialog;
import android.content.Context;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import android.os.AsyncTask;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.HashMap;
import java.util.Iterator;

public class GroupTasklists extends AppCompatActivity {

    ListView listView;
    String mTitle[] = {"test", "yeet"};
    String sTitle[] = {"oop", "oof"};

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_grouptasklists);

        listView = findViewById(R.id.lv);

        class RetrieveTasklists extends AsyncTask<String, String, Void> {
            private Exception exception;
            ProgressDialog iets;

            protected Void doInBackground(String... url) {
                try {
                    Response response = Connection.Send("tasklist", "GET");
                    response.PrettyPrint();
                    JSONArray yeet = response.GetJSON().getJSONArray("results");

                    final String[] titles = new String[yeet.length()];
                    for (int i = 0; i < yeet.length(); i++) {
                        titles[i] = yeet.getJSONObject(i).getString("Name");
                    }

                    for (String title : titles){
                        System.out.println(title);
                    }

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            MyAdapter adapter = new MyAdapter(GroupTasklists.this, titles, titles);
                            listView.setAdapter(adapter);
                        }
                    });

                    return null;

                } catch (Exception e) {
                    this.exception = e;
                    e.printStackTrace();
                    System.out.println("Fuck");
                    return null;
                }
            }

            protected void onPostExecute(Void jsonObject) {
                //String oof[] = jsonObject.toString();

            }
        }

        System.out.println("Yeeting tasklist from server");
        RetrieveTasklists eh = new RetrieveTasklists();
        eh.execute();
        //Log.d("yeet", yeet.toString());
//
//        MyAdapter adapter = new MyAdapter(this, mTitle, sTitle);
//        listView.setAdapter(adapter);

       // 145.137.121.58

        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                Toast.makeText(GroupTasklists.this, "yeet", Toast.LENGTH_SHORT).show();
            }
        });
    }

    class MyAdapter extends ArrayAdapter<String> {

        Context context;
        String rTitle[];
        String rSubtitle[];

        MyAdapter (Context c, String title[], String subtitle[]) {
            super(c, R.layout.layout_grouptasklists, R.id.textView1, title);
            this.context = c;
            this.rTitle = title;
            this.rSubtitle = subtitle;
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater layoutInflater = (LayoutInflater)getApplicationContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View row = layoutInflater.inflate(R.layout.layout_grouptasklists, parent, false);
            TextView myTitle = row.findViewById(R.id.textView1);
            TextView mySubtitle = row.findViewById(R.id.textView2);

            myTitle.setText(rTitle[position]);
            mySubtitle.setText(rSubtitle[position]);

            return row;
        }
    }
}
