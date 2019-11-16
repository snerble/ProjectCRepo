package com.example.alldone;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

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

import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        final EditText username = (EditText)findViewById(R.id.user);
        final EditText password = (EditText)findViewById(R.id.password);
        final Button login = (Button)findViewById(R.id.buttonLogin);

        login.setOnClickListener(new View.OnClickListener() {
         @Override
         public void onClick(View v) {
            /*if(username.getText().toString().equals("admin") && password.getText().toString().equals("admin")) {
                Toast.makeText(getApplicationContext(), "Je bent ingelogd",Toast.LENGTH_SHORT).show();
                Intent intent0 = new Intent(getApplicationContext(), Takenlijst.class);
                intent0.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                intent0.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);

                startActivity(intent0);
            }
            else {
                Toast.makeText(getApplicationContext(), "Verkeerde gebruikersnaam en/of wachtwoord",Toast.LENGTH_SHORT).show();
            }*/
            System.out.println("Yo wassup");
            Thread e = new Thread(new Runnable() {
                @Override
                public void run() {
                    NetworkingShit();
                }
            });
            e.start();
         }
      });
    }

    public void NetworkingShit() {
        HttpURLConnection urlConnection = null;
        try{
            URL url = new URL("http://192.168.178.18/logintest");
            urlConnection = (HttpURLConnection) url.openConnection();
            urlConnection.setRequestProperty("Content-Type", "application/json");
            urlConnection.setDoInput(true);
            urlConnection.setDoOutput(true);
            urlConnection.setRequestMethod("GET");
            urlConnection.connect();
            System.out.println("Connected.");

//            DataOutputStream os = new DataOutputStream(urlConnection.getOutputStream());
//            os.flush();
//            os.close();

            BufferedReader br = new BufferedReader(
                    new InputStreamReader(
                            new DataInputStream(
                                    urlConnection.getInputStream()
                            )
                    )
            );
            StringBuilder sb = new StringBuilder();
            String s = null;
            while ((s = br.readLine()) != null)
                sb.append(s);
            br.close();
            System.out.println(sb.toString());
        } catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        } finally {
            if (urlConnection != null)
                urlConnection.disconnect();
        }
    }
}
