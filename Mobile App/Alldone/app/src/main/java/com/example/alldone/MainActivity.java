package com.example.alldone;

import androidx.appcompat.app.AppCompatActivity;

import android.app.ProgressDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;

import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.android.volley.AuthFailureError;
import com.android.volley.Request;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

public class MainActivity extends AppCompatActivity implements View.OnClickListener {
    private EditText editTextUsername;
    private EditText editTextPassword;
    private Button login;
    private Button toRegister;
    private ProgressDialog progressDialog;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        /*if(SharedPrefManager.getInstance(this).isLoggedIn()){
            finish();
            startActivity(new Intent(this, Takenlijst.class));
            return;
        } */

        editTextUsername = (EditText)findViewById(R.id.username);
        editTextPassword = (EditText)findViewById(R.id.password);
        login = (Button)findViewById(R.id.buttonLogin);
        toRegister = (Button)findViewById(R.id.buttonToRegist);

        progressDialog = new ProgressDialog(this);
        progressDialog.setMessage("Please wait...");

        toRegister.setOnClickListener(this);
        login.setOnClickListener(this);
    }

    public void toRegister() {
        Intent intent1 = new Intent(getApplicationContext(), Register.class);
        intent1.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        intent1.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TASK);

        startActivity(intent1);
    }

    private void login() {
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
            final String username = editTextUsername.getText().toString().trim();
            final String password = editTextPassword.getText().toString().trim();

            progressDialog.show();

            StringRequest stringRequest = new StringRequest(
                    Request.Method.POST,
                    constants.URL_LOGIN,
                    new Response.Listener<String>() {
                        @Override
                        public void onResponse(String response) {
                            progressDialog.dismiss();
                            try {
                                JSONObject obj = new JSONObject(response);
                                if(!obj.getBoolean("error")){
                                    SharedPrefManager.getInstance(getApplicationContext())
                                            .login(
                                                    obj.getInt("id"),
                                                    obj.getString("username"),
                                                    obj.getString("email")
                                            );
                                    startActivity(new Intent(getApplicationContext(), Takenlijst1.class));
                                    finish();
                                }else{
                                    Toast.makeText(
                                            getApplicationContext(),
                                            obj.getString("message"),
                                            Toast.LENGTH_LONG
                                    ).show();
                                }
                            } catch (JSONException e) {
                                e.printStackTrace();
                            }
                        }
                    },
                    new Response.ErrorListener() {
                        @Override
                        public void onErrorResponse(VolleyError error) {
                            progressDialog.dismiss();

                            Toast.makeText(
                                    getApplicationContext(),
                                    error.getMessage(),
                                    Toast.LENGTH_LONG
                            ).show();
                        }
                    }
            ){
                @Override
                protected Map<String, String> getParams() throws AuthFailureError {
                    Map<String, String> params = new HashMap<>();
                    params.put("username", username);
                    params.put("password", password);
                    return params;
                }

            };

            RequestHandler.getInstance(this).addToRequestQueue(stringRequest);
    }

    @Override
    public void onClick(View view) {
        if (view == toRegister) {
            toRegister();
        }

        if (view == login) {
            login();
        }
    }
}
