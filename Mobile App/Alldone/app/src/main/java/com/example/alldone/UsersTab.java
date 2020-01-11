package com.example.alldone;


import android.content.Context;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Date;
import java.util.List;


/**
 * A simple {@link Fragment} subclass.
 */
public class UsersTab extends Fragment {
    public int taskId;

    private LayoutInflater inflater;
    private Context context;
    private View rootView;

    public UsersTab() {
        // Required empty public constructor
    }

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        this.inflater = inflater;
        rootView = inflater.inflate(R.layout.usertab, container, false);
        context = inflater.getContext();

        Button enroll = rootView.findViewById(R.id.enrollBtn);
        Button finish = rootView.findViewById(R.id.finishBtn);

        ListView listView = rootView.findViewById(R.id.lv);
        listView.setAdapter(new MyAdapter(context, new JSONObject[1]));

        enroll.setOnClickListener(new Button.OnClickListener() {
            @Override
            public void onClick(View v) {
                new Enroll_Task().execute();
            }
        });

        finish.setOnClickListener(new Button.OnClickListener() {
            @Override
            public void onClick(View v) {
                new FinishTask_Task().execute();
            }
        });

        // Inflate the layout for this fragment
        return rootView;
    }

    class FinishTask_Task extends AsyncTask<Void, Void, Response> {
        @Override
        protected Response doInBackground(Void... voids) {
            try {
                JSONObject json = new JSONObject()
                        .put("task", TakenDetails.id);
                return Connection.Send("taskenroll", "PATCH", json.toString());
            } catch (JSONException e) {
                throw new RuntimeException(e);
            }
        }

        @Override
        protected void onPostExecute(Response response) {
            if (response.IsSuccessful()) {
                Toast.makeText(context, "Taak successvol afgerond.", Toast.LENGTH_LONG).show();
                TakenDetails.GetStatus_Task task = new TakenDetails.GetStatus_Task();
                task.userFragment = UsersTab.this;
                task.execute();
            } else if (response.StatusCode == 409) {
                Toast.makeText(context, "Je hebt deze taak al afgerond!", Toast.LENGTH_LONG).show();
            } else if (response.StatusCode == 400) {
                Toast.makeText(context, "Je bent nog niet ingeschreven voor deze taak!", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(context, "Iets ging verkeerd. Probeer het later nog een keer.", Toast.LENGTH_LONG).show();
            }
        }
    }

    class Enroll_Task extends AsyncTask<Void, Void, Response> {
        @Override
        protected Response doInBackground(Void... voids) {
            try {
                JSONObject json = new JSONObject()
                        .put("task", TakenDetails.id);
                return Connection.Send("taskenroll", "POST", json.toString());
            } catch (JSONException e) {
                throw new RuntimeException(e);
            }
        }

        @Override
        protected void onPostExecute(Response response) {
            if (response.IsSuccessful()) {
                Toast.makeText(context, "Je bent nu ingeschreven voor de taak.", Toast.LENGTH_LONG).show();
                TakenDetails.GetStatus_Task task = new TakenDetails.GetStatus_Task();
                task.userFragment = UsersTab.this;
                task.execute();
            } else if (response.StatusCode == 409) {
                Toast.makeText(context, "Je bent al ingeschreven voor deze taak!", Toast.LENGTH_LONG).show();
            } else {
                Toast.makeText(context, "Iets ging verkeerd. Probeer het later nog een keer.", Toast.LENGTH_LONG).show();
            }
        }
    }

    public void UpdateList(JSONObject[] elements) {
        ListView listView = rootView.findViewById(R.id.lv);
        MyAdapter adapter = new MyAdapter(context, elements);
        adapter.notifyDataSetChanged();
        listView.setAdapter(adapter);
    }

    class MyAdapter extends ArrayAdapter<JSONObject> {
        Context context;
        public ArrayList<JSONObject> enrollments = new ArrayList<>();

        MyAdapter (Context c, JSONObject... enrollments) {
            super(c, R.layout.layout_userstab, enrollments);
            this.context = c;
            setData(enrollments);
        }

        public void setData(JSONObject... elements) {
            enrollments.clear();
            for (JSONObject element : elements) {
                enrollments.add(element);
            }
        }

        @NonNull
        @Override
        public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
            LayoutInflater inflater = (LayoutInflater)parent.getContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            View row = inflater.inflate(R.layout.layout_userstab, parent, false);

            JSONObject enrollment = enrollments.get(position);
            if (enrollment == null) return row;

            TextView usernameText = row.findViewById(R.id.usernameText);
            TextView startDateText = row.findViewById(R.id.startDateText);
            TextView endDateText = row.findViewById(R.id.endDateText);

            try {
                usernameText.setText(enrollment.getString("Username"));
                startDateText.setText(formatUnixSeconds(enrollment.getLong("Start")));
                if (enrollment.has("End"))
                    endDateText.setText(formatUnixSeconds(enrollment.getLong("End")));
            } catch (JSONException e) {
                throw new RuntimeException(e);
            }

            return row;
        }
    }

    private String formatUnixSeconds(long unixSeconds) {
        // convert seconds to milliseconds
        Date date = new java.util.Date(unixSeconds*1000L);
        // Get the standard datetime format
        DateFormat formatter = SimpleDateFormat.getDateTimeInstance();
        // give a timezone reference for formatting (see comment at the bottom)
        formatter.setTimeZone(java.util.TimeZone.getDefault());
        return formatter.format(date);
    }
}
