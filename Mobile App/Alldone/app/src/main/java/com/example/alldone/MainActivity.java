package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;

import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends AppCompatActivity {
    EditText username = (EditText)findViewById(R.id.user);
    EditText password = (EditText)findViewById(R.id.password);
    Button login = (Button)findViewById(R.id.buttonLogin);

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        login.setOnClickListener(new View.OnClickListener() {
         @Override
         public void onClick(View v) {
            if(username.getText().toString().equals("admin") && password.getText().toString().equals("admin")) {
                Toast.makeText(getApplicationContext(), "Doorsturen...",Toast.LENGTH_SHORT).show();
                startActivity(new Intent(MainActivity.this, Takenlijst.class));
            }
            else {
                Toast.makeText(getApplicationContext(), "Verkeerde gebruikersnaam en/of wachtwoord",Toast.LENGTH_SHORT).show();
            }
         }
      });

      login.setOnClickListener(new View.OnClickListener() {
         @Override
         public void onClick(View v) {
            finish();
         }
      });	
    }
}
