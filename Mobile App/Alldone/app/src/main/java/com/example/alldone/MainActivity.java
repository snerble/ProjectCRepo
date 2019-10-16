package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

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
            if(username.getText().toString().equals("admin") && password.getText().toString().equals("admin")) {
                Toast.makeText(getApplicationContext(), "Je bent ingelogd",Toast.LENGTH_SHORT).show();
                Intent intent0 = new Intent(getApplicationContext(), Takenlijst.class);
                intent0.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                intent0.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);

                startActivity(intent0);
            }
            else {
                Toast.makeText(getApplicationContext(), "Verkeerde gebruikersnaam en/of wachtwoord",Toast.LENGTH_SHORT).show();
            }
         }
      });
    }
}
