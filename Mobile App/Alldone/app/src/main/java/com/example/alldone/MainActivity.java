package com.example.alldone;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.text.InputType;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.DataOutput;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {
    String msg = "";
    private JSONObject json;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        final EditText username = findViewById(R.id.username);
        final EditText password = findViewById(R.id.password);
        final Button login = findViewById(R.id.buttonLogin);
        final Button regist = findViewById(R.id.buttonToRegist);
        final Button editIp = findViewById(R.id.editServerIpBtn);

        editIp.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String currentDomain = "";
                String[] tokens = Connection.DOMAIN.split("/");
                if (tokens.length == 3) currentDomain = tokens[2];

                final AlertDialog.Builder msgbox = getInputBox("Enter a new server IP", currentDomain);
                msgbox.setPositiveButton("OK", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        EditText text = ((AlertDialog) dialog).findViewById((R.id.titleEditText));
                        Connection.DOMAIN = "http://" + text.getText().toString() + "/";
                    }
                });
                msgbox.show();
            }
        });

        regist.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent toRegist = new Intent(getApplicationContext(), Register.class);
                startActivity(toRegist);
            }
        });

        login.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                JSONObject jObj = null;
                try {
                    jObj = new JSONObject()
                            .put("username", username.getText())
                            .put("password", password.getText());
                } catch (JSONException e) {
                    e.printStackTrace();
                }

                jObj.toString();

                final JSONObject finalJObj = jObj;
                Thread e = new Thread(new Runnable() {
                    @Override
                    public void run() {
                        Response response = Connection.Send("login", "POST", finalJObj.toString());

                        response.PrettyPrint();
                        if(response.IsSuccessful()) {
                            msg = "Je bent ingelogd";
                            Connection.session = response.GetJSON().optString("sessionId");
                            Intent intent0 = new Intent(getApplicationContext(), GroupTasklists.class);
                            startActivity(intent0);
                        }
                        else {
                            msg = "Er is iets fout gegaan";
                        }
                        MainActivity.this.runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                Toast.makeText(getApplicationContext(), msg,Toast.LENGTH_SHORT).show();
                            }
                        });
                    }
                });
                e.start();
            }
        });
    }

    private AlertDialog.Builder getInputBox(String title, String placeholder) {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle(title);

        // Set up the input
        final EditText input = new EditText(this);
        input.setId(R.id.titleEditText);
        input.setText(placeholder);
        // Specify the type of input expected
        input.setInputType(InputType.TYPE_CLASS_TEXT);
        builder.setView(input);

        // Set up the cancel button
        builder.setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
            @Override
            public void onClick(DialogInterface dialog, int which) {
                dialog.cancel();
            }
        });

        return builder;
    }
}