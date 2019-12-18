package com.example.alldone;

import android.app.ProgressDialog;
import android.content.Context;
import android.os.AsyncTask;
import android.widget.ListView;
import android.widget.Toast;

import com.example.alldone.model.Task;
import com.example.alldone.ListAdapter;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;

public class DataParser  extends AsyncTask<Void,Void,Integer>{
    // DataParser adds the tasks from the database

    Context c;
    ListView lv;
    String jsonData;

    ProgressDialog pd;
    ArrayList<Task> tasks = new ArrayList<>();

    public DataParser(Context c, ListView lv, String jsonData) {
        this.c = c;
        this.lv = lv;
        this.jsonData = jsonData;
    }

    @Override
    protected void onPreExecute() {
        super.onPreExecute();

        pd = new ProgressDialog(c);
        pd.setTitle("Parse");
        pd.setMessage("Parsing...Please wait");
        pd.show();
    }

    @Override
    protected Integer doInBackground(Void... params) {
        return this.parseData();
    }

    @Override
    protected void onPostExecute(Integer result) {
        super.onPostExecute(result);

        pd.dismiss();
        if(result == 0) {
            Toast.makeText(c,"Unable to parse",Toast.LENGTH_SHORT).show();
        } else {
            ListAdapter adapter = new ListAdapter(c,tasks);
            lv.setAdapter(adapter);
        }
    }

    private int parseData()
    {
        try {
            JSONArray ja = new JSONArray(jsonData);
            JSONObject jo = null;

            tasks.clear();
            Task s = null;

            for(int i = 0; i<ja.length(); i++)
            {
                jo = ja.getJSONObject(i);

                int id = jo.getInt("id");
                String title = jo.getString("title");
                String priority = jo.getString("priority");
                String description = jo.getString("description");

                s = new Task();
                s.setId(id);
                s.setTitle(title);
                s.setPriority(priority);
                s.setDescription(description);

                tasks.add(s);
            }
            return 1;

        } catch (JSONException e) {
            System.out.println("Exception was thrown");
            e.printStackTrace(); // Prints the stack trace to System.err
        }

        return 0;
    }
}
