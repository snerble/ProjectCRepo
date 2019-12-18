package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;

import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

import android.widget.EditText;

public class Register extends AppCompatActivity implements View.OnClickListener {

    private EditText editTextUsername, editTextPassword, editTextEmail;
    private Button buttonRegister;
    public Button toLogin;
    private ProgressDialog progressDialog;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_register);

        editTextUsername = findViewById(R.id.username);
        editTextPassword = findViewById(R.id.password);
        editTextEmail = findViewById(R.id.email);

        buttonRegister = findViewById(R.id.buttonRegister);

        progressDialog = new ProgressDialog(this);

        buttonRegister.setOnClickListener(this);

        toLogin = findViewById(R.id.buttonToLogin);
        toLogin.setOnClickListener(this);
    }

    public void toLogin() {
        Intent intent0 = new Intent(getApplicationContext(), MainActivity.class);
        intent0.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        intent0.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);

        startActivity(intent0);
    }

    @Override
    public void onClick(View view) {
        if (view == toLogin) {
            toLogin();
        }
    }
}
