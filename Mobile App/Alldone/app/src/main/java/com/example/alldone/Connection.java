package com.example.alldone;

import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.ConnectException;
import java.net.HttpURLConnection;
import java.net.URL;
import android.util.Base64;

import org.json.JSONException;
import org.json.JSONObject;

public class Connection {
	//public static String DOMAIN = "http://145.137.121.146/";
	//public static String DOMAIN = "http://145.137.121.58/";
	//public static String DOMAIN = "http://192.168.188.62/";
	public static String DOMAIN = "http://145.137.121.239/";

	public static String session;
	public static byte[] key;

	static Response Send(String link, String method) {
		return Send(link, method, (byte[])null);
	}
	static Response Send(String link, String method, String data) {
		return Send(link, method, data == null ? null : data.getBytes());
	}
	static Response Send(String link, String method, byte[] data) {
		method = method.toUpperCase();

		HttpURLConnection conn = null;
		try {
			URL url = null;
			// Switch GET method to POST if the data isn't null
			if ((method.equals("GET") || method.equals("HEAD")) && data != null) {
				url = new URL(DOMAIN + link + "?method=" + method);
				method = "POST";
			} else url = new URL(DOMAIN + link);

			conn = (HttpURLConnection) url.openConnection();
			conn.addRequestProperty("Content-Type", "application/json");
			conn.setRequestMethod(method);
			conn.setDoInput(!method.equals("HEAD"));
			conn.setDoOutput(!(method.equals("GET") || method.equals("HEAD")));
			
			if (data != null && session != null && key != null)
			{
				// Create AES instance with our key
				AES aes = new AES(key);
				data = aes.Encode(data);

				// Set headers indicating encryption
				conn.setRequestProperty("Content-Type", "application/octet-stream");
				conn.addRequestProperty("Content-IV", Base64.encodeToString(aes.GetIV(), Base64.DEFAULT));
			}
			// Add session cookie if it isn't null
			if (session != null) conn.addRequestProperty("Cookie", "session=" + session);
			conn.connect();

			// write data
			if (conn.getDoOutput()) {
				OutputStream out = conn.getOutputStream();
				if (data != null) out.write(data);
				out.close();
			}
			
			// read data
			String readData = null;
			if (conn.getDoInput()) {
				if (session != null && key != null && conn.getHeaderField("Content-IV") != null) {
					// If the request was encrypted and the response is also encrypted, decode the response
					AES aes = new AES(key, conn.getHeaderField("Content-IV"));
					// Read response bytes
					ByteArrayOutputStream memStream = new ByteArrayOutputStream();
					InputStream in = conn.getInputStream();
					// Write all bytes in the input stream to the byte array stream
					while (true) {
						int readByte = in.read();
						if (readByte == -1) break;
						memStream.write(readByte);
					}
					byte[] encoded = memStream.toByteArray();
					// Decode the data
					readData = new String(aes.Decode(encoded));
				} else {
					// If the data is not encrypted, just read it as plaintext
					BufferedReader in = new BufferedReader(new InputStreamReader(conn.getInputStream()));
					StringBuilder sb = new StringBuilder();

					for (String line; (line = in.readLine()) != null; )
						sb.append(line);
					in.close();

					readData = sb.toString();
				}
			}
			
			return new Response(readData, conn.getResponseCode(), conn.getResponseMessage());
		} catch(ConnectException e) {
			e.printStackTrace();
			return new Response();
		} catch(IOException e) {
			e.printStackTrace();
			try {
				// read error stream if it isn't null
				InputStream is = conn.getErrorStream();
				if (is != null) {
					BufferedReader in = new BufferedReader(new InputStreamReader(conn.getErrorStream()));	
					StringBuilder sb = new StringBuilder();
					while (in.ready()) sb.append(in.readLine());
					in.close();
					return new Response(sb.toString(), conn.getResponseCode(), conn.getResponseMessage());
				}
				return new Response(null, conn.getResponseCode(), conn.getResponseMessage());
			} catch (IOException _e) {
				return new Response();
			}
		} finally {
			conn.disconnect();
		}
	}
}