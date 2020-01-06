package com.example.alldone;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLEncoder;
import java.security.InvalidParameterException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class Request {
	// Default fields for request objects
	private static String DEFAULT_PROTOCOL = "http";
	private static String DEFAULT_HOST = "localhost";
	private static int DEFAULT_PORT = 80;
	private static int DEFAULT_TIMEOUT = 5000;

	// Locals that reflect the defaults but can be changed when needed.
	private String Protocol = DEFAULT_PROTOCOL;
	private String Host = DEFAULT_HOST;
	private int Port = DEFAULT_PORT;
	private int Timeout = DEFAULT_TIMEOUT;

	private String UrlPath;
	private URL Url;
	private String Method = "GET";

	public String ContentType = "text/plain";
	public Map<?, ?> Query = new HashMap<String, Object>();

	public Request(String urlPath) {
		UrlPath = urlPath;
		SetURL();
	}

	public Request(String urlPath, String method) {
		this(urlPath);
		Method = method;
	}

	public HttpURLConnection OpenConnection() throws IOException {
		HttpURLConnection connection = (HttpURLConnection) Url.openConnection();
		connection.setRequestMethod(Method);
		connection.setConnectTimeout(Timeout);
		return connection;
	}

	/**
	 * Sets the Url member and in doing so, parses and validates the url parameters.
	 */
	private void SetURL() {
		try {
			// Build query parameters
			List<String> params = new ArrayList<>();
			if (Query != null) {
				for (Map.Entry<?, ?> entry : Query.entrySet()) {
					params.add(String.format("%s=%s",
						URLEncoder.encode(entry.getKey().toString(), "UTF-8"),
						URLEncoder.encode(entry.getValue().toString(), "UTF-8")
					));
				}
			}
			// Join query parameters into a string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < params.size(); i++) {
				if (i != 0) sb.append('&');
				sb.append(params.get(i));
			}

			// Create URL object
			Url = new URL(Protocol, Host, Port, UrlPath + '?' + sb.toString());
		} catch (MalformedURLException | UnsupportedEncodingException e) {
			// Unlikely exceptions get rethrown as RuntimeExceptions for quality-of-life
			throw new RuntimeException(e);
		}
	}

	/**
	 * @return A URL built from the parameters, and in doing so, validates them.
	 * @throws MalformedURLException When the default url parameters are invalid.
	 */
	private static URL GetURL(String protocol, String host, int port) throws MalformedURLException {
		return new URL(protocol, host, port, "/");
	}

	/**
	 * Sets the default protocol parameter for all subsequent Request instances.
	 * @param protocol The protocol to set as default.
	 */
	public static void SetDefaultProtocol(String protocol) {
		// Create a URL to try to invoke the exception
		try {
			GetURL(protocol, DEFAULT_HOST, DEFAULT_PORT);
		} catch (MalformedURLException e) {
			// Rethrow as runtime exception to avoid the throws clause (malformed url is unlikely anyway)
			throw new RuntimeException(e);
		}
		// Set the value
		DEFAULT_PROTOCOL = protocol;
	}

	/**
	 * Sets the default host parameter for all subsequent Request instances.
	 * @param host The hostname to set as default.
	 */
	public static void SetDefaultHost(String host) {
		// Create a URL to try to invoke the exception
		try {
			GetURL(DEFAULT_PROTOCOL, host, DEFAULT_PORT);
		} catch (MalformedURLException e) {
			// Rethrow as runtime exception to avoid the throws clause (malformed url is unlikely anyway)
			throw new RuntimeException(e);
		}
		// Set the value
		DEFAULT_HOST = host;
	}

	/**
	 * Sets the default port parameter for all subsequent Request instances.
	 */
	public static void SetDefaultPort(int port) {
		// Create a URL to try to invoke the exception
		try {
			GetURL(DEFAULT_PROTOCOL, DEFAULT_HOST, port);
		} catch (MalformedURLException e) {
			// Rethrow as runtime exception to avoid the throws clause (malformed url is unlikely anyway)
			throw new RuntimeException(e);
		}
		// Set the value
		DEFAULT_PORT = port;
	}

	/**
	 * Sets the default timeout for all subsequent Request instances.
	 * @param timeout The timeout to set as default.
	 */
	public static void SetDefaultTimeout(int timeout) {
		if (timeout < 0) throw new InvalidParameterException("Timeout may not be less than 0.");
		DEFAULT_TIMEOUT = timeout;
	}
}