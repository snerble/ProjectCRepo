package com.example.alldone;

import java.net.HttpURLConnection;
import java.net.URL;

public class Connector {
    // Connector get connects to the given link

    public static HttpURLConnection connect(String urlAddress)
    {
        try {
            URL url = new URL(urlAddress);
            HttpURLConnection con= (HttpURLConnection) url.openConnection();

            con.setRequestMethod("GET");
            con.setConnectTimeout(20000);
            con.setReadTimeout(20000);
            con.setDoInput(true);

            return con;

        } catch (Exception e) {
            System.out.println("Exception was thrown");
            e.printStackTrace(); // Prints the stack trace to System.err
        }

        return null;
    }

}
