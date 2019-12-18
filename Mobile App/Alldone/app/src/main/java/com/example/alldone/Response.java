package com.example.alldone;

import org.json.JSONException;
import org.json.JSONObject;

public class Response {
    public final String Data;
    public final int StatusCode;
    public final String StatusDescription;

    public Response() {
        this(null, -1, "");
    }
    public Response(String data, int statusCode, String statusDescription) {
        this.Data = data;
        this.StatusCode = statusCode;
        this.StatusDescription = statusDescription;
    }

    public boolean IsInformational() { return Integer.toString(StatusCode).startsWith("1"); }
    public boolean IsSuccessful() { return Integer.toString(StatusCode).startsWith("2"); }
    public boolean IsRedirect() { return Integer.toString(StatusCode).startsWith("3"); }
    public boolean IsClientError() { return Integer.toString(StatusCode).startsWith("4"); }
    public boolean IsServerError() { return Integer.toString(StatusCode).startsWith("5"); }

	public JSONObject GetJSON() {
        try {
            return new JSONObject(Data);
        } catch (Exception e) {
            return null;
        }
    }

    @Override
    public String toString() {
        return String.format("%d - '%s'", StatusCode, StatusDescription);
    }
    
    public void PrettyPrint() {
        try {
            JSONObject json = GetJSON();
            System.out.println(toString());
            System.out.println("Response: " + (json == null ? '"' + (Data == null ? "" : Data) + '"' : json.toString(4)));
        } catch (JSONException e) {
            throw new RuntimeException(e);
        }
    }
}