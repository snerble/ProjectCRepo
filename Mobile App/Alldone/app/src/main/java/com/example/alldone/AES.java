package com.example.alldone;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.security.GeneralSecurityException;
import java.security.InvalidParameterException;
import android.util.Base64;

import javax.crypto.Cipher;
import javax.crypto.CipherOutputStream;
import javax.crypto.KeyGenerator;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;

public class AES {
    private final Cipher cipher;

    private byte[] key;
    private byte[] iv;

    public AES() {
        try {
            cipher = Cipher.getInstance("AES/CBC/NoPadding");
        } catch (GeneralSecurityException e) {
            throw new RuntimeException(e);
        }
    }
    public AES(byte[] key) {
        this();
        SetKey(key);
    }
    public AES(byte[] key, byte[] iv) {
        this(key);
        SetIV(iv);
    }
    public AES(byte[] key, String base64IV) {
        this(key);
        SetIV(base64IV);
    }
    public AES(String base64Key) {
        this();
        SetKey(base64Key);
    }
    public AES(String base64Key, byte[] iv) {
        this(base64Key);
        SetIV(iv);
    }
    public AES(String base64Key, String base64IV) {
        this(base64IV);
        SetIV(base64IV);
    }

    public byte[] GetKey() {
        if (key == null) {
            try {
                SetKey(KeyGenerator.getInstance("AES").generateKey().getEncoded());
            } catch (GeneralSecurityException e) {
                throw new RuntimeException(e);
            }
        }
        return key;
    }
    public String GetBase64Key() {
        byte[] key = GetKey();
        return key == null ? null : Base64.encodeToString(key, Base64.DEFAULT);
    }
    public void SetKey(byte[] key) {
        if (key.length != 32) throw new InvalidParameterException("Key must contain 32 elements.");
        this.key = key;
    }
    public void SetKey(String base64Key) {
        SetKey(Base64.decode(base64Key, Base64.DEFAULT));
    }

    public byte[] GetIV() {
        if (iv == null) return cipher.getIV();
        return iv;
    }
    public String GetBase64IV() {
        byte[] iv = GetIV();
        return iv == null ? null : Base64.encodeToString(iv, Base64.DEFAULT);
    }
    public void SetIV(byte[] iv) {
        if (iv.length != 16) throw new InvalidParameterException("IV must contain 16 elements.");
        this.iv = iv;
    }
    public void SetIV(String base64IV) {
        SetIV(Base64.decode(base64IV, Base64.DEFAULT));
    }

    private void Init(int opmode) {
        // Prevent init for encrypt mode if an IV was already set
        if (iv != null && opmode == Cipher.ENCRYPT_MODE)
            throw new InvalidParameterException("Encrypt mode is only available without a predefined IV.");

        SecretKeySpec keySpec = new SecretKeySpec(key, "AES");
        try {
            if (iv != null) {
                // Initialize with custom IV if it is set
                IvParameterSpec ivSpec = new IvParameterSpec(iv);
                cipher.init(opmode, keySpec, ivSpec);
            } else {
                // Initialize without iv
                cipher.init(opmode, keySpec);
            }
        } catch (GeneralSecurityException e) {
            throw new RuntimeException(e);
        }
    }

    public byte[] Encode(byte[] data) {
        // Initialize the cipher
        Init(Cipher.ENCRYPT_MODE);
        
        // Create memory stream for building the encoded message
        ByteArrayOutputStream memStream = new ByteArrayOutputStream();
        CipherOutputStream cs = new CipherOutputStream(memStream, cipher); // Create cipherstream for encoding
        try {
             // Write and encode the data
            cs.write(data);
            
            // Calculate the required amount of zeroes padding and write it
            int padding = data.length % cipher.getBlockSize();
            while ((padding--) >= 0) cs.write(0);
            // Flush and close the cipherstream
            cs.close();
		} catch (IOException e) {
			throw new RuntimeException(e);
        }
        // Return encoded data
        return memStream.toByteArray();
    }
    public String EncodeToBase64(byte[] data) {
        return Base64.encodeToString(Encode(data), Base64.DEFAULT);
    }

    public byte[] Decode(byte[] data) {
        // Initialize cipher for decoding
        Init(Cipher.DECRYPT_MODE);
        
        // Create memory stream for building the  message
        ByteArrayOutputStream memStream = new ByteArrayOutputStream();
        CipherOutputStream cs = new CipherOutputStream(memStream, cipher); // Create cipherstream for decoding
        try {
            // Write data to the cipherstream to decode it onto the memStream
            cs.write(data);
            // Close and flush the cipherstream
            cs.close();
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
        // Return the decoded data
        return memStream.toByteArray();
    }
    public byte[] DecodeFromBase64(String base64Data) {
        return Decode(Base64.decode(base64Data, Base64.DEFAULT));
    }
}