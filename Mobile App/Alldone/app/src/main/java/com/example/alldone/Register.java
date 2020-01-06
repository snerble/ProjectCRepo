package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;

import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import android.widget.EditText;

public class Register extends AppCompatActivity implements View.OnClickListener {

    String msg = "";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        final EditText username = (EditText)findViewById(R.id.username);
        final EditText password = (EditText)findViewById(R.id.password);
        final EditText passrep = (EditText)findViewById(R.id.passRepeat);
        final Button regist = (Button)findViewById(R.id.buttonRegister);
        final Button login = (Button)findViewById(R.id.buttonToLogin);

        login.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent toLogin = new Intent(getApplicationContext(), MainActivity.class);
                startActivity(toLogin);
            }
        });

        regist.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (password.getText().toString().equals(passrep.getText().toString())) {
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
                        Response response = Connection.Send("register", "POST", finalJObj.toString());

                            response.PrettyPrint();

                            switch(response.StatusCode){
                                case 201:
                                    msg = "Account aangemaakt.";
                                    break;

                                case 409:
                                    msg = "Deze gebruikersnaam bestaat al.";
                                    break;

                                default:
                                    msg = "Er is iets fout gegaan.";
                                    break;
                            }

                            Register.this.runOnUiThread(new Runnable() {
                                @Override
                                public void run() {
                                    Toast.makeText(getApplicationContext(), msg,Toast.LENGTH_SHORT).show();
                                }
                            });
                            //NetworkThread(finalJObj);
                        }
                    });
                    e.start();
                } else {
                    Toast.makeText(getApplicationContext(), "De ingevoerde wachtwoorden zijn niet hetzelfde.",Toast.LENGTH_SHORT).show();
                }
            }
        });
    }

    public void NetworkThread(JSONObject json) {
        HttpURLConnection urlConnection = null;
        try{
            URL url = new URL("http://192.168.178.18/register");
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.setRequestProperty("Content-Type", "application/json");
            urlConnection.setDoInput(true);
            urlConnection.setDoOutput(true);
            urlConnection.connect();
            System.out.println("Connected.");

            msg = "Account aangemaakt";

            switch(urlConnection.getResponseCode()){
                case 201:
                    break;

                case 409:
                    msg = "Deze gebruikersnaam bestaat al.";
                    break;

                default:
                    return;
            }

            Toast.makeText(getApplicationContext(), msg,Toast.LENGTH_SHORT).show();


        } catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        } finally {
            if (urlConnection != null)
                urlConnection.disconnect();
        }
    }

    @Override
    public void onClick(View v) {

    }
}
